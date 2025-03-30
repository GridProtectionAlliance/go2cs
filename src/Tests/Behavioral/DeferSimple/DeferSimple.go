package main

import "fmt"

func main() {
	fmt.Println("Open file")
	defer fmt.Println("Close file")
	fmt.Println("Write data to file")
}
