package main

import "fmt"

type Person struct {
    name string
    age int32
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

func main() {
    person := Person{name: "Michał", age: 29}
    fmt.Println(person)  // {Michał 29}
    record := Record{}
    record.name = "Michał"
    record.age = 29
    record.position = "software engineer"

    fmt.Println(record) // {{Michał 29} {software engineer}}
    fmt.Println(record.name) // Michał
    fmt.Println(record.age) // 29
    fmt.Println(record.position) // software engineer
    fmt.Println(record.IsAdult()) // true
    fmt.Println(record.IsManager()) // false
}