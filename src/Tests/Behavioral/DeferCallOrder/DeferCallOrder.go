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

	fmt.Println("Main function")
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
