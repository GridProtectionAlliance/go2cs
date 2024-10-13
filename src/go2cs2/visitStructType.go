package main

import (
	"go/ast"
	"go/token"
	"strconv"
)

// Handles struct types in the context of a TypeSpec
func (v *Visitor) visitStructType(structType *ast.StructType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, structType.Pos())

	v.writeOutputLn("[GoType(\"struct\")]")
	v.writeOutputLn("%s partial struct %s {", getAccess(name), getSanitizedIdentifier(name))
	v.indentLevel++

	for _, field := range structType.Fields.List {
		if len(field.Names) != 1 {
			println("WARNING: Expected one name per StructType field, found " + strconv.Itoa(len(field.Names)))
		}

		v.writeDoc(field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeOutput("[GoTag(")
			v.targetFile.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false}))
			v.targetFile.WriteString(")]")
			v.targetFile.WriteString(v.newline)
		}

		if ident := getIdentifier(field.Type); ident != nil {
			goTypeName := ident.Name
			csTypeName := convertToCSTypeName(goTypeName)
			typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

			v.writeOutput("public %s %s;", csTypeName, getSanitizedIdentifier(field.Names[0].Name))
			v.writeComment(field.Comment, field.Type.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		} else {
			v.writeOutputLn("// %s", v.getPrintedNode(field))
		}
	}

	v.indentLevel--
	v.writeOutputLn("}")
}
