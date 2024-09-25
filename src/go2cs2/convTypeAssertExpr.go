package main

import (
	"go/ast"
)

func (v *Visitor) convTypeAssertExpr(typeAssertExpr *ast.TypeAssertExpr) string {
	return "/* " + v.getPrintedNode(typeAssertExpr) + " */"
}
