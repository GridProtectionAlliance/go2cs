package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
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

	var targetIdent, typeVar, guardExpr, operandExpr string
	assign := typeSwitchStmt.Assign

	// Check if the assignment statement is a ExprStmt
	if exprStmt, ok := assign.(*ast.ExprStmt); ok {
		operandExpr = v.convExpr(exprStmt.X, nil)
	} else {
		assignStmt := assign.(*ast.AssignStmt)
		targetIdent = v.convExpr(assignStmt.Lhs[0], nil)
		typeVar = v.convExpr(assignStmt.Rhs[0], nil)

		// The DEFAULT arm re-binds the guard to the ORIGINAL interface value: the switch
		// operand form (`x.type()`) is object, which cannot flow back out as the interface
		// (go/build/constraint pushNot's `default: return x`, CS0266).
		if typeAssert, ok := assignStmt.Rhs[0].(*ast.TypeAssertExpr); ok {
			guardExpr = v.convExpr(typeAssert.X, nil)

			// Go evaluates the type-switch tag exactly ONCE, but the multi-type and default
			// arms re-emit the tag expression to re-bind the case variable at the guard's
			// interface type — a tag containing a call or channel receive would then evaluate
			// once per matched arm entry (`switch p := recover().(type)` re-probing recover).
			// Hoist such a tag into a one-time temporary and dispatch/re-bind from it; pure
			// tags keep the direct form (byte-identical output, no temp).
			if tagHasSideEffects(typeAssert.X) && typeSwitchHasRebindArm(caseClauses, targetIdent) {
				hoistVar := getGlobalTempVarName("switch")

				v.targetFile.WriteString(v.newline)
				v.writeOutput("var %s = %s;", hoistVar, guardExpr)

				typeVar = fmt.Sprintf("%s.type()", hoistVar)
				guardExpr = hoistVar
			}
		}

		operandExpr = typeVar
	}

	v.targetFile.WriteString(v.newline)

	v.writeOutput("switch (")
	v.targetFile.WriteString(operandExpr)
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
			// clause lists exactly ONE type. Emit stacked labels binding only a DISCARD over a
			// single shared body and re-bind the variable to the guard expression (like the
			// default arm), so every body use compiles at the interface type (`isGeneric(t)`
			// with `t` a Type, not a ж<Alias> — go/types nonGeneric, CS1503/CS0266/CS1929 ×18).
			// The `_` designation is load-bearing, not stylistic: it forces the label into
			// PATTERN context. A bare `case int8:` label resolves the identifier as an
			// EXPRESSION first, where `using static go.builtin` finds the same-named conversion
			// FUNCTION (`int8(…)`) — a method group, not a constant or type — failing CS8917
			// (encoding/binary's Size/intDataSize stacks). A single-type clause keeps the
			// concrete declaration-pattern binding.
			multiTypeCase := len(caseClause.List) > 1
			bindIdent := targetIdent

			if multiTypeCase {
				bindIdent = "_"
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

				// An INTERFACE-typed case label (named or anonymous) dispatches by Go METHOD-SET
				// semantics, not C# nominal implementation: the operand (`x.type()`) surfaces the
				// Go dynamic value — for a pointer-sourced implementation that is the raw receiver
				// box ж<T>, which no plain C# type pattern `case Iface t:` can match even though
				// *T implements the interface in Go. The `when` guard routes through golib's
				// type-assert machinery (`_<Iface>`), which matches nominal implementers directly
				// and re-wraps a raw box in its registered pointer adapter (see AdapterRegistry).
				if v.isInterfaceCaseLabel(expr) {
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

// isInterfaceCaseLabel reports whether a type-switch case label denotes an interface type —
// anonymous (dynamic) or named — the labels that must dispatch by Go method-set semantics
// through the `when` guard form rather than a plain C# type pattern.
func (v *Visitor) isInterfaceCaseLabel(expr ast.Expr) bool {
	if v.isDynamicInterface(expr) {
		return true
	}

	labelType := v.getType(expr, false)

	if labelType == nil {
		return false
	}

	labelType = types.Unalias(labelType)

	// A type-parameter label matches its concrete type ARGUMENT, not a method set — its
	// underlying constraint interface must not route it to dynamic interface dispatch.
	if _, isTypeParam := labelType.(*types.TypeParam); isTypeParam {
		return false
	}

	if _, isNamed := labelType.(*types.Named); !isNamed {
		return false
	}

	_, isIface := labelType.Underlying().(*types.Interface)

	return isIface
}

// tagHasSideEffects reports whether a type-switch tag expression contains a call or a channel
// receive — the forms whose re-evaluation is observable, requiring the tag be hoisted into a
// one-time temporary before the switch. Conversions (also *ast.CallExpr) hoist conservatively:
// the temp is still correct, just not strictly required.
func tagHasSideEffects(expr ast.Expr) bool {
	hasSideEffects := false

	ast.Inspect(expr, func(node ast.Node) bool {
		switch unaryExpr := node.(type) {
		case *ast.CallExpr:
			hasSideEffects = true
		case *ast.UnaryExpr:
			if unaryExpr.Op == token.ARROW {
				hasSideEffects = true
			}
		}

		return !hasSideEffects
	})

	return hasSideEffects
}

// typeSwitchHasRebindArm reports whether any arm re-emits the guard expression to re-bind the
// case variable: the default arm (any bound ident, including the blank one) or a multi-type
// clause (non-blank ident) — the arms whose re-bind makes an impure tag evaluate twice.
func typeSwitchHasRebindArm(caseClauses []*ast.CaseClause, targetIdent string) bool {
	if len(targetIdent) == 0 {
		return false
	}

	for _, caseClause := range caseClauses {
		if caseClause.List == nil {
			return true
		}

		if len(caseClause.List) > 1 && targetIdent != "_" {
			return true
		}
	}

	return false
}
