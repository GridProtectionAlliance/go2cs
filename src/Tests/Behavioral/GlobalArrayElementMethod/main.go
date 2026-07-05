// Regression test: a package-level value ARRAY whose element has a POINTER-receiver method
// called on it (`pool[i].inc()`) needs the array heap-boxed so the element address
// `Ꮡpool.at<T>(i)` resolves. collectAddressedGlobals peeled value FIELD selectors to the
// receiver root but not value ARRAY INDEX hops, so `matchPool[i].Get()` (regexp's `[N]sync.Pool`)
// bailed before reaching the `matchPool` ident — the array was never boxed and `Ꮡmatchpool`
// did not exist (CS0103). Adding the IndexExpr hop boxes the array; the mutation through the
// pointer-receiver method then persists (the box backs the original storage).
package main

import "fmt"

type counter struct{ n int }

func (c *counter) inc() { c.n++ } // POINTER receiver — the call needs the element's address

// package-level value array; pool[i].inc() takes &pool[i] implicitly
var pool [3]counter

func main() {
	pool[0].inc()
	pool[0].inc()
	pool[1].inc()
	fmt.Println(pool[0].n, pool[1].n, pool[2].n) // 2 1 0
}
