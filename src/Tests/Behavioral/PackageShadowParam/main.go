// Regression test: a function parameter whose name equals an imported package the function
// references — crypto/rsa's `func emsaPSSEncode(…, hash hash.Hash)`, where the param `hash` shadows
// the `hash` package named in the signature type `hash.Hash`.
//
// The parameter name collides with the `using <pkg> = <pkg>_package;` alias, so C#'s whole-block
// scoping would bind the alias to the parameter. The variable analysis renames the parameter (and
// every usage) with the shadow marker — `io` → `ioΔ1`. The parameter DECLARATION must emit the
// renamed name too; it previously kept the raw name (`nint io`) while its uses rendered `ioΔ1`, so
// every use was CS0103 ("the name 'ioΔ1' does not exist") — 40 such sites in crypto/rsa's `hash`,
// 27 in testing/quick's `rand`.
package main

import (
	"fmt"
	"io"
)

// total's first parameter `io` shadows the imported `io` package, which the signature references via
// the second parameter's type `io.Writer`. The declaration and all uses must agree on the renamed
// identifier (`ioΔ1`). The `io.Writer` parameter is unused here; it exists only to reference the
// package so the shadow is triggered.
func total(io int, w io.Writer) int {
	sum := 0
	for i := 0; i < io; i++ {
		sum += io
	}
	_ = w
	return sum
}

func main() {
	fmt.Println(total(4, nil))
}
