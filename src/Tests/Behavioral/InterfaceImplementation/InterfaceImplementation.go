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

	// An `any` (EMPTY interface) compared with a concrete string via != — encoding/gob's
	// registerName `n != name`, where n is `any` from sync.Map.LoadOrStore. Go compares the
	// interface's dynamic value; C# has no operator between `object` and the golib `@string`
	// value type (CS0019), so the converter routes empty-interface-vs-concrete through AreEqual.
	var stored any = "gob"
	fmt.Printf("any cmp = %v %v\n", stored != "gob", stored != "xml") // false true

	checkErr(1) // got again + switch: again
	checkErr(0) // not again + switch: nil
	useAndRelease()

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

// errno mirrors syscall.Errno: a named numeric implementing the error interface. Comparing an
// error INTERFACE against the concrete value (`err == errAgain`) has no C# operator between the
// interface and the implementing struct - the converter emits AreEqual, whose boxed same-type
// value compare reproduces Go's dynamic type+value interface equality.
type errno uintptr

func (e errno) Error() string {
	return "errno"
}

const errAgain errno = 11

func mayFail(n int) error {
	if n > 0 {
		return errAgain
	}
	return nil
}

func checkErr(n int) {
	err := mayFail(n)
	if err == errAgain {
		fmt.Println("got again")
	}
	if err != errAgain {
		fmt.Println("not again")
	}
	// A switch on the error INTERFACE with concrete Errno-style case labels routes each case
	// equality through AreEqual (no operator exists between the interface and the implementing
	// struct - syscall's mkErr switches).
	switch err {
	case errAgain:
		fmt.Println("switch: again")
	case nil:
		fmt.Println("switch: nil")
	default:
		fmt.Println("switch: other")
	}
}

// release returns a value like syscall's CloseHandle - a DEFERRED call to it discards the
// result, requiring golib deferǃ Func twins (a value-returning method group cannot bind the
// Action overloads, CS0407).
func release(e errno) error {
	fmt.Println("released", uintptr(e))
	return errAgain
}

func useAndRelease() {
	defer release(errAgain)
	fmt.Println("using")
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
