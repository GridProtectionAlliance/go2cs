package main

import (
	"fmt"
	"go/ast"
	"strings"
)

type StmtContext interface {
	getDefault() StmtContext
}

type ParentBlockContext struct {
	parentBlock *ast.BlockStmt
}

func DefaultParentBlockContext() ParentBlockContext {
	return ParentBlockContext{
		parentBlock: nil,
	}
}

func (c ParentBlockContext) getDefault() StmtContext {
	return DefaultParentBlockContext()
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
	switch stmtType := stmt.(type) {
	case *ast.AssignStmt:
		source := getStmtContext[ParentBlockContext](contexts)
		format := getStmtContext[FormattingContext](contexts)
		v.visitAssignStmt(stmtType, source, format)
	case *ast.BlockStmt:
		context := getStmtContext[BlockStmtContext](contexts)
		v.visitBlockStmt(stmtType, context)
	case *ast.BranchStmt:
		v.visitBranchStmt(stmtType)
	case *ast.CommClause:
		v.visitCommClause(stmtType)
	case *ast.DeclStmt:
		source := getStmtContext[ParentBlockContext](contexts)
		v.visitDeclStmt(stmtType, source)
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
		source := getStmtContext[ParentBlockContext](contexts)
		v.visitIfStmt(stmtType, source)
	case *ast.IncDecStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitIncDecStmt(stmtType, format)
	case *ast.LabeledStmt:
		v.visitLabeledStmt(stmtType)
	case *ast.RangeStmt:
		v.visitRangeStmt(stmtType)
	case *ast.ReturnStmt:
		v.visitReturnStmt(stmtType)
	case *ast.SelectStmt:
		v.visitSelectStmt(stmtType)
	case *ast.SendStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitSendStmt(stmtType, format)
	case *ast.SwitchStmt:
		source := getStmtContext[ParentBlockContext](contexts)
		v.visitSwitchStmt(stmtType, source)
	case *ast.TypeSwitchStmt:
		source := getStmtContext[ParentBlockContext](contexts)
		v.visitTypeSwitchStmt(stmtType, source)
	case *ast.BadStmt:
		println(fmt.Sprintf("WARNING: BadStmt encountered: %#v", stmtType))
	case *ast.EmptyStmt:
		// Nothing to do
	default:
		panic(fmt.Sprintf("Unexpected Stmt type: %#v", stmtType))
	}
}
