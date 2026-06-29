package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt, format FormattingContext) {
	// `x++` / `x--` reads AND writes the operand, so it must be assignable. A field accessed through
	// a pointer must therefore use the assignable `.val` dereference, not the value-returning `~`:
	// `(~mp).ncgocall++` increments a field of an rvalue (CS1059). Convert the operand in assignment
	// context so a pointer-field selector emits `mp.val.ncgocall++`.
	assignContext := DefaultLambdaContext()
	assignContext.isAssignment = true

	ident := v.convExpr(incDecStmt.X, []ExprContext{assignContext})

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
