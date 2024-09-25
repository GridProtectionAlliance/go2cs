package main

import (
	"go/ast"
)

func (v *Visitor) visitCommClause(commClause *ast.CommClause) {
	v.writeOutputLn("/* " + v.getPrintedNode(commClause) + " */")
}
