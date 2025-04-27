package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr, context StarExprContext) string {
	ident := getIdentifier(starExpr.X)
	pointerRecv, recvName := v.isPointerReceiver()

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
		baseExpr := v.convExpr(starExpr.X, nil)
		pointerDepth := v.getSelectorExprPointerDepth(selectorExpr)

		// For multi-level pointers, we need to add enough .val
		if pointerDepth > 1 {
			baseExpr += ".val"
		} else if _, ok := v.getIdentType(selectorExpr.Sel).(*types.Pointer); !ok {
			// Selector is not a pointer, assume this is a pointer cast operation
			return fmt.Sprintf("%s<%s>", PointerPrefix, baseExpr)
		}

		return baseExpr + ".val"
	}

	// In a parenthesis, we are applying a pointer cast operation
	if context.inParenExpr {
		// Check if the pointer target type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(starExpr); structType != nil && !v.liftedTypeExists(structType) {
			v.indentLevel++
			v.visitStructType(structType, exprType, "type", nil, true, nil)
			v.indentLevel--
		}

		// Check if the pointer target type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(starExpr); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			v.indentLevel++
			v.visitInterfaceType(interfaceType, exprType, "type", nil, true, nil)
			v.indentLevel--
		}

		starType := v.getType(starExpr.X, false)
		pointerType := convertToCSTypeName(v.getTypeName(starType, false))
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
						result := v.convExpr(starExpr.X, []ExprContext{context})
						return fmt.Sprintf("%s%s", PointerDerefOp, result)
					}
				}
			}
		}
	}

	// Default behavior for other cases
	if pointerRecv && ident != nil && recvName == ident.Name {
		return v.convExpr(starExpr.X, nil)
	}

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
