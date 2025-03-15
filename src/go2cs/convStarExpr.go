package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr, context StarExprContext) string {
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

	// In a parenthesis, we are applying a pointer cast operation
	if context.inParenExpr {
		pointerType := convertToCSTypeName(v.getTypeName(v.getType(starExpr.X, true), false))
		return fmt.Sprintf("%s<%s>", PointerPrefix, pointerType)
	}

	// Check for a call expr that contains a paren expr with a star expresssion inside it where starExpr.X is an identifier
	if callExpr, ok := starExpr.X.(*ast.CallExpr); ok {
		if len(callExpr.Args) == 1 {
			if parenExpr, ok := callExpr.Fun.(*ast.ParenExpr); ok {
				if _, ok := parenExpr.X.(*ast.StarExpr); ok {
					if ident := getIdentifier(parenExpr.X); ident != nil {
						// In this case we are dealing with a casted pointer dereference, e.g., "*(*int)"
						context := DefaultLambdaContext()
						context.isPointerCast = true
						return fmt.Sprintf("%s%s", PointerDerefOp, v.convExpr(starExpr.X, []ExprContext{context}))
					}
				}
			}
		}
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
