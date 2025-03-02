package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

func (v *Visitor) convFuncType(funcType *ast.FuncType) (resultsSignature, parameterSignature string) {
	// Loop through function results to check if any are structs
	if funcType.Results != nil {
		for index, resultField := range funcType.Results.List {
			var fieldName string

			if resultField.Names == nil {
				fieldName = fmt.Sprintf("func_R%d", index)
			} else {
				fieldName = fmt.Sprintf("func_%s", resultField.Names[0].Name)
			}

			// Check if the return type is a struct or pointer to a struct
			if structType, exprType := v.extractStructType(resultField.Type); structType != nil {
				v.indentLevel++
				v.visitStructType(structType, exprType, fieldName, resultField.Comment, true)
				v.indentLevel--
			}
		}
	}

	// Loop through function parameters to check if any are structs
	if funcType.Params != nil {
		for _, paramField := range funcType.Params.List {
			for _, paramName := range paramField.Names {
				// Check if the parameter type is a struct or pointer to a struct
				if structType, exprType := v.extractStructType(paramField.Type); structType != nil {
					v.indentLevel++
					v.visitStructType(structType, exprType, fmt.Sprintf("func_%s", paramName.Name), paramField.Comment, true)
					v.indentLevel--
				}
			}
		}
	}

	signature := v.getSignature(funcType)
	resultsSignature = v.generateResultSignature(signature)
	parameterSignature, _ = v.generateParametersSignature(signature, false)
	return
}

func (v *Visitor) getSignature(funcType *ast.FuncType) *types.Signature {
	// Gather parameters
	params := make([]*types.Var, 0)
	isVariadic := false

	for i, param := range funcType.Params.List {
		paramType := v.info.TypeOf(param.Type)

		// Check if this is the last parameter and if it’s variadic
		if i == len(funcType.Params.List)-1 {
			if _, ok := param.Type.(*ast.Ellipsis); ok {
				isVariadic = true
			}
		}

		if len(param.Names) == 0 {
			params = append(params, types.NewVar(token.NoPos, nil, "_", paramType))
		} else {
			for _, paramName := range param.Names {
				params = append(params, types.NewVar(token.NoPos, nil, paramName.Name, paramType))
			}
		}
	}

	// Gather results
	results := make([]*types.Var, 0)

	if funcType.Results != nil {
		for _, result := range funcType.Results.List {
			resultType := v.info.TypeOf(result.Type)
			if len(result.Names) == 0 {
				results = append(results, types.NewVar(token.NoPos, nil, "", resultType))
			} else {
				for _, resultName := range result.Names {
					results = append(results, types.NewVar(token.NoPos, nil, resultName.Name, resultType))
				}
			}
		}
	}

	// Construct the Signature using NewSignatureType
	paramTuple := types.NewTuple(params...)
	resultTuple := types.NewTuple(results...)

	// Since there’s no receiver or type parameters, we pass nil for those
	return types.NewSignatureType(nil, nil, nil, paramTuple, resultTuple, isVariadic)
}
