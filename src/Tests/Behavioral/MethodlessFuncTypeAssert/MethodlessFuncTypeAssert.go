// Guards a TYPE ASSERTION whose target is a METHODLESS named func type — archive/zip's
// `ci.(Compressor)` where `type Compressor func(io.Writer) (io.WriteCloser, error)`. Such a type
// collapses to its base C# delegate everywhere it is referenced, so its NAME is never emitted; the
// type-assertion target must render the collapsed `Func<…>`, not the bare (undefined) name (CS0246).
package main

import "fmt"

type Compressor func(w int) int

func lookup(i any) Compressor {
	// i.(Compressor) — the type assertion whose target collapsed to a delegate.
	if c, ok := i.(Compressor); ok {
		return c
	}
	return nil
}

func main() {
	var i any = Compressor(func(w int) int { return w * 2 })
	c := lookup(i)
	fmt.Println(c(21)) // 42

	// a non-matching dynamic type asserts false.
	var j any = 7
	fmt.Println(lookup(j) == nil) // true
}
