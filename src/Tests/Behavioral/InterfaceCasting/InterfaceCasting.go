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

// Counter has POINTER-receiver methods; Inc takes the address of a receiver field, so its
// C# emission is direct-zh (box receiver). An interface value created from &Counter (Go: the
// interface holds the POINTER) must alias the original: mutations through the interface are
// visible through the pointer and vice versa, and a type assert back to *Counter recovers the
// same pointer. The pointer-sourced GoImplement adapter models exactly this.
type Counter struct {
	n int
}

func addTo(p *int, delta int) {
	*p += delta
}

func (c *Counter) Inc() string {
	addTo(&c.n, 1)
	return "inc"
}

func (c *Counter) Total() int {
	return c.n
}

type Incrementer interface {
	Inc() string
	Total() int
}

func main() {
	var err error

	err = MyError{"bar"}

	fmt.Printf("%v %v\n", f(), err) // error: foo

	animals := []Animal{new(Dog), new(Cat), Llama{}, JavaProgrammer{}}
	for _, animal := range animals {
		fmt.Println(animal.Speak())
	}

	c := &Counter{}
	var inc Incrementer = c // interface value holds the POINTER
	inc.Inc()
	inc.Inc()
	fmt.Println("via pointer:", c.Total()) // 2 - interface calls mutated the original

	c.n = 10
	fmt.Println("via interface:", inc.Total()) // 10 - pointer writes visible through interface

	back, ok := inc.(*Counter) // assert back to the pointer
	back.Inc()
	fmt.Println("assert-back:", ok, c.Total(), back == c) // true 11 true
}
