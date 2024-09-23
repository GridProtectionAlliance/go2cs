package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitStmt(n ast.Stmt) {
	switch x := n.(type) {
	case *ast.AssignStmt:
		//v.visitAssignStmt(x, n)
	case *ast.BlockStmt:
		//v.visitBlockStmt(x, n)
	case *ast.BranchStmt:
		//v.visitBranchStmt(x, n)
	case *ast.CaseClause:
		//v.visitCaseClause(x, n)
	case *ast.CommClause:
		//v.visitCommClause(x, n)
	case *ast.DeclStmt:
		//v.visitDeclStmt(x, n)
	case *ast.DeferStmt:
		//v.visitDeferStmt(x, n)
	case *ast.EmptyStmt:
		//v.visitEmptyStmt(x, n)
	case *ast.ExprStmt:
		//v.visitExprStmt(x, n)
	case *ast.ForStmt:
		//v.visitForStmt(x, n)
	case *ast.GoStmt:
		//v.visitGoStmt(x, n)
	case *ast.IfStmt:
		//v.visitIfStmt(x, n)
	case *ast.IncDecStmt:
		//v.visitIncDecStmt(x, n)
	case *ast.LabeledStmt:
		//v.visitLabeledStmt(x, n)
	case *ast.RangeStmt:
		//v.visitRangeStmt(x, n)
	case *ast.ReturnStmt:
		//v.visitReturnStmt(x, n)
	case *ast.SelectStmt:
		//v.visitSelectStmt(x, n)
	case *ast.SendStmt:
		//v.visitSendStmt(x, n)
	case *ast.SwitchStmt:
		//v.visitSwitchStmt(x, n)
	case *ast.TypeSwitchStmt:
		//v.visitTypeSwitchStmt(x, n)
	case *ast.BadStmt:
		println(fmt.Sprintf("WARNING: BadStmt encountered: %#v", x))
	default:
		panic(fmt.Sprintf("unexpected ast.Stmt: %#v", x))
	}
}
