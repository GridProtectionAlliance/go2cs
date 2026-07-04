package main

import "fmt"

// base is a struct; view is a DEFINED type over base, so the [GoType] value
// conversions view<->base exist. The (*view)(*base) pointer reinterpret below
// is the construct under test: the arg is a genuine pointer box (a call result),
// which the converter must dereference before the value conversion binds.
type base struct {
	a int64
	b int64
}

type view base

func makeBase() *base {
	x := base{a: 10, b: 20}
	return &x
}

// intRef is a DEFINED POINTER type (`type dequeueNil *struct{}`, sync/poolqueue): the TypeSpec
// must emit a [GoType("zh<T>")] partial class forward decl (a bare star-type line is CS1585),
// with nil-conversions (`intRef(nil)`), any-boxing comparisons, and derefs working through it.
type intRef *int64

// tail is a DEFINED type over the BASIC string: (*tail)(&s) is the third reinterpret
// direction, basic -> named (fmt's `(*stringReader)(&str)` Sscan family, CS0030 x3). The
// address-of collapses with the value deref and re-boxes a copy; all reads and writes go
// back through the same pointer, so the copy semantics are faithful.
type tail string

func (t *tail) chop() byte {
	b := (*t)[0]
	*t = (*t)[1:]
	return b
}

func consume(s string) string {
	t := (*tail)(&s)
	b1 := t.chop()
	b2 := t.chop()
	return string([]byte{b1, b2}) + string(*t)
}

// sliceToArray: Go's slice-to-array conversions route through golib's copy ctor - the 1.20
// VALUE form copies exactly as Go does (CS1955, netip AddrFromSlice), and the 1.17 POINTER
// form boxes the same copy (CS0030, edwards25519); reads back through the pointer are
// faithful.
func sliceToArray(s []byte) int {
	a := [4]byte(s)
	p := (*[2]byte)(s)
	return int(a[3]) + int(p[1])
}

func classify(v any) string {
	if v == intRef(nil) {
		return "nilref"
	}
	return "other"
}

func main() {
	pb := makeBase()  // *base — a genuine box (call result), not a deref-aliased param
	pv := (*view)(pb) // reinterpret *base -> *view (identical underlying struct)
	bb := base(*pv)   // convert the view value back to base to read its fields
	fmt.Println(bb.a, bb.b)

	var boxed any = intRef(nil)
	fmt.Println(classify(boxed))
	n := int64(5)
	var r intRef = &n
	fmt.Println(*r, classify(r))

	fmt.Println(consume("abcd")) // abcd

	fmt.Println(sliceToArray([]byte{1, 2, 3, 4})) // 6

	// A composite literal of a DEFINED-over-NAMED-STRUCT type constructs the underlying
	// and wraps (`new view(new base(a: 5, b: 6))` - encoding/binary's decoder/encoder
	// over coder, CS1739 x5).
	v2 := view{a: 5, b: 6}
	fmt.Println(v2.a + v2.b) // 11
}
