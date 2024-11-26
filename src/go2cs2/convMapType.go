package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convMapType(mapType *ast.MapType) string {
	if v.options.preferVarDecl {
		var mapKeyTypeName, mapValueTypeName string

		if ident := getIdentifier(mapType.Key); ident != nil {
			mapKeyTypeName = convertToCSTypeName(ident.Name)
		} else {
			typeName := v.getPrintedNode(mapType.Key)
			println(fmt.Sprintf("WARNING: @convMapType - Failed to resolve `ast.MapType` key %s", typeName))
			mapKeyTypeName = fmt.Sprintf("/* %s */", typeName)
		}

		if ident := getIdentifier(mapType.Value); ident != nil {
			mapValueTypeName = convertToCSTypeName(ident.Name)
		} else {
			typeName := v.getPrintedNode(mapType.Key)
			println(fmt.Sprintf("WARNING: @convMapType - Failed to resolve `ast.MapType` value %s", typeName))
			mapValueTypeName = fmt.Sprintf("/* %s */", typeName)
		}

		return fmt.Sprintf("map<%s, %s>", mapKeyTypeName, mapValueTypeName)
	}

	return "()"
}
