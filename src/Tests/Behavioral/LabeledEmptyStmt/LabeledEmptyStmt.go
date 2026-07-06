// Guards a Go label attached to an EMPTY statement — `done:` as the last line of a block,
// a `goto`/`break`/`continue` target with no statement between the label and the closing
// brace. C# requires a statement after a label, so the converter emits an explicit empty
// statement (`done:;`); a bare `done:` before `}` was CS1525/CS1002. Mirrors internal/trace
// gc.go's `keep:` goto-target at the tail of a for-loop body.
package main

import "fmt"

// scan uses a goto to a label positioned at the very end of the for-loop body (nothing
// follows `keep:` before the loop's `}`).
func scan(vals []int) int {
	best := -1
	for _, v := range vals {
		if v < 0 {
			goto keep
		}
		if v > best {
			best = v
		}
	keep:
	}
	return best
}

// gotoEndOfFunc: a goto to a label that is the last line of the function body (empty
// statement before the closing `}` of the func, not a loop).
func gotoEndOfFunc(v int) string {
	msg := "small"
	if v > 10 {
		goto big
	}
	return msg
big:
	msg = "big"
	return msg
}

// nestedGotoEnd: a goto to a label at the end of an inner block.
func nestedGotoEnd(rows [][]int) int {
	total := 0
	for _, row := range rows {
		for _, v := range row {
			if v == 0 {
				goto skip
			}
			total += v
		skip:
		}
	}
	return total
}

func main() {
	fmt.Println(scan([]int{3, -1, 7, 2, -5, 5}))     // 7
	fmt.Println(gotoEndOfFunc(3), gotoEndOfFunc(20))  // small big
	fmt.Println(nestedGotoEnd([][]int{{1, 2, 3}, {4, 0, 6}, {7, 8}})) // 6 + 10 + 15 = 31
}
