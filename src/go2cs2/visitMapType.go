package main

import (
	"go/ast"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitMapType(mapType *ast.MapType) {
	v.writeOutputLn("/* %s */", v.getPrintedNode(mapType))
}
