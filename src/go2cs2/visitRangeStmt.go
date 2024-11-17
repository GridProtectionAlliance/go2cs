package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt) {
	v.targetFile.WriteString(v.newline)

	rangeExpr := v.convExpr(rangeStmt.X, nil)
	_, isMap := v.getExprType(rangeStmt.X).(*types.Map)
	var isStr, untypedStr bool

	if basicType, ok := v.getExprType(rangeStmt.X).(*types.Basic); ok {
		kind := basicType.Kind()
		isStr = kind == types.String || kind == types.UntypedString
		untypedStr = kind == types.UntypedString
	}

	var valExpr, valType, keyExpr, keyType string
	var assignVars bool

	// key/value in a slice or array: index/value
	// key/value in a map: key/value
	// key/value in a string: index/rune

	context := DefaultBlockStmtContext()
	context.format.useNewLine = false

	if rangeStmt.Key != nil {
		keyExpr = v.convExpr(rangeStmt.Key, nil)
		assignVars = rangeStmt.Tok == token.ASSIGN
	}

	if keyExpr == "" {
		keyExpr = "_"
	}

	if rangeStmt.Value != nil {
		valExpr = v.convExpr(rangeStmt.Value, nil)
	}

	if valExpr == "" {
		valExpr = "_"
	}

	// If defining new variables, perform escape analysis on the key and value expressions
	if !assignVars {
		var wroteHeapTypeDecl bool

		if keyExpr != "_" {
			// Get ident for key expression and check for heap allocation
			if ident, ok := rangeStmt.Key.(*ast.Ident); ok {
				v.performEscapeAnalysis(ident, rangeStmt.Body)
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					v.writeOutput(heapTypeDecl)
					v.targetFile.WriteString(v.newline)
					wroteHeapTypeDecl = true
				} else if !v.options.preferVarDecl {
					keyType = getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}
			}
		}

		if valExpr != "_" {
			// Get ident for value expression and check for heap allocation
			if ident, ok := rangeStmt.Value.(*ast.Ident); ok {
				v.performEscapeAnalysis(ident, rangeStmt.Body)
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					v.writeOutput(heapTypeDecl)
					v.targetFile.WriteString(v.newline)
					wroteHeapTypeDecl = true
				} else if !v.options.preferVarDecl {
					valType = getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}
			}
		}

		if wroteHeapTypeDecl {
			v.targetFile.WriteString(v.newline)
		}
	}

	var varInit string

	if v.options.preferVarDecl && !(keyExpr == "_" && valExpr == "_") {
		varInit = "var "
	}

	if isStr {
		if untypedStr {
			rangeExpr = fmt.Sprintf("@string(%s)", rangeExpr)
		}

		if assignVars {
			var innerPrefix, tempKeyExpr, tempValExpr string

			if keyExpr == "_" {
				tempKeyExpr = "_"
			} else {
				tempKeyExpr = v.getTempVarName("i")

				if !v.options.preferVarDecl {
					keyType = getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), keyExpr, tempKeyExpr)
			}

			if valExpr == "_" {
				tempValExpr = "_"
			} else {
				tempValExpr = v.getTempVarName("r")

				if !v.options.preferVarDecl {
					valType = getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), valExpr, tempValExpr)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in %s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr)
		}
	} else {
		if assignVars {
			var innerPrefix, tempKeyExpr, tempValExpr string

			if keyExpr == "_" {
				tempKeyExpr = "_"
			} else {
				var keyName string

				if isMap {
					keyName = "k"
				} else {
					keyName = "i"
				}

				tempKeyExpr = v.getTempVarName(keyName)

				if !v.options.preferVarDecl {
					keyType = getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), keyExpr, tempKeyExpr)
			}

			if valExpr == "_" {
				tempValExpr = "_"
			} else {
				tempValExpr = v.getTempVarName("v")

				if !v.options.preferVarDecl {
					valType = getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), valExpr, tempValExpr)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in %s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr)
		}
	}

	// Option to use for loop instead of foreach
	//  else {
	// 	if keyExpr == "_" && !assignVars {
	// 		v.writeOutput("foreach (var %s in %s)", valExpr, rangeExpr)
	// 	} else {
	// 		if assignVars {
	// 			if keyExpr == "_" {
	// 				keyExpr = v.getTempVarName("i")
	// 				v.writeOutput("for (var %s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)
	// 			} else {
	// 				v.writeOutput("for (%s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)
	// 			}

	// 			if valExpr != "_" {
	// 				context.innerPrefix = fmt.Sprintf("%s%s%s = %s[%s];%s", v.newline, v.indent(v.indentLevel+1), valExpr, rangeExpr, keyExpr, v.newline)
	// 			}
	// 		} else {
	// 			v.writeOutput("for (var %s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)

	// 			if valExpr != "_" {
	// 				context.innerPrefix = fmt.Sprintf("%s%svar %s = %s[%s];%s", v.newline, v.indent(v.indentLevel+1), valExpr, rangeExpr, keyExpr, v.newline)
	// 			}
	// 		}
	// 	}
	// }

	v.visitBlockStmt(rangeStmt.Body, context)
}
