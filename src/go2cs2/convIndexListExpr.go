package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexListExpr(indexListExpr *ast.IndexListExpr) string {
	return fmt.Sprintf("%s[%s]", v.convExpr(indexListExpr.X), v.convExprList(indexListExpr.Indices))
}
