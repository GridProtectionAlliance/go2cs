package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	v.enterLambdaConversion(funcLit)
	defer v.exitLambdaConversion()

	v.prepareStmtCaptures(funcLit)

	result := strings.Builder{}

	if decls := v.generateCaptureDeclarations(); decls != "" {
		if context.deferredDecls == nil {
			result.WriteString(decls)
		} else {
			context.deferredDecls.WriteString(strings.TrimRight(decls, " "))
		}
	}

	var parameterSignature string

	// For C#, lambda return type is inferred and not explicitly declared
	_, parameterSignature = v.convFuncType(funcLit.Type)

	blockStatementContext := DefaultBlockStmtContext()
	blockStatementContext.format.useNewLine = false

	v.pushBlock()
	v.visitBlockStmt(funcLit.Body, blockStatementContext)
	body := v.popBlockAppend(false)

	if v.firstStatementIsReturn {
		// Find return statement in string and remove it
		returnIndex := strings.Index(body, "return ")

		if returnIndex != -1 {
			body = body[returnIndex+7:]
			body = strings.TrimSuffix(body, "}")

			// Remove trailing brace
			braceIndex := strings.LastIndex(body, "}")

			if braceIndex != -1 {
				body = body[:braceIndex]
			}

			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, ";")
		}
	} else {
		body = strings.TrimSpace(body)
	}

	result.WriteString("(" + parameterSignature + ") => " + body)

	return result.String()
}
