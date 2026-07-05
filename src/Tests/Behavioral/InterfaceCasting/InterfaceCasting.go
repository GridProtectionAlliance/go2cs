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

func makeCounter() (*Counter, error) {
	return &Counter{n: 5}, nil
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

// Self returns the POINTER RECEIVER as an interface - the adapter must wrap the box
// (new CatᴵAnimal(Ꮡc)), not the deref-aliased receiver (io/fs subFS.Sub, CS1503).
func (c *Cat) Self() Animal { return c }

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
	cp := &Cat{}
	fmt.Println(cp.Self().Speak())

	// Writing a POINTER through a *interface param: `*a = &Cat{}` assigns a ж<Cat> into
	// the deref'd interface storage — the RHS must take the pointer-adapter wrap (dwarf
	// zeroArray's `*t = &tt` with t *Type, CS0266). Interface-ness of a star-deref LHS
	// comes from the deref'd type, not the pointer ident.
	var swapped Animal = Dog{}
	replaceAnimal(&swapped)
	fmt.Println("replaced:", swapped.Speak()) // Meow!

	// A tuple DECONSTRUCTION whose call component needs the pointer-adapter interface
	// conversion: `inc2, err = makeCounter()` assigns a *Counter into an Incrementer —
	// the C# tuple assignment can't convert implicitly, so the call hoists into temps and
	// each component converts (net dial.go's `c, err = sd.dialTCP(…)` with `var c Conn`,
	// CS0266 x11).
	var inc2 Incrementer
	inc2, err = makeCounter()
	inc2.Inc()
	fmt.Println("deconstructed into iface:", inc2.Total(), err == nil) // 6 true

	// An interface-returning LITERAL whose arms return DISTINCT concrete types has no C#
	// best-common-type — the lambda states its return type explicitly (net ipsock.go's
	// `inetaddr := func(ip IPAddr) Addr` with three adapter-class arms, CS8917).
	makeAnimal := func(feline bool) Animal {
		if feline {
			return &Cat{}
		}
		return Dog{}
	}
	fmt.Println("made:", makeAnimal(true).Speak(), makeAnimal(false).Speak()) // made: Meow! Woof!

	fmt.Println("plumbed:", runPlumbing()) // plumbed: n1:b:x,b:y

	// wrapSink embeds an INTERFACE field (zip's nopCloser{io.Writer}): Speak comes from
	// the field's interface value, Shut is the struct's own. A POINTER cast to the wider
	// local interface must forward Speak through the FIELD in the generated adapter
	// (m_box.Value.Animal.Speak() — CS1929 with no forward).
	var ss speakShutter = &wrapSink{Animal: Dog{}}
	fmt.Println(ss.Speak(), ss.Shut()) // Woof! shut

	// An interface method whose name is a C# KEYWORD (`string`/`int`), implemented by a
	// POINTER receiver — encoding/gob's `gobType.string()` shape. The generated pointer
	// adapter must @-escape the method in BOTH the explicit-interface signature and the
	// forwarding call; bare `labeler.string()` was a C# parse error (CS1525/CS0539/CS0501).
	var lb labeler = &badge{text: "id", num: 9}
	fmt.Println("keyword-method:", lb.string(), lb.int()) // keyword-method: id 9
}

// labeler has C#-keyword method names (`string`, `int`) — legal Go identifiers.
type labeler interface {
	string() string
	int() int
}

type badge struct {
	text string
	num  int
}

func (b *badge) string() string { return b.text }
func (b *badge) int() int       { return b.num }

func replaceAnimal(a *Animal) {
	*a = &Cat{}
}

type speakShutter interface {
	Speak() string
	Shut() string
}

type wrapSink struct {
	Animal
}

func (w wrapSink) Shut() string { return "shut" }

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

// plumbing/sinks: two composite/variadic recording guards (image/gif). plumbing's FIRST
// field is an interface; a KEYED literal setting only a LATER field must not pair that
// value with field s (encoder{g: *g} recorded a bogus GoImplement<GIF, writer>, CS1929 x3).
// sinks is a named slice of the interface; append's SPREAD arg passes elements directly —
// wrapping it in a single element-adapter was CS1061 + a bogus GoImplement (CS1503).
type sink interface{ drain() string }

type basin struct{ tag string }

func (b basin) drain() string { return "b:" + b.tag }

type plumbing struct {
	s    sink
	name string
}

type sinks []sink

func runPlumbing() string {
	p := plumbing{name: "n1"}
	batch := sinks{basin{tag: "x"}, basin{tag: "y"}}
	var all []sink
	all = append(all, batch...)
	return p.name + ":" + all[0].drain() + "," + all[1].drain()
}
