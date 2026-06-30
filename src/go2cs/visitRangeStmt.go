package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt, target LabeledStmtContext) {
	v.targetFile.WriteString(v.newline)
	var ptrDeref string

	rangeExpr := v.convExpr(rangeStmt.X, nil)
	rangeType := v.getExprType(rangeStmt.X)

	if ptrType, ok := rangeType.(*types.Pointer); ok {
		rangeType = ptrType.Elem()
		ptrDeref = ".val"

		// A pointer parameter is implicitly dereferenced to a value local in the body
		// (`ref var p = ref Ꮡp.val`), so `p` already denotes the pointed-to value — ranging over
		// it must not add a second `.val` (which would be applied to the value type, CS1061).
		if ident, ok := rangeStmt.X.(*ast.Ident); ok && v.identIsParameter(ident) {
			ptrDeref = ""
		}
	}

	// Get the underlying type if it's a named type
	if named, ok := rangeType.(*types.Named); ok {
		rangeType = named.Underlying()
	}

	// Get the constraint type if it's a type parameter
	if typeParam, ok := rangeType.(*types.TypeParam); ok {
		rangeType = v.getConstraintType(typeParam)
	}

	var isSlice, isArray, isMap, isChan, isStr, untypedStr, isInt, untypedInt bool
	yieldFunc := -1

	// Slice type is expected to be most common, so this is checked first
	_, isSlice = rangeType.(*types.Slice)

	if !isSlice {
		_, isArray = rangeType.(*types.Array)
	}

	if !isSlice && !isArray {
		_, isMap = rangeType.(*types.Map)

		if !isMap {
			_, isChan = rangeType.(*types.Chan)
		}

		if !isMap && !isChan {
			yieldFunc = isYieldFunc(rangeType)
		}

		if !isMap && !isChan && yieldFunc == -1 {
			if basicType, ok := rangeType.(*types.Basic); ok {
				kind := basicType.Kind()

				isInt = kind == types.Int || kind == types.UntypedInt
				untypedInt = kind == types.UntypedInt

				if !isInt {
					isStr = kind == types.String || kind == types.UntypedString
					untypedStr = kind == types.UntypedString
				}
			}
		}
	}

	if !isSlice && !isArray && !isMap && !isChan && yieldFunc == -1 && !isInt && !isStr {
		rangeVal := v.getPrintedNode(rangeStmt)
		v.showWarning("@visitRangeStmt - unexpected 'ast.RangeStmt' expression %s", rangeVal)
		v.writeOutput("/* %s */", rangeVal)
		return
	}

	var valExpr, valType, keyExpr, keyType string
	var assignVars bool

	// key/value in a slice or array: index/value
	// key/value in a map: key/value
	// key/value in a string: index/rune
	// channel: element only
	// int: value only
	// func(func() bool): yield only
	// func(func(V) bool): yield with value
	// func(func(K, V) bool): yield with key/value

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

	if valExpr == "" && !(isChan || isInt || yieldFunc == 0 || yieldFunc == 1) {
		valExpr = "_"
	}

	// A heap-boxed range variable (its address is taken, `for i := range s { p := &i }`) in a
	// slice/array/map range must be boxed PER ITERATION (Go 1.22 per-iteration variable semantics:
	// a stored `&i` must point to a DISTINCT box each pass). Its heap decl is therefore deferred
	// into the loop body and the foreach iterates a temp var that is copied into the fresh box —
	// rather than declaring the box once before the loop (which would also clash with the foreach's
	// own re-declaration of the same name → CS0136). Captured here, emitted in the DEFINE branch.
	var keyHeapDecl, valHeapDecl string
	deferRangeVarBox := !isStr && !isChan && !isInt && yieldFunc <= -1

	// If defining new variables, perform escape analysis on the key and value expressions
	if !assignVars {
		var wroteHeapTypeDecl bool

		if keyExpr != "_" {
			// Get ident for key expression and check for heap allocation
			if ident, ok := rangeStmt.Key.(*ast.Ident); ok {
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					if deferRangeVarBox {
						keyHeapDecl = heapTypeDecl
					} else {
						v.writeOutput(heapTypeDecl)
						v.targetFile.WriteString(v.newline)
						wroteHeapTypeDecl = true
					}
				} else if !v.options.preferVarDecl {
					keyType = v.getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}
			}
		}

		if len(valExpr) > 0 && valExpr != "_" {
			// Get ident for value expression and check for heap allocation
			if ident, ok := rangeStmt.Value.(*ast.Ident); ok {
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					if deferRangeVarBox {
						valHeapDecl = heapTypeDecl
					} else {
						v.writeOutput(heapTypeDecl)
						v.targetFile.WriteString(v.newline)
						wroteHeapTypeDecl = true
					}
				} else if !v.options.preferVarDecl {
					valType = v.getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}
			}
		}

		if wroteHeapTypeDecl {
			v.targetFile.WriteString(v.newline)
		}
	}

	var varInit string

	if v.options.preferVarDecl && !(keyExpr == "_" && (len(valExpr) == 0 || valExpr == "_")) {
		varInit = "var "
	}

	if isStr {
		if untypedStr {
			rangeExpr = fmt.Sprintf("(@string)%s", rangeExpr)
		}

		if assignVars {
			var innerPrefix, tempKeyExpr, tempValExpr string

			if keyExpr == "_" {
				tempKeyExpr = "_"
			} else {
				tempKeyExpr = v.getTempVarName("i")

				if !v.options.preferVarDecl {
					keyType = v.getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), keyExpr, tempKeyExpr)
			}

			if valExpr == "_" {
				tempValExpr = "_"
			} else {
				tempValExpr = v.getTempVarName("r")

				if !v.options.preferVarDecl {
					valType = v.getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), valExpr, tempValExpr)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr, ptrDeref)
		}
	} else if isChan {
		if v.options.preferVarDecl {
			keyType = "var "
		}

		v.writeOutput("foreach (%s%s in %s%s)", keyType, keyExpr, rangeExpr, ptrDeref)
	} else if isInt {
		if untypedInt {
			rangeExpr = fmt.Sprintf("@int(%s%s)", rangeExpr, ptrDeref)
			ptrDeref = ""
		}

		if v.options.preferVarDecl {
			keyType = "var "
		}

		v.writeOutput("foreach (%s%s in range(%s%s))", keyType, keyExpr, rangeExpr, ptrDeref)
	} else if yieldFunc > -1 {
		if yieldFunc == 0 {
			if v.options.preferVarDecl {
				keyType = "var "
			}

			v.writeOutput("foreach (object %s in range(%s%s))", keyExpr, rangeExpr, ptrDeref)
		} else if yieldFunc == 1 {
			if v.options.preferVarDecl {
				keyType = "var "
			}

			v.writeOutput("foreach (%s%s in range(%s%s))", keyType, keyExpr, rangeExpr, ptrDeref)
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in range(%s%s))", varInit, keyType, keyExpr, valType, valExpr, rangeExpr, ptrDeref)
		}
	} else {
		// Handle slice, array, and map types
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
					keyType = v.getCSTypeName(v.getExprType(rangeStmt.Key)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), keyExpr, tempKeyExpr)
			}

			if valExpr == "_" {
				tempValExpr = "_"
			} else {
				tempValExpr = v.getTempVarName("v")

				if !v.options.preferVarDecl {
					valType = v.getCSTypeName(v.getExprType(rangeStmt.Value)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), valExpr, tempValExpr)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else if keyHeapDecl != "" || valHeapDecl != "" {
			// A heap-boxed range var: iterate a temp and, per iteration, allocate a fresh box and
			// copy the temp into it (Go 1.22 per-iteration semantics — a stored `&i` must point to a
			// distinct box each pass). A non-heap-boxed companion var declares directly as before.
			var innerPrefix, kExpr, vExpr string
			bodyIndent := v.indent(v.indentLevel + 1)

			if keyExpr == "_" {
				kExpr = "_"
			} else if keyHeapDecl != "" {
				name := "i"
				if isMap {
					name = "k"
				}
				kExpr = v.getTempVarName(name)
				innerPrefix += fmt.Sprintf("%s%s%s%s%s = %s;", v.newline, bodyIndent, keyHeapDecl, v.newline+bodyIndent, keyExpr, kExpr)
			} else {
				kExpr = keyExpr
			}

			if valExpr == "_" || valExpr == "" {
				vExpr = "_"
			} else if valHeapDecl != "" {
				vExpr = v.getTempVarName("v")
				innerPrefix += fmt.Sprintf("%s%s%s%s%s = %s;", v.newline, bodyIndent, valHeapDecl, v.newline+bodyIndent, valExpr, vExpr)
			} else {
				vExpr = valExpr
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, kExpr, valType, vExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr, ptrDeref)
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

	// A labeled range loop carries the same `continue_<label>`/`break_<label>` targets a labeled
	// `for` does: a Go `continue L`/`break L` from a nested loop emits `goto continue_L`/`goto break_L`,
	// so the labels must exist — `continue_L:` at the end of the body, `break_L:` after the loop
	// (otherwise CS0159 "no such label"). Mirrors visitForStmt.
	if len(target.label) > 0 {
		context.innerSuffix = fmt.Sprintf("%s%s:;", v.newline, getContinueLabelName(target.label))
		context.outerSuffix = fmt.Sprintf("%s%s:;", v.newline, getBreakLabelName(target.label))
	}

	v.visitBlockStmt(rangeStmt.Body, context)
}

func isYieldFunc(t types.Type) int {
	// First check if it's a function type
	sig, ok := t.(*types.Signature)

	if !ok {
		return -1
	}

	// Check if it has exactly one parameter
	params := sig.Params()

	if params.Len() != 1 {
		return -1
	}

	// Get the parameter type which should be a function
	paramType := params.At(0).Type()
	funcType, ok := paramType.(*types.Signature)

	if !ok {
		return -1
	}

	// Check if return type is bool
	results := funcType.Results()

	if results.Len() != 1 || !types.Identical(results.At(0).Type(), types.Typ[types.Bool]) {
		return -1
	}

	// Now check the parameters of the inner function
	innerParams := funcType.Params()

	if innerParams.Len() < 3 {
		return innerParams.Len()
	}

	return -1
}
