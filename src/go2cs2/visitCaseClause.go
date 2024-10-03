package main

import (
	"go/ast"
)

func (v *Visitor) visitCaseClause(caseClause *ast.CaseClause) {
	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("/* visitCaseClause: " + v.getPrintedNode(caseClause) + " */")
}
