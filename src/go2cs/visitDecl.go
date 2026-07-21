// visitDecl.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
		panic(fmt.Sprintf("@visitDecl - Unexpected Decl type: %#v", v.getPrintedNode(declType)))
	}
}
