package main

import (
	"go/ast"
)

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit) string {
	return "/* convFuncLit: " + v.getPrintedNode(funcLit) + " */"
}
