package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// typeCollidingFieldName renames a struct field whose C# name would equal its enclosing
// struct's type name (C# forbids it — CS0542) by prefixing the disambiguation marker.
func typeCollidingFieldName(name string) string {
	return ShadowVarMarker + name
}

// fieldCollidesWithType reports whether a field selector's name equals the C# type name of the
// struct it belongs to (`type Node struct{ Node *Node }` → field `Node` in struct `Node`).
func (v *Visitor) fieldCollidesWithType(sel *ast.Ident, x ast.Expr) bool {
	xType := v.info.TypeOf(x)

	if xType == nil {
		return false
	}

	if ptr, ok := xType.Underlying().(*types.Pointer); ok {
		xType = ptr.Elem()
	}

	named, ok := xType.(*types.Named)

	if !ok {
		return false
	}

	// Only a package-level named type keeps its Go name as the C# type name. A function-local
	// (or otherwise lifted) type is emitted under a qualified name (e.g. `Uncommon_u`), so its
	// field does not collide in C# even when the Go field and type names match — and
	// visitStructType only renames the field declaration in the package-level case.
	obj := named.Obj()

	if obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope() {
		return false
	}

	return getSanitizedIdentifier(sel.Name) == getSanitizedIdentifier(obj.Name())
}

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr, context LambdaContext) string {
	// Check if this is a method value being used in an assignment
	if v.isMethodValue(selectorExpr, context.isCallExpr) && context.isAssignment {
		// Check if selector expression needs to be converted to a lambda function for assignment
		if ident, ok := selectorExpr.X.(*ast.Ident); ok {
			if v.isPackageIdentifier(ident) {
				// This is a package selector (like fmt.Println) -- no need for lambda
				return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}

		return fmt.Sprintf("() => %s.%s()", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
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
					return fmt.Sprintf("(%s).val.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
				}
			}

			return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
		}

		if selection, ok := v.info.Selections[selectorExpr]; ok && selection.Kind() == types.FieldVal {
			if ptrType, isPtrType := exprType.(*types.Pointer); isPtrType {
				// Check if the expression is *directly* an intra-function identifier (the
				// receiver or a parameter) — a value-ref receiver/param accesses its fields
				// without a deref. Use a direct type assertion, NOT getIdentifier (which digs
				// to the root of a selector chain): for `e.list.root` the X is `e.list` (a
				// pointer field), which must be dereferenced even though the chain roots at `e`.
				if exprIdent, isIdent := selectorExpr.X.(*ast.Ident); v.inFunction && isIdent {
					if obj := v.info.ObjectOf(exprIdent); obj != nil {
						// Check if it's a receiver or parameter pointer variable
						if selVar, ok := obj.(*types.Var); ok {
							// If it's a receiver, skip dereferencing
							if v.currentFuncSignature.Recv() == selVar {
								return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}

							// Check if it's a function parameter
							params := v.currentFuncSignature.Params()

							for i := range params.Len() {
								// It's a function parameter, skip dereferencing
								if params.At(i) == selVar {
									return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
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
										return fmt.Sprintf("%s.val.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
									} else {
										return fmt.Sprintf("(%s%s).%s", PointerDerefOp, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Route a capture-mode method called on a heap-boxed value receiver through the ж
	// (pointer) overload, which sets up the receiver box the method needs for
	// `&recv.field` — e.g. `var i atomic.Int32; i.Store(10)` → `Ꮡi.Store(10)`. The
	// receiver is heap-boxed by escape analysis (see collectCaptureModeMethods), so its
	// "Ꮡname" companion exists.
	// The receiver may be a heap-boxed value var (escape analysis), the current method's own
	// direct-ж receiver (its box `Ꮡrecv` is the parameter) — e.g. `func (r *Ring) Next() {
	// return r.init() }` — or a deref'd pointer parameter (its box `Ꮡp` is the parameter), e.g.
	// `func (r *Ring) Link(s *Ring) { s.Prev() }`. In each case route through the box.
	if context.isCallExpr && v.isCaptureModeMethod(selectorExpr) && (v.isHeapBoxedExpr(selectorExpr.X) || v.exprIsCurrentDirectBoxReceiver(selectorExpr.X) || v.exprIsDerefdPointerParam(selectorExpr.X)) {
		return getAliasedTypeName(fmt.Sprintf("%s%s.%s", AddressPrefix, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	return getAliasedTypeName(fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
}

func (v *Visitor) getSelIdentContext(selectorExpr *ast.SelectorExpr) IdentContext {
	context := DefaultIdentContext()
	context.isMethod = true

	// Flag a field selector whose name collides with its enclosing struct's type name so the
	// access is renamed to match the renamed field declaration (CS0542).
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.FieldVal {
		context.fieldCollidesWithType = v.fieldCollidesWithType(selectorExpr.Sel, selectorExpr.X)
	}

	return context
}
