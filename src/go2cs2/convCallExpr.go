package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr) string {
	constructType := ""

	if funName := getIdentifier(callExpr.Fun); funName != nil {
		funcInfo := v.info.ObjectOf(funName)

		if funcInfo != nil {
			// TODO: Most types in C# require a "new" keyword to instantiate. Check for other types that require this.
			if _, ok := funcInfo.Type().(*types.Named); ok {
				constructType = "new "
			}
		}
	}

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	context := &CallExprContext{u8StringArgOK: make(map[int]bool)}

	// Check if any parameters of callExpr.Fun are interface types
	if funType, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		for i := 0; i < funType.Params().Len(); i++ {
			var paramType types.Type

			context.u8StringArgOK[i] = true

			if paramType, ok = getParameterType(funType, i); !ok {
				continue
			}

			if _, ok := paramType.Underlying().(*types.Interface); ok {
				context.u8StringArgOK[i] = false
			}
		}
	}

	return fmt.Sprintf("%s%s(%s)", constructType, getSanitizedIdentifier(v.convExpr(callExpr.Fun, nil)), v.convExprList(callExpr.Args, callExpr.Lparen, context))
}
