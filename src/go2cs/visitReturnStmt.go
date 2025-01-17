package main

import (
	"go/ast"
	"strings"
)

const DeferredDeclsMarker = ">>MARKER:DEFERRED_DECLS<<"

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	result := strings.Builder{}
	deferredDecls := strings.Builder{}

	result.WriteString(DeferredDeclsMarker)
	result.WriteString(v.indent(v.indentLevel))
	result.WriteString("return")

	signature := v.currentFuncType.Signature()

	if returnStmt.Results == nil {
		// Check if result signature has named return values
		if v.currentFuncType != nil {
			results := &strings.Builder{}

			for i := 0; i < signature.Results().Len(); i++ {
				if i > 0 && results.Len() > 0 {
					results.WriteString(", ")
				}

				param := signature.Results().At(i)

				if param.Name() != "" {
					ident := v.getVarIdent(param)

					if ident != nil {
						results.WriteString(getSanitizedIdentifier(v.getIdentName(ident)))
					} else {
						results.WriteString(getSanitizedIdentifier(param.Name()))
					}
				}
			}

			if results.Len() > 0 {
				result.WriteRune(' ')

				if signature.Results().Len() > 1 {
					result.WriteRune('(')
					result.WriteString(results.String())
					result.WriteRune(')')
				} else {
					result.WriteString(results.String())
				}
			}
		}
	} else {
		result.WriteRune(' ')

		basicLitContext := DefaultBasicLitContext()

		if len(returnStmt.Results) > 1 {
			result.WriteRune('(')

			// u8 readonly spans are not supported in value tuple
			basicLitContext.u8StringOK = false
		}

		lambdaContext := DefaultLambdaContext()

		resultParams := signature.Results()
		resultParamIsInterface := paramsAreInterfaces(resultParams, true)
		resultParamIsPointer := paramsArePointers(resultParams)

		for i, expr := range returnStmt.Results {
			if i > 0 {
				result.WriteString(", ")
			}

			lambdaContext.deferredDecls = &strings.Builder{}

			resultExpr := v.convExpr(expr, []ExprContext{basicLitContext, lambdaContext})

			if lambdaContext.deferredDecls.Len() > 0 {
				deferredDecls.WriteString(lambdaContext.deferredDecls.String())
			}

			if resultParamIsInterface != nil && resultParamIsInterface[i] {
				resultParamType := resultParams.At(i).Type()
				result.WriteString(convertToInterfaceType(resultParamType, v.getType(expr, false), resultExpr))
			} else {
				if resultParamIsPointer != nil && resultParamIsPointer[i] {
					ident := getIdentifier(expr)

					if ident != nil && v.identIsParameter(ident) {
						result.WriteString(AddressPrefix)
						resultExpr = strings.TrimPrefix(resultExpr, "@")
					}
				}

				result.WriteString(resultExpr)
			}
		}

		if len(returnStmt.Results) > 1 {
			result.WriteRune(')')
		}
	}

	result.WriteRune(';')

	if deferredDecls.Len() == 0 {
		deferredDecls.WriteString(v.newline)
	}

	v.targetFile.WriteString(strings.ReplaceAll(result.String(), DeferredDeclsMarker, deferredDecls.String()))
}
