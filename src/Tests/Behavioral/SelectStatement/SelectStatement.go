package main

import (
	"fmt"
)

func g1(ch chan int) {
	ch <- 12
}

func g2(ch chan int) {
	ch <- 32
}

func sum(s []int, c chan int) {
	sum := 0
	for _, v := range s {
		sum += v
	}
	c <- sum // send sum to c
}

func fibonacci(f, quit chan int) {
	x, y := 0, 1
	for {
		select {
		case f <- x:
			x, y = y, x+y
		case <-quit:
			fmt.Println("quit")
			return
		}
	}
}

func sendOnly(s chan<- string) {
	s <- "output"
}

func main() {
	ch := make(chan int, 2)

	ch <- 1
	ch <- 2

	fmt.Println(<-ch)
	fmt.Println(<-ch)

	ch1 := make(chan int)
	ch2 := make(chan int)
	ch3 := make(chan int)

	go g1(ch1)
	go g2(ch2)
	go g1(ch3)

	for i := 0; i < 3; i++ {
		select {
		case v1 := <-ch1:
			fmt.Println("Got: ", v1)
		case v1 := <-ch2:
			fmt.Println("Got: ", v1)
		case v1, ok := <-ch3:
			fmt.Println("OK: ", ok, " -- got: ", v1)
			//default:
			//    fmt.Println("Default")
		}
	}

	s := []int{7, 2, 8, -9, 4, 0}

	c := make(chan int)
	go sum(s[:len(s)/2], c)
	go sum(s[len(s)/2:], c)
	go sum(s[2:5], c)
	x, y := <-c, <-c // receive from c
	z := <-c
	fmt.Println(x, y, x+y, z)

	f := make(chan int)
	quit := make(chan int)
	go func() {
		for i := 0; i < 10; i++ {
			fmt.Println(<-f)
		}
		quit <- 0
	}()
	fibonacci(f, quit)

	mychanl := make(chan string)

	// function converts bidirectional channel to send only channel
	go sendOnly(mychanl)

	result, ok := <-mychanl
	fmt.Println(result, ok)
}
