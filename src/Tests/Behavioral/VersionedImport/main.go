// Versioned-import-path guard: a package whose import path ends in a major-version
// segment (vlib/v2, like math/rand/v2) is named for the PARENT segment, so every
// converter reference must target the parent-named package class (vlib_package under
// namespace go.vlib), never a path-derived v2_package. Exercises the two renderers
// that mis-derived it (sort's test suite importing math/rand/v2): the struct-field
// display type (`ж<vlib.Rand>`, not `vlib.v2.Rand` — CS0426) and the recorded
// GoImplement pointer-implement pair for a *PCG → Source cast (`go.vlib.vlib_package.PCG`,
// not the nonexistent `go.vlib.v2_package.PCG` — CS0234).
package main

import (
	"fmt"

	"vlib/v2"
)

type wrapper struct {
	r *vlib.Rand
}

func main() {
	w := wrapper{r: vlib.New(vlib.NewPCG(42))}

	for i := 0; i < 3; i++ {
		fmt.Println(w.r.IntN(100))
	}
}
