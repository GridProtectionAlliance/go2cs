package main

import (
	"go/ast"
)

func (v *Visitor) convFuncType(funcType *ast.FuncType) string {
	return "/* " + v.getPrintedNode(funcType) + " */"
}
