package main

import "fmt"

// accumulate reproduces crypto/x509 buildChains' `considerCandidate` closure shape: a closure
// captures a `*int` PARAMETER and REASSIGNS it (`counter = new(int)`), then dereferences it. The
// converter models a pointer param as a box plus a deref-alias ref-local (`ref var counter =
// ref Ꮡcounter.Value`); reassigning it emits a box repoint PLUS a ref-local re-alias
// (`counter = ref Ꮡcounter.Value`). Inside a closure the captured value alias is an outer `ref`
// local, which C# forbids referencing (CS8175). The fix suppresses the re-alias inside a lambda
// (every in-lambda / post-lambda deref routes through the box). The `counter == nil` guard makes the
// reassignment reachable in Go (so it compiles to the CS8175 shape) while the runtime always passes a
// non-nil counter, keeping output deterministic.
func accumulate(counter *int, vals []int) int {
	add := func(v int) {
		if counter == nil {
			counter = new(int)
		}
		*counter += v
	}

	for _, v := range vals {
		add(v)
	}

	return *counter
}

func main() {
	n := 0
	fmt.Println(accumulate(&n, []int{1, 2, 3, 4}))
	fmt.Println(n)
}
