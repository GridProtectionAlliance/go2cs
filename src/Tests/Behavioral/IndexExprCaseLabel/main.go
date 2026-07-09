package main

import "fmt"

var words = [3]string{"alpha", "beta", "gamma"}

const one = 1

// A case label that is an INDEX into a package-level array var is not a constant:
// both the single-label and multi-label clauses must fall back to equality compares
// (a C# `is` pattern would parse `words[one]` as an array type).
func classify(s string) string {
	switch s {
	case words[0], words[2]:
		return "ends"
	case words[one]:
		return "middle"
	default:
		return "none"
	}
}

func main() {
	fmt.Println(classify("alpha"))
	fmt.Println(classify("beta"))
	fmt.Println(classify("gamma"))
	fmt.Println(classify("delta"))
}
