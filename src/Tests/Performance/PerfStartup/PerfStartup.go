package main

import "fmt"

// Empty workload: wall-clock time of this program is pure process startup +
// runtime initialization + a single fmt round-trip. The harness reports the
// wall time; elapsed_ns is a constant so Go and C# outputs match exactly.

func main() {
	fmt.Println("checksum:", 42)
	fmt.Println("elapsed_ns:", 0)
}
