package main

import (
	"go/ast"
	"go/token"
)

// Handles struct types in the context of a TypeSpec
func (v *Visitor) visitStructType(structType *ast.StructType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, structType.Pos())

	v.writeOutputLn("[GoType] partial struct %s {", getSanitizedIdentifier(name))
	v.indentLevel++

	for _, field := range structType.Fields.List {
		v.writeDoc(field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeOutput("[GoTag(")
			v.targetFile.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false}))
			v.targetFile.WriteString(")]")
			v.targetFile.WriteString(v.newline)
		}

		fieldType := v.getType(field.Type, false)
		goTypeName := getCSTypeName(fieldType)
		csTypeName := convertToCSTypeName(goTypeName)
		typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

		for _, ident := range field.Names {
			v.writeOutput("public %s %s;", csTypeName, getSanitizedIdentifier(ident.Name))
			v.writeComment(field.Comment, field.Type.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		}
	}

	v.indentLevel--
	v.writeOutputLn("}")
}
