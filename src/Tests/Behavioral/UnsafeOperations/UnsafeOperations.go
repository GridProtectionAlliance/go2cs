package main

import (
	"fmt"
	"unsafe"
)

func main() {
	b := []byte{}

	for ch := 32; ch < 80; ch++ {
		b = append(b, string(rune(ch))...)
	}

	str := unsafe.String(&b[0], len(b))
	fmt.Println(str)

	ptr := unsafe.StringData(str)
	fmt.Println("ptr =", ptr)
	fmt.Println(unsafe.String(ptr, len(str)))
}
