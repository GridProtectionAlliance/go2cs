package main

import (
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, parentBlock *ast.BlockStmt) {
	result := &strings.Builder{}
	lhsLen := len(assignStmt.Lhs)
	rhsLen := len(assignStmt.Rhs)

	isAssignment := false

	// Handle LHS
	if lhsLen > 1 {
		result.WriteString("(")
	} else if assignStmt.Tok == token.DEFINE {
		result.WriteString("var ")
		isAssignment = true
	}

	for i, lhs := range assignStmt.Lhs {
		if i > 0 {
			result.WriteString(", ")
		}

		if isAssignment {
			if ident, ok := lhs.(*ast.Ident); ok {
				// Perform escape analysis to determine if the variable needs heap allocation
				v.performEscapeAnalysis(ident, parentBlock)
			}
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
