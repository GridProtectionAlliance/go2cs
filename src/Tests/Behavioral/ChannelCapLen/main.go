// Buffered channel cap/len through fill, wrap-around and drain, len/cap of unbuffered
// and nil channels, and the comma-ok drain-after-close fix: a closed channel yields its
// remaining buffered values with ok == true FIRST, then (zero, false) — the old runtime
// returned (0, false) as soon as the channel was closed, discarding buffered data.
// Single-threaded — fully deterministic.
package main

import "fmt"

func main() {
	ch := make(chan int, 3)
	fmt.Println(len(ch), cap(ch))
	ch <- 1
	fmt.Println(len(ch), cap(ch))
	ch <- 2
	ch <- 3
	fmt.Println(len(ch), cap(ch))
	fmt.Println(<-ch, len(ch))
	ch <- 4 // wraps the circular buffer
	fmt.Println(len(ch), cap(ch))
	fmt.Println(<-ch, <-ch, <-ch)
	fmt.Println(len(ch), cap(ch))

	u := make(chan string)
	fmt.Println(len(u), cap(u))

	var n chan int
	fmt.Println(len(n), cap(n))

	d := make(chan int, 2)
	d <- 7
	d <- 8
	close(d)
	v, ok := <-d
	fmt.Println(v, ok)
	v, ok = <-d
	fmt.Println(v, ok)
	v, ok = <-d
	fmt.Println(v, ok)
	fmt.Println(len(d), cap(d))
}
