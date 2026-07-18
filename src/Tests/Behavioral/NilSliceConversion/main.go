package main

import "fmt"

// Exercises []T(nil) conversions: Go converts a nil source to a nil slice, never a
// fault. The append([]T(nil), src...) form is the stdlib's copy idiom (strings, sort).
func main() {
	s := append([]string(nil), "x", "y")
	fmt.Println(len(s), cap(s), s == nil)

	var b []byte
	c := append([]byte(nil), b...)
	fmt.Println(len(c), cap(c), c == nil)

	n := []int(nil)
	fmt.Println(len(n), cap(n), n == nil)

	var zs []string
	t := zs[0:0]
	fmt.Println(len(t), cap(t), t == nil)
}
