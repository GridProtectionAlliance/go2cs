package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt, format FormattingContext) {
	ident := v.convExpr(incDecStmt.X, nil)

	if format.useNewLine {
		v.targetFile.WriteString(v.newline)
	}

	if format.useIndent {
		v.writeOutput("")
	}

	if incDecStmt.Tok == token.INC {
		v.targetFile.WriteString(fmt.Sprintf("%s++", ident))
	} else {
		v.targetFile.WriteString(fmt.Sprintf("%s--", ident))
	}

	if format.includeSemiColon {
		v.targetFile.WriteRune(';')
	}
}
