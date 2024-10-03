package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexExpr(ident *ast.IndexExpr) string {
	return fmt.Sprintf("%s[%s]", v.convExpr(ident.X, nil), v.convExpr(ident.Index, nil))
}
