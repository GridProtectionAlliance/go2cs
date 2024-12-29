package main

import (
	"fmt"
	"go/ast"
	"go/token"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	ident := v.convExpr(sliceExpr.X, nil)

	// sliceExpr[:] => sliceExpr[..]
	if sliceExpr.Low == nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[..]"
	}

	// sliceExpr[Low:] => sliceExpr[Low..]
	if sliceExpr.Low != nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + "..]"
	}

	// sliceExpr[:High] => sliceExpr[..High]
	if sliceExpr.Low == nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[.." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[Low:High] => sliceExpr[Low..High]
	if sliceExpr.Low != nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + ".." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[:High:Max] => sliceExpr.slice(-1, High, Max)
	if sliceExpr.Low == nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(-1, " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	// sliceExpr[Low:High:Max] => sliceExpr.slice(Low, High, Max)
	if sliceExpr.Low != nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(" + v.convExpr(sliceExpr.Low, nil) + ", " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	expr := v.getPrintedNode(sliceExpr)
	println(fmt.Sprintf("WARNING: @convSliceEpr - Failed to convert `ast.SliceExpr` format %s", expr))
	return fmt.Sprintf("/* %s */", expr)
}

func (v *Visitor) getRangeIndexer(expr ast.Expr) string {
	if isIntegerLiteral(expr) {
		return v.convExpr(expr, nil)
	}

	return fmt.Sprintf("(int)(%s)", v.convExpr(expr, nil))
}

func isIntegerLiteral(expr ast.Expr) bool {
	if basicLit, ok := expr.(*ast.BasicLit); ok {
		if basicLit.Kind == token.INT {
			return true
		}
	}

	return false
}
