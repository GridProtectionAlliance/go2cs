package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"slices"
)

// caseBodyHasSwitchBreak reports whether a switch case body contains a bare `break` that targets the
// SWITCH itself — i.e. a `break` not enclosed by a nested loop/switch/select (which would catch it)
// or a function literal. A Go `break` inside a switch case exits the switch; when the switch is
// lowered to an `if / else if` chain (no enclosing C# switch), that `break` has no target (CS0139),
// so such a case body is wrapped in a `do { … } while (false)` whose `break` exits the case.
func caseBodyHasSwitchBreak(body []ast.Stmt) bool {
	found := false

	for _, stmt := range body {
		ast.Inspect(stmt, func(n ast.Node) bool {
			if found {
				return false
			}

			switch node := n.(type) {
			case *ast.ForStmt, *ast.RangeStmt, *ast.SwitchStmt, *ast.TypeSwitchStmt, *ast.SelectStmt, *ast.FuncLit:
				return false // a nested loop/switch/select/closure catches its own breaks
			case *ast.BranchStmt:
				if node.Tok == token.BREAK && node.Label == nil {
					found = true
					return false
				}
			}

			return true
		})
	}

	return found
}

// isTerminatingStmt reports whether a statement is a Go "terminating statement" (spec §Terminating
// statements) — one after which control cannot fall through. Used to decide whether a switch case can
// fall OUT of the switch: a non-terminating (and non-`fallthrough`) case means the switch is NOT terminal,
// so no unreachable trailing `return default!;` may follow it (see visitSwitchStmt). CONSERVATIVE by
// design — it returns false for any form it does not fully analyze (`for`/`switch`/`select`), which only
// forgoes the trailing return (leaving the rarer CS0161), NEVER a false "terminal" that would make the
// trailing return reachable and silently return the zero value. `fallthrough` is NOT terminating here
// (tracked separately as a case that continues into the next clause); a lone `break`/`continue` is not.
func isTerminatingStmt(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.ReturnStmt:
		return true
	case *ast.BranchStmt:
		return s.Tok == token.GOTO
	case *ast.LabeledStmt:
		return isTerminatingStmt(s.Stmt)
	case *ast.BlockStmt:
		return isTerminatingStmtList(s.List)
	case *ast.IfStmt:
		// Terminating only with an `else` where BOTH branches terminate.
		return s.Else != nil && isTerminatingStmt(s.Body) && isTerminatingStmt(s.Else)
	case *ast.ExprStmt:
		// A call to the built-in `panic` is terminating.
		if call, ok := s.X.(*ast.CallExpr); ok {
			if ident, ok := call.Fun.(*ast.Ident); ok && ident.Name == "panic" {
				return true
			}
		}
	}

	return false
}

// isTerminatingStmtList reports whether a statement list terminates — its final non-empty statement is a
// terminating statement (Go spec: an empty trailing statement does not affect termination).
func isTerminatingStmtList(list []ast.Stmt) bool {
	for i := len(list) - 1; i >= 0; i-- {
		if _, empty := list[i].(*ast.EmptyStmt); empty {
			continue
		}

		return isTerminatingStmt(list[i])
	}

	return false
}

// visitSwitchStmt wraps the core emission with the labeled-statement break target: a
// LABELED switch's `break Label` statements emit `goto break_Label` (a bare C# break
// exits only the innermost construct), so the label must be DECLARED after the switch
// (regexp/syntax parse.go's BigSwitch, CS0159 x11). Switches take no continue label.
func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt, target LabeledStmtContext) {
	v.visitSwitchStmtCore(switchStmt)

	if len(target.label) > 0 {
		v.targetFile.WriteString(v.newline)
		v.writeOutput("%s:;", getBreakLabelName(target.label))
	}
}

func (v *Visitor) visitSwitchStmtCore(switchStmt *ast.SwitchStmt) {
	// A tagged switch whose tag is the CONSTANT `true` (`switch c := s[i]; true { case cond: … }`
	// — strconv readFloat) is Go's idiom for an expressionless switch with an init statement:
	// each boolean case CONDITION matches when it holds. Normalize to the expressionless form
	// (nil tag) — the tagged routes would otherwise emit the conditions as C# case labels /
	// constant patterns, which must be compile-time constants (CS9135).
	tag := switchStmt.Tag

	if tag != nil {
		if tv, ok := v.info.Types[tag]; ok && tv.Value != nil && tv.Value.Kind() == constant.Bool && constant.BoolVal(tv.Value) {
			tag = nil
		}
	}

	var caseClauses []*ast.CaseClause
	var caseHasFallthroughStmt []bool

	for _, stmt := range switchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
			caseHasFallthroughStmt = append(caseHasFallthroughStmt, false)
		} else {
			v.showWarning("@visitSwitchStmt - unexpected Stmt type (non CaseClause) encountered in SwitchStmt: %T", stmt)
		}
	}

	allConst := true
	namedTypes := false
	hasFallthroughs := false
	defaultCaseFallsThrough := false

	// A TAG that is itself a CONSTANT emitted as `static readonly` — an untyped const's
	// UntypedInt wrapper (`switch goarch.PtrSize`, reflect abi.go) or a uintptr-struct const —
	// cannot govern a C# switch: the int case labels are not constants OF the wrapper struct
	// type (CS9135 ×2). Force the if-else (==) form, whose wrapper == operators compare
	// cleanly. The recorded tag TYPE is no help here (go/types records the untyped constant's
	// DEFAULT type in tag position), so gate on the object resolution: a constant-valued tag
	// that is not a true C# const. A variable tag (Value == nil) stays switchable. The same
	// tag also disables the `is` constant-pattern form below (`exprᴛ1 is 4` needs 4 to be a
	// constant OF the wrapper type — same CS9135); only the `==` operator compares cleanly.
	tagIsStaticReadonlyConst := false

	if tag != nil {
		if tv, ok := v.info.Types[tag]; ok && tv.Value != nil && !v.isCSharpConstantExpr(tag) {
			allConst = false
			tagIsStaticReadonlyConst = true
		}
	}

	for i, caseClause := range caseClauses {
		if caseClause.List == nil {
			if i > 0 && caseHasFallthroughStmt[i-1] {
				defaultCaseFallsThrough = true
			}
			continue
		}

		// Check if all case clauses are constant values
		for _, expr := range caseClause.List {
			// Check if the expression is a function call, a non-value type or is an untyped expression
			if !v.isNonCallValue(expr) || v.containsUntypedExpr(expr) {
				allConst = false
				break
			}

			// A C# `switch` case label must be a compile-time constant. An identifier/selector label
			// that resolves to anything other than a C# `const` — a plain variable, a struct field
			// (`frame.fp`), a const emitted as `static readonly` (untyped/named/cross-package, e.g.
			// `goarch.PtrSize`), or an address-of expression — is not, so a C# switch is invalid
			// (CS9135). containsUntypedExpr/the *types.Named check above miss these because the label
			// is typed to the switch tag's type in context. Force the if-else (==) form for them.
			switch expr.(type) {
			case *ast.Ident, *ast.SelectorExpr:
				if !v.isCSharpConstantExpr(expr) {
					allConst = false
				}
			case *ast.UnaryExpr:
				// An address-of (`&frame.fp` → `Ꮡframe.Value.fp`) is a runtime value, never a constant.
				if expr.(*ast.UnaryExpr).Op == token.AND {
					allConst = false
				}
			}

			if !allConst {
				break
			}

			tv, ok := v.info.Types[expr]

			if !ok {
				break
			}

			// A case label that is not even a GO compile-time constant (`case 1<<flt.expbits - 1:`
			// — flt is a variable; Go tagged-switch labels may be runtime expressions) can never
			// be a C# case label (CS9135). Ident/Selector/&-labels are screened above; this
			// catches the compound forms (binary/index/paren…). Force the if-else (==) form.
			if tv.Value == nil {
				allConst = false
				break
			}

			// Named typed are not constant values in C# conversion
			if _, ok := tv.Type.(*types.Named); ok {
				allConst = false
				namedTypes = true
				break
			}

			// A uintptr-typed label (even a plain literal — it adopts the switch tag's type in
			// context, so `case 4:` under `switch t.Size_` types as uintptr) can never be a C#
			// constant: uintptr is a golib STRUCT (golib/uintptr.cs), so a constant-label switch
			// over it is invalid (CS9135). Force the if-else (==) form.
			if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
				allConst = false
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

	// When the switch lowers to an if/else-if chain (non-constant cases on a tagged switch), the
	// `default` clause becomes the trailing `else`. Go allows `default` in any position, but in the
	// chain a leading/middle default emits a bare `{ /* default: */ … }` followed by `else if`
	// (CS8641 "else cannot start a statement"). Move the single default clause to the end. This is
	// safe even when the switch has fallthroughs, AS LONG AS the default itself does not participate
	// in fallthrough — the preceding clause must not fall through into it and it must not fall
	// through out (those are source-order-sensitive); removing a fallthrough-independent default
	// preserves every other clause's fallthrough link. The C# `switch` branches accept `default`
	// anywhere, so this is only for the if/else-chain path.
	if !allConst && tag != nil {
		for i, caseClause := range caseClauses {
			if caseClause.List != nil || i == len(caseClauses)-1 {
				continue
			}

			fallsIntoDefault := i > 0 && caseHasFallthroughStmt[i-1]

			if fallsIntoDefault || caseHasFallthroughStmt[i] {
				break // default participates in fallthrough — keep source order
			}

			reorderedClauses := make([]*ast.CaseClause, 0, len(caseClauses))
			reorderedFlags := make([]bool, 0, len(caseClauses))

			for j := range caseClauses {
				if j == i {
					continue
				}

				reorderedClauses = append(reorderedClauses, caseClauses[j])
				reorderedFlags = append(reorderedFlags, caseHasFallthroughStmt[j])
			}

			reorderedClauses = append(reorderedClauses, caseClause)
			reorderedFlags = append(reorderedFlags, caseHasFallthroughStmt[i])
			caseClauses = reorderedClauses
			caseHasFallthroughStmt = reorderedFlags
			break
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

	if hasFallthroughs || (!allConst && tag != nil) {
		// Most complex scenario with standalone if's, and fallthrough
		exprVarName := v.getTempVarName("expr")

		var matchVarName string

		if hasFallthroughs {
			matchVarName = v.getTempVarName("match")
		}

		if tag != nil {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				exprType := convertToCSTypeName(v.getExprTypeName(tag, false))
				v.targetFile.WriteString(exprType)
				v.targetFile.WriteRune(' ')
			}

			v.targetFile.WriteString(exprVarName)
			v.targetFile.WriteString(" = ")
			v.targetFile.WriteString(v.convExpr(tag, nil))
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

		// A `default:` reached via fallthrough is emitted as a GUARDED `if (fallthrough || !match)` block
		// (below), whose condition C# cannot prove always executes — so if the switch is the last
		// statement of a value-returning function and every case is terminal, C# reports CS0161 ("not all
		// code paths return a value") even though the Go `default` makes it exhaustive. Track that form to
		// emit an (unreachable) trailing `return default!;` after the if-chain — but ONLY when the switch
		// is genuinely terminal (allCasesTerminal below). A switch with a case that can fall OUT (a
		// `break`, or a body that neither returns nor falls through) is NOT terminal: reachable Go code
		// follows it, so a trailing `return default!;` there would execute and wrongly return the zero value.
		guardedTerminalDefault := false
		allCasesTerminal := true

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

					// A trailing default emitted in the guarded form leaves C# unable to prove the switch
					// is exhaustive (see guardedTerminalDefault above).
					if i == len(caseClauses)-1 {
						guardedTerminalDefault = true
					}
				} else {
					v.targetFile.WriteString("{ /* default: */")
				}
			} else {
				caseClauseCount := len(caseClause.List)

				v.targetFile.WriteString("if (")

				usePattenMatch := !namedTypes && !tagIsStaticReadonlyConst && v.canUsePatternMatch(caseClauseCount, caseClause, tag != nil)

				if caseFallsThrough {
					v.targetFile.WriteString(fmt.Sprintf("fallthrough || !%s && ", matchVarName))

					if caseClauseCount > 1 || !usePattenMatch && tag == nil {
						v.targetFile.WriteRune('(')
					}
				}

				// An INTERFACE-typed tag compared against concrete case labels (`switch e {
				// case ERROR_FILE_NOT_FOUND:` where e is error and the labels are Errno) has no
				// C# operator — route the equality through AreEqual, mirroring convBinaryExpr's
				// interface-vs-concrete compare arm (syscall syscall_windows CS0019).
				tagNeedsAreEqual := false

				if !usePattenMatch && tag != nil {
					// EMPTY interfaces included: `switch err := recover(); err { case ErrLarge: }`
					// compares object against a named-string value — no C# operator either
					// (regexp/syntax parse.go, CS0019 ×2).
					if tagIface, _ := isInterface(v.getType(tag, false)); tagIface {
						tagNeedsAreEqual = true
					}
				}

				for i, expr := range caseClause.List {
					needAreEqualClose := false

					if i == 0 {
						if tag != nil {
							if usePattenMatch {
								v.targetFile.WriteString(exprVarName)
								v.targetFile.WriteString(" is ")
							} else if tagNeedsAreEqual {
								v.targetFile.WriteString(fmt.Sprintf("AreEqual(%s, ", exprVarName))
								needAreEqualClose = true
							} else {
								v.targetFile.WriteString(exprVarName)
								v.targetFile.WriteString(" == ")
							}
						}
					} else {
						if usePattenMatch {
							v.targetFile.WriteString(" or ")
						} else {
							v.targetFile.WriteString(" || ")

							if tag != nil {
								if tagNeedsAreEqual {
									v.targetFile.WriteString(fmt.Sprintf("AreEqual(%s, ", exprVarName))
									needAreEqualClose = true
								} else {
									v.targetFile.WriteString(exprVarName)
									v.targetFile.WriteString(" == ")
								}
							}
						}
					}

					context := DefaultPatternMatchExprContext()

					if usePattenMatch {
						context.usePattenMatch = true
						context.declareIsExpr = i == 0
					} else if caseClauseCount > 1 && tag == nil {
						v.targetFile.WriteRune('(')
					}

					v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))

					if needAreEqualClose {
						v.targetFile.WriteRune(')')
					}

					if !usePattenMatch && caseClauseCount > 1 && tag == nil {
						v.targetFile.WriteRune(')')
					}

					if i == caseClauseCount-1 {
						if hasFallthroughs {
							// Only close the wrapping paren when it was actually opened above
							// (same guard as the matching '(' write). Otherwise a fallthrough
							// case with a single pattern-matched value emits an unbalanced ')'.
							if caseFallsThrough && (caseClauseCount > 1 || !usePattenMatch && tag == nil) {
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

			// A Go `break` targeting this (now if-else) switch has no C# target — wrap the body in a
			// `do { … } while (false)` so the `break` exits the case. Only when such a break exists,
			// to avoid disturbing every other case.
			switchBreakWrap := caseBodyHasSwitchBreak(caseClause.Body)

			if switchBreakWrap {
				// Emit `do {` on its own properly-indented line. writeOutput prepends the indent then
				// the text, so the leading newline must be written raw first — otherwise `do {` glues
				// onto the preceding `) {` with the indent spaces between them (`) {        do {`).
				v.targetFile.WriteString(v.newline)
				v.writeOutput("do {")
				v.indentLevel++
			}

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{})
			}

			// A case whose body can fall OUT of the switch — it wraps a switch-`break`, or it is neither a
			// Go terminating statement list (isTerminatingStmtList — a genuine spec check, so an
			// `if { return }` WITHOUT an `else` or a possibly-zero-trip loop is correctly NON-terminating)
			// nor a fallthrough — makes the switch non-terminal, so no trailing `return default!;` may be
			// added (see guardedTerminalDefault). Using the shallow "last emitted line was a return" flag
			// here is WRONG: it false-positives on `if { return }` and lets a reachable `return default!;`
			// silently return the zero value.
			if switchBreakWrap || !(isTerminatingStmtList(caseClause.Body) || caseHasFallthroughStmt[i]) {
				allCasesTerminal = false
			}

			if switchBreakWrap {
				v.indentLevel--
				// Likewise close on its own indented line. A `"%s} while…"` format would emit the
				// indent BEFORE the newline (trailing whitespace on the prior line) and drop the
				// `} while (false);` to column 0; write the newline raw, then the indented text.
				v.targetFile.WriteString(v.newline)
				v.writeOutput("} while (false);")
			}

			v.targetFile.WriteString(v.newline)

			if caseHasFallthroughStmt[i] {
				v.writeOutputLn("fallthrough = true;")
			}

			v.indentLevel--
			v.writeOutputLn("}")
		}

		// A trailing `default:` in the guarded (fallthrough) form is a runtime-conditional `if` that C#
		// cannot prove exhaustive, so a value-returning function ending in this switch fails CS0161 ("not
		// all code paths return a value") even though the Go `default` makes it exhaustive. Emit an
		// unreachable `return default!;` — a guarded-terminal-default switch cannot be legally followed by
		// reachable Go code (the switch always returns/exits), so nothing else can run after it. Skip in
		// namedReturnDefer mode: there the body sits inside a `void` `func((defer, recover) => …)` wrapper,
		// so a value `return default!;` is CS8030 — and the void wrapper needs no trailing return anyway.
		if guardedTerminalDefault && allCasesTerminal && !v.namedReturnDeferMode && v.currentReturnSignature != nil && v.currentReturnSignature.Results().Len() > 0 {
			v.writeOutputLn("return default!;")
		}
	} else if allConst && tag != nil {
		// Most simple scenario when all case values are constant, a common C# switch will suffice
		v.writeOutput("switch (")
		v.targetFile.WriteString(v.convExpr(tag, nil))
		v.targetFile.WriteString(") {")
		v.targetFile.WriteString(v.newline)

		for i, caseClause := range caseClauses {
			if i > 0 {
				v.targetFile.WriteString(v.newline)
			}

			if caseClause.List == nil {
				v.writeOutput("default: {")
			} else {
				for i, expr := range caseClause.List {
					if i == 0 {
						v.writeOutput("case ")
					} else {
						v.targetFile.WriteString(" or ")
					}

					v.targetFile.WriteString(v.convExpr(expr, nil))

					if i == len(caseClause.List)-1 {
						v.targetFile.WriteString(": {")
					}
				}
			}

			v.indentLevel++

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

		v.targetFile.WriteRune('}')
		v.targetFile.WriteString(v.newline)
	} else {
		// Most common scenario with expression switches
		v.writeOutput("switch (%s) {%s", TrueMarker, v.newline)

		for clauseIndex, caseClause := range caseClauses {
			if clauseIndex > 0 {
				v.targetFile.WriteString(v.newline)
			}

			if caseClause.List == nil {
				v.writeOutput("default: {")
			} else {
				// Use pattern match when all case list expressions are
				// use comparison operators and the same target
				caseClauseCount := len(caseClause.List)
				usePattenMatch := !namedTypes && !tagIsStaticReadonlyConst && v.canUsePatternMatch(caseClauseCount, caseClause, tag != nil)

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

					// A constant-TRUE case condition ahead of other clauses (`switch {
					// case true: … case cond: …}` — time parseStrictRFC3339 deliberately
					// disabling its strict checks) makes every LATER case provably
					// unreachable, which Go compiles as dead code but C# rejects (CS8120
					// ×3). Emit the golib `ᐧᐧ` marker (a static readonly bool the compiler
					// cannot fold) instead of the literal; a trailing constant-true clause
					// keeps the literal (nothing follows to be unreachable — no churn).
					caseIsFoldableTrue := false

					if clauseIndex < len(caseClauses)-1 {
						if tv, ok := v.info.Types[expr]; ok && tv.Value != nil && tv.Value.Kind() == constant.Bool && constant.BoolVal(tv.Value) {
							caseIsFoldableTrue = true
						}
					}

					if caseIsFoldableTrue {
						v.targetFile.WriteString(OpaqueTrueMarker)
					} else {
						v.targetFile.WriteString(v.convExpr(expr, []ExprContext{context}))
					}

					if !usePattenMatch && caseClauseCount > 1 {
						v.targetFile.WriteRune(')')
					}

					if i == caseClauseCount-1 {
						v.targetFile.WriteString(": {")
					}
				}
			}

			v.indentLevel++

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

		v.targetFile.WriteRune('}')
		v.targetFile.WriteString(v.newline)
	}

	// Close any locally scoped declared variable sub-block
	if switchStmt.Init != nil {
		v.indentLevel--
		v.writeOutputLn("}")
	}
}

func (v *Visitor) containsUntypedExpr(expr ast.Expr) bool {
	// First check the type information directly
	if t := v.info.TypeOf(expr); t != nil {
		if basic, ok := t.Underlying().(*types.Basic); ok {
			isUntyped := basic.Info()&types.IsUntyped != 0
			if isUntyped {
				return true
			}
		}
	}

	// Handle identifiers specifically (including references to untyped constants)
	if ident, ok := expr.(*ast.Ident); ok {
		// Check if this identifier refers to a constant
		if obj := v.info.ObjectOf(ident); obj != nil {
			// Check if it's a constant
			if cnst, ok := obj.(*types.Const); ok {
				// Check if the constant has an untyped type
				if basic, ok := cnst.Type().(*types.Basic); ok {
					isUntyped := basic.Info()&types.IsUntyped != 0
					return isUntyped
				}
			}
		}

		// Also check type-and-value information
		if tv, ok := v.info.Types[ident]; ok {
			if tv.IsValue() && tv.Value != nil && tv.Type != nil {
				if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
					isUntyped := basic.Info()&types.IsUntyped != 0
					return isUntyped
				}
			}
		}
	}

	// Recursive checking for compound expressions
	switch e := expr.(type) {
	case *ast.BinaryExpr:
		return v.containsUntypedExpr(e.X) || v.containsUntypedExpr(e.Y)
	case *ast.UnaryExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.StarExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.ParenExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.SelectorExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.IndexExpr:
		return v.containsUntypedExpr(e.X) || v.containsUntypedExpr(e.Index)
	case *ast.SliceExpr:
		untyped := v.containsUntypedExpr(e.X)

		if e.Low != nil {
			untyped = untyped || v.containsUntypedExpr(e.Low)
		}

		if e.High != nil {
			untyped = untyped || v.containsUntypedExpr(e.High)
		}

		if e.Max != nil {
			untyped = untyped || v.containsUntypedExpr(e.Max)
		}

		return untyped
	case *ast.CallExpr:
		return slices.ContainsFunc(e.Args, v.containsUntypedExpr)
	case *ast.CompositeLit:
		if e.Type != nil && v.containsUntypedExpr(e.Type) {
			return true
		}

		return slices.ContainsFunc(e.Elts, v.containsUntypedExpr)
	case *ast.KeyValueExpr:
		return v.containsUntypedExpr(e.Key) || v.containsUntypedExpr(e.Value)
	}

	return false
}

func (v *Visitor) canUsePatternMatch(caseClauseCount int, caseClause *ast.CaseClause, inferExprTarget bool) bool {
	usePattenMatch := true

	// Verify that all expressions are not untyped (do not pattern match with implicitly type conversions)
	if slices.ContainsFunc(caseClause.List, v.containsUntypedExpr) {
		return false
	}

	if inferExprTarget {
		// Verify that all expressions are non-call values
		for _, expr := range caseClause.List {
			if !v.isNonCallValue(expr) {
				usePattenMatch = false
				break
			}

			// A C# constant pattern (`tag is Y`) requires Y to be a compile-time constant. An
			// identifier/selector case label that resolves to a const emitted as `static readonly`
			// (untyped/named/cross-package, e.g. `goarch.PtrSize`) or to a plain variable is not, so
			// the `is` form is invalid (CS9135/CS0150). Fall back to `==` equality for those. (Computed
			// literal expressions like `a + b` remain valid constant patterns and are left untouched.)
			switch expr.(type) {
			case *ast.Ident, *ast.SelectorExpr:
				if !v.isCSharpConstantExpr(expr) {
					usePattenMatch = false
				}
			}

			// A uintptr-typed label — even a plain literal, which adopts the tag's type in context
			// (`exprᴛ1 is 4` under a uintptr tag) — can never be a constant pattern: uintptr is a
			// golib STRUCT (CS9135). Fall back to `==` (the struct's operator).
			if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
				if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
					usePattenMatch = false
				}
			}

			if !usePattenMatch {
				break
			}

			// A compound (binary) case label under a TAGGED switch cannot use the `is` form:
			// convBinaryExpr's pattern arm renders comparison OPERANDS (`X is <op> Y`), yielding
			// `tag is X is <op> Y` — never valid — and a runtime label is no constant pattern
			// anyway (CS0150). Fall back to `==` equality for the clause.
			if _, ok := expr.(*ast.BinaryExpr); ok {
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

			// Check for bitwise operations in the left operand
			if containsBitwiseOperation(binaryExpr.X) {
				usePattenMatch = false
				break
			}

			// Make sure no rhs expression targets are call expressions
			if !v.isNonCallValue(binaryExpr.Y) {
				usePattenMatch = false
				break
			}

			// A relational/constant pattern (`x is <op> Y`) requires Y to be a C# compile-time
			// constant. A variable (`case x == y`, y a parameter) or a const emitted as
			// `static readonly` (untyped/named/cross-package) makes the pattern invalid (CS9135);
			// fall back to a `when` guard for those.
			if !v.isCSharpConstantExpr(binaryExpr.Y) {
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

// Helper function to check if an expression contains bitwise operations
func containsBitwiseOperation(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BinaryExpr:
		return isBitwiseOperator(e.Op) ||
			containsBitwiseOperation(e.X) ||
			containsBitwiseOperation(e.Y)
	case *ast.ParenExpr:
		return containsBitwiseOperation(e.X)
	default:
		return false
	}
}

// Helper function to check if an operator is a bitwise operator
func isBitwiseOperator(op token.Token) bool {
	return op == token.AND || op == token.OR || op == token.XOR ||
		op == token.SHL || op == token.SHR || op == token.AND_NOT
}

// Helper function to check if expression is bitwise operation followed by comparison
func isBitwiseFollowedByComparison(expr *ast.BinaryExpr) bool {
	// Check if this is a comparison expression
	if isComparisonOperator(expr.Op) {
		// Check if left side contains bitwise operations
		return containsBitwiseOperation(expr.X)
	}
	return false
}
