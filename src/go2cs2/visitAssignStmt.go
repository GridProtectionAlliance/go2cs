package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, parentBlock *ast.BlockStmt) {
	result := &strings.Builder{}
	lhsLen := len(assignStmt.Lhs)
	rhsLen := len(assignStmt.Rhs)

	if lhsLen != rhsLen {
		println("WARNING: AssignStmt Lhs and Rhs lengths do not match")
		v.writeOutputLn("/* visitAssignStmt mismatch: " + v.getPrintedNode(assignStmt) + " */")
		return
	}

	reassignedCount := 0
	declaredCount := 0

	// Count the number of reassigned and declared variables
	for _, lhs := range assignStmt.Lhs {
		ident := getIdentifier(lhs)

		if ident != nil {
			if v.isReassignment(ident) {
				reassignedCount++
			} else {
				// Perform escape analysis to determine if the variable needs heap allocation
				v.performEscapeAnalysis(ident, parentBlock)

				if !v.identEscapesHeap[ident] {
					declaredCount++
				}
			}
		}
	}

	v.targetFile.WriteString(v.newline)

	if lhsLen == reassignedCount || lhsLen == declaredCount {
		// Handle LHS
		if declaredCount > 0 {
			result.WriteString("var ")
		}

		if lhsLen > 1 {
			result.WriteString("(")
		}

		for i, lhs := range assignStmt.Lhs {
			if i > 0 {
				result.WriteString(", ")
			}

			result.WriteString(v.convExpr(lhs, nil))
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
			result.WriteString(v.convExpr(rhs, nil))
		}

		if rhsLen > 1 {
			result.WriteString(")")
		}

		result.WriteString(";")
	} else {
		// Some variables are declared and some are reassigned
		for i := 0; i < lhsLen; i++ {
			lhs := assignStmt.Lhs[i]
			rhs := assignStmt.Rhs[i]

			if i > 0 {
				result.WriteString(v.newline)
				result.WriteString(v.indent(v.indentLevel))
			}

			ident := getIdentifier(lhs)

			if ident == nil {
				// Handle other types of LHS expressions
				result.WriteString("// " + v.getPrintedNode(lhs))
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(" = ")
					result.WriteString(v.convExpr(rhs, nil))
					result.WriteString(";")
				} else {
					heapTypeDecl := v.convertToHeapTypeDecl(ident)

					if len(heapTypeDecl) > 0 {
						result.WriteString(heapTypeDecl)
						result.WriteString(v.newline)
						result.WriteString(v.indent(v.indentLevel))
					} else {
						result.WriteString("var ")
					}

					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(" = ")
					result.WriteString(v.convExpr(rhs, nil))
					result.WriteString(";")
				}
			}
		}
	}

	v.targetFile.WriteString(v.newline)
	v.writeOutput(result.String())
}
