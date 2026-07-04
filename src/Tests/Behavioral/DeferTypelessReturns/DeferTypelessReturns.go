package main

import "fmt"

type item struct{ n int }
type handle uintptr

const invalid = ^handle(0)        // typed named-uintptr const, folded value > int32 (CS0266 shape)
const appErr = 1 << 29            // untyped const
const big handle = appErr + 5     // named-uintptr const, folded beyond int32 (same CS0266 shape)

// idx subtracts an untyped const from a named-uintptr receiver (CS0034 shape: the cast on the
// const must target the NAMED type, not golib's uintptr struct, to stay unambiguous).
func (h handle) idx() int { return int(h - appErr) }

// find has unnamed results, a defer, and EVERY return contains nil — no return statement
// contributes a natural type, so the value-returning execution wrapper needs its explicit
// result type argument (CS8030 shape: syscall getProcessEntry).
func find(xs []item, want int) (*item, error) {
	defer fmt.Println("find done")
	for i := range xs {
		if xs[i].n == want {
			return &xs[i], nil
		}
	}
	return nil, fmt.Errorf("not found")
}

// closeIt is a deferred method-value call passing untyped nil to a pointer param (CS0123 shape:
// the nil argument must cast to the parameter's C# type so deferǃ's generic inference binds).
func closeIt(p *int, tag int) error {
	fmt.Println("closeIt", p == nil, tag)
	return nil
}

// first takes the address of an element of a BY-VALUE ARRAY PARAMETER (CS0103 shape: params are
// cloned but never heap-boxed, so the element address boxes a copy sharing element storage).
func first(value [4]byte) byte {
	p := &value[0]
	return *p
}

func main() {
	defer closeIt(nil, 3)
	// A no-arg deferred METHOD whose signature RETURNS a value (registry Key.Close in
	// time's zoneinfo_windows): the bare method-group form is a Func<error> (CS1503) -
	// the lambda form discards the result, matching Go deferred-call semantics.
	h := res{id: 4}
	defer h.close()
	xs := []item{{1}, {2}}
	p, err := find(xs, 2)
	fmt.Println(p != nil, err == nil)
	_, err = find(xs, 9)
	fmt.Println(err)
	fmt.Println(first([4]byte{7, 8, 9, 10}))
	fmt.Println(big.idx())
	fmt.Println(invalid == big, invalid == invalid)
}

// res.close returns error - the defer must not bind it as a bare method group.
type res struct{ id int }

func (h res) close() error {
	fmt.Println("closed", h.id)
	return nil
}
