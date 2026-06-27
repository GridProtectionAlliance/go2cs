package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// funcBodyDeferRecover reports whether a function body (a func declaration or a func literal)
// directly uses defer and/or recover — i.e. a `defer` statement at this function's own level,
// or a `recover()` call that is effective for this function (called directly, or inside one of
// this function's deferred closures). Nested function literals have their OWN defer/recover
// scope and are not counted, except that a deferred closure's recover() recovers this function.
//
// This is the per-function view used to decide whether a function needs the `func((defer,
// recover) => …)` execution context, so an enclosing function is not wrapped merely because a
// nested IIFE/closure uses defer/recover.
func (v *Visitor) funcBodyDeferRecover(body *ast.BlockStmt) (hasDefer bool, hasRecover bool) {
	if body == nil {
		return false, false
	}

	var walk func(n ast.Node) bool

	walk = func(n ast.Node) bool {
		if hasDefer && hasRecover {
			return false
		}

		switch node := n.(type) {
		case *ast.FuncLit:
			// A nested function literal owns its own defer/recover scope.
			return false
		case *ast.DeferStmt:
			hasDefer = true

			// A recover() inside the deferred call recovers THIS function.
			if v.exprContainsRecover(node.Call) {
				hasRecover = true
			}

			return false
		case *ast.CallExpr:
			if v.isRecoverCall(node) {
				hasRecover = true
			}
		}

		return true
	}

	ast.Inspect(body, walk)

	return hasDefer, hasRecover
}

// exprContainsRecover reports whether the expression contains a recover() call. It descends
// into function literals (a deferred closure — `defer func(){ recover() }()` — places recover
// inside the literal), which is where recover almost always appears.
func (v *Visitor) exprContainsRecover(expr ast.Node) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if call, ok := n.(*ast.CallExpr); ok && v.isRecoverCall(call) {
			found = true
			return false
		}

		return true
	})

	return found
}

// isRecoverCall reports whether the call is the built-in recover().
func (v *Visitor) isRecoverCall(call *ast.CallExpr) bool {
	ident, ok := call.Fun.(*ast.Ident)

	if !ok || ident.Name != "recover" {
		return false
	}

	_, isBuiltin := v.info.Uses[ident].(*types.Builtin)

	return isBuiltin
}

// iifeParamNames renders an IIFE's parameter list as names only (the delegate-cast supplies the
// types): `()` for none, `name` for one, `(n1, n2, …)` for several. Names follow variable
// analysis (shadow-renames), so the body's references resolve.
func (v *Visitor) iifeParamNames(sig *types.Signature) string {
	params := sig.Params()

	if params.Len() == 0 {
		return "()"
	}

	names := make([]string, params.Len())

	for i := range params.Len() {
		names[i] = v.iifeParamName(params.At(i))
	}

	if params.Len() == 1 {
		return names[0]
	}

	return "(" + strings.Join(names, ", ") + ")"
}

// iifeParamName returns the C# identifier the body uses for a parameter (honoring shadow-renames
// recorded by variable analysis); a blank parameter stays `_`.
func (v *Visitor) iifeParamName(param *types.Var) string {
	if isDiscardedVar(param.Name()) {
		return "_"
	}

	name := param.Name()

	if adjusted, ok := v.varNames[param]; ok {
		name = adjusted
	}

	return getSanitizedIdentifier(name)
}

// iifeDelegateType builds the C# delegate type for casting an IIFE's lambda so it can be invoked
// directly: `Action`/`Action<…>` for a void literal, `Func<…, TResult>` for a value-returning
// one (a tuple result type for multiple returns).
func (v *Visitor) iifeDelegateType(sig *types.Signature) string {
	params := sig.Params()
	results := sig.Results()

	typeArgs := make([]string, 0, params.Len()+1)

	for i := range params.Len() {
		typeArgs = append(typeArgs, v.getCSTypeName(params.At(i).Type()))
	}

	if results.Len() == 0 {
		if len(typeArgs) == 0 {
			return "Action"
		}

		return "Action<" + strings.Join(typeArgs, ", ") + ">"
	}

	var resultType string

	if results.Len() == 1 {
		resultType = v.getCSTypeName(results.At(0).Type())
	} else {
		resultTypes := make([]string, results.Len())

		for i := range results.Len() {
			resultTypes[i] = v.getCSTypeName(results.At(i).Type())
		}

		resultType = "(" + strings.Join(resultTypes, ", ") + ")"
	}

	typeArgs = append(typeArgs, resultType)

	return "Func<" + strings.Join(typeArgs, ", ") + ">"
}

// namedResultName returns the C# identifier the body uses for a named result parameter
// (honoring shadow-renames recorded by variable analysis).
func (v *Visitor) namedResultName(param *types.Var) string {
	if ident := v.getVarIdent(param); ident != nil {
		return getSanitizedIdentifier(v.getIdentName(ident))
	}

	return getSanitizedIdentifier(param.Name())
}

// detectNamedReturnDefer reports whether a function with the given signature needs the
// named-return-defer handling: it uses defer/recover AND all of its results are named. When so,
// it returns the result identifiers in order (used to declare them outside the func() wrapper
// and to return them after it runs, so deferred code — including recover — can mutate them).
func (v *Visitor) detectNamedReturnDefer(sig *types.Signature, hasDefer, hasRecover bool) (bool, []string) {
	if !(hasDefer || hasRecover) || sig == nil || sig.Results() == nil {
		return false, nil
	}

	results := sig.Results()

	if results.Len() == 0 {
		return false, nil
	}

	names := make([]string, 0, results.Len())

	for i := range results.Len() {
		param := results.At(i)

		if param.Name() == "" || isDiscardedVar(param.Name()) {
			return false, nil
		}

		names = append(names, v.namedResultName(param))
	}

	return true, names
}

// namedReturnDeclLines renders the `Type name = default!;` declarations for a signature's named
// results, each on its own line at the given indent level.
func (v *Visitor) namedReturnDeclLines(sig *types.Signature, indentLevel int) string {
	results := sig.Results()
	decls := strings.Builder{}

	for i := range results.Len() {
		param := results.At(i)
		decls.WriteString(fmt.Sprintf("%s%s%s %s = default!;", v.newline, v.indent(indentLevel), v.getCSTypeName(param.Type()), v.namedResultName(param)))
	}

	return decls.String()
}

// wrapIIFEDeferContext wraps an IIFE lambda body in a `func((defer, recover) => …)` execution
// context when the literal uses defer/recover, so it runs with its own scope. Both parameters
// are named (the func() built-in takes both); unused-parameter warnings are suppressed.
func wrapIIFEFuncContext(body string, hasDefer, hasRecover bool) string {
	if !hasDefer && !hasRecover {
		return body
	}

	return fmt.Sprintf("func((defer, recover) => %s)", body)
}
