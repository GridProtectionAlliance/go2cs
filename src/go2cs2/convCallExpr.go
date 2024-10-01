package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr) string {
	constructType := ""

	if funName, ok := callExpr.Fun.(*ast.Ident); ok {
		funcInfo := v.info.ObjectOf(funName)

		if funcInfo != nil {
			// TODO: Most types in C# require a "new" keyword to instantiate. Check for other types that require this.
			if _, ok := funcInfo.Type().(*types.Named); ok {
				constructType = "new "
			}
		}
	}

	return fmt.Sprintf("%s%s(%s)", constructType, v.convExpr(callExpr.Fun), v.convExprList(callExpr.Args, callExpr.Lparen))
}
