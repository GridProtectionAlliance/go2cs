package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt) {
	v.targetFile.WriteString(v.newline)

	rangeExpr := v.convExpr(rangeStmt.X, nil)
	var valExpr, keyExpr string

	context := DefaultBlockStmtContext()
	context.format.useNewLine = false

	if rangeStmt.Value != nil {
		valExpr = v.convExpr(rangeStmt.Value, nil)

		if valExpr == "_" {
			valExpr = ""
		}
	}

	if rangeStmt.Key != nil {
		keyExpr = v.convExpr(rangeStmt.Key, nil)

		if keyExpr == "_" {
			keyExpr = ""
		}
	}

	// TODO: Handle assign vs define - check for heap allocations on define

	if len(keyExpr) == 0 {
		v.writeOutput("foreach (var %s in %s)", valExpr, rangeExpr)
	} else {
		v.writeOutput("for (var %s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)

		if len(valExpr) > 0 {
			context.innerPrefix = fmt.Sprintf("%s%s%s := %s[%s];", v.newline, v.indent(v.indentLevel+1), valExpr, rangeExpr, keyExpr)
		}
	}

	v.visitBlockStmt(rangeStmt.Body, context)
}
