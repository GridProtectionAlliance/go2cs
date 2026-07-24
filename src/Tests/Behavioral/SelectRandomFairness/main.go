// Go chooses uniformly at random among READY select cases. 200 iterations of a
// two-ready-case blocking select must take both branches (the probability a uniform
// chooser picks one side 200 times in a row is 2^-199); the old WaitAny lowering always
// took the lowest index. The summary prints totals only, so the output is deterministic.
package main

import "fmt"

func main() {
	a := make(chan int, 1)
	b := make(chan int, 1)
	aCount, bCount := 0, 0

	for i := 0; i < 200; i++ {
		a <- 1
		b <- 1
		select {
		case <-a:
			aCount++
		case <-b:
			bCount++
		}
		// Drain whichever value remains so the next round starts clean.
		select {
		case <-a:
		case <-b:
		}
	}

	fmt.Println("total:", aCount+bCount)
	fmt.Println("both branches taken:", aCount > 0 && bCount > 0)
}
