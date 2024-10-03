package main

import (
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType) string {
	return convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + "[]"
}
