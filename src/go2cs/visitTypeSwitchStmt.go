package main

import (
	"fmt"
	"go/ast"
	"strings"
)

// visitTypeSwitchStmt wraps the core emission with the labeled-statement break target
// (see visitSwitchStmt - same CS0159 rule).
func (v *Visitor) visitTypeSwitchStmt(typeSwitchStmt *ast.TypeSwitchStmt, target LabeledStmtContext) {
	v.visitTypeSwitchStmtCore(typeSwitchStmt)

	if len(target.label) > 0 {
		v.targetFile.WriteString(v.newline)
		v.writeOutput("%s:;", getBreakLabelName(target.label))
	}
}

func (v *Visitor) visitTypeSwitchStmtCore(typeSwitchStmt *ast.TypeSwitchStmt) {
	var caseClauses []*ast.CaseClause

	for _, stmt := range typeSwitchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
		} else {
			v.showWarning("@visitTypeSwitchStmt - unexpected Stmt type (non CaseClause) encountered in TypeSwitchStmt: %T", stmt)
		}
	}

	if typeSwitchStmt.Init != nil {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(typeSwitchStmt.Init, []StmtContext{})
	}

	v.targetFile.WriteString(v.newline)

	v.writeOutput("switch (")

	var targetIdent, typeVar, guardExpr string
	assign := typeSwitchStmt.Assign

	// Check if the assignment statement is a ExprStmt
	if exprStmt, ok := assign.(*ast.ExprStmt); ok {
		v.targetFile.WriteString(v.convExpr(exprStmt.X, nil))
	} else {
		assignStmt := assign.(*ast.AssignStmt)
		targetIdent = v.convExpr(assignStmt.Lhs[0], nil)
		typeVar = v.convExpr(assignStmt.Rhs[0], nil)

		// The DEFAULT arm re-binds the guard to the ORIGINAL interface value: the switch
		// operand form (`x.type()`) is object, which cannot flow back out as the interface
		// (go/build/constraint pushNot's `default: return x`, CS0266).
		if typeAssert, ok := assignStmt.Rhs[0].(*ast.TypeAssertExpr); ok {
			guardExpr = v.convExpr(typeAssert.X, nil)
		}

		v.targetFile.WriteString(typeVar)
	}

	v.targetFile.WriteString(") {")
	v.targetFile.WriteString(v.newline)

	identContext := DefaultIdentContext()
	identContext.isType = true

	// Go `int`/`uint` are matched in a type switch by BOTH their native (`nint`/`nuint`) and a
	// synthetic concrete (`int32`/`uint32`) boxed form (added below). But if the SAME switch also
	// has an explicit `case int32:`/`case uint32:` (or `case rune:` ≡ int32), the synthetic case
	// would be emitted first and make the explicit one unreachable (CS8120) — and steal its values.
	// Pre-collect the explicitly-listed concrete case types so the synthetic is skipped when it
	// would duplicate one (runtime's printpanicval switches over int, int32, uint, uint32, …).
	explicitCaseTypes := map[string]bool{}

	for _, caseClause := range caseClauses {
		for _, expr := range caseClause.List {
			if ident, ok := expr.(*ast.Ident); ok {
				switch ident.Name {
				case "int32", "rune":
					explicitCaseTypes["int32"] = true
				case "uint32":
					explicitCaseTypes["uint32"] = true
				}
			}
		}
	}

	// Go type distinctions can VANISH in the C# type map — historically `case uint:` and
	// `case uintptr:` both resolved to System.UIntPtr, making the later case unreachable (CS8120;
	// runtime error.go's printpanicval). A duplicate-mapped case is merged (skipped, with a marker
	// comment) ONLY when its Go body is byte-identical to the first occurrence's — then the earlier
	// label already routes it exactly; DIFFERING bodies keep both labels, preferring the compile
	// error over silently routing one Go case into another's body. Keyed by the RESOLVED C# type;
	// values are the printed Go bodies. (uintptr is now a DISTINCT golib struct — golib/uintptr.cs
	// — so the uint/uintptr pair no longer collides and both original labels emit; the merge
	// machinery remains for the alias pairs that DO share a C# type.)
	emittedCaseTypes := map[string]string{}

	resolveCaseType := func(caseExpr string) string {
		typeToken, _, _ := strings.Cut(caseExpr, " ")

		switch typeToken {
		case "rune":
			return "int32"
		case "byte":
			return "uint8"
		}

		return typeToken
	}

	for i, caseClause := range caseClauses {
		if i > 0 {
			v.targetFile.WriteString(v.newline)
		}

		if caseClause.List == nil {
			// Emit `default: {` WITHOUT a trailing newline (mirroring the `case …: {` path below):
			// each visited body statement supplies its own LEADING newline, so a trailing newline here
			// would double up into a spurious blank line after the `{`.
			v.writeOutput("default: {")
			v.indentLevel++

			if len(targetIdent) > 0 {
				v.targetFile.WriteString(v.newline)

				boundExpr := typeVar

				if guardExpr != "" {
					boundExpr = guardExpr
				}

				if v.options.preferVarDecl || guardExpr != "" {
					v.writeOutput("var")
				} else {
					v.writeOutput("object")
				}

				v.targetFile.WriteString(fmt.Sprintf(" %s = %s;", targetIdent, boundExpr))
			}

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{})
			}

			v.targetFile.WriteString(v.newline)

			if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
				v.writeOutputLn("break;")
			}

			v.indentLevel--
			v.writeOutput("}")
		} else {
			var caseExprs []string

			// A MULTI-TYPE case clause (`case *Alias, *Named:`) binds its case variable at the
			// TAG's static (interface) type — Go binds the listed concrete type only when the
			// clause lists exactly ONE type. Emit stacked UNBOUND labels over a single shared
			// body and re-bind the variable to the guard expression (like the default arm), so
			// every body use compiles at the interface type (`isGeneric(t)` with `t` a Type, not
			// a ж<Alias> — go/types nonGeneric, CS1503/CS0266/CS1929 ×18). A single-type clause
			// keeps the concrete declaration-pattern binding.
			multiTypeCase := len(caseClause.List) > 1
			bindIdent := targetIdent

			if multiTypeCase {
				bindIdent = ""
			}

			for i, expr := range caseClause.List {
				// A `case nil` in a type switch matches a nil interface — emit the C# `case null:`
				// pattern (which binds nothing), not `case default! x:` (convExpr(nil) → "default!",
				// which is not a valid pattern). The bound variable is the nil value and is rarely
				// used in such a case.
				if ident, ok := expr.(*ast.Ident); ok && ident.Name == "nil" {
					caseExprs = append(caseExprs, "null")
					continue
				}

				caseExpr := v.convExpr(expr, []ExprContext{identContext})

				if v.isDynamicInterface(expr) {
					if len(bindIdent) > 0 && bindIdent != "_" {
						// case {} Δx when Δx._<liftedIfaceType>(out var x):
						tempTarget := fmt.Sprintf("%s%s", ShadowVarMarker, bindIdent)
						caseExpr = fmt.Sprintf("{} %s when %s._<%s>(out var %s)", tempTarget, tempTarget, caseExpr, bindIdent)
					} else {
						// case {} t1 when t1._<liftedIfaceType>(out _):
						tempTarget := fmt.Sprintf("%s%d", TempVarMarker, i)
						caseExpr = fmt.Sprintf("{} %s when %s._<%s>(out var _)", tempTarget, tempTarget, caseExpr)
					}
				} else {
					caseExpr = fmt.Sprintf("%s %s", caseExpr, bindIdent)
				}

				caseExprs = append(caseExprs, caseExpr)

				if strings.HasPrefix(caseExpr, "nint ") {
					if !explicitCaseTypes["int32"] {
						caseExprs = append(caseExprs, fmt.Sprintf("int32 %s", bindIdent))
					}
				} else if strings.HasPrefix(caseExpr, "nuint ") {
					if !explicitCaseTypes["uint32"] {
						caseExprs = append(caseExprs, fmt.Sprintf("uint32 %s", bindIdent))
					}
				}
			}

			// The clause's printed Go body — the identity key for the duplicate-mapped-case merge.
			printedBody := &strings.Builder{}

			for _, stmt := range caseClause.Body {
				printedBody.WriteString(v.getPrintedNode(stmt))
			}

			bodyKey := printedBody.String()
			outputs := 0

			if multiTypeCase {
				// Stacked labels: apply the duplicate-mapped-case merge per label first (writing
				// its marker), then emit every surviving label over ONE shared body.
				var labels []string

				for _, caseExpr := range caseExprs {
					// Duplicate-mapped concrete case: skip only on an identical Go body (see the
					// merge note above). Dynamic (`{} … when`) and `null` labels are never duplicates.
					if !strings.HasPrefix(caseExpr, "{}") && caseExpr != "null" {
						resolved := resolveCaseType(caseExpr)

						if priorBody, seen := emittedCaseTypes[resolved]; seen && priorBody == bodyKey {
							if outputs > 0 {
								v.targetFile.WriteString(v.newline)
							}

							outputs++

							v.writeOutput("/* case %s: merged with an earlier case mapping to the same C# type (identical body) */", strings.TrimRight(caseExpr, " "))
							continue
						}

						if _, seen := emittedCaseTypes[resolved]; !seen {
							emittedCaseTypes[resolved] = bodyKey
						}
					}

					labels = append(labels, strings.TrimRight(caseExpr, " "))
				}

				if len(labels) > 0 {
					for li, label := range labels {
						if outputs > 0 {
							v.targetFile.WriteString(v.newline)
						}

						outputs++

						if li == len(labels)-1 {
							v.writeOutput("case %s: {", label)
						} else {
							v.writeOutput("case %s:", label)
						}
					}

					v.indentLevel++

					// Re-bind the case variable at the guard's static (interface) type — the
					// same re-bind the default arm uses. Bound at body entry, before any body
					// statement can mutate the guard, so the value matches the dispatched one.
					if len(targetIdent) > 0 && targetIdent != "_" {
						v.targetFile.WriteString(v.newline)

						boundExpr := typeVar

						if guardExpr != "" {
							boundExpr = guardExpr
						}

						if v.options.preferVarDecl || guardExpr != "" {
							v.writeOutput("var")
						} else {
							v.writeOutput("object")
						}

						v.targetFile.WriteString(fmt.Sprintf(" %s = %s;", targetIdent, boundExpr))
					}

					// Reset (see the single-type arm below): an EMPTY Go case body must not
					// inherit a STALE lastStatementWasReturn from the prior case (CS0163).
					v.lastStatementWasReturn = false

					for _, stmt := range caseClause.Body {
						v.visitStmt(stmt, []StmtContext{})
					}

					v.targetFile.WriteString(v.newline)

					if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
						v.writeOutputLn("break;")
					}

					v.indentLevel--
					v.writeOutput("}")
				}
			} else {
				for _, caseExpr := range caseExprs {
					// Duplicate-mapped concrete case: skip only on an identical Go body (see the
					// merge note above). Dynamic (`{} … when`) and `null` labels are never duplicates.
					if !strings.HasPrefix(caseExpr, "{}") && caseExpr != "null" {
						resolved := resolveCaseType(caseExpr)

						if priorBody, seen := emittedCaseTypes[resolved]; seen && priorBody == bodyKey {
							if outputs > 0 {
								v.targetFile.WriteString(v.newline)
							}

							outputs++

							v.writeOutput("/* case %s: merged with an earlier case mapping to the same C# type (identical body) */", strings.TrimRight(caseExpr, " "))
							continue
						}

						if _, seen := emittedCaseTypes[resolved]; !seen {
							emittedCaseTypes[resolved] = bodyKey
						}
					}

					if outputs > 0 {
						v.targetFile.WriteString(v.newline)
					}

					outputs++

					v.writeOutput("case ")
					// Trim the trailing space left when a concrete-type case carries no binding variable
					// (`fmt.Sprintf("%s %s", typeName, "")` → `"uint32 "`), so the label reads `case uint32:`
					// not `case uint32 :`. Trimmed only at emission — the stored caseExpr keeps the space the
					// nint/nuint synthetic-case prefix detection above relies on.
					v.targetFile.WriteString(strings.TrimRight(caseExpr, " "))
					v.targetFile.WriteString(": {")
					v.indentLevel++

					// Reset per case: an EMPTY Go case body (`case *ActionNode:` with no statements,
					// text/template/parse's IsEmptyTree) runs no visitStmt, so lastStatementWasReturn
					// would stay STALE from the prior case's `return` and wrongly suppress the `break;`
					// below — the empty C# case then falls through (CS0163).
					v.lastStatementWasReturn = false

					for _, stmt := range caseClause.Body {
						v.visitStmt(stmt, []StmtContext{})
					}

					v.targetFile.WriteString(v.newline)

					if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
						v.writeOutputLn("break;")
					}

					v.indentLevel--
					v.writeOutput("}")
				}
			}
		}
	}

	v.targetFile.WriteRune('}')

	if len(targetIdent) == 0 {
		v.targetFile.WriteString(v.newline)
	}

	// Close any locally scoped declared variable sub-block
	if typeSwitchStmt.Init != nil {
		v.indentLevel--
		v.targetFile.WriteString(v.newline)
		v.writeOutput("}")
	}
}
