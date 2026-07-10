// Func-type bracket-balance guard (the go/importer gccgo importer-field shape, builtin-typed).
// A struct field whose func type carries a NESTED func param returning a TUPLE renders through
// the string-based func-type conversion, whose parameter split must honor bracket depth: the
// naive comma split shredded the nested tuple at its interior comma, emitting the unbalanced
// `Func<nint, Func<nint, (nint>, error), (nint, error)` (a whole-project syntax cascade).
// Covers the anonymous field form, the methodless NAMED func type form (which collapses to the
// same base delegate), and a named-tuple-result sibling that must keep its C# tuple names —
// all invoked at runtime so the delegates are exercised, not just declared.
package main

import "fmt"

// applyFunc is a methodless NAMED func type with a nested func param returning a tuple — it
// collapses to (and must render identically to) the anonymous field form below.
type applyFunc func(seed int, op func(int) (int, error)) (int, error)

type machine struct {
	apply  func(seed int, op func(int) (int, error)) (int, error)
	named  applyFunc
	lookup func(name string) (val string, ok bool)
}

func double(x int) (int, error) {
	return x * 2, nil
}

func main() {
	m := machine{
		apply: func(seed int, op func(int) (int, error)) (int, error) {
			v, err := op(seed)
			return v + 1, err
		},
		named: func(seed int, op func(int) (int, error)) (int, error) {
			v, err := op(seed)
			return v * 10, err
		},
		lookup: func(name string) (val string, ok bool) {
			return name + "!", true
		},
	}

	v1, err1 := m.apply(20, double)
	fmt.Println(v1, err1)

	v2, err2 := m.named(3, double)
	fmt.Println(v2, err2)

	s, ok := m.lookup("go")
	fmt.Println(s, ok)
}
