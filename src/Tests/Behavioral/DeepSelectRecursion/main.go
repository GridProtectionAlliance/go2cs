// ONE textual select whose guard out-target expression RECURSES: at depth n there are n
// LIVE pending receive commits on this thread — live frame depth equals DYNAMIC recursion
// depth, not textual select nesting. This is the counter-example that falsified the
// depth-64 pending-frame cap-and-drop: under a cap, the pushes beyond it silently dropped
// the OLDEST — live — frames, and on unwind those selects' guards found an empty stack,
// probed a drained channel, and fired ZERO cases (sum 2080 = 1+..+64 instead of 5050,
// silently, in Debug and Release alike). Live frames are inherently bounded by the call
// stack; the pending stack must never cap. Single-goroutine deterministic.
package main

import "fmt"

var sink [101]int

// f commits a receive on its OWN ready channel, then the guard's out-target index
// expression recurses through rec BEFORE the commit is consumed.
func f(n int) {
	if n == 0 {
		return
	}
	ch := make(chan int, 1)
	ch <- n
	select {
	case sink[rec(n)] = <-ch:
	}
}

// rec recurses to the next level (stacking another live pending commit), then returns n
// as the index — so each level stores its own committed value in its own slot.
func rec(n int) int {
	f(n - 1)
	return n
}

func main() {
	f(100)

	sum := 0
	for _, v := range sink {
		sum += v
	}

	fmt.Println("sum:", sum)
}
