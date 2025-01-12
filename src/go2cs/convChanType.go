package main

import (
	"go/ast"
)

func (v *Visitor) convChanType(chanType *ast.ChanType) string {
	return "/* convChanType: " + v.getPrintedNode(chanType) + " */"
}
