package main

import (
	"fmt"
)

func main() {

	i := 0
	
	for ; i < 10; {
		f(i)
		 i++
	}
	
	i = 0
	
	for ; i < 10; i++ {
		f(i)
	}

	for i := 0; i < 10; i++ {
		f(i)
	}
}

func f(y int) {
	fmt.Println(y)
}
