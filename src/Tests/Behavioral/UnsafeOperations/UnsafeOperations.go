package main

import (
	"fmt"
	"unsafe"
)

type T1 struct {
	a int32
}

type T2 struct {
	a int32
}

func Float64bits(f float64) uint64 {
	return *(*uint64)(unsafe.Pointer(&f))
}

func Float64frombits(b uint64) float64 {
	return *(*float64)(unsafe.Pointer(&b))
}

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

	var t1 T1
	t1.a = 42

	// Convert t1 to type T2
	t2 := *(*T2)(unsafe.Pointer(&t1))
	fmt.Println("Value of t2.a:", t2.a)

	var i int8 = -1
	var j = int16(i)
	fmt.Println(i, j)
	var k uint8 = *(*uint8)(unsafe.Pointer(&i))
	fmt.Println(k)

	var x struct {
		a int64
		b bool
		c string
	}
	const M, N = unsafe.Sizeof(x.c), unsafe.Sizeof(x)
	fmt.Println(M, N)

	fmt.Println(unsafe.Alignof(x.a))
	fmt.Println(unsafe.Alignof(x.b))
	fmt.Println(unsafe.Alignof(x.c))

	fmt.Println(unsafe.Offsetof(x.a))
	fmt.Println(unsafe.Offsetof(x.b))
	fmt.Println(unsafe.Offsetof(x.c))

	i2 := Float64bits(9.5)
	f2 := Float64frombits(i2)
	fmt.Println(i2)
	fmt.Println(f2)
}
