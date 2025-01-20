package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitBlockStmt(blockStmt *ast.BlockStmt, context BlockStmtContext) {
	v.pushBlock()

	if len(context.outerPrefix) > 0 {
		v.targetFile.WriteString(context.outerPrefix)
	}

	if context.format.useNewLine {
		v.targetFile.WriteString(v.newline)
		v.targetFile.WriteString(v.indent(v.indentLevel))
		v.targetFile.WriteRune('{')
	} else {
		v.targetFile.WriteString(" {")
	}

	if len(context.innerPrefix) > 0 {
		v.targetFile.WriteString(context.innerPrefix)
	}

	v.firstStatementIsReturn = false

	if context.format.useIndent {
		v.indentLevel++
	}

	var lastStmt ast.Stmt
	prefix := v.newline + v.indent(v.indentLevel)

	if len(blockStmt.List) > 0 && v.options.includeComments {
		v.writeStandAloneCommentString(v.targetFile, blockStmt.List[0].Pos(), nil, prefix)
	}

	for _, stmt := range blockStmt.List {
		if v.options.includeComments {
			if lastStmt != nil {
				if v.file == nil {
					v.file = v.fset.File(stmt.Pos())
				}

				currentLine := v.file.Line(stmt.Pos())
				lastLine := v.file.Line(lastStmt.End())

				comments := &strings.Builder{}
				var wrote bool

				if wrote, lines := v.writeStandAloneCommentString(comments, stmt.Pos(), nil, prefix); wrote {
					lastLine -= lines - 1
				}

				if wrote && currentLine-lastLine > 1 {
					v.targetFile.WriteString(strings.Repeat(v.newline, currentLine-lastLine-1))
				}

				if comments.Len() > 0 {
					v.targetFile.WriteString(comments.String())
				}
			}
		}

		v.visitStmt(stmt, []StmtContext{})

		lastStmt = stmt
	}

	if context.format.useIndent {
		v.indentLevel--
	}

	statementList := blockStmt.List

	// Check if the first statement is a return statement
	if len(statementList) > 0 {
		_, ok := statementList[0].(*ast.ReturnStmt)
		v.firstStatementIsReturn = ok
	}

	if len(context.innerSuffix) > 0 {
		v.targetFile.WriteString(context.innerSuffix)
	}

	v.targetFile.WriteString(v.newline)
	v.writeOutput("}")

	if len(context.outerSuffix) > 0 {
		v.targetFile.WriteString(context.outerSuffix)
	}

	// if (!m_firstTopLevelDeclaration && IndentLevel > 2)
	// 	m_targetFile.Append(CheckForCommentsRight(context));

	v.popBlock()
}

func (v *Visitor) pushBlock() {
	v.blocks.Push(v.targetFile)
	v.targetFile = &strings.Builder{}
}

func (v *Visitor) popBlock() string {
	return v.popBlockAppend(true)
}

func (v *Visitor) popBlockAppend(appendToPrevious bool) string {
	lastTarget := v.blocks.Pop()
	block := v.targetFile.String()

	if appendToPrevious {
		lastTarget.WriteString(block)
	}

	v.targetFile = lastTarget

	return block
}
