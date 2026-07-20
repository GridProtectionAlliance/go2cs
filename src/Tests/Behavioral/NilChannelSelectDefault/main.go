// Guards Go's NIL-channel semantics inside a `select` with a `default` clause.
//
// A nil channel is never ready and never closed: a receive or send on one blocks forever, so in a
// select that has a default the nil case is simply not chosen and the default runs. golib models a
// channel as a STRUCT whose nil value is the zero struct — every field null — and its IsClosed
// probe dereferenced that absent state, throwing a NullReferenceException instead of reporting
// "not ready".
//
// os/exec's Start runs exactly this shape — `select { case <-c.ctx.Done(): return c.ctx.Err();
// default: }` — and context.Background().Done() IS a nil channel, so every child process launched
// through a background context crashed there. That is the math/rand TestDefaultRace path.
package main

import "fmt"

func main() {
	var nilRecv chan int

	// The nil case is not ready, so the default runs.
	select {
	case v := <-nilRecv:
		fmt.Println("nil recv took case", v)
	default:
		fmt.Println("nil recv default")
	}

	// NOTE: the SEND direction is deliberately NOT covered here. A `select` send case with a
	// `default` is currently emitted as an unguarded `case ᐧ:` that always wins — the send
	// readiness probe is missing entirely — so it takes the case even for a FULL real channel,
	// where Go takes the default. That is a separate, pre-existing gap in the select lowering,
	// independent of nil channels; covering it here would only bake the wrong output into a
	// golden. Add send coverage to this test when that lowering is fixed.

	// A nil channel reports no length and no capacity, and is not closed.
	fmt.Println("nil len:", len(nilRecv), "nil cap:", cap(nilRecv))

	// The comma-ok receive form on a nil channel is likewise never ready.
	select {
	case v, ok := <-nilRecv:
		fmt.Println("nil comma-ok took case", v, ok)
	default:
		fmt.Println("nil comma-ok default")
	}

	// A REAL channel alongside must still behave normally — empty takes the default, then the
	// buffered value is received.
	ready := make(chan int, 1)

	select {
	case v := <-ready:
		fmt.Println("real recv took case", v)
	default:
		fmt.Println("real empty default")
	}

	ready <- 7

	select {
	case v := <-ready:
		fmt.Println("real recv", v)
	default:
		fmt.Println("real default")
	}

	// A mixed select: the nil case must never win over a ready real case.
	ready <- 9

	select {
	case v := <-nilRecv:
		fmt.Println("mixed took nil", v)
	case v := <-ready:
		fmt.Println("mixed took real", v)
	}
}
