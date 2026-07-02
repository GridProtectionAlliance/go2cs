package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr, context IndexExprContext) string {
	var contexts []ExprContext
	var ptrDeref string

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		// Check if the type is a map and its key is an empty interface
		if mapType, isMap := typeAndVal.Type.(*types.Map); isMap {
			// Comma-ok map access (`v, ok := m[k]`): use golib's two-value indexer
			// `m[key, ꟷ]`, which returns `(value, present)`.
			if context.isTupleResult {
				keyContexts := []ExprContext{}

				if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
					basicLitContext := DefaultBasicLitContext()
					basicLitContext.u8StringOK = false
					keyContexts = append(keyContexts, basicLitContext)
				}

				return fmt.Sprintf("%s[%s, %s]", v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, keyContexts), OverloadDiscriminator)
			}

			// Check if the key type is an empty interface
			if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
				context := DefaultBasicLitContext()
				context.u8StringOK = false
				contexts = []ExprContext{context}
			}
		} else if _, isPtr := typeAndVal.Type.(*types.Pointer); isPtr {
			ident := getIdentifier(indexExpr.X)

			if ident == nil || !v.identIsParameter(ident) {
				ptrDeref = ".val"
			}
		}
	}

	if v.isGenericTypeArgument(indexExpr) {
		context := DefaultIdentContext()
		context.isType = true

		if len(contexts) > 0 {
			contexts = append(contexts, contexts...)
		} else {
			contexts = []ExprContext{context}
		}

		return fmt.Sprintf("%s%s<%s>", v.convExpr(indexExpr.X, nil), ptrDeref, v.convExpr(indexExpr.Index, contexts))
	}

	index := v.convExpr(indexExpr.Index, contexts)

	// A STRING base indexed by a wide/unsigned integer: a string LITERAL renders as a
	// ReadOnlySpan<byte> (`"…"u8`) whose indexer takes int — a uintptr index is CS1503
	// (runtime heapdump.go's `"0123456789abcdef"[pc&15]`, pc a uintptr). Go converts any
	// integer index to int for the access; route the wide kinds (uint/uint32/uint64/
	// uintptr/int64) through the same `(int)` cast the element-address seams use. An
	// `@string` variable's indexer binds an int argument too, so the cast is safe for
	// both renders; an int/small-integer index is emitted unchanged (no churn).
	if baseType := v.getType(indexExpr.X, false); baseType != nil {
		if basic, ok := baseType.Underlying().(*types.Basic); ok && basic.Info()&types.IsString != 0 {
			index = v.castWideIntegerToInt(indexExpr.Index)
		}
	}

	return fmt.Sprintf("%s%s[%s]", v.convExpr(indexExpr.X, nil), ptrDeref, index)
}

func (v *Visitor) isGenericTypeArgument(indexExpr *ast.IndexExpr) bool {
	switch index := indexExpr.Index.(type) {
	case *ast.Ident:
		// Check if this identifier refers to a type
		if obj := v.info.Uses[index]; obj != nil {
			_, isTypeName := obj.(*types.TypeName)
			return isTypeName
		}
	}

	return false
}
