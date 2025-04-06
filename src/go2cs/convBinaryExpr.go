package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext) string {
	lhsType := v.getExprType(binaryExpr.X)
	rhsType := v.getExprType(binaryExpr.Y)

	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()
	basicLitContext.u8StringOK = v.isStringType(binaryExpr.X) && v.isStringType(binaryExpr.Y)

	identContext.isPointer = isPointer(rhsType)
	leftOperand := v.convExpr(binaryExpr.X, []ExprContext{identContext, basicLitContext})

	binaryOp := binaryExpr.Op.String()

	identContext.isPointer = isPointer(lhsType)
	rightOperand := v.convExpr(binaryExpr.Y, []ExprContext{identContext, basicLitContext})

	if !context.usePattenMatch {
		// Check for comparisons between interface and pointer types,
		// dereferencing pointer type for the comparison if necessary
		if binaryOp == "==" || binaryOp == "!=" {
			lhsIsInterface, isEmpty := isInterface(lhsType)

			if lhsIsInterface && !isEmpty && isPointer(rhsType) {
				rightOperand = fmt.Sprintf("%s%s", PointerDerefOp, rightOperand)
			} else {
				rhsIsInterface, isEmpty := isInterface(rhsType)

				if rhsIsInterface && !isEmpty && isPointer(lhsType) {
					leftOperand = fmt.Sprintf("%s%s", PointerDerefOp, leftOperand)
				}

				if lhsIsInterface && rhsIsInterface {
					// Handle interface comparison with special runtime function
					if binaryOp == "==" {
						return fmt.Sprintf("AreEqual(%s, %s)", leftOperand, rightOperand)
					} else {
						return fmt.Sprintf("!AreEqual(%s, %s)", leftOperand, rightOperand)
					}
				}
			}
		} else if binaryOp == "<<" || binaryOp == ">>" {
			rightOperand = fmt.Sprintf("(int)(%s)", rightOperand)
		}

		bitwiseOp := binaryOp == "&" || binaryOp == "|" || binaryOp == "^" || binaryOp == "&^"

		if binaryOp == "&^" {
			binaryOp = " & ~"
		} else {
			binaryOp = " " + binaryOp + " "
		}

		// bitwise operations need to be in parentheses in C# and usually case to target type
		if bitwiseOp {
			binaryType := v.info.Types[binaryExpr].Type
			var binaryTypeName string

			if binaryType != nil {
				binaryTypeName = convertToCSTypeName(v.getTypeName(binaryType, false))
			}

			return fmt.Sprintf("(%s)(%s%s%s)", binaryTypeName, leftOperand, binaryOp, rightOperand)
		}

		return fmt.Sprintf("%s%s%s", leftOperand, binaryOp, rightOperand)
	}

	// Handle start of pattern match expression with "is" keyword:
	if context.declareIsExpr {
		if binaryOp == "==" {
			binaryOp = ""
		} else {
			binaryOp = " " + binaryOp
		}

		return fmt.Sprintf("%s is%s %s", leftOperand, binaryOp, rightOperand)
	}

	// Handle remaining pattern match expressions:
	if binaryOp == "==" {
		binaryOp = ""
	} else {
		binaryOp = binaryOp + " "
	}

	return fmt.Sprintf("%s%s", binaryOp, rightOperand)
}
