package main

import "fmt"

// shape is a NON-EMPTY interface: a pointer-receiver implementation reaches it only
// through the generated C# pointer adapter (circleжshape wrapping ж<circle>), unlike
// an empty interface which holds the raw ж<circle> box directly.
type shape interface {
	name() string
}

// circle implements shape via POINTER receiver — shape(&circle{}) holds *circle.
type circle struct {
	r int
}

func (c *circle) name() string {
	return "circle"
}

// square is a second pointer-receiver implementation for the multi-type case.
type square struct {
	s int
}

func (q *square) name() string {
	return "square"
}

// dot implements shape via VALUE receiver — the interface holds a copy (control case).
type dot struct {
	tag int
}

func (d dot) name() string {
	return "dot"
}

// classify: single-type pointer cases — `case *circle:` must match an interface value
// created from &circle{} (the pointer-adapter dispatch this test guards).
func classify(v shape) string {
	switch t := v.(type) {
	case *circle:
		return "ptr " + t.name()
	case *square:
		return "ptr " + t.name()
	case dot:
		return "val " + t.name()
	case nil:
		return "nil shape"
	default:
		return "unknown"
	}
}

// classifyMulti: multi-type pointer case — both pointer types share one clause and the
// guard binds as the interface type.
func classifyMulti(v shape) string {
	switch t := v.(type) {
	case *circle, *square:
		return "multi ptr " + t.name()
	case dot:
		return "multi val " + t.name()
	default:
		return "multi other"
	}
}

// grow writes through the matched case variable — the bound *circle must alias the
// ORIGINAL object exactly as Go's interface holds the pointer, not a copy.
func grow(v shape) {
	switch t := v.(type) {
	case *circle:
		t.r += 10
	case *square:
		t.s += 10
	}
}

func main() {
	c := &circle{r: 1}
	q := &square{s: 2}
	d := dot{tag: 3}

	fmt.Println(classify(c))
	fmt.Println(classify(q))
	fmt.Println(classify(d))
	fmt.Println(classify(nil))

	fmt.Println(classifyMulti(c))
	fmt.Println(classifyMulti(q))
	fmt.Println(classifyMulti(d))

	grow(c)
	grow(q)
	fmt.Println(c.r, q.s)

	// Control: an EMPTY interface holds the raw pointer box (no adapter) — the same
	// `case *circle:` label must keep matching it after the adapter unwrap.
	var x any = c
	switch x.(type) {
	case *circle:
		fmt.Println("any holds *circle")
	default:
		fmt.Println("any miss")
	}

	// No-bind form over the non-empty interface.
	var v shape = q
	switch v.(type) {
	case *square:
		fmt.Println("shape holds *square")
	default:
		fmt.Println("shape miss")
	}
}
