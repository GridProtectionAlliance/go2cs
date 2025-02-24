package main

import "fmt"

type Animal interface {
	Type() string
	Swim() string
}

type Test interface {
}

type Dog struct {
	Name  string
	Breed string
}

type Frog struct {
	Name  string
	Color string
}

func main() {
	f := new(Frog)
	d := new(Dog)
	zoo := [...]Animal{f, d}
	var t Test

	fmt.Printf("Iface cmp result = %v\n", zoo[0] == f)
	fmt.Printf("Iface cmp result = %v\n", zoo[0] == zoo[0])
	fmt.Printf("Iface cmp result = %v\n", zoo[0] != t)

	var a Animal = nil
	fmt.Printf("%T\n", a)

	for _, a := range zoo {
		fmt.Println(a.Type(), "can", a.Swim())
	} // Redclared post comment

	fmt.Printf("%T\n", a)

	ShowZoo(&zoo)

	// Post function comment
	fmt.Printf("%T\n", a)

	// vowels[ch] is true if ch is a vowel
	vowels := [128]bool{'a': true, 'e': true, 'i': true, 'o': true, 'u': true, 'y': true}
	fmt.Println(vowels)
}

func ShowZoo(zoo *[2]Animal) {
	var a Animal = nil

	for _, a = range *zoo {
		fmt.Println(a.Type(), "can", a.Swim())
	}
}

func (f *Frog) Type() string {
	return "Frog"
}

func (f *Frog) Swim() string {
	return "Kick"
}

func (d *Dog) Swim() string {
	return "Paddle"
}

func (d *Dog) Type() string {
	return "Doggie"
}
