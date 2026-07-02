package main

import (
	"fmt"
	"time"
)

// Dense float64 matrix multiply: measures floating-point throughput and
// nested slice-of-slice element access.

func matmul(n int) float64 {
	a := make([][]float64, n)
	b := make([][]float64, n)
	c := make([][]float64, n)

	for i := 0; i < n; i++ {
		a[i] = make([]float64, n)
		b[i] = make([]float64, n)
		c[i] = make([]float64, n)
		for j := 0; j < n; j++ {
			a[i][j] = float64((i*n+j)%100) * 0.5
			b[i][j] = float64((i+j)%100) * 0.25
		}
	}

	for i := 0; i < n; i++ {
		for j := 0; j < n; j++ {
			sum := 0.0
			for k := 0; k < n; k++ {
				sum += a[i][k] * b[k][j]
			}
			c[i][j] = sum
		}
	}

	trace := 0.0
	for i := 0; i < n; i++ {
		trace += c[i][i]
	}
	return trace
}

func main() {
	start := time.Now().UnixNano()

	total := 0.0
	for r := 0; r < 4; r++ {
		total += matmul(256)
	}

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", int64(total))
	fmt.Println("elapsed_ns:", elapsed)
}
