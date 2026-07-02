package main

import (
	"fmt"
	"time"
)

// Recursive Fibonacci: measures raw function-call and integer-op overhead.

func fib(n int) int {
	if n < 2 {
		return n
	}
	return fib(n-1) + fib(n-2)
}

func main() {
	start := time.Now().UnixNano()

	sum := 0
	for i := 0; i < 5; i++ {
		sum += fib(34)
	}

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", sum)
	fmt.Println("elapsed_ns:", elapsed)
}
