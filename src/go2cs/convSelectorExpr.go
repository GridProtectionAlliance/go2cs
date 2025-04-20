package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr, context LambdaContext) string {
	// Check if this is a method value being used in an assignment
	if v.isMethodValue(selectorExpr, context.isCallExpr) && context.isAssignment {
		// Check if selector expression needs to be converted to a lambda function for assignment
		if ident, ok := selectorExpr.X.(*ast.Ident); ok {
			if v.isPackageIdentifier(ident) {
				// This is a package selector (like fmt.Println) -- no need for lambda
				return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
			}
		}

		return fmt.Sprintf("() => %s.%s()", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
	}

	// Check if an expression contains an explicit dereference at any level
	containsExplicitDeref := func(expr ast.Expr) bool {
		found := false
		ast.Inspect(expr, func(n ast.Node) bool {
			if _, isStarExpr := n.(*ast.StarExpr); isStarExpr {
				found = true
				return false // Stop traversal if we found a star expression
			}
			return true // Continue traversal
		})
		return found
	}

	// Get the original expression type and check if it's a pointer
	if exprType := v.info.TypeOf(selectorExpr.X); exprType != nil {
		// Check if there's an explicit dereference at any level in the expression
		if containsExplicitDeref(selectorExpr.X) {
			if callExpr, ok := selectorExpr.X.(*ast.CallExpr); ok {
				// Check if the call expressions is a parenthesized expression
				if _, ok := callExpr.Fun.(*ast.ParenExpr); ok {
					return fmt.Sprintf("(%s).val.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
				}
			}

			return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
		}

		if selection, ok := v.info.Selections[selectorExpr]; ok && selection.Kind() == types.FieldVal {
			if ptrType, isPtrType := exprType.(*types.Pointer); isPtrType {
				// Check if the expression is an intra-function identifier (could be a receiver or parameter)
				if exprIdent := getIdentifier(selectorExpr.X); v.inFunction && exprIdent != nil {
					if obj := v.info.ObjectOf(exprIdent); obj != nil {
						// Check if it's a receiver or parameter pointer variable
						if selVar, ok := obj.(*types.Var); ok {
							// If it's a receiver, skip dereferencing
							if v.currentFuncSignature.Recv() == selVar {
								return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
							}

							// Check if it's a function parameter
							params := v.currentFuncSignature.Params()

							for i := range params.Len() {
								// It's a function parameter, skip dereferencing
								if params.At(i) == selVar {
									return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
								}
							}
						}
					}
				}

				if obj := v.info.ObjectOf(selectorExpr.Sel); obj != nil {
					// Check if the field belongs to the struct that the pointer points to, rather than
					// to the pointer itself, if so, the pointer has been automatically dereferenced
					if _, ok := obj.(*types.Var); ok {
						// Make sure the field is not receiver target
						if structType, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
							for i := range structType.NumFields() {
								field := structType.Field(i)
								if field.Name() == selectorExpr.Sel.Name {
									// If the field belongs to the struct, automatically dereference the pointer
									if context.isAssignment {
										// Left-hand side of assignment cannot use pointer dereference operator
										return fmt.Sprintf("%s.val.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
									} else {
										return fmt.Sprintf("(%s%s).%s", PointerDerefOp, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext()))
									}
								}
							}
						}
					}
				}
			}
		}
	}

	return getAliasedTypeName(fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, getSelIdentContext())))
}

func getSelIdentContext() IdentContext {
	context := DefaultIdentContext()
	context.isMethod = true
	return context
}
