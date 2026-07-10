package main

import "fmt"

type Tally struct {
	total int
	log   string
}

func (t *Tally) Add(n int) {
	defer func() {
		t.log = fmt.Sprintf("%s+%d", t.log, n)
	}()
	t.total += n
}

// report mutates its own copy of t — the callers' source Tally stays untouched.
// (The deferred/go func literals below route the pointer-receiver call through this
// named function: a literal's OWN value param used as a pointer receiver is a separate
// known converter defect, out of this guard's scope.)
func report(prefix string, t Tally, n int) {
	t.Add(n)
	fmt.Println(prefix, t.total, t.log)
}

func main() {
	base := Tally{total: 2, log: "d"}

	// A heap-boxed outer local passed as an EAGER argument to a deferred func literal
	// inside an IIFE: the argument must render as the IIFE's capture snapshot, not the
	// raw ref-local (CS8175).
	func() {
		defer func(t Tally) {
			report("deferred:", t, 4)
		}(base)
	}()

	// Same shape with a NAMED callee — the defer statement's own capture machinery
	// (prepareStmtCaptures) runs on this path; the eager argument still must follow
	// the enclosing lambda's rename.
	func() {
		defer report("named:", base, 5)
	}()

	done := make(chan bool)

	// Same shape through a go statement — its arguments are evaluated eagerly at
	// spawn time in the enclosing scope too.
	func() {
		go func(t Tally) {
			report("goroutine:", t, 7)
			done <- true
		}(base)
	}()

	<-done

	fmt.Println("source untouched:", base.total, base.log)
}
