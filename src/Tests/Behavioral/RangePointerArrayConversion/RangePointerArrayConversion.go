package main

import (
	"fmt"
	"unsafe"
)

func main() {
	a := [3]int{10, 20, 30}
	p := unsafe.Pointer(&a)
	sum := 0
	// (*[3]int)(p) converts an unsafe.Pointer to *[3]int — renders as a C# cast (ж<...>)(uintptr)(...).
	for i, x := range (*[3]int)(p) {
		sum += i + x
	}
	fmt.Println(sum)
}
