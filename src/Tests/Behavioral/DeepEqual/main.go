package main

import "reflect"

type point struct {
	x, y int
	tags []string
}

type node struct {
	val  int
	next *node
}

func main() {
	// Slices: elementwise equality, content mismatch, and the []byte fast path.
	println(reflect.DeepEqual([]string{"a", "bc"}, []string{"a", "bc"}))
	println(reflect.DeepEqual([]string{"a"}, []string{"b"}))
	println(reflect.DeepEqual([]int{1, 2, 3}, []int{1, 2, 3}))
	println(reflect.DeepEqual([]int{1, 2, 3}, []int{1, 2, 4}))
	println(reflect.DeepEqual([][]byte{[]byte("ab"), nil}, [][]byte{[]byte("ab"), nil}))
	println(reflect.DeepEqual([][]byte{[]byte("ab")}, [][]byte{[]byte("ac")}))

	// Nil vs empty slices are not deeply equal; nil equals nil.
	println(reflect.DeepEqual([]byte(nil), []byte{}))
	println(reflect.DeepEqual([]byte{}, []byte{}))
	var nilInts []int
	println(reflect.DeepEqual(nilInts, nilInts))
	println(reflect.DeepEqual(nilInts, []int{}))

	// The same slice is deeply equal to itself regardless of content (identity
	// short-circuit on &s[0]), while a copy of a NaN is not equal elementwise.
	zero := 0.0
	nan := []float64{zero / zero}
	println(reflect.DeepEqual(nan, nan))
	println(reflect.DeepEqual(nan, []float64{nan[0]}))

	// Structs: field-by-field, including slice-typed fields.
	p1 := point{1, 2, []string{"n"}}
	p2 := point{1, 2, []string{"n"}}
	p3 := point{1, 3, []string{"n"}}
	println(reflect.DeepEqual(p1, p2))
	println(reflect.DeepEqual(p1, p3))

	// Maps: order-independent equality, length, missing key, differing value,
	// nil vs empty, and same-map identity.
	m1 := map[string]int{"a": 1, "b": 2}
	m2 := map[string]int{"b": 2, "a": 1}
	println(reflect.DeepEqual(m1, m2))
	println(reflect.DeepEqual(m1, map[string]int{"a": 1}))
	println(reflect.DeepEqual(m1, map[string]int{"a": 1, "c": 2}))
	println(reflect.DeepEqual(m1, map[string]int{"a": 1, "b": 3}))
	var nilMap map[string]int
	println(reflect.DeepEqual(nilMap, nilMap))
	println(reflect.DeepEqual(nilMap, map[string]int{}))
	println(reflect.DeepEqual(m1, m1))

	// Pointers: deeply equal referents, same pointer, mutated referent, nils.
	q1 := &point{1, 2, nil}
	q2 := &point{1, 2, nil}
	q3 := q1
	println(reflect.DeepEqual(q1, q2))
	println(reflect.DeepEqual(q1, q3))
	q2.y = 9
	println(reflect.DeepEqual(q1, q2))
	var np1, np2 *point
	println(reflect.DeepEqual(np1, np2))
	println(reflect.DeepEqual(np1, q1))

	// Self-referential pointer cycles terminate (in-progress checks assumed true).
	a := &node{val: 1}
	a.next = a
	b := &node{val: 1}
	b.next = b
	println(reflect.DeepEqual(a, b))
	c := &node{val: 2}
	c.next = c
	println(reflect.DeepEqual(a, c))

	// Distinct types are never deeply equal; untyped nils are.
	println(reflect.DeepEqual([]int{1}, []string{"1"}))
	println(reflect.DeepEqual(nil, nil))
}
