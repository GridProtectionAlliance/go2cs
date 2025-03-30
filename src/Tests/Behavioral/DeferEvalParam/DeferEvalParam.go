package main

import "fmt"

func main() {
	printSquare(5)
}

func printSquare(n int) {
	defer fmt.Println("Deferred square:", n*n)
	n++
	fmt.Println("Immediate n:", n)
}
