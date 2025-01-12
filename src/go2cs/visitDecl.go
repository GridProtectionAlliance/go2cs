package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitDecl(decl ast.Decl) {
	switch declType := decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(declType, nil)
	case *ast.FuncDecl:
		v.visitFuncDecl(declType)
	case *ast.BadDecl:
		println(fmt.Sprintf("WARNING: BadDecl encountered: %#v", declType))
	default:
		panic(fmt.Sprintf("Unexpected Decl type: %#v", declType))
	}
}
