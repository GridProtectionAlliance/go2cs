// visitIncDecStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt, format FormattingContext) {
	// `x++` / `x--` reads AND writes the operand, so it must be assignable. A field accessed through
	// a pointer must therefore use the assignable `.Value` dereference, not the value-returning `~`:
	// `(~mp).ncgocall++` increments a field of an rvalue (CS1059). Convert the operand in assignment
	// context so a pointer-field selector emits `mp.Value.ncgocall++`.
	assignContext := DefaultLambdaContext()
	assignContext.isAssignment = true

	contexts := []ExprContext{assignContext}

	// An INDEX operand (`req.Count["hits"]++`) is likewise a write: mark it the assignment
	// target so its BASE takes the `.Value` path too (see IndexExprContext.isAssignmentTarget).
	if _, isIndex := incDecStmt.X.(*ast.IndexExpr); isIndex {
		indexContext := DefaultIndexExprContext()
		indexContext.isAssignmentTarget = true
		contexts = append(contexts, indexContext)
	}

	ident := v.convExpr(incDecStmt.X, contexts)

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
