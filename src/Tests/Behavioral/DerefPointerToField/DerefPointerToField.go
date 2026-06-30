package main

import "fmt"

type holder struct {
	xs  *[]int // pointer to a slice
	cnt *int   // pointer to a value
}

// sumRange ranges over a deref'd pointer-to-slice FIELD of a pointer parameter (`*h.xs`). The deref
// must apply to the field, not be dropped (the param-deref shortcut wrongly matched `*h.field`).
func sumRange(h *holder) int {
	s := 0
	for _, x := range *h.xs {
		s += x
	}
	return s
}

// readVal dereferences a pointer FIELD of a pointer parameter (`*h.cnt`) for a value read.
func readVal(h *holder) int {
	return *h.cnt
}

func main() {
	xs := []int{10, 20, 30}
	c := 7
	h := &holder{xs: &xs, cnt: &c}
	fmt.Println(sumRange(h)) // 60
	fmt.Println(readVal(h))  // 7
}
