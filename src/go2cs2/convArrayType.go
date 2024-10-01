package main

import (
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType) string {
	return v.convExpr(arrayType.Elt) + "[]"
}
