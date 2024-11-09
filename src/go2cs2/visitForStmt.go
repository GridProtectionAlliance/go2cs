package main

import (
	"fmt"
	"go/ast"
	"strings"
)

const ForVarInitMarker = ">>MARKER:FOR_VAR_INIT<<"

func (v *Visitor) visitForStmt(forStmt *ast.ForStmt, target LabeledStmtContext) {
	v.targetFile.WriteString(v.newline)

	// Handle while-style for loops
	if forStmt.Init == nil && forStmt.Post == nil {
		v.writeOutput("while (")

		if forStmt.Cond == nil {
			v.targetFile.WriteString("true")
		} else {
			v.targetFile.WriteString(v.convExpr(forStmt.Cond, nil))
		}

		v.targetFile.WriteRune(')')
		context := DefaultBlockStmtContext()
		context.format.useNewLine = false
		v.visitBlockStmt(forStmt.Body, context)
		return
	}

	// Escape analysis should be performed on the for loop body for
	// any initializations that may be using shadowed variables
	source := ParentBlockContext{parentBlock: forStmt.Body}

	heapTypeDeclTarget := &strings.Builder{}

	format := FormattingContext{
		useNewLine:         false,
		includeSemiColon:   false,
		useIndent:          false,
		heapTypeDeclTarget: heapTypeDeclTarget,
	}

	contexts := []StmtContext{source, format}

	v.targetFile.WriteString(ForVarInitMarker)
	v.writeOutput("for (")

	if forStmt.Init != nil {
		// Allowed statements in the init part of a for loop:
		//   - short variable declaration (can have heap allocations)
		//   - assignment
		//   - increment / decrement statement
		//   - send statement (source code):
		v.visitStmt(forStmt.Init, contexts)
	}

	// Replace the marker with any heap allocations for the for loop
	if heapTypeDeclTarget.Len() > 0 {
		heapTypeDecl := heapTypeDeclTarget.String()
		heapTypeDeclTarget.Reset()

		heapTypeDeclTarget.WriteString(v.indent(v.indentLevel))
		heapTypeDeclTarget.WriteString(heapTypeDecl)
		heapTypeDeclTarget.WriteString(v.newline)

		v.replaceMarker(ForVarInitMarker, heapTypeDeclTarget.String())
	} else {
		v.replaceMarker(ForVarInitMarker, "")
	}

	v.targetFile.WriteString("; ")

	if forStmt.Cond == nil {
		v.targetFile.WriteString("true")
	} else {
		v.targetFile.WriteString(v.convExpr(forStmt.Cond, nil))
	}

	v.targetFile.WriteString("; ")

	if forStmt.Post != nil {
		// Allowed statements in the post part of a for loop:
		//   - assignment
		//   - increment / decrement statement
		//   - send statement (source code):
		v.visitStmt(forStmt.Post, contexts)
	}

	v.targetFile.WriteRune(')')

	blockContext := DefaultBlockStmtContext()
	blockContext.format.useNewLine = false

	if len(target.label) > 0 {
		blockContext.innerSuffix = fmt.Sprintf("%s%s%s:;", v.newline, v.newline, getContinueLabelName(target.label))
		blockContext.outerSuffix = fmt.Sprintf("%s:;", getBreakLabelName(target.label))
	}

	v.visitBlockStmt(forStmt.Body, blockContext)
}
