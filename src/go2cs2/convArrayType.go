package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	if ident := getIdentifier(arrayType.Elt); ident != nil {
		typeName := convertToCSTypeName(ident.Name)

		if context.compositeInitializer {
			var arraySize string

			if arrayType.Len != nil {
				arraySize = "/*" + v.convExpr(arrayType.Len, nil) + "*/"
			}

			// Use basic array type for composite literal initialization of slice or array
			return fmt.Sprintf("%s[%s]", typeName, arraySize)
		}

		var suffix string

		if context.maxLength > 0 {
			suffix = fmt.Sprintf("(%d)", context.maxLength)
		}

		if v.options.preferVarDecl {
			if arrayType.Len == nil {
				return fmt.Sprintf("slice<%s>%s", typeName, suffix)
			} else {
				return fmt.Sprintf("array<%s>%s", typeName, suffix)
			}
		}

		if len(suffix) > 0 {
			return suffix
		}

		return "()"
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		println(fmt.Sprintf("WARNING: @convArrayType - Failed to resolve `ast.ArrayType` element %s", typeName))
		return fmt.Sprintf("/* [...]%s */", typeName)
	}
}
