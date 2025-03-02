package main

import (
	"fmt"
	"go/ast"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType, name string) {
	var resultsSignature, parameterSignature string

	resultsSignature, parameterSignature = v.convFuncType(funcType)

	typeParams, constraints := v.getGenericDefinition(v.getType(funcType, false))

	if len(constraints) > 0 {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("%s delegate %s %s%s(%s)%s;", getAccess(name), resultsSignature, name, typeParams, parameterSignature, constraints)
}
