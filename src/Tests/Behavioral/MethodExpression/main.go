package main

import "fmt"

// Locks in Go METHOD EXPRESSIONS — the unbound method as a func value whose first parameter is
// the receiver: `(*timers).run` (runtime time.go, passed to abi.FuncPCABIInternal). Selecting a
// method off a TYPE naively rendered the type in value position (`(ж<timers>).run` —
// CS0119/CS1503). Go types the expression as the func signature with the receiver prepended;
// the converter renders that signature as the concrete delegate type and casts the method's
// static form to it: `(Func<ж<counter>, nint, nint>)(add)` — a [GoRecv] method's
// RecvGenerator ж-overload matches the delegate exactly; a value-receiver method's extension
// form matches its value-typed delegate.

type counter struct{ n int }

func (c *counter) add(d int) int { c.n += d; return c.n }
func (c counter) get() int       { return c.n }

func main() {
	// pointer-receiver method expression: func(*counter, int) int — writes accumulate
	// through the receiver box, proving the delegate binds the real method
	f := (*counter).add
	c := &counter{n: 10}
	fmt.Println(f(c, 5)) // 15
	fmt.Println(f(c, 2)) // 17
	cv := *c
	fmt.Println(cv.get()) // 17 — the mutations landed on c (read back via a value copy;
	// calling a value-receiver method directly on a pointer LOCAL is a separate,
	// pre-existing auto-deref gap with zero runtime sites)

	// value-receiver method expression: func(counter) int
	g := counter.get
	fmt.Println(g(*c)) // 17

	// used inline as a call argument (the FuncPCABIInternal shape, but invoked)
	apply := func(fn func(*counter, int) int, base *counter, d int) int { return fn(base, d) }
	fmt.Println(apply((*counter).add, c, 3)) // 20
	fmt.Println(g(*c))                       // 20

	// BOUND method value with parameters (the runtime metrics.go `d.compute = read.compute`
	// shape): the receiver is captured, and the forwarding lambda carries the method's OWN
	// parameters (the old emission hardcoded arity zero — CS1593 against a non-nullary target).
	var bump func(int) int = c.add
	fmt.Println(bump(4)) // 24
	fmt.Println(bump(1)) // 25 — mutations accumulate through the bound receiver
	fmt.Println(g(*c))   // 25

	// The FULL metrics.go shape: a NAMED FUNC type conversion (`reader(read)` — a C# delegate
	// declaration needs `new reader(read)`, not a cast) with a bound method value on the
	// converted receiver, assigned to a func field and invoked.
	var d dispatcher
	d.compute = reader(readTen).sum
	fmt.Println(d.compute(3)) // 13
}

type reader func() int

func (f reader) sum(extra int) int { return f() + extra }

type dispatcher struct {
	compute func(int) int
}

func readTen() int { return 10 }
