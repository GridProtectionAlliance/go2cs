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

// payload is a STRUCT so a local of it escapes to the heap as a ref-local
// (`ref var p = ref heap<payload>`), mirroring testing/fuzz.go's `fn := reflect.ValueOf(ff)`.
type payload struct{ vals []int }

func (p payload) sum() int {
	t := 0
	for _, v := range p.vals {
		t += v
	}
	return t
}

// nestedStructCapture: a heap-boxed struct local is captured by an OUTER closure whose INNER
// goroutine also uses it BY VALUE. The inner snapshot must read the OUTER closure's snapshot
// (`var pʗ2 = pʗ1;`), not the enclosing method's ref-local (`var pʗ2 = p;` — a ref local is
// uncapturable inside a closure, CS8175; testing/fuzz.go's run closure with its inner
// `go tRunner(t, func(){ fn.Call(args) })`).
func nestedStructCapture() int {
	p := payload{vals: []int{1, 2, 3, 4}}
	out := make(chan int, 1)
	outer := func() {
		go func() {
			out <- p.sum()
		}()
	}
	outer()
	return <-out
}

// selfRefCapture: a go-statement calls a captured func-value AND passes an inner func-literal
// argument. The func-value's snapshot is generated while converting that inner literal, so the
// outer (owner) capture state is on the stack; its RHS must stay the outer name, not a
// self-reference (`var workerʗ1 = workerʗ1;` — CS0841; net/lookup.go's
// `go dnsWaitGroupDone(ch, func(){})`).
func selfRefCapture() int {
	done := make(chan int, 1)
	worker := func(cb func()) {
		cb()
		done <- 5
	}
	go worker(func() {})
	return <-done
}

// deferArgCapture mirrors x/net/nettest conntest.go's `defer once.Do(func() { stop() })`: a func
// literal passed as the ARGUMENT of a DEFERRED call captures the local pointer `pf`. The capture
// snapshot (`var pfʗ1 = pf;`) must hoist BEFORE the deferǃ(...) call — emitting it inline in the
// argument list was invalid C# (CS1001/CS1002/CS1003/CS1026). The deferred call runs at return,
// writing 77 through the shared pointer box; the caller observes it after this returns.
func deferArgCapture(out *box) {
	pf := out
	defer run(func() {
		pf.x = 77
	})
	pf.x = 5
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

	// 12. Heap-boxed struct local captured by an outer closure, re-captured by value in an inner
	//     goroutine — the inner snapshot must read the outer snapshot (`pʗ2 = pʗ1`), else CS8175.
	fmt.Println("12:", nestedStructCapture())

	// 13. Go-statement over a captured func-value with an inner func-literal argument — the outer
	//     capture's snapshot RHS must stay the outer name, not a self-reference (`workerʗ1`).
	fmt.Println("13:", selfRefCapture())

	// 14. Func literal passed as the ARGUMENT of a DEFERRED call, capturing a local pointer — the
	//     x/net/nettest `defer once.Do(func(){ stop() })` shape. The snapshot must hoist BEFORE the
	//     deferǃ(...) call, not emit inline in its argument list. The deferred write lands through
	//     the shared pointer box; observed after deferArgCapture returns.
	var g box
	deferArgCapture(&g)
	fmt.Println("14:", g.x)
}
