package main

import fmt "fmt"

type Stringy func() string

func foo() string {
	return "Stringy function"
}

func takesAFunction(foo Stringy) {
	fmt.Printf("takesAFunction \111: %v\n", foo())
}

func returnsAFunction() Stringy {
	return func() string {
		fmt.Printf("Inner stringy function\n")
		return "bar" // have to return a string to be stringy
	}
}

func half(n int) (int, error) {
	if n%2 != 0 {
		return 0, fmt.Errorf("odd")
	}
	return n / 2, nil
}

func main() {
	// A body-level := that REUSES a lambda PARAMETER (os CopyFS WalkDir shape): only
	// the new names declare; err stays the parameter (CS0841/CS0128 x5).
	probe := func(n int, err error) error {
		if err != nil {
			return err
		}
		m, err := half(n)
		if err != nil {
			return err
		}
		k, err := half(m)
		fmt.Println("halved", m, k)
		return err
	}
	fmt.Println(probe(8, nil), probe(3, nil))

	takesAFunction(foo)
	var f Stringy = returnsAFunction()
	f()
	var baz Stringy = func() string {
		return "anonymous stringy\n"
	}
	fmt.Print(baz())
	fmt.Println(cached(), cached())
	loader = func(name string) ([]byte, error) { return []byte(name), nil }
	b, err := loader("zone")
	fmt.Println(len(b), err == nil)
}

// loader is an UNINITIALIZED package-level var of an ANONYMOUS func type with a slice
// result (time zoneinfo_read loadTzinfoFromTzdata): the signature must render through the
// delegate-aware path (Func<@string, (slice<byte>, error)>), not the raw text form
// (`(<>byte, error)` - CS1003 cascade).
var loader func(name string) ([]byte, error)

// A func literal in a PACKAGE-LEVEL var initializer (sync.OnceValue shape,
// internal/syscall/windows SupportUnixSocket): the literal BODY is function scope,
// so its locals must emit as locals, not package fields (CS1002 cascade).
var cached = memo(func() int {
	var n int
	for i := 1; i <= 4; i++ {
		n += i
	}
	return n
})

func memo(f func() int) func() int {
	done := false
	v := 0
	return func() int {
		if !done {
			v = f()
			done = true
		}
		return v
	}
}
