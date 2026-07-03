// Regression test: ranging (with index+value) over a pointer-to-array PARAMETER. A pointer param
// is implicitly dereferenced to a value local (`ref var buf = ref Ꮡbuf.Value`), so `buf` already
// denotes the array; the range conversion must not add a second `.Value` (`buf.Value` on the value
// array is CS1061, which also broke the foreach deconstruction inference, CS8130). This is the
// internal/chacha8rand `block_generic(buf *[32]uint64)` / `for i, x := range buf` pattern.
package main

import "fmt"

func swap(buf *[4]uint64) {
	for i, x := range buf { // pointer param -> deref'd value; no extra `.Value`
		buf[i] = x>>32 | x<<32
	}
}

func main() {
	b := [4]uint64{0x100000002, 0x300000004, 5, 0}
	swap(&b)
	fmt.Println(b[0], b[1], b[2], b[3])
}
