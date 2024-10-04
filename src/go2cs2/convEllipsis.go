package main

import (
	"go/ast"
)

func (v *Visitor) convEllipsis(ellipsis *ast.Ellipsis) string {
	return "/* convEllipsis: " + v.getPrintedNode(ellipsis) + " */"
}
