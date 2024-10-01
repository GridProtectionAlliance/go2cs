package main

import (
	"go/ast"
)

// Handles channel types in context of a TypeSpec
func (v *Visitor) visitChanType(chanType *ast.ChanType) {
	v.writeOutputLn("/* visitChanType: %s */", v.getPrintedNode(chanType))
}
