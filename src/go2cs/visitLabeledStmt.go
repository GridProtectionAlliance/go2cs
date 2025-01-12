package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitLabeledStmt(labeledStmt *ast.LabeledStmt) {
	v.targetFile.WriteString(v.newline)

	labelName := getSanitizedIdentifier(labeledStmt.Label.Name)

	v.targetFile.WriteString(labelName + ":")

	target := LabeledStmtContext{label: labelName}

	v.visitStmt(labeledStmt.Stmt, []StmtContext{target})
}

func getBreakLabelName(label string) string {
	return getSanitizedIdentifier("break_" + strings.TrimPrefix(label, "@"))
}

func getContinueLabelName(label string) string {
	return getSanitizedIdentifier("continue_" + strings.TrimPrefix(label, "@"))
}
