package main

import (
	"go/ast"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType, name string) {
	var resultsSignature, parameterSignature string
	resultsSignature, parameterSignature = v.convFuncType(funcType)
	v.targetFile.WriteString(v.newline)
	v.writeOutputLn("%s delegate %s %s(%s);", getAccess(name), resultsSignature, name, parameterSignature)
}
