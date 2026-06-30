package main

import (
	"fmt"
	"time"
)

func main() {

	i := 0

	for i < 10 {
		// Inner comment
		f(&i) // Call function
		// Increment i
		i++ // Post i comment
	} // Post for comment

	fmt.Println()
	fmt.Println("i =", i)

	for i = 0; i < 10; i++ {
		f(&i)

		for j := 0; j < 3; j++ {
			fmt.Println(i + j)
		}
		fmt.Println()
	}

	fmt.Println("i =", i)
	fmt.Println()

out:
	for i := 0; i < 5; i++ {
		// a
		f(&i) // b

		for i := 12; i < 15; i++ {
			f(&i)
			break out
		} //c

		if i > 13 {
			continue out
		}

		fmt.Println()
	} //d

	fmt.Println()
	fmt.Println("i =", i)
	fmt.Println()

	// Labeled RANGE loop: a `continue scan`/`break scan` from a nested loop targets the
	// outer labeled range loop. The converter emits `goto continue_scan`/`goto break_scan`,
	// so the `continue_scan:`/`break_scan:` labels must be placed (regression for CS0159).
	nums := []int{1, 2, 3, 4}
scan:
	for _, n := range nums {
		for _, m := range nums {
			if n == m {
				continue scan
			}
			if n+m > 5 {
				break scan
			}
			fmt.Println("pair", n, m)
		}
	}

	fmt.Println()

	x := 99
	fmt.Println("i before thread and", i, "x before thread", x)
	go fmt.Println("i from thread and", i, "x from thread", x)

	for {
		i++
		x++
		f(&i)

		if i > 12 {
			break
		}
	}

	fmt.Println()
	fmt.Println("i =", i)
	fmt.Println("x = ", x)
	
	// Wait for go routines to complete
	time.Sleep(1)

	fmt.Println("i after thread and", i, "x after thread", x)
}

func f(y *int) {
	fmt.Print(*y)
}
