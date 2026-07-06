package main

import "fmt"

// A named NON-struct (scalar) type implementing the error interface by VALUE receiver —
// the hpack `type InvalidIndexError int` shape.
type myErr int

func (e myErr) Error() string {
	return fmt.Sprintf("myErr(%d)", int(e))
}

// Another named scalar over a different base, implementing a local interface.
type tag string

func (t tag) Name() string { return "tag:" + string(t) }

type named interface {
	Name() string
}

// Structs with INTERFACE fields.
type box struct {
	err error
}

type holder struct {
	n named
}

func main() {
	// Positional composite literal filling the interface field with a named-scalar value
	// that implements it (the hpack DecodingError{InvalidIndexError(idx)} shape). Without
	// recording GoImplement<myErr, error>, the value is passed bare to the interface-typed
	// constructor parameter → CS1503.
	b := box{myErr(7)}
	fmt.Println(b.err.Error())

	// Keyed form of the same.
	b2 := box{err: myErr(42)}
	fmt.Println(b2.err.Error())

	// A named string-based scalar into a local interface field.
	h := holder{tag("x")}
	fmt.Println(h.n.Name())
}
