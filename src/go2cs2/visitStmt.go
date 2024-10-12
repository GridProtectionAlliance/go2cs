package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitStmt(stmt ast.Stmt, parentBlock *ast.BlockStmt) {
	switch stmtType := stmt.(type) {
	case *ast.AssignStmt:
		v.visitAssignStmt(stmtType, parentBlock)
	case *ast.BlockStmt:
		v.visitBlockStmt(stmtType, true, true)
	case *ast.BranchStmt:
		v.visitBranchStmt(stmtType)
	case *ast.CommClause:
		v.visitCommClause(stmtType)
	case *ast.DeclStmt:
		v.visitDeclStmt(stmtType, parentBlock)
	case *ast.DeferStmt:
		v.visitDeferStmt(stmtType)
	case *ast.EmptyStmt:
		v.visitEmptyStmt(stmtType)
	case *ast.ExprStmt:
		v.visitExprStmt(stmtType)
	case *ast.ForStmt:
		v.visitForStmt(stmtType)
	case *ast.GoStmt:
		v.visitGoStmt(stmtType)
	case *ast.IfStmt:
		v.visitIfStmt(stmtType)
	case *ast.IncDecStmt:
		v.visitIncDecStmt(stmtType)
	case *ast.LabeledStmt:
		v.visitLabeledStmt(stmtType)
	case *ast.RangeStmt:
		v.visitRangeStmt(stmtType)
	case *ast.ReturnStmt:
		v.visitReturnStmt(stmtType)
	case *ast.SelectStmt:
		v.visitSelectStmt(stmtType)
	case *ast.SendStmt:
		v.visitSendStmt(stmtType)
	case *ast.SwitchStmt:
		v.visitSwitchStmt(stmtType, parentBlock)
	case *ast.TypeSwitchStmt:
		v.visitTypeSwitchStmt(stmtType)
	case *ast.BadStmt:
		println(fmt.Sprintf("WARNING: BadStmt encountered: %#v", stmtType))
	default:
		panic(fmt.Sprintf("Unexpected Stmt type: %#v", stmtType))
	}
}
