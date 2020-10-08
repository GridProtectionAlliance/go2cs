package main

import "fmt"

type Buffer struct {
	buf      []byte
	off      int
	lastRead int8
}

const (
	opRead    int8 = -1
	opInvalid int8 = 0
)

func main() {
	var a int
	var ptr *int
	var pptr **int
	var ppptr ***int

	a = 3000

	/* take the address of var */
	ptr = &a

	/* take the address of ptr using address of operator & */
	pptr = &ptr
	ppptr = &pptr

	/* take the value using pptr */
	fmt.Printf("Value of a = %d\n", a)
	PrintValPtr(ptr)
	fmt.Printf("Main-function return value available at *ptr = %d\n", *EscapePrintValPtr(ptr))
	fmt.Printf("Main-function updated value available at *ptr = %d\n", *ptr)
	PrintValPtr2Ptr(pptr)
	PrintValPtr2Ptr2Ptr(ppptr)

	a = 1900

	fmt.Printf("Value of a = %d\n", a)
	PrintValPtr(ptr)
	fmt.Printf("Main-function return value available at *ptr = %d\n", *EscapePrintValPtr(ptr))
	fmt.Printf("Main-function updated value available at *ptr = %d\n", *ptr)
	PrintValPtr2Ptr(pptr)
	PrintValPtr2Ptr2Ptr(ppptr)
}

func (b *Buffer) Read(p []byte) (n int, err error) {
	b.lastRead = opInvalid
	b.off += n
	if n > 0 {
		b.lastRead = opRead
	}

    (&Buffer{buf: p}).Read(p)

	return n, nil
}

func NewBuffer(buf []byte) (b1 *Buffer) {
	return &Buffer{buf: buf}
}

func PrintValPtr(ptr *int) {
	fmt.Printf("Value available at *ptr = %d\n", *ptr)
	*ptr++
}

func EscapePrintValPtr(ptr *int) *int {
	fmt.Printf("Value available at *ptr = %d\n", *ptr)
	i := 99
	ptr = &i
	fmt.Printf("Intra-function updated value available at *ptr = %d\n", *ptr)
	PrintValPtr(ptr)
	return ptr
}

func PrintValPtr2Ptr(pptr **int) {
	fmt.Printf("Value available at **pptr = %d\n", **pptr)
}

func PrintValPtr2Ptr2Ptr(ppptr ***int) {
	fmt.Printf("Value available at ***pptr = %d\n", ***ppptr)
}
