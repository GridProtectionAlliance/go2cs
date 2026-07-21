package main

import "fmt"

// setErr writes through the pointer to a named result whose address was taken.
func setErr(err *error) {
	*err = fmt.Errorf("written via pointer")
}

// addOne mutates a value-typed named result through its address.
func addOne(n *int) {
	*n++
}

// handlePanic mirrors text/tabwriter: a deferred handler that recovers and
// writes the named result through the pointer taken at the defer site.
func handlePanic(err *error) {
	if e := recover(); e != nil {
		*err = fmt.Errorf("recovered: %v", e)
	}
}

// errResult has an inherently-heap (interface) named result whose address is
// taken by a deferred handler that writes it unconditionally. The deferred
// write must be observed by `return err`.
func errResult(fail bool) (err error) {
	defer setErr(&err)
	if fail {
		return fmt.Errorf("original")
	}
	return nil
}

// intResult has a value-typed named result whose address is taken and mutated
// by a deferred handler after the return expression is evaluated.
func intResult() (n int) {
	defer addOne(&n)
	n = 5
	return
}

// recoverResult recovers a panic and reports it through the address-taken
// named result — the exact tabwriter shape.
func recoverResult() (err error) {
	defer handlePanic(&err)
	panic("boom")
}

func main() {
	fmt.Println(errResult(false))
	fmt.Println(errResult(true))
	fmt.Println(intResult())
	fmt.Println(recoverResult())
}
