// Regression test: a native-sized integer constant (nint/nuint, incl. the uintptr alias) whose
// value does not fit a C# constant of that type — `const MaxU = ^uintptr(0)` = 0xFFFF...FFFF, a
// ulong literal needing a non-constant nuint conversion — must be emitted as `static readonly`
// with an unchecked cast, not `const` (which fails CS0133/CS0266). This is the
// runtime/internal/math `MaxUintptr` pattern.
package main

import "fmt"

const MaxU = ^uintptr(0) // max uintptr (0xFFFFFFFFFFFFFFFF on 64-bit)

func main() {
	var zero uintptr = 0
	var one uintptr = 1
	fmt.Println(MaxU > zero)         // true
	fmt.Println(MaxU+one == zero)    // true (uintptr wraps)
	fmt.Println(MaxU-one < MaxU)     // true
}
