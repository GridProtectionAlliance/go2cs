package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"sort"
	"strings"
)

// Analyze captures before processing a lambda expression
func (v *Visitor) performLambdaAnalysis(node ast.Node, context *LambdaContext) {
	v.lambdaContext.Push(context)
	defer v.lambdaContext.Pop()
	defer v.exitLambda()

	captureMap := make(map[*ast.Ident]*ast.Ident)
	v.lambdaStack.Push(captureMap)
	v.inLambda = true

	// Find all identifiers that might be captured
	ast.Inspect(node, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok {
			if v.shouldCapture(ident, context) {
				v.createCapture(ident, captureMap, context)
			}
		}
		return true
	})
}

func (v *Visitor) shouldCapture(ident *ast.Ident, context *LambdaContext) bool {
	if _, exists := v.capturedVars[ident]; exists {
		return false
	}

	// Check if identifier is from outer scope
	obj := v.info.ObjectOf(ident)
	if obj == nil || obj.Parent() == nil {
		return false
	}

	// Check for recursive captures
	if context.parentIdent != nil {
		if obj.Parent() == v.info.ObjectOf(context.parentIdent).Parent() {
			return false
		}
	}

	// Determine if the variable needs copying
	return v.capturedLambdaVarRequiresCopy(obj.Type())
}

func (v *Visitor) createCapture(ident *ast.Ident, captureMap map[*ast.Ident]*ast.Ident, context *LambdaContext) {
	v.needsCopy[ident] = true

	copyName := v.getCapturedVarName(context.parentIdent.Name)
	copyIdent := &ast.Ident{Name: copyName}

	v.capturedVars[ident] = copyIdent
	captureMap[ident] = copyIdent

	v.pendingCaptures[copyName] = &captureInfo{
		copyIdent:    copyIdent,
		origIdent:    context.parentIdent, // Use parent ident here
		used:         false,
		originalType: context.originalType,
	}
}

// Single entry point for lambda analysis
func (v *Visitor) preAnalyzeLambdas(node ast.Node, context LambdaContext) {
	switch n := node.(type) {
	case *ast.FuncLit:
		v.performLambdaAnalysis(n, &context)
	case *ast.SelectorExpr:
		if v.isMethodValue(n, context.isCallExpr) {
			v.performLambdaAnalysis(n, &context)
		}
	case *ast.AssignStmt:
		for i, rhs := range n.Rhs {
			if v.requiresLambdaConversion(rhs) {
				ctx := context
				if i < len(n.Lhs) {
					if ident, ok := n.Lhs[i].(*ast.Ident); ok {
						ctx.parentIdent = ident
						if typ := v.info.TypeOf(ident); typ != nil {
							ctx.originalType = typ
						}
					}
				}
				v.performLambdaAnalysis(rhs, &ctx)
			}
		}
	}
}

// Generate declarations for pending captures
func (v *Visitor) generateCaptureDeclarations() string {
	if len(v.pendingCaptures) == 0 {
		return ""
	}

	var decls strings.Builder

	// Sort names for consistent output
	names := make([]string, 0, len(v.pendingCaptures))

	for name, info := range v.pendingCaptures {
		if !info.used {
			names = append(names, name)
		}
	}

	sort.Strings(names)

	// Generate declarations
	for _, name := range names {
		info := v.pendingCaptures[name]
		decls.WriteString(v.newline)
		decls.WriteString(v.indent(v.indentLevel))
		decls.WriteString("var ")
		decls.WriteString(info.copyIdent.Name)
		decls.WriteString(" = ")
		decls.WriteString(info.origIdent.Name)
		decls.WriteString(";")
		info.used = true
	}

	return decls.String()
}

// Determine if a type needs to be copied when captured
func (v *Visitor) capturedLambdaVarRequiresCopy(t types.Type) bool {
	switch typ := t.Underlying().(type) {
	case *types.Array, *types.Struct:
		return true
	case *types.Named:
		return v.capturedLambdaVarRequiresCopy(typ.Underlying())
	case *types.Interface:
		// Check if interface could contain struct/array
		// This is conservative - might want to refine based on known interface types
		return true
	}
	return false
}

func (v *Visitor) getCapturedVarName(varPrefix string) string {
	if v.capturedVarCount == nil {
		v.capturedVarCount = make(map[string]int)
	}

	count := v.capturedVarCount[varPrefix]
	count++
	v.capturedVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, CapturedVarMarker, count)
}

// Clean up after processing a lambda
func (v *Visitor) exitLambda() {
	if !v.lambdaStack.IsEmpty() {
		v.lambdaStack.Pop()
	}
	v.inLambda = v.lambdaStack.Len() > 0
}

// Helper to determine if an expression requires lambda conversion
func (v *Visitor) requiresLambdaConversion(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.FuncLit:
		return true

	case *ast.SelectorExpr:
		return v.isMethodValue(e, false)

	case *ast.Ident:
		// Check if identifier refers to a function value
		if obj := v.info.ObjectOf(e); obj != nil {
			_, isFunc := obj.(*types.Func)
			return isFunc
		}

	case *ast.CallExpr:
		// Check if this is a function call that returns a function
		if typ := v.info.TypeOf(e); typ != nil {
			_, isFunc := typ.Underlying().(*types.Signature)
			return isFunc
		}
	}
	return false
}

func (v *Visitor) isMethodValue(sel *ast.SelectorExpr, isCallExpr bool) bool {
	if sel.Sel == nil {
		return false
	}

	obj := v.info.ObjectOf(sel.Sel)
	if obj == nil {
		return false
	}

	_, isFunc := obj.(*types.Func)
	if !isFunc {
		return false
	}

	// Not a method value if it's being called
	return !isCallExpr
}
