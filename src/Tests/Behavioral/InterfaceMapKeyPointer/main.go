package main

import "fmt"

type Node interface {
	Pos() int
}

type Item struct {
	pos int
}

func (i *Item) Pos() int {
	return i.pos
}

// lookup indexes an interface-keyed map by a pointer PARAMETER: the deref-aliased param must
// re-render as its box for the interface adapter wrap (`new ItemжNode(Ꮡit)`, not the value
// alias `it`) — the go/types api.go PkgNameOf shape (`info.Implicits[imp]`, CS1503).
func lookup(seen map[Node]string, it *Item) (string, bool) {
	value, ok := seen[it]
	return value, ok
}

// record writes through the same parameter-operand key shape.
func record(seen map[Node]string, it *Item, label string) {
	seen[it] = label
}

// plainRead is the single-value (non-comma-ok) read through the parameter operand.
func plainRead(seen map[Node]string, it *Item) string {
	return seen[it]
}

func main() {
	item := &Item{pos: 7}
	seen := map[Node]string{}

	seen[item] = "kept"
	value, ok := seen[item]

	fmt.Println(value, ok, item.Pos())

	// Parameter-operand cases: comma-ok read, plain read, and write.
	value, ok = lookup(seen, item)
	fmt.Println(value, ok)

	other := &Item{pos: 9}
	record(seen, other, "added")
	fmt.Println(plainRead(seen, other), len(seen))
}
