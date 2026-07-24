// A nil channel's case is NEVER ready in a select: with a live case present the select
// must always take the live case — registering (or polling) the nil case must not
// crash, a send to it must not fire, and it must not be treated as closed.
package main

import "fmt"

func main() {
	var nilCh chan int

	fmt.Println("nil len:", len(nilCh), "cap:", cap(nilCh))

	// Ready live case: the nil recv and nil send cases are skipped.
	live := make(chan int, 1)
	live <- 42
	select {
	case v := <-nilCh:
		fmt.Println("nil recv (wrong):", v)
	case nilCh <- 1:
		fmt.Println("nil send (wrong)")
	case v := <-live:
		fmt.Println("live recv:", v)
	}

	// Parked select: the nil case is not registered; the select wakes only through the
	// real channel's rendezvous.
	u := make(chan int)
	go func() { u <- 7 }()
	select {
	case v := <-nilCh:
		fmt.Println("nil recv (wrong):", v)
	case v := <-u:
		fmt.Println("rendezvous recv:", v)
	}

	// Send case on a live channel beside a nil case.
	out := make(chan int, 1)
	select {
	case nilCh <- 9:
		fmt.Println("nil send (wrong)")
	case out <- 5:
		fmt.Println("live send fired")
	}
	fmt.Println("out:", <-out)
}
