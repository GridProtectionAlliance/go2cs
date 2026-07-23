package main

import (
	"fmt"
)

// stopFn: a NAMED func type deferred/goroutine'd by VALUE (context.CancelFunc in net
// dial) — a distinct C# delegate has no conversion to Action, so the emission keeps the
// lambda form `defer(() => cancel())` instead of the bare trimmed delegate (CS1503 x5).
type stopFn func()

func makeStop(tag string, out chan<- string) stopFn {
	return func() { out <- tag }
}

func main() {
	defer fmt.Println("First")
	defer fmt.Println("Second")
	defer fmt.Println("Third")

	f1 := fmt.Println
	defer f1("Fourth")

	msgs := make(chan string, 2)
	cancel := makeStop("stopped", msgs)
	defer cancel()
	go makeStop("go-stopped", msgs)()
	fmt.Println(<-msgs) // go-stopped

	// A BUILTIN deferred with arguments (`defer close(returned)`, net dial): the generic
	// in-param method group binds neither Action<T> nor inference — the temp-param lambda
	// keeps the eager-argument deferǃ form (CS1503).
	drained := make(chan int, 1)
	defer func() {
		v, open := <-drained
		fmt.Println("after close:", v, open) // after close: 0 false
	}()
	defer close(drained)

	defer GetPrintLn()("Fifth")

	c := &acc{}
	s1, e1 := c.add(5)
	fmt.Println(s1, e1)
	s2, e2 := c.add(-1)
	fmt.Println(s2, e2, c.total)

	sm := &sema{}
	acquireAndWork(sm)
	fmt.Println("after:", sm.held) // after: false

	watchAndSend(sm)
	fmt.Println("sent:", <-sm.out, "| held:", sm.held) // sent: 9 | held: false

	fmt.Println("notify:", notifyAll(1, 2, 3)) // notified / notify: 6

	fmt.Println("Main function")
}

type sema struct {
	held bool
	out  chan int
}

func (s *sema) release() {
	s.held = false
	fmt.Println("sema released")
}

func (s *sema) send(n int) {
	s.out <- n
}

// watchAndSend mirrors os/exec's `go c.watchCtx(resultc)`: a pointer-receiver method group
// WITH matching-arity arguments deferred/goroutine'd through an already-pointer receiver has
// the same CS1113 as the nullary form — both `goǃ` and `deferǃ` bind the BOX overload
// (`goǃ(Ꮡs.send, 7)`), not the deref-alias value trim.
func watchAndSend(s *sema) {
	s.out = make(chan int, 2)
	s.held = true
	go s.send(7)
	fmt.Println("sent-first:", <-s.out) // sent-first: 7 (synchronizes the goroutine)
	defer s.send(9)                     // LIFO: release() runs first, then send(9)
	defer s.release()
	fmt.Println("watching, held:", s.held)
}

// acquireAndWork mirrors net nss.go's `defer conf.releaseSema()`: a NULLARY pointer-receiver
// method deferred through an already-pointer receiver binds the BOX method group
// (`defer(Ꮡs.release)`) — the deref-alias trim was a struct VALUE against the [GoRecv] ref
// extension, which cannot create a delegate (CS1113 ×2). A PROMOTED method (declared on an
// embedded type) keeps the prior emission — no extension exists on the outer box (CS1061).
func acquireAndWork(s *sema) {
	s.held = true
	defer s.release()
	fmt.Println("working, held:", s.held)
}

// notifyAll mirrors os/signal's Notify: a variadic function with defer. The params Span is
// passed by ref into the execution wrapper, where an aliasing sslice prologue remains legal.
func notifyAll(vals ...int) int {
	defer fmt.Println("notified")
	total := 0
	for _, v := range vals {
		total += v
	}
	return total
}

type acc struct{ total int }

// add: a pointer-receiver method with a function-level defer+recover and NAMED results - the
// whole body is emitted inside the synthesized execution-context lambda, so the receiver must
// take the direct-ж box form (a `ref acc` reference inside the lambda is CS1628; fmt ss.Token).
func (a *acc) add(n int) (sum int, err error) {
	defer func() {
		if e := recover(); e != nil {
			err = fmt.Errorf("boom")
		}
	}()
	a.total += n
	if n < 0 {
		panic("negative")
	}
	sum = a.total
	return
}

func GetPrintLn() func(string) {
	return func(src string) {
		fmt.Println(src)
	}
}
