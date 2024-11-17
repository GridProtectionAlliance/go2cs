package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	if context.compositeInitializer {
		// Use basic array type for composite literal initialization
		return convertToCSTypeName(v.convExpr(arrayType.Elt, nil)) + "[]"
	}

	var suffix string

	if context.maxLength > 0 {
		suffix = fmt.Sprintf("(%d)", context.maxLength)
	}

	if v.options.preferVarDecl {
		if arrayType.Len == nil {
			return fmt.Sprintf("slice<%s>%s", convertToCSTypeName(v.convExpr(arrayType.Elt, nil)), suffix)
		} else {
			return fmt.Sprintf("array<%s>%s", convertToCSTypeName(v.convExpr(arrayType.Elt, nil)), suffix)
		}
	}

	if len(suffix) > 0 {
		return suffix
	}

	return "()"
}
