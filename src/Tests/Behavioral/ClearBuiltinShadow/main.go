package main

import "fmt"

// A package that declares a METHOD named like a Go 1.21 built-in (`clear`) — as runtime does with
// `func (b *pageBits) clear()` — shadows the built-in for an unqualified free call in C#: the method
// is emitted as a `clear(this ref box)` extension on the package's static class, and C# member lookup
// binds that (requiring `ref`) before the using-static `go.builtin.clear` (CS1620/CS1503). So a free
// built-in `clear(s)` call is emitted qualified as `builtin.clear(s)`, while the method stays `b.clear()`.
// (golib also had to gain the `clear` built-in itself — slice/span/map forms.)

type box struct{ data []int }

func (b *box) clear() { b.data = nil } // a METHOD named clear — shadows the built-in

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
}
