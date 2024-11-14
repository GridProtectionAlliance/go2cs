package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	if arrayType.Len == nil && !context.compositeInitializer {
		return "slice<" + convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + ">"
	}

	var arraySuffix string

	if context.maxLength > 0 {
		arraySuffix = fmt.Sprintf("[%d]", context.maxLength)
	} else {
		arraySuffix = "[]"
	}

	return convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + arraySuffix
}
