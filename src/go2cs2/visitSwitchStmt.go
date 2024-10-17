package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt, source ParentBlockContext) {
	var caseClauses []*ast.CaseClause
	var caseClauseFallsthrough []bool

	for _, stmt := range switchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
			caseClauseFallsthrough = append(caseClauseFallsthrough, false)
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
			// Check if the expression is a function call or a non-value type
			_, isCallExpr := expr.(*ast.CallExpr)

			if !v.info.Types[expr].IsValue() || isCallExpr {
				allConst = false
				break
			}
		}

		// Check if any case clause has a fallthrough statement
		if caseClause.Body != nil {
			for _, stmt := range caseClause.Body {
				if branchStmt, ok := stmt.(*ast.BranchStmt); ok && branchStmt.Tok == token.FALLTHROUGH {
					hasFallthroughs = true
					caseClauseFallsthrough[i] = true
				}
			}
		}
	}

	hasSwitchInit := switchStmt.Init != nil || hasFallthroughs || !allConst && switchStmt.Tag != nil

	if hasSwitchInit {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		if switchStmt.Init != nil {
			v.visitStmt(switchStmt.Init, []StmtContext{source})
		}
	}

	v.targetFile.WriteString(v.newline)

	if hasFallthroughs || (!allConst && switchStmt.Tag != nil) {
		// Most complex scenario with standalone if's, and fallthrough
		exprVarName := v.getTempVarName("expr")
		matchVarName := v.getTempVarName("match")

		if switchStmt.Tag != nil {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				exprType := convertToCSTypeName(v.getTypeName(switchStmt.Tag, false))
				v.targetFile.WriteString(exprType)
				v.targetFile.WriteRune(' ')
			}

			v.targetFile.WriteString(exprVarName)
			v.targetFile.WriteString(" = ")
			v.targetFile.WriteString(v.convExpr(switchStmt.Tag, nil))
			v.targetFile.WriteString(";" + v.newline)
		}

		if v.options.preferVarDecl {
			v.writeOutput("var ")
		} else {
			v.writeOutput("bool ")
		}

		v.targetFile.WriteString(matchVarName)
		v.targetFile.WriteString(" = false;" + v.newline)

		// Write "if" statements for each case clause
		for i, caseClause := range caseClauses {
			v.writeOutput("if (")

			if i > 0 && caseClauseFallsthrough[i-1] {
				v.targetFile.WriteString("fallthrough || ")
			}

			if caseClause.List == nil {
				// Handle default case
				v.targetFile.WriteString(fmt.Sprintf("!%s) { /* default: */", matchVarName))
			} else {
				caseClauseCount := len(caseClause.List)
				usePattenMatch := v.canUsePatternMatch(caseClauseCount, caseClause)

				for i, expr := range caseClause.List {
					if i == 0 {
						if switchStmt.Tag != nil {
							v.targetFile.WriteString(exprVarName)

							if usePattenMatch {
								v.targetFile.WriteString(" is ")
							} else {
								v.targetFile.WriteString(" == ")
							}
						}
					} else {
						if usePattenMatch {
							v.targetFile.WriteString(" or ")
						} else {
							v.targetFile.WriteString(" || ")

							if switchStmt.Tag != nil {
								v.targetFile.WriteString(exprVarName)
								v.targetFile.WriteString(" == ")
							}
						}
					}

					context := DefaultPatternMatchExprContext()

					if usePattenMatch {
						context.usePattenMatch = true
						context.declareIsExpr = i == 0
					} else if caseClauseCount > 1 && switchStmt.Tag == nil {
						v.targetFile.WriteString("(")
					}

					v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))

					if !usePattenMatch && caseClauseCount > 1 && switchStmt.Tag == nil {
						v.targetFile.WriteString(")")
					}

					if i == caseClauseCount-1 {
						v.targetFile.WriteString(") { ")
						v.targetFile.WriteString(matchVarName)
						v.targetFile.WriteString(" = true;")
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{source})
			}

			v.targetFile.WriteString(v.newline)

			if caseClauseFallsthrough[i] {
				v.writeOutputLn("fallthrough = true;")
			}

			v.indentLevel--
			v.writeOutputLn("}")
		}
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
					if i == 0 {
						v.writeOutput("case ")
					} else {
						v.targetFile.WriteString(" or ")
					}

					v.targetFile.WriteString(v.convExpr(expr, nil))

					if i == len(caseClause.List)-1 {
						v.targetFile.WriteString(":")
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{source})
			}

			v.targetFile.WriteString(v.newline)
			v.writeOutputLn("break;")

			v.indentLevel--
		}

		v.writeOutputLn("}")
	} else {
		// Most common scenario with expression switches
		v.writeOutput("switch (%s) {%s", ExprSwitchMarker, v.newline)

		for _, caseClause := range caseClauses {
			if caseClause.List == nil {
				v.writeOutput("default:")
			} else {
				// Use pattern match when all case list expressions are
				// use comparison operators and the same target
				caseClauseCount := len(caseClause.List)
				usePattenMatch := v.canUsePatternMatch(caseClauseCount, caseClause)

				for i, expr := range caseClause.List {
					if i == 0 {
						v.writeOutput("case %s when ", ExprSwitchMarker)
					} else {
						if usePattenMatch {
							v.targetFile.WriteString(" or ")
						} else {
							v.targetFile.WriteString(" || ")
						}
					}

					context := DefaultPatternMatchExprContext()

					if usePattenMatch {
						context.usePattenMatch = true
						context.declareIsExpr = i == 0
					} else if caseClauseCount > 1 {
						v.targetFile.WriteString("(")
					}

					v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))

					if !usePattenMatch && caseClauseCount > 1 {
						v.targetFile.WriteString(")")
					}

					if i == caseClauseCount-1 {
						v.targetFile.WriteString(":")
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{source})
			}

			v.targetFile.WriteString(v.newline)
			v.writeOutputLn("break;")

			v.indentLevel--
		}

		v.writeOutputLn("}")
	}

	// Close any locally scoped declared variable sub-block
	if hasSwitchInit {
		v.indentLevel--
		v.writeOutputLn("}")
	}
}

func (v *Visitor) canUsePatternMatch(caseClauseCount int, caseClause *ast.CaseClause) bool {
	usePattenMatch := true

	if caseClauseCount > 0 {
		var firstExpr string

		for i, expr := range caseClause.List {
			// Check if expression uses comparison operators
			binaryExpr, ok := expr.(*ast.BinaryExpr)

			if !ok {
				usePattenMatch = false
				break
			}

			if !isComparisonOperator(binaryExpr.Op.String()) {
				usePattenMatch = false
				break
			}

			// Check if all expression targets are the same
			if i == 0 {
				firstExpr = v.convExpr(binaryExpr.X, nil)
			} else if firstExpr != v.convExpr(binaryExpr.X, nil) {
				usePattenMatch = false
				break
			}
		}
	} else {
		usePattenMatch = false
	}

	return usePattenMatch
}
