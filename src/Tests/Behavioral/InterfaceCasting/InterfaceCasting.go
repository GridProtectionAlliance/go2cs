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

// reversed EMBEDS the Animal interface and overrides nothing - a pointer cast &reversed{a}
// satisfies Animal purely through the promoted embedded-interface field (sort's
// `type reverse struct{ Interface }` shape). The generated pointer adapter must forward
// promoted members through the interface field (m_box.Value.Animal.Speak()), and the
// Promoted flag may live on a sibling GoImplement attribute instance.
type reversed struct {
	Animal
}

func Reversed(a Animal) Animal {
	return &reversed{a}
}

// pick reassigns an INTERFACE local from &local inside a switch - dnsmessage's unpack shape.
// The RHS converts through the pointer-interface adapter (the adapter IS the interface value),
// so the assignment must stay plain (`a = new DogᴵAnimal(Ꮡd)`) - the pointer-box LHS form
// referenced a nonexistent box and ref-realiased a non-ref local (CS0103/CS8373).
func pick(kind int) (Animal, string) {
	var a Animal
	var name string
	switch kind {
	case 0:
		var d Dog
		a = &d
		name = "dog"
	case 1:
		var c Cat
		a = &c
		name = "cat"
	default:
		var l Llama
		a = &l
		name = "llama"
	}
	return a, name
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

	r := Reversed(Dog{})
	fmt.Println("promoted via pointer adapter:", r.Speak()) // Woof!

	for k := 0; k < 3; k++ {
		a, name := pick(k)
		fmt.Println("picked:", name, a.Speak())
	}

	var rd rdr = strRdr{} // populates rdr's implementation set (triggers the inheritance prune)
	fmt.Println(rd.read())
	rc := open("data")
	fmt.Println(rc.read(), rc.close())
	readers := []rdr{strRdr{}}
	readers[0] = fileRdr{name: "x"} // index-element interface assignment (io eofReader shape)
	fmt.Println(readers[0].read())

	// census F8: appending a POINTER value to an interface-typed slice - the *T-to-iface
	// ADAPTER-ctor form (`new CatᴵAnimal(...)`) leaves both builtin append overloads
	// applicable at the adapter class type (CS0121, reflect Method construction x3); the
	// element is cast to the interface type so the slice<T> overload binds.
	var pack []Animal
	pack = append(pack, &Cat{})
	pack = append(pack, Dog{})
	fmt.Println(len(pack), pack[0].Speak(), pack[1].Speak())
}

// rdCloser mirrors io.ReadCloser: an interface INHERITING other interfaces. A concrete type
// returned as the inheriting interface (open below) records GoImplement<fileRdr, rdCloser>;
// the package_info inheritance prune must drop only the COMMON implementations from the LOWER
// interface (fileRdr from rdr), never the derived-only recordings (the in-place intersect
// emptied rdCloser's whole set - io NopCloser CS0029).
type rdr interface{ read() string }
type clsr interface{ close() string }

type rdCloser interface {
	rdr
	clsr
}

type strRdr struct{}

func (strRdr) read() string { return "strRdr" }

type fileRdr struct{ name string }

func (f fileRdr) read() string  { return "read:" + f.name }
func (f fileRdr) close() string { return "close:" + f.name }

func open(name string) rdCloser { return fileRdr{name} }
