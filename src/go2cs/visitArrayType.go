package main

import (
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
)

// Handles array and slice types in context of a TypeSpec
func (v *Visitor) visitArrayType(arrayType *ast.ArrayType, identType types.Type, name string, comment *ast.CommentGroup) {
	// Resolve the element type's C# name. A simple identifier element (e.g. `type d [3]rune`)
	// keeps its written name so the GoType attribute reads `[3]rune`; a composite element — a
	// selector, a (generic) instantiation, a pointer, etc. (e.g. `[N]atomic.Pointer[entry[K, V]]`)
	// — must be resolved through the type system, since getIdentifier would collapse it to just
	// its leading identifier (`atomic`), mangling the GoType and the generated array element.
	var csTypeName, goTypeName string

	// An anonymous-struct (or -interface) element of a NAMED array/slice type — `type semTable
	// [N]struct{…}` (runtime/sema) — must be lifted to a named type, otherwise the GoType
	// attribute and any `&t[i].field` (which emits `.of(ElemType.ᏑField)`) reference a raw,
	// un-compilable `struct{…}`. Lift it (named after the array type) and use the lifted name.
	if structType, ok := arrayType.Elt.(*ast.StructType); ok && !isEmptyStruct(structType) {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(structType) {
			liftedName := v.visitStructType(structType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	} else if interfaceType, ok := arrayType.Elt.(*ast.InterfaceType); ok && !interfaceType.Incomplete && len(interfaceType.Methods.List) > 0 {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(interfaceType) {
			liftedName := v.visitInterfaceType(interfaceType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	}

	if csTypeName != "" {
		// element already resolved to a lifted name above
	} else if ident := getIdentifier(arrayType.Elt); ident != nil && isSimpleIdentExpr(arrayType.Elt) {
		goTypeName = ident.Name
		csTypeName = convertToCSTypeName(goTypeName)
	} else if eltType := v.info.TypeOf(arrayType.Elt); eltType != nil {
		// Use the fully-qualified name (getFullTypeName) rather than the package-aliased form: the
		// GoType attribute is consumed by the generated array-backed partial, which lives in a file
		// without this file's package-relative `using` aliases (e.g. `using atomic = ...`), so an
		// aliased element type would be unresolvable there (CS0246).
		csTypeName = convertToCSTypeName(v.getFullTypeName(eltType, false))
		goTypeName = csTypeName
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		v.showWarning("@visitArrayType - Failed to resolve 'ast.ArrayType' element %s", typeName)
		v.writeOutputLn("// [...]%s", typeName)
		return
	}

	typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

	v.targetFile.WriteString(v.newline)

	if arrayType.Len == nil {
		// Handle slice type
		v.writeOutput("[GoType(\"[]%s\")] ", csTypeName)
	} else {
		// Handle array type
		var arrayLenValue string
		arrayLenExpr := v.convExpr(arrayType.Len, nil)

		// Check if length expression is in type information
		if tv, ok := v.info.Types[arrayType.Len]; ok {
			// Check if it's a constant
			if tv.Value != nil {
				length := tv.Value
				intLength, _ := constant.Int64Val(length)
				arrayLenValue = strconv.FormatInt(intLength, 10)
			}
		}

		if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
			v.writeOutput("[GoType(\"[%s]%s\")] /* [%s]%s */%s", arrayLenValue, csTypeName, arrayLenExpr, csTypeName, v.newline)
		} else {
			v.writeOutput("[GoType(\"[%s]%s\")] ", arrayLenExpr, csTypeName)
		}
	}

	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// Append generic type parameters and constraints (e.g. `<K, V> where K : new()`) for a generic
	// named array type so the forward declaration matches its uses, and the constraints propagate
	// type-wide to the generated array-backed partial (whose element type may require them).
	typeParams, constraints := v.getGenericDefinition(identType)

	v.writeOutput("%spartial struct %s%s%s;", access, getSanitizedIdentifier(name), typeParams, constraints)
	v.writeComment(comment, arrayType.Elt.End()+typeLenDeviation)
	v.targetFile.WriteString(v.newline)
}

// isSimpleIdentExpr reports whether an expression is a bare identifier (not a selector, index,
// star, etc.), so a single-identifier array element keeps its written name in the GoType attribute.
func isSimpleIdentExpr(expr ast.Expr) bool {
	_, ok := expr.(*ast.Ident)
	return ok
}
