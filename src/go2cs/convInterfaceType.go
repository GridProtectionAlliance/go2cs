package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convInterfaceType(interfaceType *ast.InterfaceType, context IdentContext) string {
	var name string
	var identType types.Type

	t := v.getType(interfaceType, false)

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

	return v.visitInterfaceType(interfaceType, identType, name, nil, true)
}
