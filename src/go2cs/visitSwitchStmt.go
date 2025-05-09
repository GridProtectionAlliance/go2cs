package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"slices"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt) {
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

		for i, caseClause := range caseClauses {
			if i > 0 {
				v.targetFile.WriteString(v.newline)
			}

			if caseClause.List == nil {
				v.writeOutput("default: {")
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

			// Check for binary expressions with bitwise operations
			if binaryExpr, ok := expr.(*ast.BinaryExpr); ok {
				// Check if expression involves bitwise operation followed by comparison
				if isBitwiseFollowedByComparison(binaryExpr) {
					usePattenMatch = false
					break
				}
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
