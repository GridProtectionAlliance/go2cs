// UnsafeBuiltinIntegerLen guards calling the unsafe builtins unsafe.Slice / unsafe.String
// (and unsafe.Add) with a length/offset of ANY integer type — Go's `IntegerType`
// constraint, which includes uintptr / uint. golib's Add/Slice/String took a plain `nint`
// length, so a `uintptr`/`uint` argument failed (CS1503). The length parameter is now a
// generic `IBinaryInteger`, truncated to the int offset. Mirrors runtime's
// `unsafe.Slice(p, uintptrLen)` / `unsafe.String(p, uintptrLen)` / `unsafe.Add(p, uintptr)`.
package main

import (
	"fmt"
	"unsafe"
)

func main() {
	arr := [5]int32{10, 20, 30, 40, 50}

	var n uintptr = 3
	s := unsafe.Slice(&arr[0], n) // uintptr length
	fmt.Println(len(s), s[0], s[2]) // 3 10 30

	var m uint = 2
	s2 := unsafe.Slice(&arr[0], m) // uint length
	fmt.Println(len(s2), s2[1]) // 2 20

	bytes := []byte("hello")
	str := unsafe.String(&bytes[0], uintptr(3)) // uintptr length
	fmt.Println(str) // hel
}
