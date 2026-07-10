package main

import "fmt"

// Guards the write-visibility routing of closure captures (processPotentialCapture's shared-capture
// arm): Go closures share the ONE variable, so a capture that is written after the capture point —
// by the closure, or by the body while the closure can still run — must NOT be value-snapshotted
// (`var tʗ1 = t;`). A heap-boxed variable routes through its box (`Ꮡt.Value`); an unboxed variable
// (value parameter, slice/map local) is captured natively by the C# display class. Read-only-after-
// capture variables (A3) and go/defer ARGUMENT evaluation (E2) keep their snapshots — those are
// correct Go semantics. Each probe was proven DIVERGENT under the pre-fix converter (silently wrong
// values, compiling cleanly) except the marked controls.

type Tally struct {
	total int
	log   string
}

func (t *Tally) Add(n int) {
	t.total += n
	t.log += "+"
}

// A1: boxed LOCAL struct — closure WRITES, body reads after. Go: 106
func probeA1() {
	t := Tally{5, "s"}
	bump := func() { t.total += 100 }
	bump()
	t.total++
	fmt.Println("A1:", t.total, t.log)
}

// A2: boxed LOCAL struct — closure READS only, body writes AFTER creation. Go: 15
func probeA2() {
	t := Tally{5, "s"}
	get := func() int { return t.total }
	t.total += 10
	fmt.Println("A2:", get())
}

// A3 control: closure reads only, all writes BEFORE creation — snapshot stays. Go: 7
func probeA3() {
	t := Tally{5, "s"}
	t.total += 2
	get := func() int { return t.total }
	fmt.Println("A3:", get())
}

// B1: plain PARAM struct — closure writes, body reads after (native capture). Go: 106
func probeB1(t Tally) {
	bump := func() { t.total += 100 }
	bump()
	t.total++
	fmt.Println("B1:", t.total)
}

// B2: plain PARAM struct — closure reads, body writes after. Go: 15
func probeB2(t Tally) {
	get := func() int { return t.total }
	t.total += 10
	fmt.Println("B2:", get())
}

// C1: boxed LOCAL — closure writes, ptr-recv method writes between calls. Go: 208 s+
func probeC1() {
	t := Tally{5, "s"}
	bump := func() { t.total += 100 }
	bump()
	t.Add(3)
	bump()
	fmt.Println("C1:", t.total, t.log)
}

// C2: boxed LOCAL — closure reads only, ptr-recv method writes after. Go: 8
func probeC2() {
	t := Tally{5, "s"}
	get := func() int { return t.total }
	t.Add(3)
	fmt.Println("C2:", get())
}

// D1: plain PARAM + ptr-recv method + writing closure (`ref` extension binds the
// param directly, so no heap box exists — native capture shares it). Go: 208 s+
func probeD1(t Tally) {
	bump := func() { t.total += 100 }
	bump()
	t.Add(3)
	bump()
	fmt.Println("D1:", t.total, t.log)
}

// E1: DEFER func-lit reads local; body writes after registration — the deferred
// closure observes the FINAL value. Go: 42
func probeE1() {
	t := Tally{5, "s"}
	defer func() { fmt.Println("E1:", t.total) }()
	t.total = 42
}

// E2 control: defer ARG evaluation — registration-time value, snapshot semantics stay. Go: 5
func probeE2() {
	t := Tally{5, "s"}
	defer fmt.Println("E2:", t.total)
	t.total = 42
}

// F1: GO func-lit writes local; channel-synced read after. Go: 105
func probeF1() {
	t := Tally{5, "s"}
	done := make(chan int)
	go func() {
		t.total += 100
		done <- 1
	}()
	<-done
	fmt.Println("F1:", t.total)
}

// G1: struct var OUTSIDE loop, closures created IN loop write it. Go: 25
func probeG1() {
	t := Tally{5, "s"}
	var fs []func()
	for i := 0; i < 2; i++ {
		fs = append(fs, func() { t.total += 10 })
	}
	fs[0]()
	fs[1]()
	fmt.Println("G1:", t.total)
}

// G3 control: RANGE value var captured per-iteration (foreach semantics). Go: 10 20 30
func probeG3() {
	var fs []func() int
	for _, x := range []int{10, 20, 30} {
		fs = append(fs, func() int { return x })
	}
	fmt.Println("G3:", fs[0](), fs[1](), fs[2]())
}

// H1: TWO closures sharing one var — one writes, one reads. Go: 7
func probeH1() {
	t := Tally{5, "s"}
	inc := func() { t.total++ }
	get := func() int { return t.total }
	inc()
	inc()
	fmt.Println("H1:", get())
}

// I1: slice REASSIGNED in closure (unboxed reference local — reassignment diverges). Go: 2 1
func probeI1() {
	s := []int{1}
	app := func() { s = append(s, 2) }
	app()
	fmt.Println("I1:", len(s), s[0])
}

// J1: IIFE writes local struct. Go: 105
func probeJ1() {
	t := Tally{5, "s"}
	func() { t.total += 100 }()
	fmt.Println("J1:", t.total)
}

// K1: alias `&t` taken BEFORE the closure; write through the alias after — the alias
// makes later writes syntactically invisible, so it counts as written-after. Go: 50
func probeK1() {
	t := Tally{5, "s"}
	p := &t
	get := func() int { return t.total }
	p.total = 50
	fmt.Println("K1:", get())
}

// L1: map REASSIGNED in closure. Go: 1
func probeL1() {
	m := map[int]int{}
	set := func() { m = map[int]int{1: 1} }
	set()
	fmt.Println("L1:", len(m))
}

// M1: writer closure created first, reader created after — routing is per-VARIABLE. Go: 105 105
func probeM1() {
	t := Tally{5, "s"}
	bump := func() { t.total += 100 }
	get := func() int { return t.total }
	bump()
	fmt.Println("M1:", get(), t.total)
}

// N1 control: plain int written by closure — basics already capture natively. Go: 1
func probeN1() {
	n := 0
	inc := func() { n++ }
	inc()
	fmt.Println("N1:", n)
}

// N2: ESCAPING int (aliased) written by closure — boxed basic routes by-box too. Go: 7
func probeN2() {
	n := 0
	p := &n
	inc := func() { n += 5 }
	inc()
	*p += 2
	fmt.Println("N2:", n)
}

// P1: boxed TYPE-PARAM local written by closure (iter.Pull's coroutine-state shape:
// `heap<V>` box, `Ꮡv.ValueSlot` writes, tuple-target assignment). Go: 30 3
func probeP1[V any](seq []V) (V, int) {
	var v V
	n := 0
	p := &v
	set := func(x V) { v, n = x, n+1 }
	for _, x := range seq {
		set(x)
	}
	return *p, n
}

func main() {
	probeA1()
	probeA2()
	probeA3()
	probeB1(Tally{5, "s"})
	probeB2(Tally{5, "s"})
	probeC1()
	probeC2()
	probeD1(Tally{5, "s"})
	probeE1()
	probeE2()
	probeF1()
	probeG1()
	probeG3()
	probeH1()
	probeI1()
	probeJ1()
	probeK1()
	probeL1()
	probeM1()
	probeN1()
	probeN2()
	v, n := probeP1([]int{10, 20, 30})
	fmt.Println("P1:", v, n)
}
