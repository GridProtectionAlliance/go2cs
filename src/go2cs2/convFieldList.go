package main

import (
	"go/ast"
)

func (v *Visitor) convFieldList(fieldList *ast.FieldList) string {
	return "/* " + v.getPrintedNode(fieldList) + " */"
}
