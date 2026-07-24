// Guards the converter's `&t[i]` element-address emission when t is a POINTER to an
// ARRAY (Go auto-derefs the index — `(*t)[i]`). The element lives in the pointed-to
// heap array, so the address must ALIAS it; a boxed copy silently drops writes made
// through the returned pointer. This is exactly hash/crc32's slicingMakeTable pattern
// (`simplePopulateTable(poly, &t[0])`), which left every CRC table all-zeros before
// the fix (TestGolden/TestSlicing). See convUnaryExpr.go's pointer-to-array branch.
package main

import "fmt"

type row [4]uint32

type grid [3]row

// populate writes through the element pointer &row[i]. If the caller's &t[j] aliases
// the real array element, these writes are visible after populate returns; if it was
// a copy, they are lost.
func populate(t *row, base uint32) {
	for i := 0; i < len(t); i++ {
		t[i] = base + uint32(i)
	}
}

func main() {
	// Case 1: &g[j] — address of an element of a POINTER-to-array (g is *grid). This is
	// the crc32 case: the element (row) is itself an array that populate then indexes.
	g := new(grid)
	for j := 0; j < len(g); j++ {
		populate(&g[j], uint32(j*10))
	}
	for j := 0; j < len(g); j++ {
		fmt.Println(g[j][0], g[j][1], g[j][2], g[j][3])
	}

	// Case 2: &r[0] chained back — read the populated values through a fresh alias to
	// confirm the writes landed in the shared backing, not a throwaway copy.
	first := &g[0]
	first[0] = 99
	fmt.Println(g[0][0])
}
