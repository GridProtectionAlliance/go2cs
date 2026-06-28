package main

import "fmt"

type bucketType int

const (
	memProfile bucketType = iota
	blockProfile
	mutexProfile
)

// Named-type case values force the switch to lower to an if/else-if chain (named types
// are not C# compile-time constants). With `default` listed FIRST — legal in Go, which
// is order-independent — the chain previously emitted a bare `{ /* default: */ … }`
// followed by `else if`, which is invalid C# (CS8641). The runtime package does exactly
// this (e.g. mprof.go newBucket). The default must become the trailing `else`.
func size(typ bucketType) string {
	var s string
	switch typ {
	default:
		s = "invalid"
	case memProfile:
		s = "mem"
	case blockProfile, mutexProfile:
		s = "block-or-mutex"
	}
	return s
}

// default in the MIDDLE of the cases (also order-independent in Go).
func mid(typ bucketType) int {
	switch typ {
	case memProfile:
		return 1
	default:
		return -1
	case blockProfile:
		return 2
	}
}

func main() {
	fmt.Println(size(memProfile))
	fmt.Println(size(blockProfile))
	fmt.Println(size(mutexProfile))
	fmt.Println(size(99))
	fmt.Println(mid(memProfile), mid(blockProfile), mid(99))
}
