package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr) string {
	var contexts []ExprContext
	var ptrDeref string

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		// Check if the type is a map and its key is an empty interface
		if mapType, isMap := typeAndVal.Type.(*types.Map); isMap {
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

	return fmt.Sprintf("%s%s[%s]", v.convExpr(indexExpr.X, nil), ptrDeref, v.convExpr(indexExpr.Index, contexts))
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
