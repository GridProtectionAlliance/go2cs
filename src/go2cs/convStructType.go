package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convStructType(structType *ast.StructType, context IdentContext) string {
	var name string
	var identType types.Type

	t := v.getType(structType, false)

	if liftedName, ok := v.liftedTypeMap[t]; ok {
		return liftedName
	}

	if context.ident == nil {
		name = "type"
	} else {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}

		identType = v.getIdentType(context.ident)
	}

	return v.visitStructType(structType, identType, name, nil, true)
}
