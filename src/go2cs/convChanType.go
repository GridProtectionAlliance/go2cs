package main

import (
	"go/ast"
)

func (v *Visitor) convChanType(chanType *ast.ChanType) string {
	return convertToCSTypeName(v.getExprTypeName(chanType, false))
}
