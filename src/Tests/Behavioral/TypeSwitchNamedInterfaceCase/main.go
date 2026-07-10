package main

import "fmt"

// shape is a NAMED non-empty interface: a case label of this type must dispatch by Go
// METHOD-SET semantics — a raw *circle held in an any matches `case shape:` because
// *circle implements shape, even though no C# type pattern can see that nominally.
type shape interface {
	name() string
	grow(n int)
}

// sizer is a second named interface reached only through interface-to-interface dispatch.
type sizer interface {
	size() int
}

// circle implements shape and sizer via POINTER receivers only.
type circle struct {
	r int
}

func (c *circle) name() string { return "circle" }
func (c *circle) grow(n int)   { c.r += n }
func (c *circle) size() int    { return c.r }

// dot implements shape via VALUE receivers — the interface holds a copy.
type dot struct {
	tag int
}

func (d dot) name() string { return "dot" }
func (d dot) grow(n int)   { d.tag += n } // mutates the copy; invisible in Go too

// errMark implements the universe error interface via POINTER receiver.
type errMark struct {
	code int
}

func (e *errMark) Error() string { return fmt.Sprintf("errMark(%d)", e.code) }

// describe: NAMED-interface case labels over an any tag — must match pointer-adapter
// values, raw pointer boxes, and value implementers; miss everything else.
func describe(x any) string {
	switch t := x.(type) {
	case shape:
		return "shape:" + t.name()
	case error:
		return "error:" + t.Error()
	default:
		return fmt.Sprintf("other:%T", t)
	}
}

// firstConcrete: label ORDER — the concrete pointer label precedes the interface label
// and must win for a *circle.
func firstConcrete(x any) string {
	switch t := x.(type) {
	case *circle:
		return fmt.Sprintf("concrete r=%d", t.r)
	case shape:
		return "iface " + t.name()
	default:
		return "none"
	}
}

// firstInterface: label ORDER — the interface label precedes the concrete one and wins.
func firstInterface(x any) string {
	switch t := x.(type) {
	case shape:
		return "iface " + t.name()
	case *circle:
		return fmt.Sprintf("concrete r=%d", t.r)
	default:
		return "none"
	}
}

// multi: interface labels inside a MULTI-TYPE clause bind at the TAG's type.
func multi(x any) string {
	switch t := x.(type) {
	case shape, error:
		return "multi shape-or-error"
	case nil:
		return "nil"
	default:
		return fmt.Sprintf("single? %v", t)
	}
}

// viaInterfaceTag: interface-to-interface dispatch — the tag is already a NON-EMPTY
// interface and the label is a DIFFERENT named interface.
func viaInterfaceTag(v shape) string {
	switch t := v.(type) {
	case sizer:
		return fmt.Sprintf("sizer %d", t.size())
	default:
		return "not a sizer"
	}
}

func main() {
	c := &circle{r: 1}
	var s shape = c // records the circle→shape pointer conversion (generates the adapter)
	var z sizer = c // records the circle→sizer pointer conversion
	_ = z

	// Pointer-ADAPTER value: an any loaded from a non-empty interface value.
	var x any = s
	fmt.Println(describe(x))

	// RAW receiver box: an any loaded straight from the pointer — no adapter in the
	// value's history.
	var raw any = &circle{r: 7}
	fmt.Println(describe(raw))

	// VALUE implementer and non-matching control.
	fmt.Println(describe(dot{tag: 3}))
	fmt.Println(describe(42))

	// Universe error interface via pointer receiver: adapter-carried and raw box.
	var e error = &errMark{code: 5}
	var anyErr any = e
	fmt.Println(describe(anyErr))
	fmt.Println(describe(&errMark{code: 6}))

	// Label-order precedence, both directions, same value.
	fmt.Println(firstConcrete(c))
	fmt.Println(firstInterface(c))

	// Multi-type clause with interface members.
	fmt.Println(multi(c))
	fmt.Println(multi(&errMark{code: 8}))
	fmt.Println(multi(nil))
	fmt.Println(multi(1.5))

	// Interface-to-interface dispatch: shape tag, sizer label.
	fmt.Println(viaInterfaceTag(c))
	fmt.Println(viaInterfaceTag(dot{tag: 4}))

	// Write-through: the binding matched through a named-interface label must alias the
	// ORIGINAL object exactly as Go's interface holds the pointer.
	switch t := x.(type) {
	case shape:
		t.grow(10)
	}
	fmt.Println(c.r)
}
