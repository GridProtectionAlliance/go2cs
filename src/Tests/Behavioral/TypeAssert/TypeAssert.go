package main

import "fmt"

func main() {
	// Safe assertions with 'ok'
	safeAssertions()

	// Unsafe assertion with recover
	assertionsWithPanic()

	fmt.Println("Program completed after panic recovery")
}

func safeAssertions() {
	var i interface{} = "hello"

	// Type assertion with ok check
	if s, ok := i.(string); ok {
		fmt.Println("Value is a string:", s)
	} else {
		fmt.Println("Value is not a string")
	}

	// This will not panic, just set ok to false
	if n, ok := i.(int); ok {
		fmt.Println("Value is an int:", n)
	} else {
		fmt.Println("Value is not an int")
	}
}

func assertionsWithPanic() {
	// This function will be called when defer executes, after the panic
	defer func() {
		if r := recover(); r != nil {
			fmt.Println("Recovered from panic:", r)
		}
	}()

	var i interface{} = "hello"

	// Safe type assertion
	s := i.(string)
	fmt.Println("String value:", s)

	// This will cause a panic, but we'll recover from it
	n := i.(int) // This line will panic

	// Code below the panic will not execute
	fmt.Println("Integer value:", n)
}
