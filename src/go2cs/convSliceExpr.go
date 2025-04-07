package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	ident := v.convExpr(sliceExpr.X, nil)

	// When converting a pointer expression to a slice, we use special handling
	if isMatch, ptrType := isPointerCast(ident); isMatch && v.inFunction && sliceExpr.High != nil {
		v.useUnsafeFunc = true
		prefixLength := len(ptrType) + 5

		// Remove array type prefix from the pointer type, if present
		if strings.HasPrefix(ptrType, "array<") {
			ptrRunes := []rune(ptrType)
			ptrType = string(ptrRunes[6 : len(ptrRunes)-1])
		}

		csPtrType := ptrType

		for isMatch, ptrPtrType := isPointerExpr(ptrType); isMatch; {
			csPtrType = ptrPtrType + "*"
			prefixLength--
			isMatch = false
		}

		identRunes := []rune(ident)
		return fmt.Sprintf("new Span<%s>((%s*)%s, %s)", ptrType, csPtrType, string(identRunes[prefixLength:]), v.convExpr(sliceExpr.High, nil))
	}

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

func isPointerCast(expr string) (bool, string) {
	if strings.HasPrefix(expr, "(") {
		runes := []rune(expr)

		if isMatch, ptrType := isPointerExpr(string(runes[1:])); isMatch {
			return true, ptrType
		}
	}

	return false, ""
}

func isPointerExpr(expr string) (bool, string) {
	runes := []rune(expr)

	// Check if it starts with the expected prefix
	if len(runes) < 2 || string(runes[0:2]) != "ж<" {
		return false, ""
	}

	// Initialize variables
	bracketCount := 1 // We've already encountered one opening bracket
	startPos := 2     // Start after "ж<"

	// Scan through the runes to find the matching closing bracket
	for i := startPos; i < len(runes); i++ {
		if runes[i] == '<' {
			bracketCount++
		} else if runes[i] == '>' {
			bracketCount--

			if bracketCount == 0 {
				// Found the closing bracket for our initial '<'
				return true, string(runes[startPos:i])
			}
		}
	}

	return false, ""
}
