package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr) string {
	return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X), v.convIdent(selectorExpr.Sel))
}
