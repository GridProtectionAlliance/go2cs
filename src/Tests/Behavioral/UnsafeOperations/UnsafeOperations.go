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

	strptr := unsafe.StringData(str)
	fmt.Println(unsafe.String(strptr, len(str)))

	arr := [4]int{1, 2, 3, 4}
	arrptr := &arr[0]

	// Move the pointer to the next element in the array
	nextPtr := unsafe.Pointer(uintptr(unsafe.Pointer(arrptr)) + unsafe.Sizeof(arr[0]))
	fmt.Println("Value of the next element:", *(*int)(nextPtr))
}
