// Guards a methodless named func type whose parameter references a SUBPACKAGE type — an import path
// with a slash, e.g. sync/atomic. When the func type collapses to its base C# delegate, the parameter
// type is rendered from the signature's t.String() (`func(*sync/atomic.Int32) int32`), which lost the
// file import alias. The subpackage path `sync/atomic` must resolve to the package CLASS
// `sync.atomic_package`, NOT the namespace-dotted `sync.atomic` (CS0234 — `atomic` is not a namespace
// of `go.sync`). Same shape as path/filepath's WalkDirFunc referencing io/fs.DirEntry.
package main

import (
	"fmt"
	"sync/atomic"
)

// methodless named func type with a subpackage-type (*atomic.Int32) parameter.
type applyFunc func(c *atomic.Int32) int32

func run(f applyFunc, c *atomic.Int32) int32 {
	return f(c)
}

func main() {
	var c atomic.Int32
	c.Store(5)
	fmt.Println(run(func(c *atomic.Int32) int32 { return c.Load() }, &c)) // 5
	c.Add(10)
	fmt.Println(run(func(c *atomic.Int32) int32 { return c.Load() }, &c)) // 15
}
