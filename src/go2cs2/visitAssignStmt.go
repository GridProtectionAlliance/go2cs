package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, source ParentBlockContext, format FormattingContext) {
	result := &strings.Builder{}

	lhsExprs := assignStmt.Lhs
	rhsExprs := assignStmt.Rhs

	lhsLen := len(lhsExprs)
	rhsLen := len(rhsExprs)

	parentBlock := source.parentBlock
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
	var tupleTarget bool

	if rhsLen == 1 {
		if callExpr, ok := rhsExprs[0].(*ast.CallExpr); ok {
			funType := v.info.TypeOf(callExpr.Fun)

			if signature, ok := funType.(*types.Signature); ok {
				results := signature.Results()

				if results != nil {
					tupleTarget = results.Len() > 1
				}
			}
		}
	}

	// Count the number of reassigned and declared variables
	for i, lhs := range lhsExprs {
		ident := getIdentifier(lhs)

		if ident == nil {
			// Check if lhs is a struct member
			if selectorExpr, ok := lhs.(*ast.SelectorExpr); ok {
				ident = getIdentifier(selectorExpr.Sel)
				lhsTypeIsInterface[i] = v.isInterface(ident)

				var rhs ast.Expr

				// Check if rhs is a basic literal
				if i < rhsLen {
					rhs = rhsExprs[i]
				} else {
					rhs = rhsExprs[rhsLen-1]
				}

				// Check if rhs result is a tuple
				if callExpr, ok := rhs.(*ast.CallExpr); ok {
					resultType := v.info.TypeOf(callExpr.Fun)

					if resultType != nil {
						_, tupleTarget = resultType.(*types.Tuple)
					}
				}

				if _, ok := rhs.(*ast.BasicLit); ok {
					typeName := v.getTypeName(ident, true)

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
		} else {
			if v.isReassignment(ident) {
				reassignedCount++
			} else {
				// Perform escape analysis to determine if the variable needs heap allocation
				v.performEscapeAnalysis(ident, parentBlock)

				if !v.identEscapesHeap[ident] {
					declaredCount++
				}
			}

			lhsTypeIsInterface[i] = v.isInterface(ident)

			var rhs ast.Expr

			// Check if rhs is a basic literal
			if i < rhsLen {
				rhs = rhsExprs[i]
			} else {
				rhs = rhsExprs[rhsLen-1]
			}

			if _, ok := rhs.(*ast.BasicLit); ok {
				typeName := v.getTypeName(ident, true)

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

	if tupleTarget || lhsLen == reassignedCount || lhsLen == declaredCount && !anyTypeIsString && !anyTypeIsInt {
		// Handle LHS
		if declaredCount > 0 {
			if declaredCount > 1 || v.options.preferVarDecl {
				result.WriteString("var ")
			} else {
				ident := getIdentifier(lhsExprs[0])
				lhsType := convertToCSTypeName(v.getTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}
		}

		if lhsLen > 1 {
			result.WriteRune('(')
		}

		for i, lhs := range lhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				// If rhs is a address of expression, we need to convert identifier to its pointer variable
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok {
					if unaryExpr.Op == token.AND {
						result.WriteString(AddressPrefix)
					}
				}
			}

			result.WriteString(v.convExpr(lhs, nil))
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
			if i > 0 {
				result.WriteString(", ")
			}

			rhsExpr := v.convExpr(rhs, nil)

			if lhsTypeIsInterface[i] {
				result.WriteString(v.convertToInterfaceType(lhsExprs[i], rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}
		}

		if rhsLen > 1 {
			result.WriteRune(')')
		}

		if format.includeSemiColon {
			result.WriteRune(';')
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

			if i > 0 {
				if format.useNewLine {
					result.WriteString(v.newline)
				}

				if format.useIndent {
					result.WriteString(v.indent(v.indentLevel))
				}
			}

			ident := getIdentifier(lhs)

			if ident == nil {
				result.WriteString(v.convExpr(lhs, nil))
				result.WriteString(operator)

				rhsExpr := v.convExpr(rhs, nil)

				if lhsTypeIsInterface[i] {
					result.WriteString(v.convertToInterfaceType(lhs, rhsExpr))
				} else {
					result.WriteString(rhsExpr)
				}

				result.WriteRune(';')
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, nil)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertToInterfaceType(lhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					result.WriteRune(';')
				} else if lhsTypeIsString[i] {
					// Handle string variables
					result.WriteString("@string ")
					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(operator)
					result.WriteString(v.convExpr(rhs, nil))
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
							result.WriteString("var ")
						} else {
							lhsType := convertToCSTypeName(v.getTypeName(ident, false))
							result.WriteString(lhsType)
							result.WriteRune(' ')
						}
					}

					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, nil)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertToInterfaceType(lhs, rhsExpr))
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
