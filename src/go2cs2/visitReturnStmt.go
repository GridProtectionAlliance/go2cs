package main

import (
	"go/ast"
)

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	v.writeOutput("return")

	var context *BasicLitContext

	if returnStmt.Results != nil {
		v.targetFile.WriteRune(' ')

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune('(')
			context = &BasicLitContext{u8StringOK: false}
		}

		for i, expr := range returnStmt.Results {
			if i > 0 {
				v.targetFile.WriteString(", ")
			}

			v.targetFile.WriteString(v.convExpr(expr, context))
		}

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune(')')
		}
	}

	v.targetFile.WriteRune(';')
}
