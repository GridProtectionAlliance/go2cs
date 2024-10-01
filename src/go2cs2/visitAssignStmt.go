package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt) {
	result := &strings.Builder{}
	lhsLen := len(assignStmt.Lhs)
	rhsLen := len(assignStmt.Rhs)

	// Handle LHS
	if lhsLen > 1 {
		result.WriteString("(")
	} else {
		result.WriteString("var ")
	}

	for i, lhs := range assignStmt.Lhs {
		if i > 0 {
			result.WriteString(", ")
		}
		result.WriteString(v.convExpr(lhs))
	}

	if lhsLen > 1 {
		result.WriteString(")")
	}

	result.WriteString(" = ")

	// Handle RHS
	if rhsLen > 1 {
		result.WriteString("(")
	}

	for i, rhs := range assignStmt.Rhs {
		if i > 0 {
			result.WriteString(", ")
		}
		result.WriteString(v.convExpr(rhs))
	}

	if rhsLen > 1 {
		result.WriteString(")")
	}

	result.WriteString(";")

	v.targetFile.WriteString(v.newline)
	v.writeOutput(result.String())
}
