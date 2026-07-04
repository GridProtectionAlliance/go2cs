package main

import (
	"fmt"
	"time"
)

type (
	Abser interface {
		Abs() float64
	}

	MyError struct {
		When time.Time
		What string
	}

	MyCustomError struct {
		Message string
		Abser
		*MyError
	}
)

// The following takes precedence over *MyError promoted field call to Time()
func (myErr *MyCustomError) Time() float64 {
	return 0.0
}

func (myErr MyError) Time() float64 {
	return float64(myErr.When.Unix())
}

type Device struct {
	name string
	hits int
}

// Receiver escapes (address of a receiver field is returned), so the
// converter emits the direct-ж primary form for this method
func (d *Device) Tag() *int {
	return &d.hits
}

func (d *Device) Describe() string {
	return d.name
}

// stamp/labelKind/counterKind: a POINTER-interface cast satisfied only through CHAINED
// VALUE embeds with pointer-receiver methods (dwarf's UintType -> BasicType -> CommonType
// with func (c *CommonType) Common(); CS1929 x18). The generated adapter must project the
// receiver box hop by hop onto the embedded field's box:
// m_box.of(counterKind.PkindBase).of(kindBase.Pmeta).Stamp().
type meta struct {
	label string
	count int
}

func (m *meta) Stamp() string {
	m.count++
	return m.label
}

func (m *meta) Hits() int {
	return m.count
}

type kindBase struct {
	meta
}

type counterKind struct {
	kindBase
}

type stamper interface {
	Stamp() string
	Hits() int
}

type Describer interface {
	Describe() string
	Tag() *int
}

// rig holds a Device by VALUE; probeRig's param is a VALUE too, so Tag() (a direct-ж
// method - its receiver escapes) has no box to bind: the call routes the value field-chain
// through the &-machinery, boxing a COPY (faithful: the Go value param is itself a copy;
// netip's ip.addr.halves(), CS1929).
type rig struct{ dev Device }

func probeRig(r rig) int {
	return *r.dev.Tag()
}

type deviceHandle struct {
	*Device
}

type leftSide struct{ tag string }

func (l leftSide) Ping() string { return "L" }

type rightSide struct{ tag string }

func (r rightSide) Ping() string { return "R" }

// pair embeds BOTH: tag and Ping are AMBIGUOUS at depth 1 (Go forbids the unqualified
// selector), so the TypeGenerator must not promote either — the duplicate members were
// CS0102/CS0111 x8 (bufio ReadWriter's Reader/Writer err/buf and Size/Buffered).
type pair struct {
	leftSide
	rightSide
}

type Inner struct {
	Value string
}

type Middle struct {
	*Inner // This works - single pointer promotion
}

type Outer struct {
	ptr **Inner // This works but requires a field name
}

func main() {
	e := MyError{time.Now(), "Hello"}
	a := MyCustomError{"New One", nil, &e}

	a.Message = "New"
	a.What = "World"

	fmt.Println("MyError What =", e.What)
	fmt.Println("MyCustomError What =", a.What)
	fmt.Println("MyCustomError method =", a.Time())

	inner := &Inner{Value: "hello"}
	innerPtr := &inner

	// Single pointer promotion works
	middle := Middle{Inner: inner}
	fmt.Println(middle.Value) // Prints "hello"

	// Multiple pointers require explicit field access
	outer := Outer{ptr: innerPtr}
	fmt.Println((*outer.ptr).Value) // Prints "hello"

	// Interface satisfied through a pointer embed: Tag's receiver escapes
	// (direct-ж primary), Describe's does not — both must bind through the hop
	dev := &Device{name: "sensor", hits: 3}
	var dsc Describer = deviceHandle{Device: dev}
	fmt.Println(dsc.Describe())
	p := dsc.Tag()
	*p = 7
	fmt.Println(dev.hits) // Prints 7 - box aliasing through the hop

	// Ambiguous promotions are not emitted; only the qualified selectors compile
	pw := pair{leftSide{tag: "a"}, rightSide{tag: "b"}}
	fmt.Println(pw.leftSide.tag, pw.rightSide.Ping()) // a R

	fmt.Println(probeRig(rig{dev: Device{name: "r", hits: 42}})) // 42

	// Chained value-embed promotion through a pointer-interface cast: Stamp() mutates
	// through the projected boxes, so the count must be visible through the SAME pointer.
	ck := &counterKind{}
	ck.label = "k9"
	var st stamper = ck
	fmt.Println(st.Stamp(), st.Stamp(), st.Hits()) // k9 k9 2
}
