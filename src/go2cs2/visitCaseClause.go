package main

import (
	"go/ast"
)

func (v *Visitor) visitCaseClause(caseClause *ast.CaseClause) {
	v.writeOutputLn("/* visitCaseClause: " + v.getPrintedNode(caseClause) + " */")
}