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
}
