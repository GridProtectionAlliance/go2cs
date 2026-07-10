package main

import "fmt"

type speaker interface {
	speak() string
}

type dog struct {
	name string
}

func (d dog) speak() string {
	return "woof:" + d.name
}

type cat struct {
	name string
}

func (c *cat) speak() string {
	return "meow:" + c.name
}

func describe(prefix string, v any) {
	switch s := v.(type) {
	case string:
		fmt.Println(prefix, "string:", s)
	case int:
		fmt.Println(prefix, "int:", s)
	default:
		fmt.Println(prefix, "other:", s)
	}
}

func main() {
	// A string LITERAL sent into an empty-interface element channel — statement form.
	ch := make(chan any, 2)
	ch <- "text"
	ch <- 42
	describe("send", <-ch)
	describe("send", <-ch)

	// Select-case send (no default — the registration form).
	sel := make(chan any, 1)
	select {
	case sel <- "sel":
		describe("select", <-sel)
	}

	// A string literal into a plain string-element channel keeps the direct form (control).
	sc := make(chan string, 1)
	sc <- "plain"
	fmt.Println("string chan:", <-sc)

	// Non-empty interface element: a value impl sends bare (the generator adds the
	// interface); a pointer impl wraps in its pointer adapter.
	vs := make(chan speaker, 1)
	vs <- dog{name: "rex"}
	fmt.Println((<-vs).speak())

	ps := make(chan speaker, 1)
	c := &cat{name: "tom"}
	ps <- c
	fmt.Println((<-ps).speak())

	// Type-assert identity of the received literal (the latent hazard: a bare C# string
	// boxed into any misses both dispatch forms).
	rt := make(chan any, 1)
	rt <- "assert"
	got := <-rt
	if s, ok := got.(string); ok {
		fmt.Println("assert string:", s)
	} else {
		fmt.Println("assert missed")
	}
}
