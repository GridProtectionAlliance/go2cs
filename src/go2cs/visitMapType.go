package main

import (
	"go/ast"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitMapType(mapType *ast.MapType, name string) {
	// A defined map type — `type Grades map[string]int` — emits the `[GoType("map[K, V]")]
	// partial struct` forward declaration whose Map template go2cs-gen implements (the
	// generator's attribute format is comma-separated `map[K, V]`, not Go's `map[K]V`). The
	// old stub emitted only a comment, leaving the type undeclared (CS0246).
	keyType := convertToCSTypeName(v.getTypeName(v.info.TypeOf(mapType.Key), false))
	valueType := convertToCSTypeName(v.getTypeName(v.info.TypeOf(mapType.Value), false))
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""
	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("[GoType(\"map[%s, %s]\")] %spartial struct %s;", keyType, valueType, access, getSanitizedIdentifier(name))
}
