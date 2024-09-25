package main

import (
	"go/ast"
)

func (v *Visitor) visitEmptyStmt(emptyStmt *ast.EmptyStmt) {
	v.writeOutputLn("/* ; */")
}
