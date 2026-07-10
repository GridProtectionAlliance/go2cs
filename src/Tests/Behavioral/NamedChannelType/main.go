// Named-channel-type guard (net/http's h2 closeWaiter shape). A defined channel type must emit a
// [GoType("chan T")] wrapper declaration (the old converter emitted only a comment — CS0246 at every
// use), and the wrapper must carry the full channel surface: make through the named type (pointer-
// receiver re-make), send/receive (both the value and comma-ok forms), close, len/cap, range drain,
// select registration, and the type's OWN methods (Close here shadows nothing: the wrapper keeps
// IChannel.Close explicit so this Go method binds).
package main

import "fmt"

// closeWaiter mirrors net/http's http2closeWaiter — a named channel of empty struct.
type closeWaiter chan struct{}

// Init makes the closeWaiter usable through a pointer receiver (*cw = make(...)).
func (cw *closeWaiter) Init() {
	*cw = make(closeWaiter)
}

// Close marks the waiter closed, unblocking Wait — a Go method named like the wrapper's
// IChannel member (must bind THIS method, not the template's).
func (cw closeWaiter) Close() {
	close(cw)
}

// Wait blocks until the waiter is closed — a bare receive on the named type.
func (cw closeWaiter) Wait() {
	<-cw
}

// intQueue is a named BUFFERED channel of a basic element type.
type intQueue chan int

func main() {
	var cw closeWaiter
	cw.Init()
	cw.Close()
	cw.Wait()
	fmt.Println("waited")

	q := make(intQueue, 3)
	q <- 10
	q <- 20
	fmt.Println(len(q), cap(q))

	v := <-q
	fmt.Println(v)

	v, ok := <-q
	fmt.Println(v, ok)

	close(q)
	v, ok = <-q
	fmt.Println(v, ok)

	drain := make(intQueue, 2)
	drain <- 7
	drain <- 8
	close(drain)
	sum := 0
	for x := range drain {
		sum += x
	}
	fmt.Println(sum)

	sel := make(intQueue, 1)
	sel <- 42
	select {
	case y := <-sel:
		fmt.Println(y)
	}
}
