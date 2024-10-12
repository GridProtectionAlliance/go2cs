package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	v.targetFile.WriteString(v.newline)
	v.writeOutput("return")

	var context *BasicLitContext
	signature := v.currentFunction.Signature()

	if returnStmt.Results == nil {
		// Check if result signature has named return values
		if v.currentFunction != nil {

			results := &strings.Builder{}

			for i := 0; i < signature.Results().Len(); i++ {
				if i > 0 && results.Len() > 0 {
					results.WriteString(", ")
				}

				param := signature.Results().At(i)

				if param.Name() != "" {
					ident := v.getVarIdent(param)

					if ident != nil {
						results.WriteString(v.getIdentName(ident))
					} else {
						results.WriteString(param.Name())
					}
				}
			}

			v.targetFile.WriteRune(' ')
			if results.Len() > 0 {
				if signature.Results().Len() > 1 {
					v.targetFile.WriteString("(")
					v.targetFile.WriteString(results.String())
					v.targetFile.WriteString(")")
				} else {
					v.targetFile.WriteString(results.String())
				}
			}
		}
	} else {
		v.targetFile.WriteRune(' ')

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune('(')

			// u8 readonly spans are not supported in value tuple
			context = &BasicLitContext{u8StringOK: false}
		}

		resultParams := signature.Results()
		resultParamIsInterface := paramsAreInterfaces(resultParams)

		for i, expr := range returnStmt.Results {
			if i > 0 {
				v.targetFile.WriteString(", ")
			}

			resultExpr := v.convExpr(expr, context)

			if resultParamIsInterface[i] {
				resultParamType := resultParams.At(i).Type()
				v.targetFile.WriteString(convertToInterfaceType(resultParamType, resultExpr))
			} else {
				v.targetFile.WriteString(resultExpr)
			}
		}

		if len(returnStmt.Results) > 1 {
			v.targetFile.WriteRune(')')
		}
	}

	v.targetFile.WriteRune(';')
}
