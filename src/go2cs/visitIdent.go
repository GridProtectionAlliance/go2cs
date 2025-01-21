package main

import (
	"go/ast"
	"go/types"
)

// Handles identity types in context of a TypeSpec
func (v *Visitor) visitIdent(ident *ast.Ident, name string) {
	identType := v.getIdentType(ident).Underlying()
	goTypeName := identType.String()
	csTypeName := convertToCSTypeName(goTypeName)

	v.targetFile.WriteString(v.newline)

	if isNumericType(identType) {
		// Handle numeric type
		v.writeOutput("[GoType(\"num:%s\")]", csTypeName)
	} else {
		// Handle other types
		v.writeOutput("[GoType(\"%s\")]", csTypeName)
	}

	v.writeOutput(" partial struct %s {}", getSanitizedIdentifier(name))
	v.targetFile.WriteString(v.newline)
}

func isNumericType(typ types.Type) bool {
	if typ, ok := typ.(*types.Basic); ok && typ != nil {
		kind := typ.Kind()

		return kind == types.Int || kind == types.Int8 || kind == types.Int16 || kind == types.Int32 || kind == types.Int64 ||
			kind == types.Uint || kind == types.Uint8 || kind == types.Uint16 || kind == types.Uint32 || kind == types.Uint64 ||
			kind == types.Float32 || kind == types.Float64 || kind == types.Complex64 || kind == types.Complex128
	}

	return false
}
