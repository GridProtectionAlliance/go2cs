package main

import "fmt"

// holder has a field literally named `other`, which collides with the parameter name
// the generated C# `Equals(holder other)` method uses. The TypeGenerator must qualify
// the left operand of each field comparison with `this.` so the body reads
// `this.other == other.other` (field-to-field), not `other == other.other` (which would
// bind the left `other` to the parameter and fail to compile with CS0019).
type holder struct {
	mark  int
	other int
}

func main() {
	a := holder{mark: 1, other: 2}
	b := holder{mark: 1, other: 2}
	c := holder{mark: 1, other: 3}

	fmt.Println(a == b)
	fmt.Println(a == c)
	fmt.Println(b == c)
}
