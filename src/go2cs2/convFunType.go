package main

import (
	"go/ast"
)

func (v *Visitor) convFuncType(funcType *ast.FuncType) string {
	return "/* convFuncType: " + v.getPrintedNode(funcType) + " */"
}
