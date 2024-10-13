package main

import (
	"go/ast"
)

func (v *Visitor) visitIfStmt(ifStmt *ast.IfStmt, source ParentBlockContext) {
	v.targetFile.WriteString(v.newline)

	if ifStmt.Init != nil {
		// Any declared variable will be scoped to if statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(ifStmt.Init, []StmtContext{source})
	}

	v.writeOutput("if (")
	v.targetFile.WriteString(v.convExpr(ifStmt.Cond, nil))
	v.targetFile.WriteString(") ")

	v.visitBlockStmt(ifStmt.Body, DefaultBlockStmtContext())

	if ifStmt.Else != nil {
		v.writeOutput(" else ")
		switch elseStmt := ifStmt.Else.(type) {
		case *ast.IfStmt:
			v.visitIfStmt(elseStmt, source)
		case *ast.BlockStmt:
			v.visitBlockStmt(elseStmt, DefaultBlockStmtContext())
		default:
			panic("Unexpected Else type: " + v.getPrintedNode(elseStmt))
		}
	}
}
