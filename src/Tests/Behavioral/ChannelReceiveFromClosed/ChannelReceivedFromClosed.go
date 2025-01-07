package main

import "fmt"

func main() {
        	c := make(chan int, 3)
	        c <- 1
        	c <- 2
	        c <- 3
	        close(c)
	        for i := 0; i < 4; i++ {
		                fmt.Printf("%d ", <-c) // prints 1 2 3 0
	        }
}