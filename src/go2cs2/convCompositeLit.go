package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convCompositeLit(chanType *ast.CompositeLit) string {
	return fmt.Sprintf("new %s { %s }", v.convExpr(chanType.Type), v.convExprList(chanType.Elts))
}
