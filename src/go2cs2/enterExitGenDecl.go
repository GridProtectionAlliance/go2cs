package main

import (
	"go/ast"
)

func (v *Visitor) enterGenDecl(x *ast.GenDecl) {
	for _, spec := range x.Specs {
		v.visitSpec(spec, x.Tok, x.Doc)
	}
}

func (v *Visitor) exitGenDecl(x *ast.GenDecl) {
}
