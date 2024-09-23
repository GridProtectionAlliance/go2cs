package main

import (
	"go/ast"
	"go/token"
	"strconv"
)

func (v *Visitor) enterStructType(x *ast.StructType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, x.Pos())

	v.writeOutputLn("[GoType(\"struct\")]")
	v.writeOutputLn("%s partial struct %s {", getAccess(name), getSanitizedIdentifier(name))
	v.indentLevel++

	for _, field := range x.Fields.List {
		if len(field.Names) != 1 {
			println("WARNING: Expected one name per StructType field, found " + strconv.Itoa(len(field.Names)))
		}

		v.writeDoc(field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeOutputLn("[GoTag(%s)]", getStringLiteral(field.Tag))
		}

		goTypeName := field.Type.(*ast.Ident).Name
		csTypeName := convertToCSTypeName(goTypeName)
		typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

		v.writeOutput("public %s %s;", csTypeName, getSanitizedIdentifier(field.Names[0].Name))
		v.writeComment(field.Comment, field.Type.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	}

	v.indentLevel--
	v.writeOutputLn("}")
}

func (v *Visitor) exitStructType(x *ast.StructType) {
}
