// Regression test: an unrecovered panic in a goroutine crashes the whole process like Go —
// panic message on stderr (not stdout) and exit code 2.
//
// Goroutines run on the bare thread pool via builtin.goǃ with no wrapper catch, and GoFunc's
// exception filter only captures panic-convertible exceptions, so an unrecovered panic in a
// goroutine reaches golib's AppDomain.UnhandledException backstop. That backstop used to print
// the message to STDOUT and call Environment.Exit(0) — a false-success signal to every caller
// (shells, CI, and the Phase-4 differential oracle). golib now matches Go: the report goes to
// stderr and the process exits with code 2.
//
// The output-comparison harness guards this differentially: exit codes must MATCH between the
// Go and C# binaries (both 2 here), stdout must match (the panic line must NOT leak there),
// and the first stderr line must match ("panic: goroutine boom" — the rest of Go's stderr is
// a machine-specific goroutine stack trace, so only the first line is compared).
package main

import "fmt"

func main() {
	fmt.Println("before goroutine panic")

	done := make(chan struct{})

	go func() {
		panic("goroutine boom")
	}()

	// Never satisfied: the goroutine's unrecovered panic must crash the whole process with
	// exit code 2 before this receive can proceed.
	<-done
}
