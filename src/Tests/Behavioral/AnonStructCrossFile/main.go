// Ranges over package-level anonymous-struct slices declared in SIBLING files, taking
// the loop variable's address so its per-iteration heap box must name the LIFTED struct
// type (`ref var tt = ref heap(new compareTestsᴛ1(), …)`) — the type-name render cannot
// fall back to raw Go type text. compareTests (zvars.go) exercises the deferred-marker
// path (declaring file visited after this one); sizeTests (avars.go) the direct
// registry-hit path (declaring file visited first).
package main

import "fmt"

func main() {
	for _, tt := range compareTests {
		p := &tt
		fmt.Println(string(p.a), string(p.b), p.i)
	}

	for _, st := range sizeTests {
		q := &st
		fmt.Println(q.name, q.want)
	}
}
