package main

import (
	"go/ast"
	"strings"
)

const DeferredDeclsMarker = ">>MARKER:DEFERRED_DECLS<<"

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	recvIndex := -1
	var capturedRecvName string

	// Check if receiver is directly returned
	if ptrRecv, recvName := v.isPointerReceiver(); ptrRecv {
		for i, result := range returnStmt.Results {
			if ident, ok := result.(*ast.Ident); ok {
				if ident.Name == recvName {
					v.captureReceiver = true
					recvIndex = i
					capturedRecvName = v.getCapturedReceiverName(recvName)
					break
				}
			}
		}
	}

	result := strings.Builder{}
	deferredDecls := strings.Builder{}

	result.WriteString(DeferredDeclsMarker)
	result.WriteString(v.indent(v.indentLevel))
	result.WriteString("return")

	signature := v.currentFuncSignature

	if returnStmt.Results == nil {
		// Check if result signature has named return values
		if signature != nil {
			results := &strings.Builder{}

			for i := range signature.Results().Len() {
				if i > 0 && results.Len() > 0 {
					results.WriteString(", ")
				}

				if recvIndex != -1 && i == recvIndex {
					result.WriteString(capturedRecvName)
				} else {
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

			var replacementVal string

			if resultParams != nil && i < resultParams.Len() {
				argType := v.getType(expr, false)
				targetType := resultParams.At(i).Type()
				replacementVal = v.checkForDynamicStructs(argType, targetType)
			}

			lambdaContext.deferredDecls = &strings.Builder{}

			resultExpr := v.convExpr(expr, []ExprContext{basicLitContext, lambdaContext})

			if len(replacementVal) > 0 {
				resultExpr = strings.ReplaceAll(replacementVal, DynamicCastArgMarker, resultExpr)
			}

			if lambdaContext.deferredDecls.Len() > 0 {
				deferredDecls.WriteString(lambdaContext.deferredDecls.String())
			}

			if resultParamIsInterface != nil && i < len(resultParamIsInterface) && resultParamIsInterface[i] {
				resultParamType := resultParams.At(i).Type()
				result.WriteString(v.convertToInterfaceType(resultParamType, v.getType(expr, false), resultExpr))
			} else {
				if resultParamIsPointer != nil && i < len(resultParamIsPointer) && resultParamIsPointer[i] {
					ident := getIdentifier(expr)

					if ident != nil && v.identIsParameter(ident) {
						result.WriteString(AddressPrefix)
						resultExpr = strings.TrimPrefix(resultExpr, "@")
					}
				}

				if recvIndex != -1 && i == recvIndex {
					result.WriteString(capturedRecvName)
				} else {
					result.WriteString(resultExpr)
				}
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
