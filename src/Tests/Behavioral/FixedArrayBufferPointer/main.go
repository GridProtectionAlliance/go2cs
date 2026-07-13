package main

import (
	"fmt"
	"unsafe"
)

// Guards the golib fix that makes unsafe.Pointer(&arr), for a Go fixed array, reference the array's
// DATA (pinned) rather than the transient address of the array<T> wrapper struct. This is the idiom
// go-isatty.IsCygwinTerminal uses: a Windows syscall fills a [262]uint16 buffer, then the code reads
// FILE_NAME_INFO.FileNameLength back via *(*uint32)(unsafe.Pointer(&buf)). Before the fix the wrong
// address corrupted the wrapper and faulted (AccessViolation) — the fatih/color sample went EMPTY on a
// pipe because IsCygwinTerminal crashed there.
func main() {
	// (1) array write, pointer read: both must address the same backing data. This is the go-isatty
	//     idiom — *(*uint32)(unsafe.Pointer(&buf)) reads the first 4 bytes of the array as a uint32.
	var buf [8]uint16
	buf[0] = 0x3412
	buf[1] = 0x7856
	l := *(*uint32)(unsafe.Pointer(&buf))
	fmt.Println(l) // 2018915346 == 0x78563412 (little-endian)

	// (2) the array is unharmed and still readable through its own indexer afterward (go-isatty then
	//     slices buf[2:] for the file name) — the box's backing data was pinned, not corrupted.
	fmt.Println(buf[0], buf[2]) // 13330 0

	// (3) the address is stable across repeated conversions (one cached pin per box).
	a := uintptr(unsafe.Pointer(&buf))
	b := uintptr(unsafe.Pointer(&buf))
	fmt.Println(a == b) // true
}
