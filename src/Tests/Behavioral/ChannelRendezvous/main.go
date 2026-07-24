// Real unbuffered (rendezvous) channel semantics — the core of the channels redesign.
// make(chan T) must have cap 0 / len 0 and a send must not complete without a receiver:
// under the old buffered-1 conflation, the first select below took the send case and cap
// printed 1. Deterministic: the probe selects run while NO other goroutine exists, and
// every cross-goroutine interaction is ordered by the channel operations themselves (no
// sleeps, no timing).
package main

import "fmt"

func main() {
	ch := make(chan int)
	fmt.Println("cap:", cap(ch), "len:", len(ch))

	// No receiver exists (no goroutine has been started): an unbuffered send can never
	// be ready, so the default must be taken.
	select {
	case ch <- 1:
		fmt.Println("send: ready (wrong for unbuffered)")
	default:
		fmt.Println("send: not ready (no receiver)")
	}

	// No sender exists either: receive is not ready.
	select {
	case v := <-ch:
		fmt.Println("recv: ready (wrong):", v)
	default:
		fmt.Println("recv: not ready (no sender)")
	}

	fmt.Println("after probes: len:", len(ch), "cap:", cap(ch))

	// Real rendezvous round-trip: the goroutine parks in <-ch until main's send pairs
	// with it; reply orders the observation.
	reply := make(chan int)
	go func() {
		v := <-ch
		reply <- v * 2
	}()
	ch <- 21
	fmt.Println("reply:", <-reply)
	fmt.Println("after rendezvous: len:", len(ch), "cap:", cap(ch))

	// Strict ping-pong alternation is forced by rendezvous pairing.
	ping := make(chan int)
	pong := make(chan int)
	go func() {
		for i := 0; i < 3; i++ {
			v := <-ping
			pong <- v + 1
		}
	}()
	for i := 0; i < 3; i++ {
		ping <- i * 10
		fmt.Println("pong:", <-pong)
	}
}
