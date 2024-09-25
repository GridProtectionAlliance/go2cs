package main

import (
	"go/ast"
)

func (v *Visitor) visitLabeledStmt(labeledStmt *ast.LabeledStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(labeledStmt) + " */")
}
