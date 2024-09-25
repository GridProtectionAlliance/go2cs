package main

import (
	"go/ast"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType) {
	v.writeOutputLn("/* %s */", v.getPrintedNode(interfaceType))
}
