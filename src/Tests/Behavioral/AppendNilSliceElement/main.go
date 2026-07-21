package main

import "fmt"

// Guards the converter fix for `append(s, nil)` where s's element type is an
// UNNAMED nillable type (slice/map/pointer/chan/func). Go appends ONE nil
// element; a bare `default!` in the emitted C# bound to append's `params`
// parameter as the whole null array, appending ZERO elements (tabwriter's
// addLine left b.lines empty → terminateCell panicked [-1]).
func main() {
	// []T element: unnamed slice
	var lines [][]int
	lines = append(lines, nil)
	lines = append(lines, nil)
	lines[0] = append(lines[0], 7)
	fmt.Println(len(lines), lines[0], lines[1] == nil)

	// map element: unnamed map
	var maps []map[string]int
	maps = append(maps, nil)
	fmt.Println(len(maps), maps[0] == nil)

	// pointer element: unnamed pointer
	var ptrs []*int
	ptrs = append(ptrs, nil)
	fmt.Println(len(ptrs), ptrs[0] == nil)
}
