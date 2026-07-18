package main

import "fmt"

// Locks in Go's recoverable makeslice panics for `make([]T, len[, cap])`: a negative or
// over-allocatable length/capacity panics "runtime error: makeslice: len/cap out of range",
// which recover() must observe. The golib slice length constructor previously raised
// ArgumentOutOfRangeException / OverflowException — unrecoverable .NET exceptions that killed
// the process instead of reaching the deferred recover (strings/bytes TestRepeatCatchesOverflow
// died this way through internal/bytealg.MakeNoZero, the same validation class).

//go:noinline
func tryMake(length, capacity int) (r any) {
	defer func() {
		r = recover()
	}()

	var b []byte

	if capacity < 0 {
		b = make([]byte, length)
	} else {
		b = make([]byte, length, capacity)
	}

	_ = b

	return nil
}

func main() {
	fmt.Println(tryMake(4, -1))       // in range: no panic
	fmt.Println(tryMake(0, -1))       // zero length: no panic
	fmt.Println(tryMake(-1, -1))      // negative length
	fmt.Println(tryMake(1<<62, -1))   // over-allocatable length (64-bit)
	fmt.Println(tryMake(1, 1<<62))    // over-allocatable capacity (64-bit)
}
