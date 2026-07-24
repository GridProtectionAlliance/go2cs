// A blocking select with MULTIPLE ready send cases must commit EXACTLY ONE (the old
// runtime's Sending() path delivered a phantom value on every ready send case). The
// printed summaries are winner-independent, so the output is deterministic even though
// the winning case is uniformly random.
package main

import "fmt"

func main() {
	a := make(chan int, 1)
	b := make(chan int, 1)

	select {
	case a <- 9:
	case b <- 9:
	}
	fmt.Println("delivered:", len(a)+len(b))

	var got int
	select {
	case got = <-a:
	case got = <-b:
	}
	fmt.Println("got:", got, "remaining:", len(a)+len(b))

	// Volume guard: 100 single-fire sends deliver exactly 100 values.
	c := make(chan int, 100)
	d := make(chan int, 100)
	for i := 0; i < 100; i++ {
		select {
		case c <- i:
		case d <- i:
		}
	}
	fmt.Println("after 100 selects:", len(c)+len(d))
	sum := 0
	for len(c) > 0 {
		sum += <-c
	}
	for len(d) > 0 {
		sum += <-d
	}
	fmt.Println("sum:", sum)
}
