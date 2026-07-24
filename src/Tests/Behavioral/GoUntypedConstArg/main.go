// Guards the converter's untyped-numeric-constant argument cast for a `go` call that
// takes the LAMBDA form (a value-returning callee forces `goǃ(ᴛ1 => f(ᴛ1), arg)`). The
// arg's C# type drives ᴛ1's inference, so an untyped const feeding a WIDER/other parameter
// type must be cast to that PARAMETER type, not its default Go type — otherwise ᴛ1 infers
// the default (`nint`) and `f(ᴛ1)` fails (CS1503). This is hash/crc32's
// `go MakeTable(Castagnoli)` (Castagnoli an untyped uint32 poly). See convCallExpr.go's
// deferred/go untyped-numeric-const branch.
package main

import "fmt"

// Castagnoli-like untyped polynomial constant: default type is int, but it feeds a uint32
// parameter, so the emitted cast must target uint32 (not the default nint).
const poly = 0x82f63b78

var done = make(chan uint32, 1)

// compute RETURNS a value (forces the lambda form for `go compute(...)`) and reports its
// result over a buffered channel so main can observe it deterministically.
func compute(p uint32) uint32 {
	r := p ^ 0xffffffff
	done <- r
	return r
}

func main() {
	go compute(poly)
	stored := <-done
	fmt.Printf("%#x\n", stored)
}
