package main

import (
	"fmt"
	"time"
)

// Sieve of Eratosthenes: measures slice allocation, indexing, and tight
// integer loops (bounds-check / slice-header emulation overhead in C#).

func sieve(n int) (int, int) {
	composite := make([]bool, n)
	count := 0
	sum := 0
	for i := 2; i < n; i++ {
		if !composite[i] {
			count++
			sum += i
			for j := i * i; j < n; j += i {
				composite[j] = true
			}
		}
	}
	return count, sum
}

func main() {
	start := time.Now().UnixNano()

	count := 0
	sum := 0
	for r := 0; r < 3; r++ {
		c, s := sieve(10000000)
		count += c
		sum += s
	}

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", count, sum)
	fmt.Println("elapsed_ns:", elapsed)
}
