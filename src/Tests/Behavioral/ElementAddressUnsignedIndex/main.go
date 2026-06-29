package main

import "fmt"

// The address of a slice element indexed by an UNSIGNED/wide value (`&s[i]` where i is
// uint32/uintptr) emits the golib element-address form `Ꮡ(s, index)`. Its overloads take
// `int`/`nint`, so an unsigned-32-or-wider index must be cast to int (`Ꮡ(s, (int)i)`) — a
// raw `uint`/`uintptr` argument is CS1503. This mirrors the runtime's `&datap.pclntable[funcoff]`
// (funcoff uint32) and `&filetab[fileoff]`.
func main() {
	s := []int{10, 20, 30, 40}

	var i uint32 = 2
	p := &s[i]
	*p = 99

	var j uintptr = 0
	q := &s[j]
	*q = 7

	var k uint = 3
	r := &s[k]
	*r = 55

	fmt.Println(s[0], s[1], s[2], s[3])
}
