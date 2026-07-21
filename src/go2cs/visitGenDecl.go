// visitGenDecl.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitGenDecl(genDecl *ast.GenDecl) {
	var hasValueSpec bool

	for _, spec := range genDecl.Specs {
		switch specType := spec.(type) {
		case *ast.ImportSpec:
			v.visitImportSpec(specType, genDecl.Doc)
		case *ast.ValueSpec:
			v.visitValueSpec(specType, genDecl.Doc, genDecl.Tok)
			hasValueSpec = true
		case *ast.TypeSpec:
			v.visitTypeSpec(specType, genDecl.Doc)
		default:
			panic(fmt.Sprintf("@visitGenDecl - unexpected GenDecl Spec: %#v", v.getPrintedNode(specType)))
		}
	}

	if !v.inFunction && hasValueSpec {
		v.targetFile.WriteString(v.newline)
	}
}
