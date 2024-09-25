package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) convExprList(exprs []ast.Expr) string {
	var result []string

	for _, expr := range exprs {
		result = append(result, v.convExpr(expr))
	}

	return strings.Join(result, ", ")
}
