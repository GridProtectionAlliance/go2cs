package main

import "fmt"

type Vertex struct {
	X int
	Y int
}

func main() {
	var a [2]string
	
	a[0] = "Hello"
	a[1] = "World"
	
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

	i, j := 42, 2701

	p := &i         // point to i
	fmt.Println(*p) // read i through the pointer
	*p = 21         // set i through the pointer
	fmt.Println(i)  // see the new value of i

	p = &j         // point to j
	*p = *p / 37   // divide j through the pointer
	fmt.Println(j) // see the new value of 

	v := Vertex{1, 2}
	p2 := &v
	p2.X = 1e9
	fmt.Println(v)

	p3 := &Vertex{1, 3} // has type *Vertex
	fmt.Println(p3)

	q := []int{2, 3, 5, 7, 11, 13}
	fmt.Println(q)

	r := []bool{true, false, true, true, false, true}
	fmt.Println(r)

	s := []struct {
		i int
		b bool
	}{
		{2, true},
		{3, false},
		{5, true},
		{7, true},
		{11, false},
		{13, true},
	}
	fmt.Println(s)
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