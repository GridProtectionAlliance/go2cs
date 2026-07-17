package main

import "fmt"

// reportProcs reads newprocs through the package scope: if the sibling conversion of main()
// re-declared the assignment below as a shadowing local, the package var would still be 0 here.
func reportProcs() {
	fmt.Println("newprocs:", newprocs)
}

func main() {
	// Assignment (not declaration) of a package var declared in the hand-owned file.
	newprocs = 4

	// Address of a nested anonymous-struct field declared in the hand-owned file: the emitted
	// selector must use the lifted anonymous-struct type name from the package registry.
	sched.disable.user = true
	np := &sched.disable.n
	*np = 7

	sched.label = "ok"

	reportProcs()
	fmt.Println("disable:", sched.disable.user, sched.disable.n)
	fmt.Println("label:", sched.label)
}
