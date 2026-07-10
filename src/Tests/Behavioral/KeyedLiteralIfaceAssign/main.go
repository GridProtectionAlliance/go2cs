// KeyedLiteralIfaceAssign is the NEGATIVE guard for GoImplement over-recording: a keyed composite
// literal assigned to an interface-typed variable must record ONLY the composite's own pair
// (failure → error). The sparse-array ident context used to leak the LHS interface onto each FIELD
// value, recording GoImplement<code, error> — but `code` has no Error() method, so the generated
// partial forwarded to a nonexistent method (net/http's `err = http2GoAwayError{ErrCode: …}`,
// CS0103). The record predicate now gates on the method set actually satisfying the interface; the
// COMPILE phase locks it in — a reintroduced record generates `partial struct code : error`
// forwarding Error() to a method that does not exist.
package main

import "fmt"

// code has a String() method but NO Error() — it must never be recorded against `error`.
type code uint32

func (c code) String() string {
	return fmt.Sprintf("code(%d)", uint32(c))
}

type failure struct {
	Code code
	Msg  string
}

func (f failure) Error() string {
	return f.Code.String() + ": " + f.Msg
}

func main() {
	var err error
	err = failure{Code: code(7), Msg: "boom"}
	fmt.Println(err)
}
