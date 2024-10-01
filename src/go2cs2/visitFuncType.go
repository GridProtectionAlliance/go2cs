package main

import (
	"go/ast"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType) {
	v.writeOutputLn("/* visitFuncType: %s */", v.getPrintedNode(funcType))
}
