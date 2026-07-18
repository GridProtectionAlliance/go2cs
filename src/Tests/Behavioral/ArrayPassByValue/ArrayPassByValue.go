package main

import "fmt"

func main() {
	var a [2]string

	a[0] = "Hello"
	a[1] = "World"

	p := &a[0]

	test(a)
	fmt.Println(a[0], a[1])
	fmt.Println()

	a[0] = "Hello"
	test2(&a)
	fmt.Println(a[0], a[1])
	fmt.Println()

	a[0] = "Hello"
	test3(a[:])
	fmt.Println(a[0], a[1])
	fmt.Println()

	primes := [6]int{2, 3, 5, 7, 11, 13}
	fmt.Println(primes)

	fmt.Println(a[0])
	stest(p)
	fmt.Println(a[0])

	assignCopies()
}

var garr = [3]int{1, 2, 3}

type arrHolder struct {
	arr [3]int
}

// Go array ASSIGNMENT also copies the whole array — a write through the copy
// must never reach the source (locals, globals, fields, elements, derefs).
func assignCopies() {
	// Local := local
	src := [3]int{1, 2, 3}
	cpy := src
	cpy[0] = 99
	fmt.Println(src[0], cpy[0])

	// Local := global
	d := garr
	d[0] = 77
	fmt.Println(garr[0], d[0])

	// var form
	var e = garr
	e[1] = 88
	fmt.Println(garr[1], e[1])

	// Plain reassignment over an existing variable
	e = src
	e[2] = 66
	fmt.Println(src[2], e[2])

	// From a struct field (selector RHS)
	h := arrHolder{arr: [3]int{4, 5, 6}}
	f := h.arr
	f[0] = 55
	fmt.Println(h.arr[0], f[0])

	// Into a struct field (selector LHS)
	h.arr = src
	h.arr[1] = 44
	fmt.Println(src[1], h.arr[1])

	// From an element of an array of arrays (index RHS)
	m := [2][3]int{{7, 8, 9}, {10, 11, 12}}
	row := m[1]
	row[0] = 33
	fmt.Println(m[1][0], row[0])

	// Through a pointer deref (star RHS)
	q := &src
	g := *q
	g[0] = 22
	fmt.Println(src[0], g[0])

	// Swap two arrays by value
	x, y := [2]int{1, 2}, [2]int{3, 4}
	x, y = y, x
	x[0] = 11
	fmt.Println(x[0], x[1], y[0], y[1])
}

func stest(p *string) {
	*p = "hello"
}

// Arrays are passed by value (a full copy)
func test(a [2]string) {
	// Update to array will be local
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}

func test2(a *[2]string) {
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}

func test3(a []string) {
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}
