package main

import "fmt"

// handle takes a *error whose pointee is a nil interface at address-of time. Reading *err
// (a reference-typed pointee) must NOT panic — Go yields the nil error. Before the fix, the
// converted entry deref-alias `ref var err = ref Ꮡerr.Value` threw a spurious nil-pointer
// panic because the box's held value (the nil error) is null and .Value's IsNull check fired.
func handle(label string, err *error) {
	if e := recover(); e != nil {
		// *err read-back through the SAME pointer (nil error) — legal in Go, no deref panic.
		prev := *err
		panic(fmt.Sprintf("wrapped %s: %v (prev=%v)", label, e, prev))
	}
}

func doit() (err error) {
	defer handle("doit", &err)
	panic("boom")
}

func main() {
	defer func() {
		if e := recover(); e != nil {
			fmt.Println(e)
		}
	}()
	doit()
}
