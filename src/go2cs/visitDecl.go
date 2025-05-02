package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitDecl(decl ast.Decl) {
	switch declType := decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(declType)
	case *ast.FuncDecl:
		v.visitFuncDecl(declType)
	case *ast.BadDecl:
		v.showWarning("@visitDecl - BadDecl encountered: %#v", declType)
	default:
		panic(fmt.Sprintf("@visitDecl - Unexpected Decl type: %#v", declType))
	}
}
