package main

import (
	"go/ast"
)

func (v *Visitor) convStructType(structType *ast.StructType, context IdentContext) string {
	var name string

	if len(context.name) > 0 {
		name = context.name
	} else {
		name = "type"
	}

	return v.visitStructType(structType, nil, name, nil, true)
}
