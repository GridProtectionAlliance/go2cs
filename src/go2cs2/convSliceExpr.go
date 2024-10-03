package main

import (
	"go/ast"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	ident := v.convExpr(sliceExpr.X, nil)

	// sliceExpr[:] => sliceExpr[..]
	if sliceExpr.Low == nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[..]"
	}

	// sliceExpr[Low:] => sliceExpr[Low..]
	if sliceExpr.Low != nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[" + v.convExpr(sliceExpr.Low, nil) + "..]"
	}

	// sliceExpr[:High] => sliceExpr[..High]
	if sliceExpr.Low == nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[.." + v.convExpr(sliceExpr.High, nil) + "]"
	}

	// sliceExpr[Low:High] => sliceExpr[Low..High]
	if sliceExpr.Low != nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[" + v.convExpr(sliceExpr.Low, nil) + ".." + v.convExpr(sliceExpr.High, nil) + "]"
	}

	// sliceExpr[:High:Max] => sliceExpr.slice(-1, High, Max)
	if sliceExpr.Low == nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(-1, " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	// sliceExpr[Low:High:Max] => sliceExpr.slice(Low, High, Max)
	if sliceExpr.Low != nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(" + v.convExpr(sliceExpr.Low, nil) + ", " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	return v.getPrintedNode(sliceExpr)
}
