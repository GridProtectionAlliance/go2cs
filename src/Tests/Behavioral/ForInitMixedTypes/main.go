// Regression test for the for-loop init clause with multiple variables of differing types.
//
// Go allows a single short-variable-declaration in the init clause that introduces several
// variables of different types, e.g. `for i, e := count, head; ...` where i is an int and e
// is a pointer. The converter used to emit this as two `;`-separated C# declarations inside
// the for-init clause (`for (nint i = count;var e = head; ...)`), which is invalid C# — the
// `;` terminates the init clause. The fix emits a single tuple-deconstruction declaration with
// per-element types: `(nint i, var e) = (count, head)`.
package main

import "fmt"

type node struct {
	val  int
	next *node
}

func main() {
	// 1 -> 2 -> 3
	third := &node{val: 3}
	second := &node{val: 2, next: third}
	first := &node{val: 1, next: second}

	// int + pointer mixed for-init.
	sum := 0
	for i, n := 3, first; i > 0; i, n = i-1, n.next {
		sum += n.val
	}
	fmt.Println(sum) // 6

	// int + string mixed for-init (s lags one element behind i).
	words := []string{"a", "b", "c"}
	out := ""
	for i, s := 0, ""; i < len(words); i, s = i+1, words[i] {
		out += s
	}
	fmt.Println(out) // ab

	// int + float64 mixed for-init.
	total := 0.0
	for i, f := 0, 0.5; i < 4; i, f = i+1, f*2 {
		total += f
	}
	fmt.Println(total) // 0.5 + 1 + 2 + 4 = 7.5
}
