package main

import (
	"fmt"
)

func main() {

	i := 0
	
	for ; i < 10; {
	    // Inner comment
		f(i) // Call function
		// Increment i
		i++ // Post i comment
	} // Post for comment
	
	fmt.Println()
	fmt.Println("i =", i)
	fmt.Println()
	
	i = 0
	
	for ; i < 10; i++ {
		f(i)
		
		for j := 0; j < 3; j++ {
			f(i + j)
		}
	    fmt.Println()
	}
	
	fmt.Println("i =", i)
	fmt.Println()

	for i := 0; i < 5; i++ {
		// a
		f(i) // b
	} //c
	
	fmt.Println()
	fmt.Println("i =", i)
	fmt.Println()
	
	for {
		i++
		f(i)
		
		if i > 12 {
		    break
		}
	}
	
	fmt.Println()
	fmt.Println("i =", i)
}

func f(y int) {
	fmt.Print(y)
}
