package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

// Handles array and slice types
func (v *Visitor) enterArrayType(x *ast.ArrayType, name string, comment *ast.CommentGroup) {
	goTypeName := x.Elt.(*ast.Ident).Name
	csTypeName := convertToCSTypeName(goTypeName)
	typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

	v.targetFile.WriteString(v.newline)

	if x.Len == nil {
		// Handle slice type
		v.writeOutputLn(fmt.Sprintf("[GoType(\"[]%s\")]", csTypeName))
	} else {
		// Handle array type
		arrayLen := x.Len.(*ast.BasicLit).Value
		v.writeOutputLn(fmt.Sprintf("[GoType(\"[%s]%s\")]", arrayLen, csTypeName))
	}

	v.writeOutput("public partial struct %s {}", getSanitizedIdentifier(name))
	v.writeComment(comment, x.Elt.End()+typeLenDeviation)
	v.targetFile.WriteString(v.newline)
}

func (v *Visitor) exitArrayType(x *ast.ArrayType) {
}
