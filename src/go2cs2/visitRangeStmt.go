package main

import (
	"go/ast"
)

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(rangeStmt) + " */")
}
