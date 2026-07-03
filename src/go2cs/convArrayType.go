package main

import (
	"fmt"
	"go/ast"
	"strings"
)

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	typeName := v.getExprTypeName(arrayType, false)

	if context.compositeInitializer && strings.Contains(typeName, "[") {
		// Remove array brackets for composite literal initialization
		start := strings.Index(typeName, "[")
		end := strings.Index(typeName[start:], "]") + start

		if end != -1 {
			typeName = typeName[:start] + typeName[end+1:]
		}
	}

	typeName = convertToCSTypeName(typeName)

	if context.indexedInitializer {
		// For indexed initialization use a sparse array for initialization of slice or array.
		// golib's support types live in the go.golib child namespace (renamed from go.runtime,
		// which collided with the REAL runtime package's import alias -- CS0576 in any package
		// importing runtime once golib is referenced, e.g. iter/weak).
		return fmt.Sprintf("golib.SparseArray<%s>", typeName)
	} else if context.compositeInitializer {
		// Use basic array type for composite literal initialization of slice or array
		return fmt.Sprintf("%s[]", typeName)
	}

	var suffix string

	if context.maxLength > 0 {
		suffix = fmt.Sprintf("(%d)", context.maxLength)
	}

	if v.options.preferVarDecl {
		return fmt.Sprintf("%s%s", typeName, suffix)
	}

	if len(suffix) > 0 {
		return suffix
	}

	return "()"
}
