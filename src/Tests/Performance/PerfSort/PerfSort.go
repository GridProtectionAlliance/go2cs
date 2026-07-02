package main

import (
	"fmt"
	"sort"
	"time"
)

// Sort 2M pseudo-random ints (deterministic xorshift64 fill): measures
// comparison-sort throughput through the sort package abstraction.

func run(n int) int {
	a := make([]int, n)
	var x uint64 = 88172645463325252

	for i := 0; i < n; i++ {
		x ^= x << 13
		x ^= x >> 7
		x ^= x << 17
		a[i] = int(x % 1000000007)
	}

	sort.Ints(a)

	return a[0] + a[n/4] + a[n/2] + a[3*n/4] + a[n-1]
}

func main() {
	start := time.Now().UnixNano()

	total := run(2000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
