package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt) {
	ident := v.convExpr(incDecStmt.X, nil)

	if incDecStmt.Tok == token.INC {
		v.writeOutputLn("%s++;", ident)
	} else {
		v.writeOutputLn("%s--;", ident)
	}
}
