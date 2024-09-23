package main

import (
	"go/ast"
)

func (v *Visitor) enterTypeSpec(x *ast.TypeSpec, doc *ast.CommentGroup) {
	if x.Type != nil {
		v.visitType(x.Type, x.Name.Name, doc, x.Comment)
	}
}

func (v *Visitor) exitTypeSpec(x *ast.TypeSpec) {
}
