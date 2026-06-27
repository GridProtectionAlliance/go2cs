package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext) string {
	lhsType := v.getExprType(binaryExpr.X)
	rhsType := v.getExprType(binaryExpr.Y)

	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()
	basicLitContext.u8StringOK = v.isStringType(binaryExpr.X) && v.isStringType(binaryExpr.Y)

	rhsIsPointer := isPointer(rhsType)
	identContext.isPointer = rhsIsPointer
	leftOperand := v.convExpr(binaryExpr.X, []ExprContext{identContext, basicLitContext})

	binaryOp := binaryExpr.Op.String()

	lhsIsPointer := isPointer(lhsType)
	identContext.isPointer = lhsIsPointer
	rightOperand := v.convExpr(binaryExpr.Y, []ExprContext{identContext, basicLitContext})

	if !context.usePattenMatch {
		// Check for comparisons between interface and pointer types,
		// dereferencing pointer type for the comparison if necessary
		if binaryOp == "==" || binaryOp == "!=" {
			lhsIsInterface, isEmpty := isInterface(lhsType)

			if lhsIsInterface && !isEmpty && rhsIsPointer {
				rightOperand = fmt.Sprintf("%s%s", PointerDerefOp, rightOperand)
			} else {
				rhsIsInterface, isEmpty := isInterface(rhsType)

				if rhsIsInterface && !isEmpty && lhsIsPointer {
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

			// Go's shift operators bind at the multiplicative level (tighter than `+`/`-`),
			// but C#'s `<<`/`>>` bind looser than the additive operators. So Go's
			// `x>>4 + x` (meaning `(x>>4) + x`) would re-associate in C# as `x >> (4 + x)`.
			// Parenthesize the shift so the original grouping is preserved in every context.
			shiftExpr := fmt.Sprintf("(%s %s %s)", leftOperand, binaryOp, rightOperand)

			// When the shifted (left) operand is an untyped constant — `1 << k` — Go gives the
			// whole shift the type it assumes from context (e.g. uintptr when compared with a
			// uintptr), but the bare C# literal makes the result `int`, which then can't compare
			// or combine with the typed operand (CS0034). Cast the shift to its resolved type.
			if v.isUntypedNumericConstArg(binaryExpr.X) {
				if shiftType := v.info.Types[binaryExpr].Type; shiftType != nil {
					if basic, ok := shiftType.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 && basic.Info()&types.IsUntyped == 0 {
						shiftCSType := convertToCSTypeName(v.getTypeName(shiftType, false))

						if shiftCSType != "int" && shiftCSType != "nint" {
							shiftExpr = fmt.Sprintf("(%s)%s", shiftCSType, shiftExpr)
						}
					}
				}
			}

			return shiftExpr
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
