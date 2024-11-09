package main

import (
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	if arrayType.Len == nil && !context.compositeInitializer {
		return "slice<" + convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + ">"
	}

	return convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + "[]"
}
