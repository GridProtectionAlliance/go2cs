// Regression test: a Go method VALUE (a bound method used without a call) reassigned to a
// pre-declared variable via `=` (not `:=`), as in database/sql's convert.go
// `checker = nvc.CheckNamedValue`.
//
// A method value captures its receiver, so the converter snapshots it into a `var recvʗ1 = recv;`
// statement before the lambda. In the combined LHS/RHS assignment block the LHS identifier and the
// `=` operator are already emitted, so writing that snapshot statement inline split the assignment
// into `checker = <newline> var recvʗ1 = recv; <newline> (lambda)` — three token-broken pieces
// (CS1002 "; expected"). The snapshot must be HOISTED before the whole statement, matching the
// `:=`-define path. This also exercises the reassignment inside a tagless switch case (a second
// database/sql site).
package main

import "fmt"

type validator struct{ base int }

func (v validator) check(n int) int {
	return v.base + n
}

func main() {
	var fn func(int) int

	v := validator{base: 100}
	fn = v.check // method-value reassign via = (plain standalone statement)
	fmt.Println(fn(5))

	// Reassign inside a tagless switch case (database/sql's cc.CheckNamedValue-in-case site).
	w := validator{base: 10}
	switch {
	case true:
		fn = w.check
	}
	fmt.Println(fn(7))

	x := validator{base: 1}
	fn = x.check
	fmt.Println(fn(7))
}
