package main

import (
	"fmt"
)

func main() {
	// A type `switch` compares types instead of values.  You
	// can use this to discover the type of an interface
	// value.  In this example, the variable `t` will have the
	// type corresponding to its clause.
	whatAmI := func(i interface{}) {
		switch t := i.(type) {
		case nil:
			// A nil interface matches `case nil` — emitted as the C# `case null:` pattern.
			fmt.Println("I'm nil")
		case bool:
			fmt.Println("I'm a bool")
		case int, int64, uint64:
			fmt.Printf("I'm an int, specifically type %T\n", t)
		default:
			fmt.Printf("Don't know type %T\n", t)
		}
	}
	whatAmI(true)
	whatAmI(1)
	whatAmI(int64(2))
	whatAmI(uint64(2))
	whatAmI("hey")
	whatAmI(nil)

	// A type switch listing BOTH `int` and `int32` (and `uint`/`uint32`): Go `int`/`uint` are
	// matched by their native (nint/nuint) form, so the converter must NOT also synthesize a
	// concrete `int32`/`uint32` case here — that would duplicate the explicit one (CS8120) and
	// steal its values. Typed variables keep the dynamic types distinct (a Go `int` boxes as nint,
	// a Go `int32` as int32). Mirrors runtime's printpanicval. Each value must hit its own clause.
	classify := func(i interface{}) {
		switch i.(type) {
		case int:
			fmt.Println("int")
		case int32:
			fmt.Println("int32")
		case uint:
			fmt.Println("uint")
		case uint32:
			fmt.Println("uint32")
		default:
			fmt.Println("other")
		}
	}
	var a int = 1
	var b int32 = 2
	var c uint = 3
	var d uint32 = 4
	classify(a)
	classify(b)
	classify(c)
	classify(d)

	// Go `uint` and `uintptr` are DISTINCT types, and since golib gained a dedicated `uintptr`
	// STRUCT (golib/uintptr.cs — no longer a using-alias of System.UIntPtr), the C# type map
	// preserves the distinction: both cases emit their own labels and each dynamic type routes
	// to ITS OWN case, exactly as in Go (runtime error.go's printpanicval lists both). The
	// duplicate-mapped-case merge machinery remains for alias pairs that DO share a C# type,
	// but no longer fires here. The bodies below are deliberately DIFFERENT — under the old
	// alias model this switch could not even compile.
	kind := func(i interface{}) {
		switch i.(type) {
		case uint:
			fmt.Println("uint word")
		case uintptr:
			fmt.Println("uintptr word")
		case string:
			fmt.Println("text")
		default:
			fmt.Println("other")
		}
	}
	var u uint = 5
	var p uintptr = 6
	kind(u)    // uint word — distinct dynamic type
	kind(p)    // uintptr word — distinct dynamic type
	kind("x")  // text
	kind(3.14) // other
	fmt.Println(sizeOf(int32(5)), sizeOf(int64(7))) // 6 9

	var flag bool
	var num int
	scanInto(&flag)
	scanInto(&num)
	fmt.Println(flag, num) // true 42

	fmt.Println(probe(true), probe(7), probe("ab")) // bool one many
}

// probe: an expression-switch INIT inside a type-switch clause re-declares the guard
// name - the clause scope implicitly binds the guard, so the inner v must shadow-rename
// (fmt scanOne's inner `switch v := ptr.Elem(); v.Kind()`, CS0136).
func probe(x any) string {
	switch v := x.(type) {
	case bool:
		_ = v
		return "bool"
	default:
		switch v := len(fmt.Sprint(v)); v {
		case 1:
			return "one"
		default:
			return "many"
		}
	}
}

// scanInto: POINTER case clauses write through the case var (`*t = ...`) - a star-deref
// LHS is existing storage, never a declaration, even though the clause-scoped binding has
// no reassignment record (fmt scanOne took a spurious `var` prefix - CS1003 x18).
func scanInto(v any) {
	switch t := v.(type) {
	case *bool:
		*t = true
	case *int:
		*t = 42
	}
}

// sizeOf: SIBLING case clauses each declare `sz :=` - each case body is its own braced
// scope in the emitted C#, so BOTH need `var` (the shared-scope analysis made the second a
// bare reassignment - CS0103 x2, poll sockaddrToRaw).
func sizeOf(v any) int {
	switch t := v.(type) {
	case int32:
		sz := int(t) + 1
		return sz
	case int64:
		sz := int(t) + 2
		return sz
	}
	return 0
}
