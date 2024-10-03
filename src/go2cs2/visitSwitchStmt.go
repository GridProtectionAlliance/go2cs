package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt, parentBlock *ast.BlockStmt) {
	var caseClauses []*ast.CaseClause
	var caseClauseFallthroughs []bool

	for _, stmt := range switchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
			caseClauseFallthroughs = append(caseClauseFallthroughs, false)
		} else {
			println(fmt.Sprintf("WARNING: unexpected Stmt type (non CaseClause) encountered in SwitchStmt: %T", stmt))
		}
	}

	allConst := true
	hasFallthroughs := false

	for i, caseClause := range caseClauses {
		if caseClause.List == nil {
			continue
		}

		// Check if all case clauses are constant values
		for _, expr := range caseClause.List {
			if !v.info.Types[expr].IsValue() {
				allConst = false
				break
			}
		}

		// Check if any case clause has a fallthrough statement
		if caseClause.Body != nil {
			for _, stmt := range caseClause.Body {
				if branchStmt, ok := stmt.(*ast.BranchStmt); ok && branchStmt.Tok == token.FALLTHROUGH {
					hasFallthroughs = true
					caseClauseFallthroughs[i] = true
				}
			}
		}
	}

	v.switchIndentLevel++

	// TODO: Handle variable redeclaration in switch statement init statement
	if switchStmt.Init != nil {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutputLn("{")
		v.indentLevel++

		v.visitStmt(switchStmt.Init, parentBlock)
	}

	v.targetFile.WriteString(v.newline)

	if hasFallthroughs {
		// Most complex scenario with standalone if's, fallthrough tests and goto case break-outs
	} else if allConst && switchStmt.Tag != nil {
		// Most simple scenario when all case values are constant, a common C# switch will suffice
		v.writeOutput("switch (")
		v.targetFile.WriteString(v.convExpr(switchStmt.Tag, nil))
		v.targetFile.WriteString(") {")
		v.targetFile.WriteString(v.newline)

		for _, caseClause := range caseClauses {
			if caseClause.List == nil {
				v.writeOutput("default:")
			} else {
				for i, expr := range caseClause.List {
					if i > 0 {
						v.targetFile.WriteString(v.newline)
					}

					v.writeOutput("case ")
					v.targetFile.WriteString(v.convExpr(expr, nil))
					v.targetFile.WriteString(":")
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, parentBlock)
			}

			v.targetFile.WriteString(v.newline)
			v.writeOutputLn("break;")

			v.indentLevel--
		}

		v.writeOutputLn("}")
	} else {
		// Most common scenario where expression switch becomes "if / else if / else" statements
	}

	// for i, caseClause := range caseClauses {

	// }

	// v.writeOutput("switch (")

	// if switchStmt.Tag == nil {
	// 	v.targetFile.WriteString("true")
	// } else {
	// 	v.targetFile.WriteString(v.convExpr(switchStmt.Tag))
	// }

	// v.targetFile.WriteString(")")

	//v.visitBlockStmt(switchStmt.Body, true)

	// Close any locally scoped declared variable sub-block
	if switchStmt.Init != nil {
		v.indentLevel--
		v.writeOutputLn("}")
	}

	v.switchIndentLevel--
}
