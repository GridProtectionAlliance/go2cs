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
// recorded by variable analysis); a blank parameter stays `_`. A heap-boxed value parameter
// (see funcLitHeapBoxParamIdents) arrives under its incoming `ʗp` name — the literal's prologue
// re-declares the Go name as the boxed ref alias — so it emits that name here.
func (v *Visitor) iifeParamName(param *types.Var) string {
	if isDiscardedVar(param.Name()) {
		return "_"
	}

	name := param.Name()

	if adjusted, ok := v.varNames[param]; ok {
		name = adjusted
	}

	if v.funcLitHeapBoxParamNames.Contains(name) {
		return getHeapBoxLitParamName(name)
	}

	return getSanitizedIdentifier(name)
}

// iifeDelegateType builds the C# delegate type for casting an IIFE's lambda so it can be invoked
// directly: `Action`/`Action<…>` for a void literal, `Func<…, TResult>` for a value-returning
// one (a tuple result type for multiple returns). Because getCSTypeName routes every anonymous
// (and collapsed methodless named) func type here, this is THE lowering of a Go func type used
// as a value.
//
// A VARIADIC signature lowers to the golib Actionꓸꓸꓸ/Funcꓸꓸꓸ family, whose trailing
// `params Span<T>` carries the variadic tail — the delegate's LAST type argument is the variadic
// ELEMENT type (the signature stores []T; the Span shell lives in the delegate declaration).
// This makes the three previously-incompatible lowerings agree: the named-function convention
// (`params ꓸꓸꓸT argsʗp`) and a variadic func literal convert to it natively (the parameter
// types match by identity), calls through the value pass loose Go-style arguments or an empty
// tail via C# params expansion, and a spread slice binds its `.ꓸꓸꓸ` Span in normal form —
// go/types' `reportf Action<@string, slice<any>>` took a `params ꓸꓸꓸany` literal (CS1661/CS1678)
// and loose-arg calls (CS1503/CS7036) before this.
func (v *Visitor) iifeDelegateType(sig *types.Signature) string {
	params := sig.Params()
	results := sig.Results()

	variadic := sig.Variadic() && params.Len() > 0

	typeArgs := make([]string, 0, params.Len()+1)

	for i := range params.Len() {
		paramType := params.At(i).Type()

		if variadic && i == params.Len()-1 {
			if sliceType, ok := paramType.Underlying().(*types.Slice); ok {
				paramType = sliceType.Elem()
			} else {
				// Defensive: a variadic signature's last param is always []T; if not, keep
				// the plain Action/Func lowering rather than a mis-shaped family reference.
				variadic = false
			}
		}

		typeArgs = append(typeArgs, v.aliasedElementTypeName(paramType))
	}

	family := ""

	if variadic {
		family = EllipsisOperator
	}

	if results.Len() == 0 {
		if len(typeArgs) == 0 {
			return "Action"
		}

		return "Action" + family + "<" + strings.Join(typeArgs, ", ") + ">"
	}

	var resultType string

	if results.Len() == 1 {
		resultType = v.aliasedElementTypeName(results.At(0).Type())
	} else {
		resultTypes := make([]string, results.Len())

		for i := range results.Len() {
			resultTypes[i] = v.aliasedElementTypeName(results.At(i).Type())
		}

		resultType = "(" + strings.Join(resultTypes, ", ") + ")"
	}

	typeArgs = append(typeArgs, resultType)

	return "Func" + family + "<" + strings.Join(typeArgs, ", ") + ">"
}

// signatureTypeName renders a func type structurally in GO syntax — `func(name type, …)
// results` — with every parameter/result type resolved recursively through getTypeName, so a
// cross-package element carries the short import-alias qualification the surrounding file uses
// (`types.Package`, `tls.Conn`), exactly like the map/slice/chan structural renders. The
// t.String() fall-through this replaces embeds slash-qualified import paths
// (`func(imports map[string]*go/types.Package, …)`), and convertToCSFullTypeName's slash
// heuristics then mangle them one of three ways depending on the string's shape: a naive
// whole-string path conversion (`ж<go.types.Package>`, `ж<crypto.tls.Conn>` — no `_package`
// class, and under a `go.go`-nested namespace the leading segment binds the wrong namespace),
// or an unrooted class-suffixed form (`@internal.trace_package.UtilFlags`) — CS0234 across
// go/importer, net/http's TLSNextProto maps, and traceviewer (one root, three variants).
// Same-package and builtin elements render byte-identically to the old path (t.String()'s
// package-path prefix strip), so simple signatures see no churn. A VARIADIC tail renders as
// `...elem` (which t.String()'s `..`-strip used to reduce to the unparseable `.elem`); the
// string-path func-type conversion lowers it to the golib Actionꓸꓸꓸ/Funcꓸꓸꓸ delegate family,
// mirroring iifeDelegateType. Parameter and result NAMES are preserved so a named multi-result
// signature keeps its named C# tuple (see convertToCSResultList).
func (v *Visitor) signatureTypeName(sig *types.Signature, isUnderlying bool) string {
	result := strings.Builder{}
	result.WriteString("func(")

	params := sig.Params()

	for i := range params.Len() {
		if i > 0 {
			result.WriteString(", ")
		}

		param := params.At(i)

		if name := param.Name(); name != "" {
			result.WriteString(name)
			result.WriteByte(' ')
		}

		if sig.Variadic() && i == params.Len()-1 {
			if sliceType, ok := param.Type().Underlying().(*types.Slice); ok {
				result.WriteString("...")
				result.WriteString(v.getTypeName(sliceType.Elem(), isUnderlying))
				continue
			}
		}

		result.WriteString(v.getTypeName(param.Type(), isUnderlying))
	}

	result.WriteByte(')')

	results := sig.Results()

	if results.Len() == 1 && results.At(0).Name() == "" {
		result.WriteByte(' ')
		result.WriteString(v.getTypeName(results.At(0).Type(), isUnderlying))
	} else if results.Len() > 0 {
		result.WriteString(" (")

		for i := range results.Len() {
			if i > 0 {
				result.WriteString(", ")
			}

			resultVar := results.At(i)

			if name := resultVar.Name(); name != "" {
				result.WriteString(name)
				result.WriteByte(' ')
			}

			result.WriteString(v.getTypeName(resultVar.Type(), isUnderlying))
		}

		result.WriteByte(')')
	}

	return result.String()
}

// namedResultName returns the C# identifier the body uses for a named result parameter
// (honoring shadow-renames recorded by variable analysis).
// aliasedElementTypeName renders a delegate element type, substituting the file-local
// ꓸ-alias form for a cross-package named type that is Δ-RENAMED inside its own package -
// `syscall.Handle` declares `ΔHandle`, so the raw qualified render (`Δsyscall.Handle`)
// names a type that does not exist (CS0426 x26, internal/poll's hook_windows func-typed
// globals). The imported-type-alias registry already records the foreign rename
// (`global using syscallꓸHandle = go.syscall_package.ΔHandle`); a type without a
// registered alias, a generic instantiation, and every non-named element keep the plain
// getCSTypeName render (no churn). Pointer elements recurse so `*syscall.Overlapped`
// becomes `ж<syscallꓸOverlapped>`.
func (v *Visitor) aliasedElementTypeName(t types.Type) string {
	if ptr, ok := types.Unalias(t).(*types.Pointer); ok {
		return fmt.Sprintf("%s<%s>", PointerPrefix, v.aliasedElementTypeName(ptr.Elem()))
	}

	if named, ok := types.Unalias(t).(*types.Named); ok {
		if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg && named.TypeArgs().Len() == 0 {
			plainKey := fmt.Sprintf("%s.%s", getSanitizedIdentifier(pkg.Name()), getCoreSanitizedIdentifier(named.Obj().Name()))

			packageLock.Lock()
			_, exists := importedTypeAliases[plainKey]
			packageLock.Unlock()

			// A foreign type reached ONLY through ANOTHER package's signature has no alias
			// registered: the preload covers packages the current package imports DIRECTLY,
			// but go/types renders go/ast's `FieldFilter` — whose `reflect.Value` parameter
			// type go/types itself never imports — so the rename (reflect declares ΔValue)
			// was missed and the raw `reflect.Value` resolved inside reflect_package
			// (CS0426, plus CS0123 on the mismatched delegate). Load the owning package's
			// exported aliases on demand — the dedup in loadImportedTypeAliases makes this
			// a one-time no-op per package — and re-check; the resolving `global using`
			// then rides the normal package_info emission (assembly references are
			// transitive, so the consumer sees the type through its importer's reference).
			if !exists {
				v.loadImportedTypeAliases(pkg.Path())

				packageLock.Lock()
				_, exists = importedTypeAliases[plainKey]
				packageLock.Unlock()
			}

			if exists {
				return getAliasedTypeName(plainKey)
			}
		}
	}

	return v.getCSTypeName(t)
}

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

		// A promoted-embed struct result must construct through the NilType ctor — `default!`
		// leaves the readonly embed boxes null (see structHasPromotedEmbeds).
		zeroValue := "default!"

		if v.structHasPromotedEmbeds(param.Type()) {
			zeroValue = "new(nil)"
		}

		decls.WriteString(fmt.Sprintf("%s%s%s %s = %s;", v.newline, v.indent(indentLevel), v.getCSTypeName(param.Type()), v.namedResultName(param), zeroValue))
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
