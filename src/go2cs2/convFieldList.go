package main

import (
	"go/ast"
)

func (v *Visitor) convFieldList(fieldList *ast.FieldList) string {
	return "/* convFieldList: " + v.getPrintedNode(fieldList) + " */"
}
