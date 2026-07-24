package main

import "fmt"

// Guards the golib TryTypeAssert structural fallback: an assert from a POINTER-sourced
// interface value (a generated IжAdapter wrapping the receiver box) to an ANONYMOUS
// interface must resolve against the wrapped *T's method set, not the adapter class.
// Regression shape: errors.Is's `err.(interface{ Is(error) bool })` failed for every
// pointer-wrapped error because the fallback probed the adapter type.

type thing struct{ n int }

func (t *thing) Ping() string { return fmt.Sprintf("ping %d", t.n) } // pointer receiver
func (t thing) Pong() string  { return "pong" }                      // value receiver

type speaker interface{ Pong() string } // named interface — the conversion creates the adapter

func main() {
	var s speaker = &thing{7} // pointer-sourced interface value

	// Anonymous-interface assert whose method has a POINTER receiver on thing.
	if p, ok := s.(interface{ Ping() string }); ok {
		fmt.Println("ping-ok", p.Ping())
	} else {
		fmt.Println("ping-missed")
	}

	// Both methods — value- and pointer-receiver — are in *thing's method set.
	if b, ok := s.(interface {
		Ping() string
		Pong() string
	}); ok {
		fmt.Println("both-ok", b.Ping(), b.Pong())
	} else {
		fmt.Println("both-missed")
	}

	// Negative: a method *thing does not have must still miss.
	if _, ok := s.(interface{ Quit() }); ok {
		fmt.Println("quit-matched-wrong")
	} else {
		fmt.Println("quit-missed-ok")
	}
}
