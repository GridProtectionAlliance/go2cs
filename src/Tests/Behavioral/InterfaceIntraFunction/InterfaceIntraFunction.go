package main

import "fmt"

type Message struct {
	Text string
}

// Method must be defined at package level, not inside a function!
func (m Message) Print() {
	fmt.Println(m.Text)
}

func main() {
	// Define an interface inside the function
	type Printer interface {
		Print()
	}

	// Instantiate and use the interface
	var p Printer = Message{"Hello, from a function-scoped interface!"}
	p.Print()
}
