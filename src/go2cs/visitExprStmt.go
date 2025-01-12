package main

import (
	"go/ast"
)

func (v *Visitor) visitExprStmt(exprStmt *ast.ExprStmt) {
	if exprStmt.X != nil {
		v.targetFile.WriteString(v.newline)
		v.writeOutput("%s;", v.convExpr(exprStmt.X, nil))
	}
}
