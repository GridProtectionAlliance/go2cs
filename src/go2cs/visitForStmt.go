package main

import (
	"fmt"
	"go/ast"
	"strings"
)

const ForVarInitMarker = ">>MARKER:FOR_VAR_INIT<<"

func (v *Visitor) visitForStmt(forStmt *ast.ForStmt, target LabeledStmtContext) {
	// A func literal passed as a call argument in the condition (`for (…; underIs(t, func(u){…}); …)`)
	// emits its captured-variable snapshot declarations (`var tʗ1 = t;`) — statements, invalid inside
	// the condition expression. Convert the condition into a hoist buffer so those decls can be
	// written on their own lines before the loop (mirrors visitExprStmt). Save/restore guards nesting.
	savedHoist := v.hoistedDecls

	if forStmt.Init == nil && forStmt.Post == nil {
		// Handle while-style for loops
		hoistBuf := &strings.Builder{}
		var cond string

		if forStmt.Cond != nil {
			v.hoistedDecls = hoistBuf
			cond = v.convExpr(forStmt.Cond, nil)
			v.hoistedDecls = savedHoist
		}

		if hoistBuf.Len() > 0 {
			// The buffer carries its own leading newline+indent per decl and a trailing newline (the
			// per-decl trailing indent is trimmed by convFuncLit); writeOutput supplies `while`'s indent.
			v.targetFile.WriteString(hoistBuf.String())
		} else {
			v.targetFile.WriteString(v.newline)
		}

		v.writeOutput("while (")

		if forStmt.Cond == nil {
			v.targetFile.WriteString(TrueMarker)
		} else {
			v.targetFile.WriteString(cond)
		}
	} else {
		v.targetFile.WriteString(v.newline)

		// Handle traditional for loops
		v.targetFile.WriteString(ForVarInitMarker)
		v.writeOutput("for (")

		// Escape analysis should be performed on the for loop body for
		// any initializations that may be using shadowed variables
		heapTypeDeclTarget := &strings.Builder{}

		format := FormattingContext{
			useNewLine:         false,
			includeSemiColon:   false,
			useIndent:          false,
			heapTypeDeclTarget: heapTypeDeclTarget,
		}

		contexts := []StmtContext{format}

		if forStmt.Init != nil {
			// Allowed statements in the init part of a for loop:
			//   - short variable declaration (can have heap allocations)
			//   - assignment
			//   - increment / decrement statement
			//   - send statement (source code):
			// Flag the init clause so a multi-variable mixed-type `:=` is emitted as a single
			// tuple-deconstruction declaration (a for-init clause cannot hold `;`-separated decls).
			initFormat := format
			initFormat.forInit = true
			v.visitStmt(forStmt.Init, []StmtContext{initFormat})
		}

		// Convert the condition AFTER the init (preserving capture-counter ordering). Any func-literal
		// capture-snapshot decls are collected here and hoisted before the `for`, at the same marker
		// position as the for-init heap allocations.
		condHoistBuf := &strings.Builder{}
		var cond string

		if forStmt.Cond != nil {
			v.hoistedDecls = condHoistBuf
			cond = v.convExpr(forStmt.Cond, nil)
			v.hoistedDecls = savedHoist
		}

		// Replace the marker with the hoisted condition snapshot decls followed by any heap
		// allocations for the for loop. The marker sits after a leading newline and before the
		// `<indent>for (`, so each group is emitted as `<indent>content` lines ending in a newline.
		var markerReplacement strings.Builder

		if condHoistBuf.Len() > 0 {
			// Reformat the buffer's `\r\n<indent>decl;…\r\n<indent>` into `<indent>decl;…\r\n` lines:
			// drop the leading newline (the marker already follows one) and the trailing indent.
			markerReplacement.WriteString(strings.TrimRight(strings.TrimPrefix(condHoistBuf.String(), v.newline), " "))
		}

		if heapTypeDeclTarget.Len() > 0 {
			markerReplacement.WriteString(v.indent(v.indentLevel))
			markerReplacement.WriteString(heapTypeDeclTarget.String())
			markerReplacement.WriteString(v.newline)
		}

		v.replaceMarker(ForVarInitMarker, markerReplacement.String())

		v.targetFile.WriteString("; ")

		if forStmt.Cond == nil {
			v.targetFile.WriteString(TrueMarker)
			v.targetFile.WriteRune(' ')
		} else {
			v.targetFile.WriteString(cond)
		}

		v.targetFile.WriteString("; ")

		if forStmt.Post != nil {
			// Allowed statements in the post part of a for loop:
			//   - assignment
			//   - increment / decrement statement
			//   - send statement (source code):
			v.inForPost = true
			v.forPostReAlias = ""
			v.visitStmt(forStmt.Post, contexts)
			v.inForPost = false
		}
	}

	v.targetFile.WriteRune(')')

	blockContext := DefaultBlockStmtContext()
	blockContext.format.useNewLine = false

	// A deref-aliased pointer param/box repointed in the POST (`for ; scope != nil;
	// scope = scope.Outer`) stashed its value re-alias here — inject it as the first
	// statement of the loop body so each iteration re-binds the value var to the new box
	// (the box-repoint stayed in the post). go/ast resolve.go's scope-chain walk.
	if len(v.forPostReAlias) > 0 {
		blockContext.innerPrefix = v.newline + v.indent(v.indentLevel+1) + v.forPostReAlias
		v.forPostReAlias = ""
	}

	if len(target.label) > 0 {
		blockContext.innerSuffix = fmt.Sprintf("%s%s:;", v.newline, getContinueLabelName(target.label))
		blockContext.outerSuffix = fmt.Sprintf("%s%s:;", v.newline, getBreakLabelName(target.label))
	}

	v.visitBlockStmt(forStmt.Body, blockContext)
}
