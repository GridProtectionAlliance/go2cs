package main

import (
	"fmt"
)

func main() {
	defer fmt.Println("First")
	defer fmt.Println("Second")
	defer fmt.Println("Third")

	f1 := fmt.Println
	defer f1("Fourth")

	defer GetPrintLn()("Fifth")

	fmt.Println("Main function")
}

func GetPrintLn() func(string) {
	return func(src string) {
		fmt.Println(src)
	}
}
