package main

import (
	"go/ast"
)

func (v *Visitor) convStructType(structType *ast.StructType) string {
	return "/* " + v.getPrintedNode(structType) + " */"
}
