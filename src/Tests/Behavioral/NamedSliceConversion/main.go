// Regression test: converting a *named* slice (or array) value to its underlying
// slice/array type — `[]CaseRange(special)` where `type SpecialCase []CaseRange` — must
// emit a cast through the generated implicit operator, `((slice<CaseRange>)special)`, not
// a constructor-style call `slice<CaseRange>(special)` (invalid C#, CS1729/CS1503). This is
// the unicode SpecialCase.ToUpper/ToTitle/ToLower pattern: `to(UpperCase, r, []CaseRange(special))`.
package main

import "fmt"

type CaseRange struct {
	Lo, Hi uint32
}

// named slice type (the unicode SpecialCase pattern)
type SpecialCase []CaseRange

// named slice of a basic type
type ints []int

// takes the underlying slice type, so the call site needs the conversion
func count(rs []CaseRange) int {
	return len(rs)
}

func sum(s []int) (total int) {
	for _, v := range s {
		total += v
	}
	return
}

func main() {
	special := SpecialCase{{1, 2}, {3, 4}, {5, 6}}
	fmt.Println(count([]CaseRange(special))) // 3

	n := ints{10, 20, 30}
	fmt.Println(sum([]int(n))) // 60

	// conversion in an assignment context
	var rs []CaseRange = []CaseRange(special)
	fmt.Println(len(rs), rs[2].Lo) // 3 5
}
