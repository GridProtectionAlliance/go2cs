// File Z (sorts AFTER main.go): declares a package-level slice of an ANONYMOUS struct
// type whose fields include slices (`[]byte`). main.go's range over it is visited BEFORE
// this declaration, so the lifted name is not yet in the shared registry at that point —
// the reference must defer through the post-barrier dynamic-type marker. Mirrors bytes'
// `compareTests` (declared in compare_test.go, ranged from the earlier-sorted
// bytes_test.go), whose raw `struct{a []byte; …}` emission was the B8 CS1526 cascade.
package main

var compareTests = []struct {
	a, b []byte
	i    int
}{
	{[]byte(""), []byte(""), 0},
	{[]byte("a"), []byte(""), 1},
	{[]byte(""), []byte("a"), -1},
	{[]byte("abc"), []byte("abc"), 0},
	{[]byte("ab"), []byte("x"), -1},
}
