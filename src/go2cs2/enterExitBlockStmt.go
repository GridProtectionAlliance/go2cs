package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) enterBlockStmt(x *ast.BlockStmt) {
	v.pushBlock()

	if v.blockOuterPrefixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockOuterPrefixInjection.Pop())
	}

	v.writeOutputLn(" {")

	if v.blockInnerPrefixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockInnerPrefixInjection.Pop())
	}

	v.targetFile.WriteString(v.newline)
	v.firstStatementIsReturn = false
	v.indentLevel++

	for _, stmt := range x.List {
		v.visitStmt(stmt)
	}
}

func (v *Visitor) exitBlockStmt(x *ast.BlockStmt) {
	v.indentLevel--

	statementList := x.List

	// Check if the first statement is a return statement
	if len(statementList) > 0 {
		_, ok := statementList[0].(*ast.ReturnStmt)
		v.firstStatementIsReturn = ok
	}

	if v.blockInnerSuffixInjection.Len() > 0 {
		v.targetFile.WriteString(v.blockInnerSuffixInjection.Pop())
	}

	// if (!EndsWithLineFeed(m_targetFile.ToString()))
	// 	m_targetFile.AppendLine();
	// else
	// 	m_targetFile = new StringBuilder(RemoveLastDuplicateLineFeed(m_targetFile.ToString()));

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
