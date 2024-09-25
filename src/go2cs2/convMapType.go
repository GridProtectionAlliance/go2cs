package main

import (
	"go/ast"
)

func (v *Visitor) convMapType(mapType *ast.MapType) string {
	return "/* " + v.getPrintedNode(mapType) + " */"
}
