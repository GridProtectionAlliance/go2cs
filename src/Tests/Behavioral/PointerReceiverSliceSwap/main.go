// Guards the parallel-assignment fix for an index LHS whose base is a
// parenthesized pointer deref: `(*p)[i], (*p)[j] = (*p)[j], (*p)[i]`. Such an
// LHS has no plain-ident root (getIdentifier does not unwrap ParenExpr), so it
// was not counted as a reassignment and the parallel assignment shattered into
// two sequential statements that dropped the swap's temporary — the first store
// clobbered the value the second read, losing one element and duplicating the
// other. This is the exact shape of container/heap's myHeap.Swap and of the
// internal/trace/internal/oldtrace order heap; it must stay a simultaneous
// tuple deconstruction. A single `(*p)[i] = v` write must keep emitting plainly.
package main

import "fmt"

type ints []int

func (p *ints) swap(i, j int) { (*p)[i], (*p)[j] = (*p)[j], (*p)[i] }
func (p *ints) set(i, v int)  { (*p)[i] = v }

func show(p *ints) {
	for i, x := range *p {
		if i > 0 {
			fmt.Print(" ")
		}
		fmt.Print(x)
	}
	fmt.Println()
}

func main() {
	// Two disjoint swaps then a single element write.
	p := &ints{10, 20, 30, 40}
	p.swap(0, 3)
	p.swap(1, 2)
	p.set(0, 99)
	show(p) // 99 30 20 10

	// Full reversal by repeated swaps: a lost/duplicated element corrupts it loudly.
	q := &ints{1, 2, 3, 4, 5}
	for i, j := 0, len(*q)-1; i < j; i, j = i+1, j-1 {
		q.swap(i, j)
	}
	show(q) // 5 4 3 2 1
}
