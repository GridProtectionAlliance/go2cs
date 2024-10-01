package main

import (
	"go/ast"
)

func (v *Visitor) convInterfaceType(interfaceType *ast.InterfaceType) string {
	return "/* convInterfaceType: " + v.getPrintedNode(interfaceType) + " */"
}
