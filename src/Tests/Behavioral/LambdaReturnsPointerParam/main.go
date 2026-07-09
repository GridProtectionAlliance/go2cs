package main

import "fmt"

type item struct {
	name string
}

type entry struct {
	get func() (*item, error)
}

func register(table []entry, get func() (*item, error)) []entry {
	return append(table, entry{get: get})
}

// addItem's closure returns the pointer PARAMETER whole (with a tuple result): the emitted
// lambda must yield the parameter's box, not prefix the in-lambda box READ (`ᏑᏑit.Value`).
func addItem(table []entry, it *item) []entry {
	return register(table, func() (*item, error) {
		return it, nil
	})
}

// pick covers the single-result closure shape.
func pick(it *item) func() *item {
	return func() *item {
		return it
	}
}

// passthrough returns the pointer parameter whole OUTSIDE a closure — the box render here
// must stay exactly as before.
func passthrough(it *item) *item {
	return it
}

func main() {
	var table []entry
	a := &item{name: "alpha"}
	b := &item{name: "beta"}
	table = addItem(table, a)
	table = addItem(table, b)

	for _, e := range table {
		it, err := e.get()
		fmt.Println(it.name, err)
	}

	fmt.Println(pick(a)().name)
	fmt.Println(passthrough(b).name)
}
