package main

import "fmt"

// pointerErr implements the stdlib `error` interface via a POINTER receiver. A
// *pointerErr placed into the interface field of an ELIDED struct composite (the inner
// `{...}` of a `[]struct{err error; ...}{...}`) must be wrapped in its generated adapter
// (`new pointerErrжerror(...)`); before the elided-composite interface-routing fix the
// converter emitted the bare box, which has no conversion to `error` (CS1503).
type pointerErr struct{ msg string }

func (p *pointerErr) Error() string { return p.msg }

// valueErr implements `error` via a VALUE receiver and is used ONLY inside the elided
// composite below — never converted to `error` anywhere else. The elided path must record
// its GoImplement pair, or no `partial struct valueErr : error` is generated and the bare
// value cannot satisfy the interface field (CS1503). This mirrors errors' wrap_test
// `errorUncomparable`, which the slice/named-field paths never reached.
type valueErr struct{ msg string }

func (v valueErr) Error() string { return v.msg }

func main() {
	// Elided anonymous-struct-slice composite: each element `{concrete, want}` is an
	// elided struct literal whose first field is the `error` interface — the exact shape
	// of errors' wrap_test `[]struct{err error; ...}{{&poser{...}, ...}, {errorUncomparable{}, ...}}`.
	cases := []struct {
		err  error
		want string
	}{
		{&pointerErr{"pointer-receiver"}, "pointer-receiver"},
		{valueErr{"value-receiver"}, "value-receiver"},
	}

	for _, c := range cases {
		fmt.Printf("%s | %s\n", c.err.Error(), c.want)
	}
}
