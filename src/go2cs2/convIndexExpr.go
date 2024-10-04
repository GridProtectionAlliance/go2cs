package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr) string {
	return fmt.Sprintf("%s[%s]", v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
}
