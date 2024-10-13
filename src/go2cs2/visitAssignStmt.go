package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, source ParentBlockContext, format FormattingContext) {
	result := &strings.Builder{}
	lhsLen := len(assignStmt.Lhs)
	rhsLen := len(assignStmt.Rhs)

	if lhsLen != rhsLen {
		println("WARNING: AssignStmt Lhs and Rhs lengths do not match")
		v.writeOutputLn("/* visitAssignStmt mismatch: " + v.getPrintedNode(assignStmt) + " */")
		return
	}

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

	// Count the number of reassigned and declared variables
	for i, lhs := range assignStmt.Lhs {
		ident := getIdentifier(lhs)

		if ident != nil {
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

	if lhsLen == reassignedCount || lhsLen == declaredCount && !anyTypeIsString && !anyTypeIsInt {
		// Handle LHS
		if declaredCount > 0 {
			if declaredCount > 1 || v.options.preferVarDecl {
				result.WriteString("var ")
			} else {
				ident := getIdentifier(assignStmt.Lhs[0])
				lhsType := convertToCSTypeName(v.getTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}
		}

		if lhsLen > 1 {
			result.WriteString("(")
		}

		for i, lhs := range assignStmt.Lhs {
			if i > 0 {
				result.WriteString(", ")
			}

			result.WriteString(v.convExpr(lhs, nil))
		}

		if lhsLen > 1 {
			result.WriteString(")")
		}

		result.WriteString(" = ")

		// Handle RHS
		if rhsLen > 1 {
			result.WriteString("(")
		}

		for i, rhs := range assignStmt.Rhs {
			if i > 0 {
				result.WriteString(", ")
			}

			rhsExpr := v.convExpr(rhs, nil)

			if lhsTypeIsInterface[i] {
				result.WriteString(v.convertToInterfaceType(assignStmt.Lhs[i], rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}
		}

		if rhsLen > 1 {
			result.WriteString(")")
		}

		if format.includeSemiColon {
			result.WriteString(";")
		}
	} else {
		// Some variables are declared and some are reassigned, or one of the types is a string or integer
		for i := 0; i < lhsLen; i++ {
			lhs := assignStmt.Lhs[i]
			rhs := assignStmt.Rhs[i]

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
				// Handle unexpected types of LHS expressions
				println("WARNING: Undetected AssignStmt Lhs identifier for: " + v.getPrintedNode(lhs))
				result.WriteString("// undetected identifier in 'visitAssignStmt':" + v.getPrintedNode(lhs))
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(" = ")

					rhsExpr := v.convExpr(rhs, nil)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertToInterfaceType(assignStmt.Lhs[i], rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					result.WriteString(";")
				} else if lhsTypeIsString[i] {
					// Handle string variables
					result.WriteString("@string ")
					result.WriteString(v.convExpr(lhs, nil))
					result.WriteString(" = ")
					result.WriteString(v.convExpr(rhs, nil))
					result.WriteString(";")
				} else {
					// Check if the variable needs to be allocated on the heap
					heapTypeDecl := v.convertToHeapTypeDecl(ident)

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
					result.WriteString(" = ")

					rhsExpr := v.convExpr(rhs, nil)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertToInterfaceType(assignStmt.Lhs[i], rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteString(";")
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
