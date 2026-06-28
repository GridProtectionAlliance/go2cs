package main

import "fmt"

type iter struct{ i, n int }

func (it *iter) start(n int) { it.i = 0; it.n = n }
func (it *iter) valid() bool { return it.i < it.n }
func (it *iter) next()       { it.i++ }

// A for-loop whose init and/or post clauses are method calls (not simple assignments) —
// the runtime package does this (e.g. heapdump.go / traceback.go drive an unwinder with
// `for u.initAt(...); u.valid(); u.next()`). The init/post must be emitted as `;`-free
// clauses inside `for(...)`, not as full statements with a trailing `;` and newline.
func main() {
	var u iter
	sum := 0
	for u.start(3); u.valid(); u.next() {
		sum += u.i
	}
	fmt.Println(sum)

	// post-only method call
	u.start(4)
	count := 0
	for ; u.valid(); u.next() {
		count++
	}
	fmt.Println(count)
}
