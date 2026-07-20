// Regression test: a Go range value variable reassigned inside the loop body.
//
// Go lets a range variable be reassigned (it is a per-iteration copy), but a C# `foreach`
// iteration variable is read-only — `r -= 0x10000` on the foreach var is CS1656. The converter
// iterates a temp and declares the range var as a mutable local copy in the body
// (`foreach (var (_, rᴛ1) in s) { var r = rᴛ1; … }`). Mirrors runtime's os_windows.go UTF-16
// surrogate-pair encoder, which subtracts 0x10000 from the rune in place.
package main

import "fmt"

func main() {
	s := "AB\U0001F600\U0001F601C"
	var out []uint16
	for _, r := range s {
		if r < 0x10000 {
			out = append(out, uint16(r))
		} else {
			r -= 0x10000
			out = append(out, uint16(0xD800+(r>>10)))
			out = append(out, uint16(0xDC00+(r&0x3FF)))
		}
	}
	for _, u := range out {
		fmt.Printf("%d ", u)
	}
	fmt.Println()
	fmt.Println("len", len(out))

	// Pointer-receiver method on a slice-range value var (CS1657 - dnsmessage GoString shape)
	pts := []point{{1, 2}, {3, 4}}
	total := 0
	for _, p := range pts {
		total += p.bump()
	}
	fmt.Println("total", total)
	fmt.Println("orig", pts[0].x, pts[1].x)

	// A FIELD write through the range value var (`dt.dll, _ = getString(...)`, debug/pe's
	// importedSymbols): Go mutates the per-iteration COPY; a C# foreach variable cannot
	// have members modified (CS1654) — the same mutable-copy emission applies.
	tags := []point{{10, 1}, {20, 2}}
	sum := 0
	for _, t := range tags {
		t.x, t.y = t.x+1, t.y+1
		sum += t.x + t.y
	}
	fmt.Println("fieldwrite", sum, tags[0].x, tags[1].y) // fieldwrite 37 10 2

	// A pointer-receiver method on a FIELD PATH rooted at the range value var
	// (`test.x.String()`) — the table-driven test idiom that dominates the standard library's
	// own test suites. Go auto-takes the field's address, so the range variable must be
	// addressable; a C# foreach variable's FIELDS cannot bind ref (CS1655). The direct-receiver
	// check above did not catch this: it only inspected the IMMEDIATE operand, not the path root.
	rows := []row{{point{1, 2}, &point{10, 0}}, {point{3, 4}, &point{20, 0}}}
	for _, r := range rows {
		fmt.Println("field method", r.v.bump())
	}
	fmt.Println("field method orig", rows[0].v.x, rows[1].v.x) // unchanged: 1 3

	// A write through a NESTED field path (`r.v.x = 99`) — same root, same escape.
	for _, r := range rows {
		r.v.x = 99
	}
	fmt.Println("nested write orig", rows[0].v.x, rows[1].v.x) // unchanged: 1 3

	// Taking the ADDRESS of a field of the range value var also needs an addressable root.
	// Read back through the same pointer (a non-boxed value's address is a copy here).
	for _, r := range rows {
		p := &r.v
		p.x += 5
		fmt.Println("addr", p.x)
	}

	// A method reached through a POINTER field hops OUT of the range variable: it writes
	// through the shared box, so no mutable copy is warranted and the source DOES observe it.
	for _, r := range rows {
		r.ptr.bump()
	}
	fmt.Println("through pointer", rows[0].ptr.x, rows[1].ptr.x) // 11 21
}

type point struct{ x, y int }

// row pairs a VALUE field (whose addressability flows from the range variable) with a
// POINTER field (which does not) — see the two loops above.
type row struct {
	v   point
	ptr *point
}

// bump has a pointer receiver - emitted as [GoRecv] this ref; called on a range value
// var it needs the mutable-copy emission (a foreach var cannot bind ref).
func (p *point) bump() int {
	p.x++ // mutates the per-iteration copy, as in Go
	return p.x + p.y
}
