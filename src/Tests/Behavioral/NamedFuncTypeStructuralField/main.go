// NamedFuncTypeStructuralField guards a value of a NAMED func type initializing a STRUCTURAL
// (written `func(...)`) field in a composite literal — Go's assignability bridges named ->
// underlying implicitly, but C# delegate types are nominal, so the emitted field value must
// re-wrap in the synthesized structural delegate (`by: new Func<ж<item>, ж<item>, bool>(by)`).
// This mirrors GOROOT sort's example_keys_test.go planetSorter pattern (`By` carries a method,
// so it renders as a real named C# delegate; the `by` field is the raw underlying func type —
// CS1503 without the wrap). The call-argument direction was already handled; this guards the
// composite-literal field direction. A method-group initializer (`cmp: byName`) converts
// natively and stays bare. Verified vs Go.
package main

import "fmt"

type item struct {
	name string
	rank int
}

// By is a named func type WITH a method — it renders as a named C# delegate.
type By func(a, b *item) bool

// Sort builds the sorter whose structural `by` field receives the named By value.
func (by By) Sort(items []item) {
	s := &sorter{items: items, by: by}
	s.run()
}

type sorter struct {
	items []item
	by    func(a, b *item) bool // written structural func type
}

func (s *sorter) run() {
	n := len(s.items)
	for i := 0; i < n; i++ {
		for j := i + 1; j < n; j++ {
			if s.by(&s.items[j], &s.items[i]) {
				s.items[i], s.items[j] = s.items[j], s.items[i]
			}
		}
	}
}

func byName(a, b *item) bool { return a.name < b.name }

type comparer struct {
	cmp func(a, b *item) bool
}

func main() {
	items := []item{{"delta", 4}, {"alpha", 1}, {"charlie", 3}, {"bravo", 2}}

	// Named By value -> structural field (the guarded conversion).
	By(func(a, b *item) bool { return a.rank < b.rank }).Sort(items)

	for _, it := range items {
		fmt.Println(it.name, it.rank)
	}

	// Control: a method group initializing the same structural field shape converts natively.
	c := comparer{cmp: byName}
	fmt.Println(c.cmp(&items[0], &items[1]))
}
