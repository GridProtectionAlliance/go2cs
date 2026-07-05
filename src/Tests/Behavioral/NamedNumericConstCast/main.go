// Regression test: a package-level const whose value folds to a value beyond int32 range and is
// declared as a NAMED type over a wide unsigned integer (uint / uint64 / uintptr) — go/constant's
// `_m = ^big.Word(0)` and x/text/unicode/bidi's `unknownClass = ^Class(0)`.
//
// `^Named(0)` folds to the underlying's all-ones value (`18446744073709551615`, a C# `ulong`
// literal), which has no implicit conversion to the named `[GoType]` wrapper — CS0266. The const
// declaration must emit an unchecked cast: `unchecked((Class)18446744073709551615)`. A small named
// const (`Class(5)`) still fits the wrapper's int operator and keeps the bare literal (no cast).
package main

import "fmt"

type Class uint
type Big uint64

const (
	allClass = ^Class(0)
	allBig   = ^Big(0)
	small    = Class(5)
)

func main() {
	fmt.Println(uint64(allClass))
	fmt.Println(uint64(allBig))
	fmt.Println(uint(small))
}
