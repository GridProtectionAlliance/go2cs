package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convEllipsis(chanType *ast.Ellipsis) string {
	return fmt.Sprintf("params %s", v.convExpr(chanType.Elt, nil))
}
