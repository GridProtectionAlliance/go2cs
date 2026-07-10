package main

import "fmt"

// A package that declares a METHOD named like a Go 1.21 built-in (`clear`) — as runtime does with
// `func (b *pageBits) clear()` — shadows the built-in for an unqualified free call in C#: the method
// is emitted as a `clear(this ref box)` extension on the package's static class, and C# member lookup
// binds that (requiring `ref`) before the using-static `go.builtin.clear` (CS1620/CS1503). So a free
// built-in `clear(s)` call is emitted qualified as `builtin.clear(s)`, while the method stays `b.clear()`.
// (golib also had to gain the `clear` built-in itself — slice/span/map forms.)
//
// The `recover` built-in is the EXCEPTION to that qualification: a package method named `recover`
// (text/template/parse's `func (t *Tree) recover(errp *error)`) also shadows the built-in, but a
// built-in `recover()` call is NOT emitted as `builtin.recover()` — golib has no such static. It is
// emitted as the func() execution-context lambda PARAMETER `recover()`, which is always in scope
// wherever recover is legal and correctly shadows the same-named method. Qualifying it would bind to
// the nonexistent `builtin.recover` and fall back to the method (CS0815/CS7036).

// header is a NAMED map type: `clear(h)` on its value must bind golib's IMap<K,V> overload —
// the generated wrapper implements IMap<K,V> and forwards to the shared underlying map, so
// clearing through the boxed view empties the caller's storage (net/http/httputil's `clear(h)`
// on an http.Header, CS0411 without the overload). A nil named map stays a no-op.
type header map[string][]string

type box struct{ data []int }

func (b *box) clear() { b.data = nil } // a METHOD named clear — shadows the built-in

type guard struct{ err error }

func (g *guard) recover() { // a METHOD named recover — shadows the built-in
	if e := recover(); e != nil { // the BUILT-IN recover() -> lambda param, NOT builtin.recover()
		g.err = fmt.Errorf("recovered: %v", e)
	}
}

func (g *guard) run() {
	defer g.recover()
	panic("boom")
}

func main() {
	s := []int{1, 2, 3}
	clear(s) // the built-in clear(slice): zeros elements -> builtin.clear(s)
	fmt.Println(s[0], s[1], s[2]) // 0 0 0

	b := &box{data: []int{4, 5}}
	b.clear() // the METHOD
	fmt.Println(len(b.data)) // 0

	m := map[string]int{"a": 1, "b": 2}
	clear(m) // the built-in clear(map): removes all entries
	fmt.Println(len(m)) // 0

	g := &guard{}
	g.run()
	fmt.Println(g.err) // recovered: boom

	h := header{"a": {"1"}, "b": {"2"}}
	alias := h // named-map values share storage
	clear(h)   // the built-in clear(NAMED map) -> the IMap<K,V> overload
	fmt.Println(len(h), len(alias)) // 0 0

	var hn header
	clear(hn) // clearing a nil named map is a no-op
	fmt.Println(len(hn)) // 0
}
