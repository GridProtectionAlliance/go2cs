package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr) string {
	ident := getIdentifier(starExpr.X)

	if ident != nil && v.identIsParameter(ident) {
		// Check if the star expression is a pointer to pointer dereference
		if v.isPointer(ident) {
			if _, ok := starExpr.X.(*ast.StarExpr); ok {
				return v.convExpr(starExpr.X, nil) + ".val"
			}
		}

		// Prefer to use local reference instead of dereferencing a pointer
		return v.convExpr(starExpr.X, nil)
	}

	// Special handling for field access (e.g., outer.ptr)
	if selectorExpr, ok := starExpr.X.(*ast.SelectorExpr); ok {
		pointerDepth := v.getSelectorExprPointerDepth(selectorExpr)
		baseExpr := v.convExpr(starExpr.X, nil)

		// For multi-level pointers, we need to add enough .val
		if pointerDepth > 1 {
			baseExpr += ".val"
		}
		return baseExpr + ".val"
	}

	// Default behavior for other cases
	return v.convExpr(starExpr.X, nil) + ".val"
}

// Helper to get pointer depth for selector expressions
func (v *Visitor) getSelectorExprPointerDepth(expr ast.Expr) int {
	exprType := v.info.TypeOf(expr)
	if exprType == nil {
		return 0
	}

	depth := 0
	current := exprType
	for {
		ptrType, ok := current.(*types.Pointer)
		if !ok {
			break
		}
		depth++
		current = ptrType.Elem()
	}
	return depth
}
