// Go evaluates a select case's channel operand exactly ONCE, at select entry. The emitted
// C# formerly evaluated a receive case's operand TWICE — in the registration call and again
// as the winning guard's receiver — so a non-referentially-stable operand (a fresh-channel
// factory, a time.After-shaped call, or a bare identifier the guard's out-target expression
// reassigns) re-evaluated to a DIFFERENT channel: the runtime's (correct) pending-frame
// core match then refused delivery — a stolen value, a zero-case blocking select, or a
// Debug assert kill — and the factory's side effect ran twice. The converter now hoists
// every receive-case operand into a select-scoped temp (`var selᴛN = <expr>;`) used by both
// the registration and the guard. Deterministic: single-goroutine except S2's helper, which
// is fully ordered by its rendezvous.
package main

import "fmt"

var made int

func fresh() chan int {
	made++
	ch := make(chan int, 1)
	ch <- made * 10
	return ch
}

var afterCalls int

// after is the time.After shape: each call makes a NEW channel a background sender feeds.
func after() chan int {
	afterCalls++
	ch := make(chan int)
	go func() { ch <- 99 }()
	return ch
}

var sink [2]int

func swap(ch *chan int, repl chan int) int {
	*ch = repl
	return 0
}

func main() {
	// S1: ready call-expression operand — evaluated exactly once (poll-pass commit).
	select {
	case v := <-fresh():
		fmt.Println("S1 got:", v, "made =", made)
	}

	// S2: parked call-expression operand (rendezvous factory) — evaluated exactly once
	// (parked-wake commit).
	select {
	case v := <-after():
		fmt.Println("S2 got:", v, "afterCalls =", afterCalls)
	}

	// S3: bare-identifier operand REASSIGNED by the out-target expression — the committed
	// receive must come from the ORIGINAL channel (7), and repl must keep its value.
	ch := make(chan int, 1)
	ch <- 7
	repl := make(chan int, 1)
	repl <- 8
	select {
	case sink[swap(&ch, repl)] = <-ch:
		fmt.Println("S3 sink[0] =", sink[0], "len(ch) =", len(ch), "len(repl) =", len(repl))
	}

	// S4: default form (trySelect) with a call-expression operand — same single evaluation.
	select {
	case v := <-fresh():
		fmt.Println("S4 got:", v, "made =", made)
	default:
		fmt.Println("S4 default (wrong)")
	}
}
