package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitBlockStmt(blockStmt *ast.BlockStmt) {
	v.pushBlock()

	if v.blockOuterPrefixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockOuterPrefixInjection.Pop())
	}

	v.writeOutput(" {")

	if v.blockInnerPrefixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockInnerPrefixInjection.Pop())
	}

	v.firstStatementIsReturn = false
	v.indentLevel++

	var lastStmt ast.Stmt
	prefix := v.newline + v.indent(v.indentLevel)

	if len(blockStmt.List) > 0 {
		v.writeStandAloneCommentString(v.targetFile, blockStmt.List[0].Pos(), nil, prefix)
	}

	for _, stmt := range blockStmt.List {
		if lastStmt != nil {

			currentLine := v.file.Line(stmt.Pos())
			lastLine := v.file.Line(lastStmt.End())

			comments := &strings.Builder{}

			if wrote, lines := v.writeStandAloneCommentString(comments, stmt.Pos(), nil, prefix); wrote {
				lastLine -= lines - 1
			}

			if currentLine-lastLine > 1 {
				v.targetFile.WriteString(strings.Repeat(v.newline, currentLine-lastLine-1))
			}

			if comments.Len() > 0 {
				v.targetFile.WriteString(comments.String())
			}
		}

		v.visitStmt(stmt)
		lastStmt = stmt
	}

	v.indentLevel--
	statementList := blockStmt.List

	// Check if the first statement is a return statement
	if len(statementList) > 0 {
		_, ok := statementList[0].(*ast.ReturnStmt)
		v.firstStatementIsReturn = ok
	}

	if v.blockInnerSuffixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockInnerSuffixInjection.Pop())
	}

	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("}")

	if v.blockOuterSuffixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockOuterSuffixInjection.Pop())
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

func (v *Visitor) pushInnerBlockPrefix(prefix string) {
	v.blockInnerPrefixInjection.Push(prefix)
}

func (v *Visitor) pushInnerBlockSuffix(suffix string) {
	v.blockInnerSuffixInjection.Push(suffix)
}

func (v *Visitor) pushOuterBlockPrefix(prefix string) {
	v.blockOuterPrefixInjection.Push(prefix)
}

func (v *Visitor) pushOuterBlockSuffix(suffix string) {
	v.blockOuterSuffixInjection.Push(suffix)
}
