package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context *PatternMatchExprContext) string {
	leftOperand := v.convExpr(binaryExpr.X, nil)
	binaryOp := binaryExpr.Op.String()
	rightOperand := v.convExpr(binaryExpr.Y, nil)

	if context == nil {
		if binaryOp == "<<" || binaryOp == ">>" {
			rightOperand = fmt.Sprintf("(int)(%s)", rightOperand)
		}

		if binaryOp == "&^" {
			binaryOp = " & ~"
		} else {
			binaryOp = " " + binaryOp + " "
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
