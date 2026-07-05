package main

import "fmt"

// sumTable mirrors image/jpeg writer.go's `"\x00\x10\x01\x11"u8[i]`: indexing a string LITERAL
// (which renders as a `"…"u8` ReadOnlySpan<byte>, whose indexer is int-ONLY) with a loop `int`
// variable. Go's `int` maps to C# `nint`, which does NOT implicitly narrow to int, so the
// converter must cast the literal's index (CS1503). A constant index stays uncast (C# converts
// the literal implicitly).
func sumTable(n int) int {
	total := 0
	for i := 0; i < n; i++ {
		total += int("\x01\x02\x03\x04"[i]) // string LITERAL indexed by a loop int
	}
	return total
}

func main() {
	fmt.Println(sumTable(4))   // 1+2+3+4 = 10
	fmt.Println("abcd"[2])     // 99 ('c') — constant index stays uncast
}
