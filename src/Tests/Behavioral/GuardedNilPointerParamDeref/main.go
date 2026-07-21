package main

import "fmt"

// digits mirrors text/scanner's shape: an optional out-param `*int` deref'd ONLY under a value
// guard (`i >= base`), and called once with a real pointer and once with nil. Go never derefs the
// nil because the guard is false on that path.
func digits(base int, invalid *int) int {
	n := 0
	for i := 0; i < 5; i++ {
		if i >= base && *invalid == 0 {
			*invalid = i
		}
		n++
	}
	return n
}

func main() {
	x := 0
	c1 := digits(3, &x)
	fmt.Println(c1, x)
	c2 := digits(10, nil)
	fmt.Println(c2)
}
