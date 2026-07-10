package main

import "fmt"

type node struct {
	inner any
}

type pair struct {
	label string
	value any
}

func describe(prefix string, v any) {
	switch s := v.(type) {
	case string:
		fmt.Println(prefix, "string:", s)
	default:
		fmt.Println(prefix, "other:", s)
	}
}

func main() {
	n := &node{inner: "hi"}        // keyed struct field
	p := pair{"tag", "val"}        // positional struct fields (string + any)
	m := map[string]any{"k": "mv"} // map value
	mk := map[any]int{"ky": 7}     // map key
	s := []any{"a", "b"}           // slice elements
	arr := [2]any{"x", "y"}        // array elements
	el := []node{{inner: "e1"}}    // elided keyed struct element
	ep := []pair{{"e2", "e3"}}     // elided positional struct element
	pp := []*node{{inner: "p1"}}   // pointer-elided keyed struct element
	pq := []*pair{{"q1", "q2"}}    // pointer-elided positional struct element
	sp := [3]any{1: "sp"}          // sparse array keyed element

	describe("keyed", n.inner)
	fmt.Println("label:", p.label)
	describe("positional", p.value)
	describe("mapval", m["k"])

	for k, v := range mk {
		describe("mapkey", k)
		fmt.Println("mapkeyval:", v)
	}

	describe("slice0", s[0])
	describe("slice1", s[1])
	describe("array0", arr[0])
	describe("array1", arr[1])
	describe("elided", el[0].inner)
	fmt.Println("elidedpos label:", ep[0].label)
	describe("elidedpos", ep[0].value)
	describe("ptrelided", pp[0].inner)
	fmt.Println("ptrelidedpos label:", pq[0].label)
	describe("ptrelidedpos", pq[0].value)
	describe("sparse0", sp[0])
	describe("sparse1", sp[1])
}
