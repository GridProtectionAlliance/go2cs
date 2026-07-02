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

	// Go `uint` and `uintptr` are DISTINCT types (both may appear in one Go type switch) but map
	// to the SAME C# type (nuint; `uintptr` is a using-alias of System.UIntPtr), making the later
	// case unreachable (CS8120; runtime error.go's printpanicval). With byte-IDENTICAL Go bodies
	// the converter MERGES them — a marker comment replaces the duplicate label, and the earlier
	// label routes both dynamic types to the shared body, which is exact. DIFFERING bodies keep
	// both labels: the compile error is preferred over silently routing one Go case into the
	// other's body — that variant cannot compile, so it is documented here rather than exercised.
	kind := func(i interface{}) {
		switch i.(type) {
		case uint:
			fmt.Println("word")
		case uintptr:
			fmt.Println("word")
		case string:
			fmt.Println("text")
		default:
			fmt.Println("other")
		}
	}
	var u uint = 5
	var p uintptr = 6
	kind(u)    // word
	kind(p)    // word — routed by the merged (earlier) case
	kind("x")  // text
	kind(3.14) // other
}
