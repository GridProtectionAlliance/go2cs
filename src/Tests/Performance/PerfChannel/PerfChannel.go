package main

import (
	"fmt"
	"time"
)

// Producer/consumer over a buffered channel with one goroutine: measures
// channel send/receive and goroutine scheduling (channel<T> emulation in C#).

func run(n int) int {
	ch := make(chan int, 128)
	done := make(chan int)

	go func() {
		total := 0
		for v := range ch {
			total += v & 4095
		}
		done <- total
	}()

	for i := 0; i < n; i++ {
		ch <- i
	}
	close(ch)

	return <-done
}

func main() {
	start := time.Now().UnixNano()

	total := run(1000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
