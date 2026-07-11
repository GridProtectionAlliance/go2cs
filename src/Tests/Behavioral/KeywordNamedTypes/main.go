package main

import "fmt"

// fixed is a struct whose name is a C# keyword.
type fixed struct {
	n int
}

// sizer is satisfied by fixed through a value receiver.
type sizer interface {
	size(of string) int
}

// lock is an interface whose name is a C# keyword.
type lock interface {
	held() bool
}

// short is a numeric defined type named after a C# keyword; dword is a numeric
// defined type that is not. Converting between them exercises the go2cs-gen
// implicit-conversion operators for a KEYWORD-named type — the [GoType] struct is
// @short, so the generated `partial struct`/operator/`new` must escape the keyword
// (regression guard for the ImplicitConvGenerator: CS0715/CS0057/CS0030).
type short int16
type dword uint32

func toShort(d dword) short { return short(d) }

func (f fixed) size(of string) int {
	return f.n + len(of)
}

func (f *fixed) grow(by int) {
	f.n += by
}

func (f fixed) held() bool {
	return f.n > 3
}

var std sizer = fixed{n: 3}

func main() {
	fmt.Println(std.size("hello"))

	f := fixed{n: 1}
	fmt.Println(f.size("ab"))

	f.grow(4)
	fmt.Println(f.size(""))

	var p sizer = &f
	fmt.Println(p.size("ptr"))

	var l lock = f
	fmt.Println(l.held())

	var lp lock = &f
	fmt.Println(lp.held())

	var d dword = 40000
	s := toShort(d)
	fmt.Println(int(s), int(dword(s)))
}
