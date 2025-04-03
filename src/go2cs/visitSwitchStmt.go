package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt) {
	var caseClauses []*ast.CaseClause
	var caseHasFallthroughStmt []bool

	for _, stmt := range switchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
			caseHasFallthroughStmt = append(caseHasFallthroughStmt, false)
		} else {
			println(fmt.Sprintf("WARNING: unexpected Stmt type (non CaseClause) encountered in SwitchStmt: %T", stmt))
		}
	}

	allConst := true
	namedTypes := false
	hasFallthroughs := false
	defaultCaseFallsThrough := false

	for i, caseClause := range caseClauses {
		if caseClause.List == nil {
			if i > 0 && caseHasFallthroughStmt[i-1] {
				defaultCaseFallsThrough = true
			}
			continue
		}

		// Check if all case clauses are constant values
		for _, expr := range caseClause.List {
			// Check if the expression is a function call or a non-value type
			if !v.isNonCallValue(expr) {
				allConst = false
				break
			}

			tv, ok := v.info.Types[expr]

			if !ok {
				break
			}

			// Named typed are not constant values in C# conversion
			if _, ok := tv.Type.(*types.Named); ok {
				allConst = false
				namedTypes = true
				break
			}
		}

		// Check if any case clause has a fallthrough statement
		if caseClause.Body != nil {
			for _, stmt := range caseClause.Body {
				if branchStmt, ok := stmt.(*ast.BranchStmt); ok && branchStmt.Tok == token.FALLTHROUGH {
					hasFallthroughs = true
					caseHasFallthroughStmt[i] = true
				}
			}
		}
	}

	if switchStmt.Init != nil {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(switchStmt.Init, []StmtContext{})
	}

	v.targetFile.WriteString(v.newline)

	if hasFallthroughs || (!allConst && switchStmt.Tag != nil) {
		// Most complex scenario with standalone if's, and fallthrough
		exprVarName := v.getTempVarName("expr")

		var matchVarName string

		if hasFallthroughs {
			matchVarName = v.getTempVarName("match")
		}

		if switchStmt.Tag != nil {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				exprType := convertToCSTypeName(v.getExprTypeName(switchStmt.Tag, false))
				v.targetFile.WriteString(exprType)
				v.targetFile.WriteRune(' ')
			}

			v.targetFile.WriteString(exprVarName)
			v.targetFile.WriteString(" = ")
			v.targetFile.WriteString(v.convExpr(switchStmt.Tag, nil))
			v.targetFile.WriteString(";" + v.newline)
		}

		if hasFallthroughs {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				v.writeOutput("bool ")
			}

			v.targetFile.WriteString(matchVarName)
			v.targetFile.WriteString(" = false;" + v.newline)
		}

		// Write "if" statements for each case clause
		for i, caseClause := range caseClauses {
			v.writeOutput("")

			caseFallsThrough := false

			// Case falls through if the previous case clause has a fallthrough statement
			if i > 0 && caseHasFallthroughStmt[i-1] {
				caseFallsThrough = true
			}

			nextClauseIsDefault := i < len(caseClauses)-1 && (i == len(caseClauses)-2 || caseClauses[i+1].List == nil)

			if i > 0 && !caseFallsThrough && !v.lastStatementWasReturn {
				v.targetFile.WriteString("else ")
			}

			// Handle default case
			if caseClause.List == nil {
				if caseFallsThrough {
					v.targetFile.WriteString(fmt.Sprintf("if (fallthrough || !%s) { /* default: */", matchVarName))
				} else {
					v.targetFile.WriteString("{ /* default: */")
				}
			} else {
				caseClauseCount := len(caseClause.List)

				v.targetFile.WriteString("if (")

				usePattenMatch := !namedTypes && v.canUsePatternMatch(caseClauseCount, caseClause, switchStmt.Tag != nil)

				if caseFallsThrough {
					v.targetFile.WriteString(fmt.Sprintf("fallthrough || !%s && ", matchVarName))

					if caseClauseCount > 1 || !usePattenMatch && switchStmt.Tag == nil {
						v.targetFile.WriteRune('(')
					}
				}

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
						v.targetFile.WriteRune('(')
					}

					v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))

					if !usePattenMatch && caseClauseCount > 1 && switchStmt.Tag == nil {
						v.targetFile.WriteRune(')')
					}

					if i == caseClauseCount-1 {
						if hasFallthroughs {
							if caseFallsThrough && caseClauseCount > 1 || !usePattenMatch && switchStmt.Tag == nil {
								v.targetFile.WriteRune(')')
							}

							v.targetFile.WriteString(") {")

							if !nextClauseIsDefault || defaultCaseFallsThrough {
								v.targetFile.WriteRune(' ')
								v.targetFile.WriteString(matchVarName)
								v.targetFile.WriteString(" = true;")
							}
						} else {
							v.targetFile.WriteString(") {")
						}
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{})
			}

			v.targetFile.WriteString(v.newline)

			if caseHasFallthroughStmt[i] {
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
						v.targetFile.WriteRune(':')
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{})
			}

			v.targetFile.WriteString(v.newline)

			if !v.lastStatementWasReturn {
				v.writeOutputLn("break;")
			}

			v.indentLevel--
		}

		v.writeOutputLn("}")
	} else {
		// Most common scenario with expression switches
		v.writeOutput("switch (%s) {%s", TrueMarker, v.newline)

		for _, caseClause := range caseClauses {
			if caseClause.List == nil {
				v.writeOutput("default:")
			} else {
				// Use pattern match when all case list expressions are
				// use comparison operators and the same target
				caseClauseCount := len(caseClause.List)
				usePattenMatch := !namedTypes && v.canUsePatternMatch(caseClauseCount, caseClause, switchStmt.Tag != nil)

				for i, expr := range caseClause.List {
					if i == 0 {
						v.writeOutput("case {} when ")
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
						v.targetFile.WriteRune('(')
					}

					v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))

					if !usePattenMatch && caseClauseCount > 1 {
						v.targetFile.WriteRune(')')
					}

					if i == caseClauseCount-1 {
						v.targetFile.WriteRune(':')
					}
				}
			}

			v.indentLevel++

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{})
			}

			v.targetFile.WriteString(v.newline)

			if !v.lastStatementWasReturn {
				v.writeOutputLn("break;")
			}

			v.indentLevel--
		}

		v.writeOutputLn("}")
	}

	// Close any locally scoped declared variable sub-block
	if switchStmt.Init != nil {
		v.indentLevel--
		v.writeOutputLn("}")
	}
}

func (v *Visitor) canUsePatternMatch(caseClauseCount int, caseClause *ast.CaseClause, inferExprTarget bool) bool {
	usePattenMatch := true

	if inferExprTarget {
		// Verify that all expressions are non-call values
		for _, expr := range caseClause.List {
			if !v.isNonCallValue(expr) {
				usePattenMatch = false
				break
			}
		}

		return usePattenMatch
	}

	if caseClauseCount > 0 {
		var firstExpr string

		for i, expr := range caseClause.List {
			// Check if expression uses comparison operators
			binaryExpr, ok := expr.(*ast.BinaryExpr)

			if !ok {
				usePattenMatch = false
				break
			}

			if !isComparisonOperator(binaryExpr.Op) {
				usePattenMatch = false
				break
			}

			// Make sure no rhs expression targets are call expressions
			if !v.isNonCallValue(binaryExpr.Y) {
				usePattenMatch = false
				break
			}

			// Check if all lhs expression targets are the same
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
