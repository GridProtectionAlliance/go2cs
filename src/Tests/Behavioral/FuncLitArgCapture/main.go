package main

import "fmt"

type box struct {
	x int
	y int
}

// package globals, captured by a call-arg func literal below (runtime captures
// mheap_/sched this way) — the snapshot declaration must hoist out of the arg list.
var gPtr *box
var gVal = box{x: 7}

// run takes a func value and invokes it — so a function literal passed as its
// argument is a call argument, the position that previously mis-converted: the
// captured variable's snapshot copy declaration (`var mʗ1 = m;`) was emitted as
// a statement inside the argument list (invalid C#), and the address taken inside
// the closure pointed at the copy, so writes were lost.
func run(f func()) {
	f()
}

func set(p *box) {
	p.x = 42
}

func main() {
	// 1. Function literal as a call argument that takes the bare address of a local.
	//    The closure must write through to the ORIGINAL m (not a snapshot copy).
	var m box
	run(func() {
		set(&m)
	})
	fmt.Println("1:", m.x)

	// 2. Same, but the closure also reads/writes the variable by value alongside the
	//    address-of — the value uses must route through the same box.
	var n box
	run(func() {
		set(&n)
		n.y = n.x + 1
	})
	fmt.Println("2:", n.x, n.y)

	// 3. Address of a field of the local (not the bare variable). The field pointer
	//    must alias the original local's field.
	var c box
	run(func() {
		p := &c.x
		*p = 99
	})
	fmt.Println("3:", c.x)

	// 4. Function literal stored in a local (initializer position) then invoked — the
	//    capture-decl had nowhere valid to land here either.
	var d box
	f := func() {
		set(&d)
	}
	f()
	fmt.Println("4:", d.x)

	// 5. Pointer local captured by a call-arg func literal (no address-of). Its snapshot
	//    declaration (`var peʗ1 = pe;`) must hoist out of the argument list; the pointer
	//    shares its box, so the write is visible on the original.
	var e box
	pe := &e
	run(func() {
		pe.x = 11
		pe.y = pe.x + 1
	})
	fmt.Println("5:", e.x, e.y)

	// 6. Package globals captured by a call-arg func literal — also snapshotted+hoisted.
	//    The closure reads gVal and writes through the gPtr it sets.
	gPtr = &e
	run(func() {
		gPtr.x = gVal.x
	})
	fmt.Println("6:", e.x)

	// 7. Func literal on an assignment RHS capturing a slice. The snapshot declaration
	//    (`var valsʗ1 = vals;`) must hoist before the assignment, not emit in the RHS.
	vals := []int{5}
	adder := func(k int) int {
		return k + vals[0]
	}
	fmt.Println("7:", adder(10))

	// 8. Func literal inside a composite-literal element (itself an assignment RHS). The
	//    capture decl must hoist out of the initializer to before the statement.
	base := []int{3}
	handlers := []func(int) int{
		func(k int) int { return k + base[0] },
	}
	fmt.Println("8:", handlers[0](100))

	// 9. Typed var DECLARATION whose initializer is a capturing func literal (runtime
	//    tracestack.go does `var skipOrAdd func(uintptr) bool = func(){…}`). The capture decl
	//    must hoist out of the declaration's initializer (visitValueSpec), not emit inline.
	seed := []int{2}
	var mul func(int) int = func(k int) int {
		return k * seed[0]
	}
	fmt.Println("9:", mul(21))

	// 10. Func literal with NAMED RESULTS and a bare return (the iter.Pull shape:
	//     `next = func() (v1 V, ok1 bool) { ...; return }`). The lambda must DECLARE the
	//     named results at the top of its block (zero-initialized) or the emitted
	//     `return (v1, ok1);` references undeclared names (CS0103).
	pick := func(sel int) (x int, ok bool) {
		if sel > 0 {
			x = sel * 7
			ok = true
			return // bare return: named results as assigned
		}
		return // bare return: zero values
	}
	a, b := pick(3)
	zx, zok := pick(-1)
	fmt.Println("10:", a, b, zx, zok)

	// 11. Named-result literal whose FIRST statement is a bare return (must NOT collapse to
	//     an expression-bodied lambda -- the names only exist as block declarations).
	zero := func() (n int, s string) { return }
	n0, s0 := zero()
	fmt.Println("11:", n0, s0 == "")
}
