package main

import "fmt"

// gch backs the nullary case below, which cannot take a channel parameter without becoming a
// param call.
var gch = make(chan int, 1)

// sum returns a value AND has a channel side effect. `go sum(out, 3, 4)` must wrap the call in a
// discarding Action lambda: goǃ only has void Action<...> overloads, so passing the bare method
// group (a Func<...>) is CS0407 "no overload matches the delegate". This mirrors x/net/nettest's
// `go chunkedCopy(c2, c2)` (chunkedCopy returns error), the defect this test guards.
func sum(out chan int, a, b int) int {
	r := a + b
	out <- r
	return r
}

// pair returns TWO values — guards the multi-result discard (the check is sig.Results().Len() > 0,
// not == 1).
func pair(out chan int, n int) (int, error) {
	out <- n * n
	return n * n, nil
}

// nib is a NULLARY value-returning callee — exercises the paramCount == 0 arm, which wraps the
// invocation as `goǃ(() => nib())` rather than trimming to a bare (Func) method group.
func nib() int {
	gch <- 5
	return 5
}

// emit returns nothing — its `go emit(out)` keeps the bare method-group form (`goǃ(emit, out)`),
// the control proving void method groups are UNAFFECTED by the fix.
func emit(out chan int) {
	out <- 8
}

func main() {
	out := make(chan int)

	go sum(out, 3, 4) // value-returning, 3 params -> goǃ((ᴛ1, ᴛ2, ᴛ3) => sum(ᴛ1, ᴛ2, ᴛ3), out, 3, 4)
	fmt.Println("sum:", <-out)

	go pair(out, 6) // value-returning, 2 params, multi-result
	fmt.Println("pair:", <-out)

	go nib() // value-returning, nullary -> goǃ(() => nib())
	fmt.Println("nib:", <-gch)

	go func() { out <- 1 }() // control: func-literal callee, unchanged by the fix
	fmt.Println("lit:", <-out)

	go emit(out) // control: void method group, unchanged bare method group
	fmt.Println("emit:", <-out)
}
