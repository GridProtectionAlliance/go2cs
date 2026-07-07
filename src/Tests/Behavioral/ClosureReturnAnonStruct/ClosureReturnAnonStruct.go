package main

import "fmt"

// closureReturningAnonStruct guards a converter defect in the dynamic-struct implicit-conversion
// registration path. A closure whose RESULT type is an anonymous struct that RETURNS a composite
// literal of the structurally-identical anonymous struct makes the converter lift the closure-result
// type and the composite-literal type to two DISTINCT C# names (e.g. `..._func_R0` and `..._type`)
// and record an implicit conversion between them. Both type arguments of the emitted
// `[assembly: GoImplicitConv<…>]` must resolve to those lifted names: the recording used to read the
// composite literal's type BEFORE it was lifted into the registry and stringified it as raw Go
// `struct{…}` text — an invalid C# generic argument (CS1031 "Type expected"). The fix records the
// conversion AFTER converting the result expression (which performs the lift). See visitReturnStmt.go.
func closureReturningAnonStruct() {
	makeEntry := func(name string, size int) struct {
		name string
		size int
	} {
		return struct {
			name string
			size int
		}{name, size}
	}

	e1 := makeEntry("alpha", 10)
	e2 := makeEntry("beta", 20)

	fmt.Printf("entries: %s=%d %s=%d total=%d\n", e1.name, e1.size, e2.name, e2.size, e1.size+e2.size)
}

func main() {
	closureReturningAnonStruct()
}
