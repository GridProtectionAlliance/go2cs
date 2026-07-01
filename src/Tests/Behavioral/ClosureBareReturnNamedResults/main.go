package main

import "fmt"

//go:noinline
func forEach(items []int, f func(int)) {
	for _, x := range items {
		f(x)
	}
}

// run has NAMED results (n, ok); a nested VOID closure with a BARE `return` must emit `return;`, NOT
// the enclosing function's named results `return (n, ok)` — the latter is CS8030 ("void-returning
// delegate cannot return a value"). Mirrors runtime mprof `goroutineProfileWithLabelsSync`'s
// `forEachGRace(func(gp1 *g){ … return … })`.
//
//go:noinline
func run() (n int, ok bool) {
	items := []int{1, 2, 3, -1, 4}
	forEach(items, func(x int) {
		if x < 0 {
			return // BARE return in the VOID closure (captures + skips)
		}
		n += x // closure mutates the outer named result
	})
	ok = n > 0
	return // BARE return in the OUTER named-results func -> `return (n, ok)`
}

func main() {
	n, ok := run()
	fmt.Println(n, ok) // 10 true
}
