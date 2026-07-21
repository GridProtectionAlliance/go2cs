// visitExprStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitExprStmt(exprStmt *ast.ExprStmt, format FormattingContext) {
	if exprStmt.X == nil {
		return
	}

	// A func literal passed as a call argument (`systemstack(func(){ …&m… })`, pervasive in
	// runtime) captures variables whose snapshot declarations (`var mʗ1 = m;`) are statements —
	// invalid inside an argument list. For a standalone statement, collect them in a buffer and
	// write them before the statement. A for-loop init/post clause (useNewLine == false) is not a
	// standalone statement slot, so it does not hoist. Save/restore guards nesting.
	savedHoist := v.hoistedDecls
	var hoistBuf *strings.Builder

	if format.useNewLine {
		hoistBuf = &strings.Builder{}
		v.hoistedDecls = hoistBuf
	}

	defer func() { v.hoistedDecls = savedHoist }()

	expr := v.convExpr(exprStmt.X, nil)

	if hoistBuf != nil && hoistBuf.Len() > 0 {
		// The hoisted decls carry their own leading newline + per-line indentation.
		v.targetFile.WriteString(hoistBuf.String())
	} else if format.useNewLine {
		v.targetFile.WriteString(v.newline)
	}

	if format.useIndent {
		v.targetFile.WriteString(v.indent(v.indentLevel))
	}

	v.targetFile.WriteString(expr)

	// A for-loop init/post clause is `;`-free (the for-syntax supplies the separators); a standalone
	// expression statement is terminated with a semicolon.
	if format.includeSemiColon {
		v.targetFile.WriteString(";")
	}
}
