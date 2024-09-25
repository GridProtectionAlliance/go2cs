package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr) string {
	leftOperand := v.convExpr(binaryExpr.X)
	binaryOp := binaryExpr.Op.String()
	rightOperand := v.convExpr(binaryExpr.Y)

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
