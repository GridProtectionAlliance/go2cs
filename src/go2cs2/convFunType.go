package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

func (v *Visitor) convFuncType(funcType *ast.FuncType) (resultsSignature, parameterSignature string) {
	signature := v.getSignature(funcType)
	resultsSignature = generateResultSignature(signature)
	parameterSignature = generateParametersSignature(signature, false)
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

		for _, paramName := range param.Names {
			params = append(params, types.NewVar(token.NoPos, nil, paramName.Name, paramType))
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
