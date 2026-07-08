package main

import "fmt"

// anyMatch calls f for each value and reports whether any matched. When the func-literal argument
// captures an enclosing reference-typed local (a map/slice), the converter must snapshot that capture
// with a declaration (`var lookupʗ1 = lookup;`). Passing such a literal inside an `if`/`for` CONDITION
// previously emitted that declaration inline in the condition's argument list — invalid C# (the decl is
// a statement). This guards that the snapshot hoists onto its own line BEFORE the if/for statement.
func anyMatch(vals []int, f func(int) bool) bool {
	for _, v := range vals {
		if f(v) {
			return true
		}
	}
	return false
}

func main() {
	vals := []int{1, 2, 3}
	lookup := map[int]bool{2: true, 5: true}

	// (1) func-literal capture inside a plain `if` condition (no init).
	if anyMatch(vals, func(u int) bool { return lookup[u] }) {
		fmt.Println("if:", true)
	}

	// (2) func-literal capture inside an `if` condition WITH an init statement (sub-block path); the
	// snapshot must hoist between the init and the `if`.
	if extra := 7; anyMatch(vals, func(u int) bool { return lookup[u] || u == extra }) {
		fmt.Println("if-init:", true)
	}

	// (3) func-literal capture inside a traditional `for` condition (init + post present).
	for i := 0; i < 2 && anyMatch(vals, func(u int) bool { return lookup[u+i] }); i++ {
		fmt.Println("for:", i)
	}

	// (4) func-literal capture inside a while-style `for` condition (no init/post). A separate counter
	// terminates the loop; the captured map is read, never reassigned, so the once-taken snapshot is
	// faithful.
	n := 0
	for anyMatch(vals, func(u int) bool { return lookup[u] && n < 2 }) {
		fmt.Println("while:", n)
		n++
	}
}
