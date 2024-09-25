package main

import (
	"go/ast"
)

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr) string {
	return "/* " + v.getPrintedNode(keyValueExpr) + " */"
}
