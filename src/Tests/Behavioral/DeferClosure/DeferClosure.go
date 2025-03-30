package main

import "fmt"

func main() {
	count := 0
	defer func() {
		fmt.Println("Deferred count (closure):", count)
	}()
	count = 10
	fmt.Println("Count before defer:", count)
}
