// Regression test: an unexported type used as the type of an exported struct field must be
// emitted as `public`.
//
// C# requires a field's type to be at least as accessible as the field. Go's unicode package has
// `type d [MaxCase]rune` (unexported) used as the exported field `CaseRange.Delta`, which failed
// with CS0052 (and its generated constructor with CS0051). A package pre-pass now collects every
// unexported named type used as the type of an exported field (an array, struct, or basic type
// directly, or through a pointer/slice/array/map/channel) and emits it `public`; the generator honors the
// converter's explicit access modifier over its default name-based scope.
package main

import "fmt"

// unexported array type used as an exported field (the unicode `d` pattern).
type d [3]rune

// unexported struct type used as an exported field.
type inner struct {
	v int
}

// unexported named basic type used as an exported field.
type level int

// CaseRange is exported and has exported fields of the unexported types above.
type CaseRange struct {
	Lo    uint32
	Delta d
	Item  inner
	Lvl   level
}

func main() {
	var cr CaseRange
	cr.Lo = 65
	cr.Item = inner{v: 9}
	cr.Lvl = level(3)

	fmt.Println(cr.Lo)       // 65
	fmt.Println(cr.Delta[0]) // 0 (zero value of the array element)
	fmt.Println(cr.Item.v)   // 9
	fmt.Println(cr.Lvl)      // 3
}
