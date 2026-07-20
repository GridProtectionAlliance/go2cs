package main

import "fmt"

// Guards the converter fix for FUNCTION-LOCAL named type declarations (slice/map/channel).
// C# forbids a type declaration inside a method body, so a `type X []T` / `type X map[K]V` /
// `type X chan T` declared in a function must be HOISTED to member level (renamed with the
// enclosing-function prefix), exactly like a local `type X struct{...}` already is. Before the
// fix these emitted an invalid inline `[GoType(...)] partial struct X;` in the method body, so the
// following statements parsed as MEMBER declarations ("Invalid token 'foreach'/'in' in a member
// declaration"). This reproduces the slices/maps example/test-body defect at behavioral scope.
func main() {
	// A local named struct type (already hoisted before the fix) used as a slice element, so the
	// slice type's element must resolve to the lifted struct name, not the raw Go name.
	type Point struct {
		X, Y int
	}

	// Local named SLICE type over a local struct element.
	type Points []Point

	pts := Points{{1, 2}, {3, 4}, {5, 6}}

	// A range/foreach loop in the body — the exact construct that leaked to member level.
	for _, p := range pts {
		fmt.Println(p.X, p.Y)
	}

	// Local named MAP type.
	type Tally map[string]int

	tally := Tally{"a": 1, "b": 2}
	fmt.Println(tally["a"] + tally["b"])

	// Local named CHANNEL type.
	type Stream chan int

	stream := make(Stream, 1)
	stream <- 7
	fmt.Println(<-stream)

	// Local named ARRAY type (fixed length).
	type Triple [3]int

	triple := Triple{10, 20, 30}
	sum := 0
	for _, n := range triple {
		sum += n
	}
	fmt.Println(sum)
}
