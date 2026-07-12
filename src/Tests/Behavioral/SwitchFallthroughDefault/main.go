package main

import "fmt"

// classify exercises a switch with BOTH a fallthrough and a trailing default. The bug: the converter
// emitted the default as a bare `else` of the fallthrough-target case's `if`, so a matched
// NON-fallthrough case (n==1) ran its body AND the default. Go runs the default only when no case
// matches. (This is the exact shape of fmt's printValue: `case Int: …; case Ptr: fallthrough; case
// Chan,Func: fmtPointer; default: unknownType` — an int element wrongly hit unknownType.)
func classify(n int) string {
	var out string
	switch n {
	case 1:
		out = "one"
	case 2:
		out = "two"
		fallthrough
	case 3:
		out += "-three"
	default:
		out += "-other"
	}
	return out
}

func main() {
	fmt.Println(classify(1)) // one        (NOT "one-other")
	fmt.Println(classify(2)) // two-three  (fallthrough)
	fmt.Println(classify(3)) // -three
	fmt.Println(classify(9)) // -other     (default)
}
