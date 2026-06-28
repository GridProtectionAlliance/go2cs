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

// A switch WITH fallthrough among the cases AND `default` listed first. Reordering the default
// to the trailing else is still safe because the (terminal) default does not participate in
// fallthrough. The runtime package does this (preempt.go's suspendG g-status switch).
func withFallthrough(typ bucketType) int {
	n := 0
	switch typ {
	default:
		return -1
	case memProfile:
		n = 1
		fallthrough
	case blockProfile:
		n += 10
	case mutexProfile:
		n = 100
	}
	return n
}

func main() {
	fmt.Println(size(memProfile))
	fmt.Println(size(blockProfile))
	fmt.Println(size(mutexProfile))
	fmt.Println(size(99))
	fmt.Println(mid(memProfile), mid(blockProfile), mid(99))
	fmt.Println(withFallthrough(memProfile), withFallthrough(blockProfile), withFallthrough(mutexProfile), withFallthrough(99))
}
