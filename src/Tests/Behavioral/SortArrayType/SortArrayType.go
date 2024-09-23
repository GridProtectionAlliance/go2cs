// Pre-package Comments

// Package Comments
package main

// Pre-comment
import "fmt"  // EOL comment
import "sort" // EOL comment

/* Post comment */
// Post comment 2
// Post comment 3

const (
    // A1 is a constant
    A1, B = iota, iota * 100  // 0, 0
    _, D                      // 1, 100
    // E211 is a constant
    E211, F                   // 2, 200
)

var (
    // A2 is a variable
	A2, B2 = 1, "42"          // 1, "42"
	C21 = dynamicFn()         // 3
    // D21 is a variable
	D21, E2 bool             // false, false
)
func dynamicFn() int { return 4 }

// Person is a type representing a person
// with a name, age, and shoe size
type Person struct {
    Name string      // Name of the person
    Age int `Tag`    // Age of the person
    ShoeSize float32 // Shoe size of the person
}
// Another one

type PeopleByShoeSize []Person // Person slice for shoe size sorting

type PeopleByAge []Person

func (p PeopleByShoeSize) Len() int {
    return len(p)
}

func (p PeopleByShoeSize) Swap(i, j int) {
    p[i], p[j] = p[j], p[i]
}

func (p PeopleByShoeSize) Less(i, j int) bool {
    return (p[i].ShoeSize < p[j].ShoeSize)
}

func (p PeopleByAge) Len() int {
    return len(p)
}

func (p PeopleByAge) Swap(i, j int) {
    p[i], p[j] = p[j], p[i]
}

func (p PeopleByAge) Less(i, j int) bool {
    return (p[i].Age < p[j].Age)
}

func main() {
    people := []Person {
    {
        Name: "Person1",
        Age: 26,
        ShoeSize: 8,
    },
    {
        Name: "Person2",
        Age: 21,
        ShoeSize: 4,
    },
    {
        Name: "Person3",
        Age: 15,
        ShoeSize: 9,
    },
    {
        Name: "Person4",
        Age: 45,
        ShoeSize: 15,
    },
    {
        Name: "Person5",
        Age: 25,
        ShoeSize: 8.5,
    }}

    fmt.Println(people)
    
    sort.Sort(PeopleByShoeSize(people))
    fmt.Println(people)
    
    sort.Sort(PeopleByAge(people))
    fmt.Println(people)
}

// Post code comments
