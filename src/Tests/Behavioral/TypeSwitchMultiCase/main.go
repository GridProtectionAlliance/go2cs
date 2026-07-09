package main

import "fmt"

type shape interface {
	name() string
}

type circle struct{ r int }
type square struct{ s int }
type dot struct{}

func (c circle) name() string { return "circle" }
func (s square) name() string { return "square" }
func (d dot) name() string    { return "dot" }

// A multi-type case binds t at the TAG's interface type (shape), so t.name()
// dispatches dynamically; a single-type case binds the listed concrete type.
func describe(v shape) string {
	switch t := v.(type) {
	case circle, square:
		return "both:" + t.name()
	case dot:
		return "val:" + t.name()
	default:
		return "none"
	}
}

// Multi-type case over basic types with the bound variable used as any.
func classify(x any) string {
	switch v := x.(type) {
	case int, int64:
		return fmt.Sprintf("integer %v", v)
	case string, bool:
		return fmt.Sprintf("text-or-flag %v", v)
	}
	return "unknown"
}

// Multi-type case over POINTER types through an empty interface: the case
// variable stays any-typed and both pointer types share one body.
func ptrKind(x any) string {
	switch t := x.(type) {
	case *circle, *square:
		_ = t
		return "shape-ptr"
	case *dot:
		return "dot-ptr"
	}
	return "other"
}

// Unbound multi-type case (no assignment in the guard) plus nil stacked with a
// concrete type in one clause.
func kind(v shape) string {
	switch t := v.(type) {
	case nil, dot:
		_ = t
		return "nil-or-dot"
	case circle:
		return "circle-val"
	default:
		return "boxed"
	}
}

func tag(x any) string {
	switch x.(type) {
	case int, string:
		return "common"
	case float64:
		return "float"
	}
	return "rare"
}

func main() {
	fmt.Println(describe(circle{1}))
	fmt.Println(describe(square{2}))
	fmt.Println(describe(dot{}))
	fmt.Println(classify(1))
	fmt.Println(classify(int64(2)))
	fmt.Println(classify("s"))
	fmt.Println(classify(false))
	fmt.Println(classify(3.5))
	fmt.Println(ptrKind(&circle{1}))
	fmt.Println(ptrKind(&square{2}))
	fmt.Println(ptrKind(&dot{}))
	fmt.Println(ptrKind(7))
	fmt.Println(kind(nil))
	fmt.Println(kind(dot{}))
	fmt.Println(kind(circle{3}))
	fmt.Println(kind(square{4}))
	fmt.Println(tag(7))
	fmt.Println(tag("x"))
	fmt.Println(tag(1.5))
	fmt.Println(tag(true))
}
