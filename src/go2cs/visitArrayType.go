package main

import (
	"go/ast"
	"go/constant"
	"go/token"
	"strconv"
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
			v.writeOutput("[GoType(\"[]%s\")] ", csTypeName)
		} else {
			// Handle array type
			var arrayLenValue string
			arrayLenExpr := v.convExpr(arrayType.Len, nil)

			// Check if length expression is in type information
			if tv, ok := v.info.Types[arrayType.Len]; ok {
				// Check if it's a constant
				if tv.Value != nil {
					length := tv.Value
					intLength, _ := constant.Int64Val(length)
					arrayLenValue = strconv.FormatInt(intLength, 10)
				}
			}

			if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
				v.writeOutput("[GoType(\"[%s]%s\")] /* [%s]%s */%s", arrayLenValue, csTypeName, arrayLenExpr, csTypeName, v.newline)
			} else {
				v.writeOutput("[GoType(\"[%s]%s\")] ", arrayLenExpr, csTypeName)
			}
		}

		v.writeOutput("partial struct %s;", getSanitizedIdentifier(name))
		v.writeComment(comment, arrayType.Elt.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		v.showWarning("@visitArrayType - Failed to resolve 'ast.ArrayType' element %s", typeName)
		v.writeOutputLn("// [...]%s", typeName)
	}
}
