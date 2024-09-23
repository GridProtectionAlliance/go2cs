package main

import (
	"go/ast"
)

func (v *Visitor) enterTypeSpec(x *ast.TypeSpec, doc *ast.CommentGroup) {
	println("Entering TypeSpec \"" + x.Name.Name + "\"")

	v.writeDoc(doc, x.Pos())

	if x.TypeParams != nil {
		// Print the type spec
		for _, field := range x.TypeParams.List {
			println("Field Name: " + field.Names[0].Name)

			if len(field.Comment.Text()) > 0 {
				println("Field Comment: " + field.Comment.Text())
			}

			if len(field.Doc.Text()) > 0 {
				println("Field Doc: " + field.Doc.Text())
			}
		}
	}

	if x.Assign.IsValid() {
		println("TypeSpec Assign: = defined")
	}

	if x.Type != nil {
		v.visitType(x.Type, x.Name.Name, x.Comment)
	}

	if len(x.Comment.Text()) > 0 {
		println("TypeSpec Comment: " + x.Comment.Text())
	}

	if len(x.Doc.Text()) > 0 {
		println("TypeSpec Doc: " + x.Doc.Text())
	}
}

func (v *Visitor) exitTypeSpec(x *ast.TypeSpec) {
	println("Exiting TypeSpec \"" + x.Name.Name + "\"")
}
