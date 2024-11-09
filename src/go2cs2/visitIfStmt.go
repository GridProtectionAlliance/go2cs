package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitIfStmt(ifStmt *ast.IfStmt, source ParentBlockContext) {
	if ifStmt.Init == nil {
		v.targetFile.WriteString(v.newline)
		v.writeOutput("")
	} else {
		// Any declared variable will be scoped to if statement, so create a sub-block for it
		v.targetFile.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(ifStmt.Init, []StmtContext{source})
		v.targetFile.WriteRune(' ')
	}

	context := DefaultBlockStmtContext()
	context.format.useNewLine = false

	v.targetFile.WriteString("if (")
	v.targetFile.WriteString(v.convExpr(ifStmt.Cond, nil))
	v.targetFile.WriteRune(')')

	v.pushBlock()
	v.visitBlockStmt(ifStmt.Body, context)
	body := v.popBlockAppend(false)

	if ifStmt.Else == nil {
		v.targetFile.WriteString(body)
	} else {
		v.targetFile.WriteString(strings.TrimSpace(body))
		v.targetFile.WriteString(" else")

		switch elseStmt := ifStmt.Else.(type) {
		case *ast.IfStmt:
			v.targetFile.WriteRune(' ')
			v.visitIfStmt(elseStmt, source)
		case *ast.BlockStmt:
			v.visitBlockStmt(elseStmt, context)
		default:
			panic("Unexpected Else type: " + v.getPrintedNode(elseStmt))
		}
	}

	if ifStmt.Init != nil {
		v.indentLevel--
		v.targetFile.WriteString(v.newline)
		v.writeOutput("}")
	}
}
