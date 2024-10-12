package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt) {
	ident := v.convExpr(incDecStmt.X, nil)

	v.targetFile.WriteString(v.newline)

	if incDecStmt.Tok == token.INC {
		v.writeOutput("%s++;", ident)
	} else {
		v.writeOutput("%s--;", ident)
	}
}
