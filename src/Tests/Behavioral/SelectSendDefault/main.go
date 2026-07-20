package main

import (
	"fmt"
	"time"
)

// A `select` send case with a `default` is chosen only if the channel can accept the value RIGHT
// NOW; otherwise the default runs. The lowering emitted an unguarded `case ᐧ:`, so it took the send
// case unconditionally AND never performed the send — the value was silently dropped even when the
// channel was ready. These cases pin both halves: the choice and the delivery.
//
// Three neighboring gaps are deliberately NOT covered, so no golden bakes in output golib does not
// yet produce (the same discipline NilChannelSelectDefault used to leave send uncovered):
//
//   - An UNBUFFERED channel with no waiting receiver. Go takes the default; golib models unbuffered
//     as a one-slot buffer with no rendezvous, so it reports ready. Needs waiting-receiver tracking.
//   - A BLOCKING select (no default) with several send cases. `select(…)` evaluates `Sending` for
//     every case, so every send is performed; Go performs exactly one.
//   - `len`/`cap` of a capacity-1 buffered channel. `make(chan T)` and `make(chan T, 1)` both emit
//     `new channel<T>(1)`, so the two are indistinguishable and `len` of the latter reports 0.
//     Hence the assertions below read values back rather than measuring lengths of cap-1 channels.

// A FULL buffered channel is not send-ready: the default runs and nothing is enqueued. Draining it
// makes the very same select ready, so both directions are pinned on one channel.
func fullBuffered() {
	ch := make(chan int, 1)
	ch <- 1

	select {
	case ch <- 2:
		fmt.Println("full buffered: sent")
	default:
		fmt.Println("full buffered: default")
	}

	fmt.Println("full buffered: held =", <-ch)

	select {
	case ch <- 2:
		fmt.Println("drained buffered: sent")
	default:
		fmt.Println("drained buffered: default")
	}

	fmt.Println("drained buffered: held =", <-ch)
}

// A buffered channel with free capacity is send-ready: the case runs and the value IS delivered.
func freeBuffered() {
	ch := make(chan int, 2)
	ch <- 1
	select {
	case ch <- 2:
		fmt.Println("free buffered: sent")
	default:
		fmt.Println("free buffered: default")
	}
	fmt.Println("free buffered: len =", len(ch))
	fmt.Println("free buffered: drained", <-ch, <-ch)
}

// An unbuffered channel with a receiver waiting is send-ready. The poll loop keeps the observable
// output deterministic no matter how long the peer takes to park.
func unbufferedWithReceiver() {
	ch := make(chan int)
	got := make(chan int, 1)

	go func() {
		got <- <-ch
	}()

	sent := false

	for i := 0; i < 1000 && !sent; i++ {
		select {
		case ch <- 7:
			sent = true
		default:
			time.Sleep(time.Millisecond)
		}
	}

	fmt.Println("unbuffered with receiver: sent =", sent, "received =", <-got)
}

// A NIL channel is never ready, so its send case is never chosen.
func nilChannel() {
	var ch chan int
	select {
	case ch <- 1:
		fmt.Println("nil channel: sent")
	default:
		fmt.Println("nil channel: default")
	}
}

// A send on a CLOSED channel panics — the `default:` clause does not swallow it. The channel here
// has free capacity, and a closed FULL channel must panic too rather than report "not ready".
func closedChannel() {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println("closed channel: recovered:", r)
		}
	}()

	ch := make(chan int, 1)
	close(ch)

	select {
	case ch <- 1:
		fmt.Println("closed channel: sent")
	default:
		fmt.Println("closed channel: default")
	}

	fmt.Println("closed channel: NOT REACHED")
}

// With several send cases and only one ready channel, the ready one is chosen.
func oneOfManyReady() {
	full := make(chan int, 1)
	full <- 1
	open := make(chan int, 1)

	select {
	case full <- 2:
		fmt.Println("one of many: chose full")
	case open <- 3:
		fmt.Println("one of many: chose open")
	default:
		fmt.Println("one of many: default")
	}

	fmt.Println("one of many: full held", <-full, "open got", <-open)
}

// A send case and a receive case, neither ready: the default wins over both.
func neitherReady() {
	full := make(chan int, 1)
	full <- 1
	empty := make(chan int, 1)

	select {
	case full <- 2:
		fmt.Println("neither ready: sent")
	case v := <-empty:
		fmt.Println("neither ready: received", v)
	default:
		fmt.Println("neither ready: default")
	}
}

// When several send cases are ready, Go picks one at random — so only the invariant is asserted:
// EXACTLY ONE send is performed, the losing case must not enqueue its value.
func exactlyOneSend() {
	a := make(chan int, 2)
	b := make(chan int, 2)

	select {
	case a <- 1:
	case b <- 2:
	default:
		fmt.Println("exactly one send: default")
	}

	fmt.Println("exactly one send: total =", len(a)+len(b))
}

// A select with NO default still blocks until the send can proceed. Only main prints, so the
// output does not depend on how the two goroutines interleave.
func blockingSendStillBlocks() {
	ch := make(chan int)
	got := make(chan int, 1)

	go func() {
		time.Sleep(20 * time.Millisecond)
		got <- <-ch
	}()

	select {
	case ch <- 42:
		fmt.Println("blocking send: sent")
	}

	fmt.Println("blocking send: received", <-got)
}

// A NAMED channel type routes through its generated wrapper, which must forward the non-blocking
// send the guard calls — otherwise the send case does not compile at all.
type queue chan int

func namedChannelType() {
	q := make(queue, 1)

	select {
	case q <- 1:
		fmt.Println("named channel: sent")
	default:
		fmt.Println("named channel: default")
	}

	select {
	case q <- 2:
		fmt.Println("named channel full: sent")
	default:
		fmt.Println("named channel full: default")
	}

	fmt.Println("named channel: held =", <-q)
}

func main() {
	fullBuffered()
	freeBuffered()
	unbufferedWithReceiver()
	nilChannel()
	closedChannel()
	oneOfManyReady()
	neitherReady()
	exactlyOneSend()
	blockingSendStillBlocks()
	namedChannelType()
}
