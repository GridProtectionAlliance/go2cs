package main

import (
	"go/ast"
)

func (v *Visitor) visitLabeledStmt(labeledStmt *ast.LabeledStmt) {
	v.targetFile.WriteString(v.newline)

	labelName := getSanitizedIdentifier(labeledStmt.Label.Name)

	v.targetFile.WriteString(labelName + ":")

	target := LabeledStmtContext{label: labelName}

	v.visitStmt(labeledStmt.Stmt, []StmtContext{target})
}
