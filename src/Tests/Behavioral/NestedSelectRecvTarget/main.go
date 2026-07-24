// A receive case whose assignment TARGET expression runs ANOTHER select while the outer
// select's committed value is in flight: `case a[f()] = <-ch:` emits the guard
// `case N when ch.ꟷᐳ(out a[f()])`, and C# evaluates the out-argument target — running f()
// and its inner select — BEFORE the guard call consumes the outer commit. A single
// pending slot is destroyed by the inner select (outer value lost, or the NEXT buffered
// value stolen); the per-thread pending-frame STACK keeps the outer commit alive while
// the inner select pushes and pops its own frames. Single-goroutine deterministic (the
// one goroutine in scenario 4's helper is fully ordered by its rendezvous).
package main

import "fmt"

var a [4]int

var innerCh = make(chan int, 1)

// idxDefault runs an inner select-with-default (trySelect) in the middle of the outer
// guard's target-expression evaluation, then returns the index.
func idxDefault(i int) int {
	select {
	case v := <-innerCh:
		fmt.Println("  inner recv:", v)
	default:
		fmt.Println("  inner default")
	}
	return i
}

// idxBlocking runs an inner BLOCKING select (a ready rendezvous send pairs with it) in
// the middle of the outer guard's target-expression evaluation.
func idxBlocking(i int) int {
	ready := make(chan int)
	go func() { ready <- 100 + i }()
	select {
	case v := <-ready:
		fmt.Println("  inner blocking recv:", v)
	}
	return i
}

func main() {
	ch := make(chan int, 2)

	// 1: the inner select takes its default path (still pushes/pops nothing — the outer
	// frame must survive the inner trySelect ENTRY, which formerly cleared the slot).
	ch <- 42
	select {
	case a[idxDefault(0)] = <-ch:
		fmt.Println("outer fired: a[0] =", a[0], "len(ch) =", len(ch))
	}

	// 2: the inner select COMMITS a receive of its own — its frame stacks on top of the
	// outer commit and is popped by the inner guard before the outer guard runs.
	ch <- 43
	innerCh <- 7
	select {
	case a[idxDefault(1)] = <-ch:
		fmt.Println("outer fired: a[1] =", a[1], "len(ch) =", len(ch))
	}

	// 3: value-steal probe — two values buffered; the outer guard must deliver the
	// COMMITTED head value (44) and the next value (45) must still be in the channel.
	ch <- 44
	ch <- 45
	select {
	case a[idxDefault(2)] = <-ch:
		fmt.Println("outer fired: a[2] =", a[2], "len(ch) =", len(ch), "next =", <-ch)
	}

	// 4: an inner BLOCKING select nested in the target expression.
	ch <- 46
	select {
	case a[idxBlocking(3)] = <-ch:
		fmt.Println("outer fired: a[3] =", a[3], "len(ch) =", len(ch))
	}

	// 5: default-form OUTER select (trySelect) with the nested inner select in its
	// target expression.
	ch <- 47
	select {
	case a[idxDefault(0)] = <-ch:
		fmt.Println("outer default-form fired: a[0] =", a[0], "len(ch) =", len(ch))
	default:
		fmt.Println("outer default (wrong)")
	}
}
