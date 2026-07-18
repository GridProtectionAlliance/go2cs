package main

import (
	"go/ast"
	"go/types"
)

// typeIsArrayValue reports whether t's underlying type is a fixed-size Go array — a direct
// `[N]E`, an alias to one, or a NAMED array type (`type T [N]E`). Both renderings expose a
// strongly-typed `Clone()` (golib's `array<T>` and the generated named-array wrapper), so a
// Go by-value array copy can append `.Clone()` uniformly. A constrained type parameter never
// matches (its underlying is the constraint interface), keeping erased generics untouched.
func typeIsArrayValue(t types.Type) bool {
	if t == nil {
		return false
	}

	_, isArray := t.Underlying().(*types.Array)

	return isArray
}

// exprReadsArrayValueFromStorage reports whether expr reads an ARRAY value out of EXISTING
// storage — an ident, selector, index, or pointer-deref (after unwrapping parens) whose type's
// underlying is a *types.Array. Go copies the whole array at every value transfer (range
// element, composite-literal element, return, channel send, append element), but the emitted
// `array<T>` / named-array wrapper is a struct over a shared T[] backing store, so a plain C#
// struct copy ALIASES — such a transfer must append the strongly-typed `.Clone()`. Only
// existing storage needs it: a composite literal, call result, or make/new expression is
// freshly constructed and reachable by no other name. (The ASSIGNMENT/var-decl forms of the
// same defect are handled by cloneArrayValueCopy in visitAssignStmt.go.)
func (v *Visitor) exprReadsArrayValueFromStorage(expr ast.Expr) bool {
	for {
		paren, ok := expr.(*ast.ParenExpr)

		if !ok {
			break
		}

		expr = paren.X
	}

	switch expr.(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.StarExpr:
	default:
		return false
	}

	return typeIsArrayValue(v.getExprType(expr))
}

// withArrayValueCloneArgs flags every POSITIONAL composite-literal element that reads an ARRAY
// value out of existing storage for the `.Clone()` suffix (CallExprContext.cloneArrayArg — Go
// copies the array into the composite's slot, and the emitted struct copy would alias its
// backing). Allocates the context when nil and a flag is needed, matching the nil-context
// defaults convExprList assumes (u8 string literals stay allowed). Keyed elements clone in
// convKeyValueExpr instead.
func (v *Visitor) withArrayValueCloneArgs(elts []ast.Expr, context *CallExprContext) *CallExprContext {
	for i, elt := range elts {
		if _, isKeyed := elt.(*ast.KeyValueExpr); isKeyed {
			continue
		}

		if !v.exprReadsArrayValueFromStorage(elt) {
			continue
		}

		if context == nil {
			context = DefaultCallExprContext()

			for j := range elts {
				context.u8StringArgOK[j] = true
			}
		}

		if context.cloneArrayArg == nil {
			context.cloneArrayArg = make(map[int]bool)
		}

		context.cloneArrayArg[i] = true
	}

	return context
}
