package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

// Handles array and slice types in context of a TypeSpec
func (v *Visitor) visitArrayType(arrayType *ast.ArrayType, name string, comment *ast.CommentGroup) {
	if ident := getIdentifier(arrayType.Elt); ident != nil {
		goTypeName := ident.Name
		csTypeName := convertToCSTypeName(goTypeName)
		typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

		v.targetFile.WriteString(v.newline)

		if arrayType.Len == nil {
			// Handle slice type
			v.writeOutput("[GoType(\"[]%s\")]", csTypeName)
		} else {
			// Handle array type
			arrayLen := v.convExpr(arrayType.Len, nil)
			v.writeOutput("[GoType(\"[%s]%s\")]", arrayLen, csTypeName)
		}

		v.writeOutput(" partial struct %s {}", getSanitizedIdentifier(name))
		v.writeComment(comment, arrayType.Elt.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		println(fmt.Sprintf("WARNING: @visitArrayType - Failed to resolve `ast.ArrayType` element %s", typeName))
		v.writeOutputLn("// [...]%s", typeName)
	}
}
