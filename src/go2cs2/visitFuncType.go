package main

import (
	"go/ast"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType) {
	v.writeOutputLn("/* %s */", v.getPrintedNode(funcType))
}
