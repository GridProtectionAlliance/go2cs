package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitTypeSwitchStmt(typeSwitchStmt *ast.TypeSwitchStmt, source ParentBlockContext) {
	var caseClauses []*ast.CaseClause

	for _, stmt := range typeSwitchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
		} else {
			println(fmt.Sprintf("WARNING: unexpected Stmt type (non CaseClause) encountered in TypeSwitchStmt: %T", stmt))
		}
	}

	if typeSwitchStmt.Init != nil {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(typeSwitchStmt.Init, []StmtContext{source})
	}

	v.targetFile.WriteString(v.newline)

	v.writeOutput("switch (")

	var targetIdent, typeVar string
	assign := typeSwitchStmt.Assign

	// Check if the assignment statement is a ExprStmt
	if exprStmt, ok := assign.(*ast.ExprStmt); ok {
		v.targetFile.WriteString(v.convExpr(exprStmt.X, nil))
	} else {
		assignStmt := assign.(*ast.AssignStmt)
		targetIdent = v.convExpr(assignStmt.Lhs[0], nil)
		typeVar = v.convExpr(assignStmt.Rhs[0], nil)
		v.targetFile.WriteString(typeVar)
	}

	v.targetFile.WriteString(") {")
	v.targetFile.WriteString(v.newline)

	identContext := DefaultIdentContext()
	identContext.isType = true

	for _, caseClause := range caseClauses {
		if caseClause.List == nil {
			v.writeOutput("default:")

			if len(targetIdent) > 0 {
				v.targetFile.WriteString(" {")
				v.targetFile.WriteString(v.newline)
				v.indentLevel++

				if v.options.preferVarDecl {
					v.writeOutput("var")
				} else {
					v.writeOutput("object")
				}

				v.targetFile.WriteString(fmt.Sprintf(" %s = %s;", targetIdent, typeVar))
			} else {
				v.indentLevel++
			}

			for _, stmt := range caseClause.Body {
				v.visitStmt(stmt, []StmtContext{source})
			}

			v.targetFile.WriteString(v.newline)

			if !v.lastStatementWasReturn {
				v.writeOutputLn("break;")
			}

			v.indentLevel--

			if len(targetIdent) > 0 {
				v.writeOutput("}")
			}
		} else {
			var caseExprs []string

			for _, expr := range caseClause.List {
				caseExpr := v.convExpr(expr, []ExprContext{identContext})

				caseExprs = append(caseExprs, v.convExpr(expr, []ExprContext{identContext}))

				if caseExpr == "nint" {
					caseExprs = append(caseExprs, "int32")
				} else if caseExpr == "nuint" {
					caseExprs = append(caseExprs, "uint32")
				}
			}

			for _, caseExpr := range caseExprs {
				v.writeOutput("case ")
				v.targetFile.WriteString(caseExpr)
				v.targetFile.WriteRune(' ')
				v.targetFile.WriteString(targetIdent)
				v.targetFile.WriteRune(':')
				v.indentLevel++

				for _, stmt := range caseClause.Body {
					v.visitStmt(stmt, []StmtContext{source})
				}

				v.targetFile.WriteString(v.newline)

				if !v.lastStatementWasReturn {
					v.writeOutputLn("break;")
				}

				v.indentLevel--
			}
		}
	}

	if len(targetIdent) > 0 {
		v.targetFile.WriteRune('}')
	} else {
		v.writeOutput("}")
	}

	// Close any locally scoped declared variable sub-block
	if typeSwitchStmt.Init != nil {
		v.indentLevel--
		v.targetFile.WriteString(v.newline)
		v.writeOutput("}")
	}
}
