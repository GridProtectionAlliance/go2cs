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
			v.showWarning("@visitSelectStmt - unexpected Stmt type (non CommClause) encountered in SelectStmt")
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
					v.showWarning("@visitSelectStmt - Failed to resolve expected 'AssignStmt' arguments: %s", assignment)
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
				v.targetFile.WriteString(v.convSendValueExpr(sendStmt))

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
				v.showWarning("@visitSelectStmt - unexpected Stmt type \"" + v.getPrintedNode(comClause.Comm) + "\" encountered in SelectStmt")
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

		// visitStmt resets this per statement, but an EMPTY clause body (a bare `default:` — io
		// pipe.go read's poll select) never calls visitStmt, so the flag would carry the PREVIOUS
		// clause's terminal `return` and suppress the mandatory `break;` (C# requires every switch
		// section to end in a jump: CS8070 on a final `default:`, CS0163 otherwise).
		v.lastStatementWasReturn = false

		// Escaping comm-clause bindings receive into a temp and box it at the top of the
		// clause body (see selectCommBinding); the label emission below collects the decls.
		var commBoxDecls []string

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

								boundName, boxDecl := v.selectCommBinding(lhs, assignStmt.Tok == token.DEFINE)
								v.targetFile.WriteString(boundName)

								if boxDecl != "" {
									commBoxDecls = append(commBoxDecls, boxDecl)
								}

								if lhsCount == 2 {
									v.targetFile.WriteString(", out ")

									if assignStmt.Tok == token.DEFINE {
										if v.options.preferVarDecl {
											v.targetFile.WriteString("var ")
										} else {
											v.targetFile.WriteString("bool ")
										}
									}

									boundName, boxDecl = v.selectCommBinding(assignStmt.Lhs[1], assignStmt.Tok == token.DEFINE)
									v.targetFile.WriteString(boundName)

									if boxDecl != "" {
										commBoxDecls = append(commBoxDecls, boxDecl)
									}
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

		for _, boxDecl := range commBoxDecls {
			v.targetFile.WriteString(v.newline)
			v.writeOutput("%s", boxDecl)
		}

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

	// A blocking select (no `default:`) lowers to `switch (select(…))` whose sections are guarded
	// `case N when <recv>:` labels — C# cannot prove the switch exhaustive, so a value-returning
	// function ending in it fails CS0161 even though Go's spec makes such a select (every comm
	// clause terminating, no select-level break) a terminating statement. Mirror visitSwitchStmt's
	// guarded-terminal-default emission: an unreachable trailing `return default!;`. A clause that
	// can fall OUT (a select-targeting `break`, or a non-terminating body) disqualifies — the
	// trailing return would be reachable and silently return the zero value. Skip in
	// namedReturnDefer mode (void wrapper — a value return is CS8030, and none is needed).
	if !hasDefault && !v.namedReturnDeferMode && v.currentReturnSignature != nil && v.currentReturnSignature.Results().Len() > 0 {
		allClausesTerminal := true

		for _, comClause := range comClauses {
			if caseBodyHasSwitchBreak(comClause.Body) || !isTerminatingStmtList(comClause.Body) {
				allClausesTerminal = false
				break
			}
		}

		if allClausesTerminal {
			v.targetFile.WriteString(v.newline)
			v.writeOutput("return default!;")
		}
	}
}

// selectCommBinding returns the identifier bound in a comm-clause's `out` slot and, when a
// DEFINE-mode binding escapes to the heap, the box declaration that must open the clause body.
// A `case result := <-ch:` whose bound variable has its address taken in the clause body
// (`c.crashMinimizing = &result`, internal/fuzz coordinatorLoop) needs the same `Ꮡresult` heap
// companion an escaping `:=` local gets — the body's address-of emission references it (CS0103
// without) — but the `when` guard's `out var` slot cannot declare a ref local. Receive into a
// uniquely-numbered temp instead, and open the clause body with the entry-time box pattern
// proven by visitFuncDecl's escaping-parameter preamble: `ref var result = ref heap(resultᴛ1,
// out var Ꮡresult);`. The gate is identHasHeapBox — the exact predicate the body's `&name`
// emission uses — so the box is declared iff it is referenced. The alias/box names mirror
// convertToHeapTypeDecl: the value alias takes the SANITIZED analyzed name, the box the raw
// analyzed name behind the Ꮡ prefix (matching boxBaseName's resolution at every `&name` site).
// A non-escaping binding keeps the direct `out var <name>` form (preserving the shadow-rename
// path — convExpr renders the analyzed name, e.g. `errΔ5`), and an ASSIGN-mode rebind of an
// existing boxed local already writes through its ref alias — both unchanged, no churn.
func (v *Visitor) selectCommBinding(lhs ast.Expr, isDefine bool) (boundName string, boxDecl string) {
	if isDefine {
		if ident, ok := lhs.(*ast.Ident); ok && ident.Name != "_" {
			if obj := v.info.Defs[ident]; obj != nil && v.identHasHeapBox(obj, v.getIdentType(ident)) {
				csIDName := v.getIdentName(ident)
				tempName := getGlobalTempVarName(csIDName)

				return tempName, fmt.Sprintf("ref var %s = ref heap(%s, out var %s%s);", getSanitizedIdentifier(csIDName), tempName, AddressPrefix, csIDName)
			}
		}
	}

	return v.convExpr(lhs, nil), ""
}
