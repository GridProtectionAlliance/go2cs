package main

import (
	"go/ast"
)

func (v *Visitor) convUnaryExpr(arrayType *ast.UnaryExpr) string {
	return arrayType.Op.String() + v.convExpr(arrayType.X)
}
