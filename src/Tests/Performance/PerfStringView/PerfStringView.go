package main

import (
	"fmt"
	"time"
)

// Keyword checks — `string(buf) == "literal"` over a runtime-built buffer — the exact idiom the sstring
// stack-string emission optimizes, in its most common stdlib form (encoding/json testing a decoded token
// against "null"/"true"/"false", protocol sniffs, etc.). With @string each comparison ALLOCATES the
// literal as an @string (a byte[] copy that neither the JIT nor Native AOT elides, because it is consumed
// by the comparison); the converter instead emits a zero-copy sstring view compared against the u8 literal
// span in place — no allocation. This benchmark tracks that win: it runs many times faster than the
// equivalent @string code (~12x on JIT, ~11x on AOT in isolation), and far closer to Go than the String
// benchmark (whose conversions are ineligible and stay @string, at 7-9x). A runtime-built buffer keeps the
// compiler from constant-folding the loop.
func run(n int) int {
	total := 0
	buf := keyword() // runtime-built "null", never mutated -> sstring-eligible

	for i := 0; i < n; i++ {
		if string(buf) == "null" {
			total++
		}
		if string(buf) == "true" {
			total += 2
		}
		if string(buf) == "false" {
			total += 4
		}
	}

	return total
}

func keyword() []byte {
	src := "null"
	b := make([]byte, len(src))
	for i := 0; i < len(src); i++ {
		b[i] = src[i]
	}
	return b
}

func main() {
	start := time.Now().UnixNano()

	total := run(20000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
