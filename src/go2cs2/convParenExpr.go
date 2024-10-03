package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convParenExpr(parenExpr *ast.ParenExpr) string {
	return fmt.Sprintf("(%s)", v.convExpr(parenExpr.X, nil))
}
