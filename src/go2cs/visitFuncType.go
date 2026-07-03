package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType, identType types.Type, name string) {
	var resultsSignature, parameterSignature string

	resultsSignature, parameterSignature = v.convFuncType(funcType)

	// The type parameters of a generic defined function type — `type Seq[V any] func(yield
	// func(V) bool)` (Go 1.23 iter) — live on the NAMED type, not the *ast.FuncType's signature,
	// so the generic definition must come from identType (as the struct/array paths do). Deriving
	// it from the signature emitted a NON-generic delegate (`delegate void Seq(Func<V, bool>…)`)
	// whose V was undefined (CS0246) while uses correctly said `Seq<V>` (CS0308).
	typeParams, constraints := v.getGenericDefinition(identType)

	if len(constraints) > 0 {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("%s delegate %s %s%s(%s)%s;", getAccess(name), resultsSignature, name, typeParams, parameterSignature, constraints)
}
