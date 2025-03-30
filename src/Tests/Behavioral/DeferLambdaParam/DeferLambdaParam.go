package main

import "fmt"

func main() {
	count := 1
	defer func(cnt int) {
		fmt.Println("Deferred count (closure):", cnt)
	}(count)
	count = 10
	fmt.Println("Count before defer:", count)
}
