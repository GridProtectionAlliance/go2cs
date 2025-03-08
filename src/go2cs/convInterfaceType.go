package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convInterfaceType(interfaceType *ast.InterfaceType, context IdentContext) string {
	var name string
	var identType types.Type

	if context.ident == nil {
		name = "type"
	} else {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}

		identType = v.getIdentType(context.ident)
	}

	return v.visitInterfaceType(interfaceType, identType, name, nil, true)
}
