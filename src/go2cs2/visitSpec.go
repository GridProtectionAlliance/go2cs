package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) visitSpec(n ast.Spec, tok token.Token, doc *ast.CommentGroup) {
	switch x := n.(type) {
	case *ast.ImportSpec:
		v.enterImportSpec(x, doc)
		v.exitImportSpec(x)
	case *ast.ValueSpec:
		v.enterValueSpec(x, tok)
		v.exitValueSpec(x)
	case *ast.TypeSpec:
		v.enterTypeSpec(x, doc)
		v.exitTypeSpec(x)
	default:
		panic(fmt.Sprintf("unexpected ast.Spec: %#v", x))
	}
}
