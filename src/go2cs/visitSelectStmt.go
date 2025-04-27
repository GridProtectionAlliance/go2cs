package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"strconv"
)

func (v *Visitor) visitSelectStmt(selectStmt *ast.SelectStmt) {
	var comClauses []*ast.CommClause
	var defaultClause *ast.CommClause
	hasDefault := false

	for _, stmt := range selectStmt.Body.List {
		if comClause, ok := stmt.(*ast.CommClause); ok {

			if comClause.Comm == nil {
				defaultClause = comClause
				hasDefault = true
			} else {
				comClauses = append(comClauses, comClause)
			}
		} else {
			println("WARNING: unexpected Stmt type (non CommClause) encountered in SelectStmt")
		}
	}

	// Ensure default clause is the last clause
	if hasDefault {
		comClauses = append(comClauses, defaultClause)
	}

	v.targetFile.WriteString(v.newline)

	v.writeOutput("switch (")

	if hasDefault {
		v.targetFile.WriteString(TrueMarker)
	} else {
		v.targetFile.WriteString("select(")

		for i, comClause := range comClauses {
			if i > 0 {
				v.targetFile.WriteString(", ")
			}

			handled := false

			// Check if CommClause.Comm is an AssignStmt
			if assignStmt, ok := comClause.Comm.(*ast.AssignStmt); ok {
				lhsCount := len(assignStmt.Lhs)

				if (lhsCount == 1 || lhsCount == 2) && len(assignStmt.Rhs) == 1 {
					rhs := assignStmt.Rhs[0]

					if unaryExpr, ok := rhs.(*ast.UnaryExpr); ok {
						if unaryExpr.Op == token.ARROW {

							if v.options.useChannelOperators {
								v.targetFile.WriteString(ChannelLeftOp)
								v.targetFile.WriteRune('(')
								v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
								v.targetFile.WriteString(", ")
								v.targetFile.WriteString(EllipsisOperator)
								v.targetFile.WriteRune(')')
							} else {
								v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
								v.targetFile.WriteString(".Receiving")
							}
							handled = true
						}
					}
				} else {
					assignment := v.getPrintedNode(assignStmt)
					println(fmt.Sprintf("WARNING: @visitSelectStmt - Failed to resolve expected `AssignStmt` arguments: %s", assignment))
					v.targetFile.WriteString(fmt.Sprintf("/* %s */", assignment))
				}
			} else if sendStmt, ok := comClause.Comm.(*ast.SendStmt); ok {
				v.targetFile.WriteString(v.convExpr(sendStmt.Chan, nil))
				v.targetFile.WriteRune('.')

				if v.options.useChannelOperators {
					v.targetFile.WriteString(ChannelLeftOp)
				} else {
					v.targetFile.WriteString("Sending")
				}

				v.targetFile.WriteRune('(')
				v.targetFile.WriteString(v.convExpr(sendStmt.Value, nil))

				if v.options.useChannelOperators {
					v.targetFile.WriteString(", ")
					v.targetFile.WriteString(EllipsisOperator)
				}

				v.targetFile.WriteRune(')')
				handled = true
			} else if exprStmt, ok := comClause.Comm.(*ast.ExprStmt); ok {
				if unaryExpr, ok := exprStmt.X.(*ast.UnaryExpr); ok {
					if unaryExpr.Op == token.ARROW {
						if v.options.useChannelOperators {
							v.targetFile.WriteString(ChannelLeftOp)
							v.targetFile.WriteRune('(')
							v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
							v.targetFile.WriteString(", ")
							v.targetFile.WriteString(EllipsisOperator)
							v.targetFile.WriteRune(')')
						} else {
							v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
							v.targetFile.WriteString(".Receiving")
						}
						handled = true
					}
				}
			}

			if !handled {
				println("WARNING: unexpected Stmt type \"" + v.getPrintedNode(comClause.Comm) + "\" encountered in SelectStmt")
			}
		}

		v.targetFile.WriteRune(')')
	}

	v.targetFile.WriteString(") {")
	v.targetFile.WriteString(v.newline)

	for i, comClause := range comClauses {
		if i > 0 {
			v.targetFile.WriteString(v.newline)
		}

		if comClause.Comm == nil {
			v.writeOutput("default: {")
		} else {
			v.writeOutput("case ")

			if hasDefault {
				v.targetFile.WriteString(TrueMarker)
			} else {
				v.targetFile.WriteString(strconv.Itoa(i))
			}

			if comClause.Comm != nil {
				// Check if CommClause.Comm is an AssignStmt
				if assignStmt, ok := comClause.Comm.(*ast.AssignStmt); ok {
					lhsCount := len(assignStmt.Lhs)

					if (lhsCount == 1 || lhsCount == 2) && len(assignStmt.Rhs) == 1 {
						lhs := assignStmt.Lhs[0]
						rhs := assignStmt.Rhs[0]

						if unaryExpr, ok := rhs.(*ast.UnaryExpr); ok {
							if unaryExpr.Op == token.ARROW {
								v.targetFile.WriteString(" when ")
								v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
								v.targetFile.WriteRune('.')

								if v.options.useChannelOperators {
									v.targetFile.WriteString(ChannelRightOp)
								} else {
									v.targetFile.WriteString("Received")
								}

								v.targetFile.WriteString("(out ")

								if assignStmt.Tok == token.DEFINE {
									if v.options.preferVarDecl {
										v.targetFile.WriteString("var ")
									} else {
										exprType := convertToCSTypeName(v.getExprTypeName(lhs, false))
										v.targetFile.WriteString(exprType)
										v.targetFile.WriteRune(' ')
									}
								}

								v.targetFile.WriteString(v.convExpr(lhs, nil))

								if lhsCount == 2 {
									v.targetFile.WriteString(", out ")

									if assignStmt.Tok == token.DEFINE {
										if v.options.preferVarDecl {
											v.targetFile.WriteString("var ")
										} else {
											v.targetFile.WriteString("bool ")
										}
									}

									v.targetFile.WriteString(v.convExpr(assignStmt.Lhs[1], nil))
								}

								v.targetFile.WriteRune(')')
							}
						}
					}
				} else if exprStmt, ok := comClause.Comm.(*ast.ExprStmt); ok {
					if unaryExpr, ok := exprStmt.X.(*ast.UnaryExpr); ok {
						if unaryExpr.Op == token.ARROW {
							v.targetFile.WriteString(" when ")
							v.targetFile.WriteString(v.convExpr(unaryExpr.X, nil))
							v.targetFile.WriteRune('.')

							if v.options.useChannelOperators {
								v.targetFile.WriteString(ChannelRightOp)
							} else {
								v.targetFile.WriteString("Received")
							}

							v.targetFile.WriteString("(out _)")
						}
					}
				}
			}

			v.targetFile.WriteString(": {")
		}

		v.indentLevel++

		for _, stmt := range comClause.Body {
			v.visitStmt(stmt, []StmtContext{})
		}

		v.targetFile.WriteString(v.newline)

		if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
			v.writeOutputLn("break;")
		}

		v.indentLevel--
		v.writeOutput("}")
	}

	v.targetFile.WriteRune('}')
}
