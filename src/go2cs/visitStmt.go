package main

import (
	"fmt"
	"go/ast"
	"strings"
)

type StmtContext interface {
	getDefault() StmtContext
}

type FormattingContext struct {
	useNewLine         bool
	includeSemiColon   bool
	useIndent          bool
	heapTypeDeclTarget *strings.Builder
}

func DefaultFormattingContext() FormattingContext {
	return FormattingContext{
		useNewLine:         true,
		includeSemiColon:   true,
		useIndent:          true,
		heapTypeDeclTarget: nil,
	}
}

func (c FormattingContext) getDefault() StmtContext {
	return DefaultFormattingContext()
}

type BlockStmtContext struct {
	format      FormattingContext
	innerPrefix string
	innerSuffix string
	outerPrefix string
	outerSuffix string
}

func DefaultBlockStmtContext() BlockStmtContext {
	return BlockStmtContext{
		format:      DefaultFormattingContext(),
		innerPrefix: "",
		innerSuffix: "",
		outerPrefix: "",
		outerSuffix: "",
	}
}

func (c BlockStmtContext) getDefault() StmtContext {
	return DefaultBlockStmtContext()
}

type LabeledStmtContext struct {
	label string
}

func DefaultLabeledStmtContext() LabeledStmtContext {
	return LabeledStmtContext{
		label: "",
	}
}

func (c LabeledStmtContext) getDefault() StmtContext {
	return DefaultLabeledStmtContext()
}

func getStmtContext[TContext StmtContext](contexts []StmtContext) TContext {
	var zeroValue TContext

	if len(contexts) == 0 {
		return zeroValue.getDefault().(TContext)
	}

	for _, context := range contexts {
		if context != nil {
			if targetContext, ok := context.(TContext); ok {
				return targetContext
			}
		}
	}

	return zeroValue.getDefault().(TContext)
}

func (v *Visitor) visitStmt(stmt ast.Stmt, contexts []StmtContext) {
	v.lastStatementWasReturn = false

	switch stmtType := stmt.(type) {
	case *ast.AssignStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitAssignStmt(stmtType, format)
	case *ast.BlockStmt:
		context := getStmtContext[BlockStmtContext](contexts)
		v.visitBlockStmt(stmtType, context)
	case *ast.BranchStmt:
		v.visitBranchStmt(stmtType)
	case *ast.CommClause:
		v.visitCommClause(stmtType)
	case *ast.DeclStmt:
		v.visitDeclStmt(stmtType)
	case *ast.DeferStmt:
		v.visitDeferStmt(stmtType)
	case *ast.ExprStmt:
		v.visitExprStmt(stmtType)
	case *ast.ForStmt:
		target := getStmtContext[LabeledStmtContext](contexts)
		v.visitForStmt(stmtType, target)
	case *ast.GoStmt:
		v.visitGoStmt(stmtType)
	case *ast.IfStmt:
		v.visitIfStmt(stmtType)
	case *ast.IncDecStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitIncDecStmt(stmtType, format)
	case *ast.LabeledStmt:
		v.visitLabeledStmt(stmtType)
	case *ast.RangeStmt:
		v.visitRangeStmt(stmtType)
	case *ast.ReturnStmt:
		v.visitReturnStmt(stmtType)
		v.lastStatementWasReturn = true
		v.lastReturnIndentLevel = v.indentLevel
	case *ast.SelectStmt:
		v.visitSelectStmt(stmtType)
	case *ast.SendStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitSendStmt(stmtType, format)
	case *ast.SwitchStmt:
		v.visitSwitchStmt(stmtType)
	case *ast.TypeSwitchStmt:
		v.visitTypeSwitchStmt(stmtType)
	case *ast.BadStmt:
		v.showWarning("@visitStmt - BadStmt encountered: %#v", stmtType)
	case *ast.EmptyStmt:
		// Nothing to do
	default:
		panic(fmt.Sprintf("@visitStmt - Unexpected Stmt type: %#v", v.getPrintedNode(stmtType)))
	}
}
