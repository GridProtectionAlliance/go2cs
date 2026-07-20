package main

import (
	"go/ast"
	"go/types"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitMapType(mapType *ast.MapType, identType types.Type, name string) {
	// A defined map type — `type Grades map[string]int` — emits the `[GoType("map[K, V]")]
	// partial struct` forward declaration whose Map template go2cs-gen implements (the
	// generator's attribute format is comma-separated `map[K, V]`, not Go's `map[K]V`). The
	// old stub emitted only a comment, leaving the type undeclared (CS0246).
	keyType := convertToCSTypeName(v.getTypeName(v.info.TypeOf(mapType.Key), false))
	valueType := convertToCSTypeName(v.getTypeName(v.info.TypeOf(mapType.Value), false))
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// A map type declared inside a function body (`type M map[int]bool`, maps_test.go) cannot be a
	// method-body statement in C#; hoist it to member level (see liftLocalTypeDecl). A package-level
	// declaration is unaffected — target is v.targetFile and finish() is a no-op.
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writeStringLn(target, "[GoType(\"map[%s, %s]\")] %spartial struct %s;", keyType, valueType, access, getSanitizedIdentifier(name))
	finish()
}
