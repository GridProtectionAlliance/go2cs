package main

import "fmt"

type node struct {
	inner any
}

// inspect's outer guard var shadows the parameter, and the INNER type-switch's tag reaches
// the shadowed guard var through a selector — the tag must follow the shadow rename, not
// bind the outer parameter.
func inspect(n any) string {
	switch n := n.(type) {
	case *node:
		switch n.inner.(type) {
		case int:
			return "node:int"
		case string:
			return "node:string"
		default:
			return "node:other"
		}
	default:
		_ = n
		return "plain"
	}
}

// classifyVia's nested func literals reuse the enclosing parameter name, so pick's `v` is
// shadow-renamed — the type-switch tag `unwrap(v)` must reference pick's own parameter; the
// unrenamed tag silently classified the OUTER value.
func classifyVia(v any) (string, string) {
	unwrap := func(v any) any {
		if p, ok := v.(*node); ok {
			return p.inner
		}
		return v
	}

	pick := func(v any) string {
		switch v := unwrap(v).(type) {
		case int:
			return fmt.Sprintf("int:%d", v)
		case string:
			return fmt.Sprintf("string:%s", v)
		default:
			return fmt.Sprintf("other:%v", v)
		}
	}

	label := "shadow"
	other := &node{}
	other.inner = label

	return pick(v), pick(other)
}

func main() {
	hi := "hi"
	sn := &node{}
	sn.inner = hi

	fmt.Println(inspect(&node{inner: 5}))
	fmt.Println(inspect(sn))
	fmt.Println(inspect(&node{inner: 1.5}))
	fmt.Println(inspect(42))

	a, b := classifyVia(&node{inner: 7})
	fmt.Println(a, b)

	c, d := classifyVia("direct")
	fmt.Println(c, d)
}
