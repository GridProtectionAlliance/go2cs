package main

import (
	"go/ast"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	return "/* convSliceExpr: " + v.getPrintedNode(sliceExpr) + " */"
}
