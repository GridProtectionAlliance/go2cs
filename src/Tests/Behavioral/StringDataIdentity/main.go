// Go compares pointers by ADDRESS: two unsafe.StringData results over the same string data
// are equal even though each call materializes a fresh pointer value. golib models StringData
// as a fresh view over the string's backing storage, so pointer equality must canonicalize
// to the storage identity — strings' TestMap asserts exactly this through Map's identity
// fast path (no rune changed ⟹ the original string, pointer-identical, comes back).
package main

import (
	"fmt"
	"unsafe"
)

func main() {
	s := "identity probe string"
	t := s // header copy — same backing data

	fmt.Println(unsafe.StringData(s) == unsafe.StringData(t)) // true: same data address
	fmt.Println(unsafe.StringData(s) == unsafe.StringData(s)) // true: same call repeated

	u := string(append([]byte(nil), s...)) // runtime copy — distinct backing data
	fmt.Println(unsafe.StringData(s) == unsafe.StringData(u)) // false: different address
	fmt.Println(s == u)                                       // true: equal content regardless
}
