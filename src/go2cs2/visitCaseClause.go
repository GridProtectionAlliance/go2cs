package main

import (
	"go/ast"
)

func (v *Visitor) visitCaseClause(caseClause *ast.CaseClause) {
	v.writeOutputLn("/* " + v.getPrintedNode(caseClause) + " */")
}
