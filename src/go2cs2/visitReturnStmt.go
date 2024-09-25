package main

import (
	"go/ast"
)

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	v.writeOutput("return")

	if returnStmt.Results != nil {
		v.targetFile.WriteRune(' ')

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune('(')
		}

		for i, expr := range returnStmt.Results {
			if i > 0 {
				v.targetFile.WriteString(", ")
			}

			v.targetFile.WriteString(v.convExpr(expr))
		}

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune(')')
		}
	}

	v.targetFile.WriteRune(';')
}
