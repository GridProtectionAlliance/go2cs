package main

import (
	"go/ast"
)

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt) {
	v.writeOutputLn("/* visitRangeStmt: " + v.getPrintedNode(rangeStmt) + " */")
}
