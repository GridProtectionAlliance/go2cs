package main

import "fmt"

type MyError struct {
	description string
}

func (err MyError) Error() string {
	return fmt.Sprintf("error: %s", err.description)
}

// error is an interface - MyError is cast to error interface upon return
func f() error {
	return MyError{"foo"}
}

type Animal interface {
	Speak() string
}

type Dog struct {
}

func (d Dog) Speak() string {
	return "Woof!"
}

type Cat struct {
}

func (c *Cat) Speak() string {
	return "Meow!"
}

type Llama struct {
}

func (l Llama) Speak() string {
	return "?????"
}

type JavaProgrammer struct {
}

func (j JavaProgrammer) Speak() string {
	return "Design patterns!"
}

func main() {
	var err error

	err = MyError{"bar"}

	fmt.Printf("%v %v\n", f(), err) // error: foo

	animals := []Animal{new(Dog), new(Cat), Llama{}, JavaProgrammer{}}
	for _, animal := range animals {
		fmt.Println(animal.Speak())
	}
}
