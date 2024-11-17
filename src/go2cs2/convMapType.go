package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convMapType(mapType *ast.MapType) string {
	if v.options.preferVarDecl {
		return fmt.Sprintf("map<%s, %s>", convertToCSTypeName(v.convExpr(mapType.Key, nil)), convertToCSTypeName(v.convExpr(mapType.Value, nil)))
	}

	return "()"
}
