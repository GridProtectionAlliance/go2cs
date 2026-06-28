package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitExprStmt(exprStmt *ast.ExprStmt) {
	if exprStmt.X == nil {
		return
	}

	// A func literal passed as a call argument (`systemstack(func(){ …&m… })`) captures variables
	// whose snapshot declarations (`var mʗ1 = m;`) are statements — invalid inside an argument list.
	// Thread a deferredDecls builder so convFuncLit hoists them here, emitted before the statement.
	lambdaContext := DefaultLambdaContext()
	lambdaContext.deferredDecls = &strings.Builder{}

	expr := v.convExpr(exprStmt.X, []ExprContext{lambdaContext})

	if lambdaContext.deferredDecls.Len() > 0 {
		// The hoisted decls already carry their own leading newline + indentation per line.
		v.targetFile.WriteString(lambdaContext.deferredDecls.String())
	} else {
		v.targetFile.WriteString(v.newline)
	}

	v.writeOutput("%s;", expr)
}
