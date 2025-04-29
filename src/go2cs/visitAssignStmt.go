package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, format FormattingContext) {
	result := &strings.Builder{}

	lhsExprs := assignStmt.Lhs
	rhsExprs := assignStmt.Rhs

	lhsLen := len(lhsExprs)
	rhsLen := len(rhsExprs)

	reassignedCount := 0
	declaredCount := 0

	// Check for interface types in LHS as RHS will need to be casted to the interface type
	lhsTypeIsInterface := make([]bool, lhsLen)

	// Check for string types in LHS, u8 readonly spans are not supported in value tuple
	lhsTypeIsString := make([]bool, lhsLen)
	anyTypeIsString := false

	// Ensure that the correct type is used for integer, we do this since int and uint in
	// converted Go code target C# nint or nuint to match original Go code behavior and a
	// "var" based assignment to an int type could result in a very subtle type mismatch
	lhsTypeIsInt := make([]bool, lhsLen)
	anyTypeIsInt := false

	// Check if rhs is a call with a tuple result
	var tupleResult bool

	if rhsLen == 1 {
		if callExpr, ok := rhsExprs[0].(*ast.CallExpr); ok {
			funType := v.info.TypeOf(callExpr.Fun)

			if signature, ok := funType.(*types.Signature); ok {
				results := signature.Results()

				if results != nil {
					tupleResult = results.Len() > 1
				}
			}
		} else if unaryExpr, ok := rhsExprs[0].(*ast.UnaryExpr); ok {
			if unaryExpr.Op == token.ARROW {
				tupleResult = lhsLen > 1
			}
		} else if _, ok := rhsExprs[0].(*ast.TypeAssertExpr); ok {
			tupleResult = lhsLen > 1
		}
	}

	// Count the number of reassigned and declared variables
	for i, lhs := range lhsExprs {
		ident := getIdentifier(lhs)

		if ident == nil {
			// Check if lhs is a struct member
			if selectorExpr, ok := lhs.(*ast.SelectorExpr); ok {
				ident = getIdentifier(selectorExpr.Sel)

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
				typeName := v.getExprTypeName(ident, true)

				lhsTypeIsString[i] = typeName == "string"

				if !anyTypeIsString && lhsTypeIsString[i] {
					anyTypeIsString = true
				}

				lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

				if !anyTypeIsInt && lhsTypeIsInt[i] {
					anyTypeIsInt = true
				}
			} else if indexExpr, ok := lhs.(*ast.IndexExpr); ok {
				ident = getIdentifier(indexExpr.X)

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
			}
		} else {
			if v.isReassignment(ident) {
				reassignedCount++
			} else {
				obj := v.info.ObjectOf(ident)

				if obj != nil {
					if !v.identEscapesHeap[obj] {
						declaredCount++
					}
				}
			}

			isInterface, isEmpty := v.isInterface(ident)
			lhsTypeIsInterface[i] = isInterface && !isEmpty
			typeName := v.getExprTypeName(ident, true)

			lhsTypeIsString[i] = typeName == "string"

			if !anyTypeIsString && lhsTypeIsString[i] {
				anyTypeIsString = true
			}

			lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

			if !anyTypeIsInt && lhsTypeIsInt[i] {
				anyTypeIsInt = true
			}
		}
	}

	// Map Go tokens to C# string equivalents
	var operator string

	switch assignStmt.Tok {
	case token.ADD_ASSIGN:
		operator = " += "
	case token.SUB_ASSIGN:
		operator = " -= "
	case token.MUL_ASSIGN:
		operator = " *= "
	case token.QUO_ASSIGN:
		operator = " /= "
	case token.REM_ASSIGN:
		operator = " %= "
	case token.AND_ASSIGN:
		operator = " &= "
	case token.OR_ASSIGN:
		operator = " |= "
	case token.XOR_ASSIGN:
		operator = " ^= "
	case token.SHL_ASSIGN:
		operator = " <<= "
	case token.SHR_ASSIGN:
		operator = " >>= "
	case token.AND_NOT_ASSIGN:
		// C# doesn't have a direct AND NOT equivalent, so expand `&^=` to `&= ~`
		operator = " &= ~"
	default:
		operator = " = "
	}

	bitwiseAssignOp := assignStmt.Tok == token.AND_ASSIGN ||
		assignStmt.Tok == token.OR_ASSIGN ||
		assignStmt.Tok == token.XOR_ASSIGN ||
		assignStmt.Tok == token.SHL_ASSIGN ||
		assignStmt.Tok == token.SHR_ASSIGN ||
		assignStmt.Tok == token.AND_NOT_ASSIGN

	if tupleResult || lhsLen == reassignedCount || lhsLen == declaredCount && !anyTypeIsString && !anyTypeIsInt {
		leftExprs := HashSet[string]{}

		// Handle LHS
		if declaredCount > 0 {
			if declaredCount > 1 || v.options.preferVarDecl {
				isDiscarded := lhsLen == 1 && getIdentifier(lhsExprs[0]).Name == "_"

				if !isDiscarded {
					result.WriteString("var ")
				}
			} else {
				ident := getIdentifier(lhsExprs[0])
				lhsType := convertToCSTypeName(v.getExprTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}
		}

		if lhsLen > 1 {
			result.WriteRune('(')
		}

		lambdaContext := DefaultLambdaContext()
		lambdaContext.isAssignment = true

		for i, lhs := range lhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])
			context := DefaultIdentContext()

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				// If rhs is a address of expression, we need to convert identifier to its pointer variable
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok {
					if unaryExpr.Op == token.AND {
						context.isPointer = true
					}
				}
			}

			lhsExpr := v.convExpr(lhs, []ExprContext{context, lambdaContext})
			leftExprs.Add(lhsExpr)

			result.WriteString(lhsExpr)
		}

		if lhsLen > 1 {
			result.WriteRune(')')
		}

		result.WriteString(operator)

		// Handle RHS
		if rhsLen > 1 {
			result.WriteRune('(')
		}

		for i, rhs := range rhsExprs {
			var lhs ast.Expr

			if i < lhsLen {
				lhs = lhsExprs[i]
			} else {
				lhs = lhsExprs[lhsLen-1]
			}

			ident := getIdentifier(lhs)

			if i > 0 {
				result.WriteString(", ")
			}

			contexts := []ExprContext{lambdaContext}

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				if v.isMethodValue(selectorExpr, false) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations
					if decls := v.generateCaptureDeclarations(); decls != "" {
						result.WriteString(decls)
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			if tupleResult {
				tupleResultContext := DefaultUnaryExprContext()
				tupleResultContext.isTupleResult = tupleResult
				contexts = append(contexts, tupleResultContext)
			}

			rhsExpr := v.convExpr(rhs, contexts)

			var binaryTypeName string

			if bitwiseAssignOp {
				binaryType := v.info.Types[rhs].Type

				if binaryType != nil {
					binaryTypeName = convertToCSTypeName(v.getTypeName(binaryType, false))
				}
			}

			if len(binaryTypeName) > 0 {
				result.WriteString(fmt.Sprintf("(%s)(", binaryTypeName))
			}

			if lhsTypeIsInterface[i] {
				result.WriteString(v.convertExprToInterfaceType(lhsExprs[i], rhs, rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}

			if len(binaryTypeName) > 0 {
				result.WriteRune(')')
			}
		}

		if rhsLen > 1 {
			result.WriteRune(')')
		}

		if format.includeSemiColon {
			result.WriteRune(';')
		}

		if len(leftExprs) > 0 && operator == " = " {
			for _, leftExpr := range leftExprs.Keys() {
				if strings.HasPrefix(leftExpr, AddressPrefix) {
					// This is a special case for pointer reassignments which should be extended
					// to also update local deference variable as well, e.g.: `x = ref Ꮡx.val`
					derefExpr := getSanitizedIdentifier(leftExpr[len(AddressPrefix):])
					result.WriteString(fmt.Sprintf(" %s = ref %s.val;", derefExpr, leftExpr))
				}
			}
		}

	} else {
		// Some variables are declared and some are reassigned, or one of the types is a string or integer
		for i := 0; i < lhsLen; i++ {
			lhs := lhsExprs[i]

			var rhs ast.Expr

			if i < rhsLen {
				rhs = rhsExprs[i]
			} else {
				rhs = rhsExprs[rhsLen-1]
			}

			lambdaContext := DefaultLambdaContext()
			lambdaContext.isAssignment = true

			if i > 0 {
				if format.useNewLine {
					result.WriteString(v.newline)
				}

				if format.useIndent {
					result.WriteString(v.indent(v.indentLevel))
				}
			}

			contexts := []ExprContext{lambdaContext}
			ident := getIdentifier(lhs)

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				if v.isMethodValue(selectorExpr, false) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations
					if decls := v.generateCaptureDeclarations(); decls != "" {
						result.WriteString(decls)
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			if ident == nil {
				if _, ok := lhs.(*ast.StarExpr); ok {
					starExprContext := DefaultStarExprContext()
					starExprContext.inLhsAssign = true
					contexts = append(contexts, starExprContext)
				}

				result.WriteString(v.convExpr(lhs, contexts))
				result.WriteString(operator)

				rhsExpr := v.convExpr(rhs, contexts)

				if lhsTypeIsInterface[i] {
					result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
				} else {
					result.WriteString(rhsExpr)
				}

				result.WriteRune(';')
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, contexts)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					result.WriteRune(';')
				} else if lhsTypeIsString[i] {
					// Handle string variables
					result.WriteString("@string ")
					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)
					result.WriteString(v.convExpr(rhs, contexts))
					result.WriteRune(';')
				} else {
					// Check if the variable needs to be allocated on the heap
					heapTypeDecl := v.convertToHeapTypeDecl(ident, false)

					if len(heapTypeDecl) > 0 {
						if format.heapTypeDeclTarget == nil {
							result.WriteString(heapTypeDecl)
							result.WriteString(v.newline)
							result.WriteString(v.indent(v.indentLevel))
						} else {
							format.heapTypeDeclTarget.WriteString(heapTypeDecl)
						}
					} else {
						if v.options.preferVarDecl && !lhsTypeIsInt[i] {
							isDiscarded := lhsLen == 1 && getIdentifier(lhsExprs[0]).Name == "_"

							if !isDiscarded {
								result.WriteString("var ")
							}
						} else {
							lhsType := convertToCSTypeName(v.getExprTypeName(ident, false))
							result.WriteString(lhsType)
							result.WriteRune(' ')
						}
					}

					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, contexts)

					_, rhsIsTypeAssert := rhs.(*ast.TypeAssertExpr)

					if lhsTypeIsInterface[i] && !rhsIsTypeAssert {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				}
			}
		}
	}

	if format.useNewLine {
		v.targetFile.WriteString(v.newline)
	}

	if format.useIndent {
		v.targetFile.WriteString(v.indent(v.indentLevel))
	}

	v.targetFile.WriteString(result.String())
}
