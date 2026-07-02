package main

import (
	"fmt"
	"time"
)

// Hash map churn: measures map insert, lookup (comma-ok), and delete
// (map<K,V> emulation overhead in C#).

func run(n int) int {
	m := make(map[int]int)

	for i := 0; i < n; i++ {
		m[i*2654435761%n] = i
	}

	total := len(m)

	for i := 0; i < n; i++ {
		if v, ok := m[i]; ok {
			total += v & 1023
		}
	}

	for i := 0; i < n; i += 2 {
		delete(m, i)
	}

	return total + len(m)
}

func main() {
	start := time.Now().UnixNano()

	total := run(2000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
