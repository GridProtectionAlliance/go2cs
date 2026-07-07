// z_main.go sorts AFTER a_boxed.go and is the ONLY file that imports CrossPkgLib — so, without the
// package-level preload, CrossPkgLib's Δ-renamed-type alias loads only here, too late for a_boxed.go.
package main

import (
	"fmt"

	"CrossPkgLib"
)

func main() {
	// Read back through the SAME pointer the transitive box handed out (Ꮡ(value) boxes a copy, so the
	// value must be read through the returned pointer, not an original).
	p := peekPtr(7).(*CrossPkgLib.Status)
	fmt.Println(p.Code)                          // 7
	fmt.Println(float64(CrossPkgLib.Freezing())) // 0
}
