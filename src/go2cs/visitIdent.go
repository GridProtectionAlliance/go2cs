package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// Handles identity types in context of a TypeSpec
func (v *Visitor) visitIdent(ident *ast.Ident, identType types.Type, name string, lifted bool) {
	underlyingIdentType := v.getIdentType(ident).Underlying()
	goTypeName := underlyingIdentType.String()
	csTypeName := convertToCSTypeName(goTypeName)

	var target *strings.Builder
	var preLiftIndentLevel int

	// Intra-function type declarations are not allowed in C#
	if lifted {
		if v.inFunction {
			target = &strings.Builder{}

			if !strings.HasPrefix(name, v.currentFuncName+"_") {
				name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
			}

			preLiftIndentLevel = v.indentLevel
			v.indentLevel = 0
		}

		name = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = name
	}

	if target == nil {
		target = v.targetFile
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	if isNumericType(underlyingIdentType) {
		// Handle numeric type
		v.writeString(target, "[GoType(\"num:%s\")]", csTypeName)
	} else {
		// Handle other types
		v.writeString(target, "[GoType(\"%s\")]", csTypeName)
	}

	v.writeString(target, " partial struct %s;", getSanitizedIdentifier(name))
	target.WriteString(v.newline)

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel = preLiftIndentLevel
	}
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
