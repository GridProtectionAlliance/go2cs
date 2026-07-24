package main

import "fmt"

// grid holds a slice-of-slices FIELD. The regression this guards: taking the
// address of an element of a slice FIELD of a pointer receiver — `&g.rows[i]` —
// and writing through it. Because getIdentifier walks the selector chain to the
// receiver root `g`, the converter used to treat `&g.rows[i]` as a receiver
// reference and emit the copy-boxing form `Ꮡ(g.rows[i])`, which boxes a COPY of
// the slice header. An append through that pointer then wrote the grown length
// into the copy, never back into g.rows — so every row stayed empty (this is
// exactly text/tabwriter's terminateCell: `line := &b.lines[len-1]; *line =
// append(*line, cell)`). The fix emits the element-aliasing `Ꮡ(g.rows, i)`.
type grid struct {
	rows [][]int
}

// addLine appends a fresh empty row (grow-in-place, mirroring tabwriter.addLine).
func (g *grid) addLine() {
	g.rows = append(g.rows, nil)
}

// push appends v to the LAST row via a pointer to that row element — the
// address-of-slice-field-element pattern under test.
func (g *grid) push(v int) {
	row := &g.rows[len(g.rows)-1]
	*row = append(*row, v)
}

func main() {
	var g grid
	g.addLine()
	g.push(1)
	g.push(2)
	g.push(3)
	g.addLine()
	g.push(10)
	g.push(20)

	// If the element address boxed a copy, the appends never landed in g.rows and
	// both rows read back empty. With the aliasing fix, the writes are visible on
	// the original.
	for i, row := range g.rows {
		fmt.Printf("row %d len=%d %v\n", i, len(row), row)
	}

	// Also exercise write-through-pointer that MUTATES an existing element in place
	// (not just append), to confirm the pointer aliases the backing array.
	p := &g.rows[0]
	(*p)[0] = 99
	fmt.Printf("after mutate: %v\n", g.rows[0])
}
