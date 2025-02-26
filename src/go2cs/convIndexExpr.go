package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr) string {
	var contexts []ExprContext

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		// Check if the type is a map and its key is an empty interface
		if mapType, isMap := typeAndVal.Type.(*types.Map); isMap {
			// Check if the key type is an empty interface
			if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
				context := DefaultBasicLitContext()
				context.u8StringOK = false
				contexts = []ExprContext{context}
			}
		}
	}

	return fmt.Sprintf("%s[%s]", v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, contexts))
}
