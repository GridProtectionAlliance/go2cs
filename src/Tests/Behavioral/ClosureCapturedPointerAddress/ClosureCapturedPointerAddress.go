package main

import "fmt"

type node struct {
	v    int
	next *node
}

// run invokes f as a closure; mirrors runtime's systemstack(func(){…}) shape that
// captures a pointer local and takes its address inside the closure.
func run(f func()) {
	f()
}

func main() {
	// A small linked list; head is retained so the through-pointer mutations can be
	// observed from the outer function after the closure walks the list.
	head := &node{v: 1, next: &node{v: 2, next: &node{v: 3, next: nil}}}
	mToFlush := head

	run(func() {
		// Guard on the captured pointer itself; the body only runs while it is non-nil.
		for mToFlush != nil {
			// Address of the captured pointer local: makes mToFlush a box-ref var, so the
			// closure must share the outer function's storage for it (not a snapshot copy).
			prev := &mToFlush
			// Mutate the current node's value (write persists on the shared heap node).
			mToFlush.v += 100
			// Walk the list by reassigning the pointer THROUGH prev (the **node). The write
			// reaches the shared box, so the outer function observes mToFlush advance to nil.
			*prev = mToFlush.next
		}
	})

	// Proof #1: the closure's reassignment through *prev reached the outer function's storage.
	fmt.Println("mToFlush is nil:", mToFlush == nil)

	// Proof #2: the closure's value mutations persisted on the shared nodes.
	sum := 0
	for n := head; n != nil; n = n.next {
		sum += n.v
	}
	fmt.Println("sum:", sum)
}
