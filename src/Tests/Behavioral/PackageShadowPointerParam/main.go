// Regression test: a POINTER parameter (and a value parameter in the same function) whose name
// shadows an imported package — the dominant pointer-param-box root from crypto/rsa and testing/quick.
//
// A pointer parameter `io *box` that shadows the `io` package is boxed as `Ꮡio` and deref-aliased
// (`ref var ioΔ1 = ref Ꮡio.Value`). The box keeps the RAW name `Ꮡio`, so passing the pointer must
// emit `Ꮡio`, not `ᏑioΔ1` (Ꮡ + the renamed value alias) — CS0103 otherwise (facet A, boxBaseName).
// A function that has a pointer parameter renders its whole signature through a rebuilt-signature
// path; a *value* parameter that shadows a package there must also emit its renamed name at the
// declaration, or the decl stays raw while its uses are renamed — CS0103 (facet B).
package main

import (
	"fmt"
	"io"
)

type box struct{ n int }

func helper(b *box) int { return b.n }

// consume's pointer param `io` shadows the `io` package (referenced by the `w io.Writer` param).
// Passing it to helper must use the box `Ꮡio` (facet A); `io.n` reads through the deref alias `ioΔ1`.
func consume(io *box, w io.Writer) int {
	_ = w
	return io.n + helper(io)
}

// combine's value param `io` (io.Writer) shadows the `io` package; the pointer param `p` forces the
// rebuilt-signature path, so `io`'s declaration must be renamed to match its use (facet B).
func combine(io io.Writer, p *box) int {
	_ = io
	return p.n * 2
}

func main() {
	b := box{n: 7}
	fmt.Println(consume(&b, nil))
	fmt.Println(combine(nil, &b))
}
