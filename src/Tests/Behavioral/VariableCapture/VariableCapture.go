package main

import "fmt"

type data struct {
	name string
}

func (d data) printName() {
	fmt.Println("Name =", d.name)
}

func main() {
	d := data{name: "James"}
	f1 := d.printName
	f1()
	d.name = "Gretchen"
	f1()
}
