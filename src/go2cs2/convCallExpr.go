package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr) string {
	return fmt.Sprintf("%s(%s)", v.convExpr(callExpr.Fun), v.convExprList(callExpr.Args))
}
