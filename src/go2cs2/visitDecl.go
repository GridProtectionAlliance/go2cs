package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitDecl(n ast.Decl) {
	switch x := n.(type) {
	case *ast.GenDecl:
		v.enterGenDecl(x)
		v.exitGenDecl(x)
	case *ast.FuncDecl:
		v.enterFuncDecl(x)
		v.exitFuncDecl(x)
	case *ast.BadDecl:
		println(fmt.Sprintf("WARNING: BadDecl encountered: %#v", x))
	default:
		panic(fmt.Sprintf("unexpected ast.Decl: %#v", x))
	}
}
