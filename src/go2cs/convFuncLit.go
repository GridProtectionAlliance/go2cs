package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	v.enterLambdaConversion(funcLit)
	defer v.exitLambdaConversion()

	// Create a map of parameters to avoid capturing them
	paramNames := make(map[string]bool)

	if funcLit.Type.Params != nil {
		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// Filter out any captures that are actually parameters
	if captures, exists := v.lambdaCapture.stmtCaptures[funcLit]; exists {
		for ident := range captures {
			if paramNames[ident.Name] {
				delete(captures, ident)
			}
		}

		// If no captures remain, remove the empty map
		if len(captures) == 0 {
			delete(v.lambdaCapture.stmtCaptures, funcLit)
		}
	}

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
