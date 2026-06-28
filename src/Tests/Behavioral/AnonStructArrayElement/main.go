package main

import "fmt"

// Anonymous struct as an ARRAY-ELEMENT field type — runtime's MemStats.BySize pattern. The element
// must be lifted to a named type so the field declaration compiles as `array<Stats_BySize>` rather
// than a raw, un-compilable `array<struct{…}>`. (Validated by the Compile/Target phases; its
// zero-value array allocation is a separate generator concern, so its elements are not indexed.)
type Stats struct {
	Total  int
	BySize [3]struct {
		Size  uint32
		Count uint64
	}
}

// Anonymous struct as the element of a package-global array var — runtime's stackpool pattern. The
// declaration must resolve to the lifted element type and be allocated. The byte-array sub-field
// `pad` exercises the element-type rendering (previously mis-rendered as `<4>byte`); it is not
// indexed at runtime (array elements are default-constructed, so a nested array field is unallocated).
var pool [2]struct {
	item int
	pad  [4]byte
}

// Plain fixed-size array globals must be allocated (`new(N)`); indexing an uninitialized array
// global previously NRE'd at runtime.
var nums [3]int
var addr [2]int

// Reference Stats so it (and its lifted Stats_BySize element type) must compile; reads only the
// scalar field, independent of the (separate) zero-value array-element allocation.
func statsTotal() int { s := Stats{Total: 42}; return s.Total }

func main() {
	pool[0].item = 5
	pool[1].item = 6

	nums[0] = 11
	nums[2] = 13

	// Addressed array global — must box a real N-sized array, not the empty default.
	p := &addr
	p[0] = 21
	p[1] = 22

	fmt.Println(pool[0].item, pool[1].item)
	fmt.Println(nums[0], nums[2], addr[0], addr[1])
	fmt.Println(statsTotal())
}
