package main

import (
	"fmt"
	"strings"
)

type Person struct {
	name string
	age  int32
}

func (p Person) IsDr() bool {
	return strings.HasPrefix(p.name, "Dr")
}

func (p Person) IsAdult() bool {
	return p.age >= 18
}

type Employee struct {
	position string
}

func (e Employee) IsManager() bool {
	return e.position == "manager"
}

type Record struct {
	Person
	Employee
}

func (p Record) IsDr() bool {
	return strings.HasPrefix(p.name, "Dr") && p.age > 18
}

// commonBase carries a POINTER-receiver method `describe`. Since it does not take the address
// of a receiver field it is emitted as a value-ref extension, so promotion into an embedder
// generates BOTH a value and a box overload.
type commonBase struct {
	tag string
}

func (c *commonBase) describe() string {
	return "base:" + c.tag
}

// ledger embeds commonBase and declares its OWN pointer-receiver `describe` that takes the
// address of a receiver field (`&l.seq`) — the converter emits it ONLY as a box primary
// (`describe(this ж<ledger> …)`). That box form is invisible to the value-receiver-only
// promotion-shadowing check, so commonBase.describe's promoted BOX overload used to duplicate
// it (CS0111 — encoding/gob's structType embedding CommonType, both declaring string()).
type ledger struct {
	commonBase
	seq int
}

func (l *ledger) describe() string {
	p := &l.seq
	*p++
	return "ledger:" + l.tag // reads the promoted commonBase.tag field
}

func main() {
	person := Person{name: "Dr. Michał", age: 29}
	fmt.Println(person)        // {Dr. Michał 29}
	fmt.Println(person.IsDr()) // true

	record := Record{}
	record.name = "Dr. Michał"
	record.age = 18
	record.position = "software engineer"

	fmt.Println(record)             // {{Dr. Michał 18} {software engineer}}
	fmt.Println(record.name)        // Dr. Michał
	fmt.Println(record.age)         // 18
	fmt.Println(record.position)    // software engineer
	fmt.Println(record.IsAdult())   // true
	fmt.Println(record.IsManager()) // false
	fmt.Println(record.IsDr())      // false

	l := &ledger{}
	l.tag = "x"
	fmt.Println(l.describe(), l.describe(), l.seq) // ledger:x ledger:x 2
}
