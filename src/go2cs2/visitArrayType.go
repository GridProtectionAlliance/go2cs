package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

// Handles array and slice types in context of a TypeSpec
func (v *Visitor) visitArrayType(arrayType *ast.ArrayType, name string, comment *ast.CommentGroup) {
	ident, ok := arrayType.Elt.(*ast.Ident)

	if ok {
		goTypeName := ident.Name
		csTypeName := convertToCSTypeName(goTypeName)
		typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

		v.targetFile.WriteString(v.newline)

		if arrayType.Len == nil {
			// Handle slice type
			v.writeOutputLn(fmt.Sprintf("[GoType(\"[]%s\")]", csTypeName))
		} else {
			// Handle array type
			arrayLen := v.convExpr(arrayType.Len)
			v.writeOutputLn(fmt.Sprintf("[GoType(\"[%s]%s\")]", arrayLen, csTypeName))
		}

		v.writeOutput("public partial struct %s {}", getSanitizedIdentifier(name))
		v.writeComment(comment, arrayType.Elt.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	} else {
		v.writeOutputLn("// %s", v.getPrintedNode(arrayType))
	}
}
