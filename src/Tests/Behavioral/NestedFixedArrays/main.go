package main

import "fmt"

// A named fixed-array type: its generated wrapper allocates its backing lazily, so its
// zero value already self-heals. Covered here to keep that working.
type row [4]byte

// A struct whose own zero value is broken by a fixed-array field: `default(inner)` skips
// the field's `= new(3)` initializer, so an ELEMENT of [2]inner needs real construction.
type inner struct {
	b [3]byte
}

type holder struct {
	grid [2][3]byte
}

var gNested [2][4]byte
var gDeep [2][3][4]byte
var gStructElem [2]inner

func main() {
	// Local nested array: the outer `new(2)` leaves every inner array default (len 0).
	var x [2][4]byte
	fmt.Println(len(x), len(x[0]), len(x[1]))

	// Inner storage must be real, not a shared/absent backing.
	x[1][2] = 7
	x[0][3] = 9
	fmt.Println(x[1][2], x[0][3], x[0][2])

	// Three levels deep.
	var deep [2][3][4]byte
	fmt.Println(len(deep), len(deep[1]), len(deep[1][2]))
	deep[1][2][3] = 5
	fmt.Println(deep[1][2][3], deep[0][0][0])

	// Nested array as a struct FIELD.
	var h holder
	fmt.Println(len(h.grid), len(h.grid[1]))
	h.grid[1][2] = 4
	fmt.Println(h.grid[1][2], h.grid[0][2])

	// Array whose ELEMENT is a struct needing construction.
	var se [2]inner
	fmt.Println(len(se), len(se[1].b))
	se[1].b[2] = 3
	fmt.Println(se[1].b[2], se[0].b[2])

	// Array of a NAMED fixed-array type.
	var nr [2]row
	fmt.Println(len(nr), len(nr[1]))
	nr[1][3] = 6
	fmt.Println(nr[1][3], nr[0][3])

	// Globals take a separate emission path from locals.
	fmt.Println(len(gNested), len(gNested[1]))
	gNested[1][2] = 8
	fmt.Println(gNested[1][2], gNested[0][2])

	fmt.Println(len(gDeep), len(gDeep[1]), len(gDeep[1][2]))
	fmt.Println(len(gStructElem), len(gStructElem[1].b))

	// Each element must be its OWN storage, not one shared inner array.
	var share [3][2]byte
	share[0][0] = 1
	share[1][0] = 2
	share[2][0] = 3
	fmt.Println(share[0][0], share[1][0], share[2][0])
}
