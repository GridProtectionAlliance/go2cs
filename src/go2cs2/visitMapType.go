package main

import (
	"go/ast"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitMapType(mapType *ast.MapType) {
	v.writeOutputLn("/* visitMapType: %s */", v.getPrintedNode(mapType))
}
