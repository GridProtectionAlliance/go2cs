// Pre-package Comments

// Package Comments
package main

// Pre-comment
import (
	"fmt"
	"sort"
) // EOL comment
// EOL comment

/* Post comment */
// Post comment 2
// Post comment 3

const (
	w       float32 = 1.0 // Other
	_addr_X         = 1   // One
	Y               = 2   // Two
	Z               = 3   // Three
)

const (
	// A1 is a constant
	A1, B = iota, iota * 100 // 0, 0
	_, D                     // 1, 100
	// E211 is a constant
	E211, F // 2, 200
	// Giant constant
	Giant  = 1 << 100 // Wow
	Giant2 = 1 << 200 // Wow2
	// String constant
	String  = "Hello"        // Hello
	String2 = "World"        // World
	String3 = "世界 \123\1123" // Extra
	// Float constant
	Float  = 3.14     // 3.14
	Float2 = 3.14e100 // 3.14e100
	// Giant float constant
	GiantFloat = 1e309 // 1e309
	// MultiLine constant (spaces EOL)
	MultiLine = `
        Line1 /123
        Line2 ""Yo""
        Line3
        `
	// MultiLine2 constant (no spaces EOL)
	MultiLine2 = `
        Line1 /123
        Line2 """Yo"""
        Line3`
	// MultiLine3 constant (no newline at start)
	MultiLine3 = `Line1
        Line2
        "Yo"
        Line3`
)

var (
	// A2 is a variable
	A2, B2 = 1, "42"      // 1, "42"
	C21    = dynamicFn1() // 3
	// D21 is a variable
	D21, E2    bool    // false, false
	Ta_package = false // special ID
	otherID    = true  // other ID
)

func dynamicFn1() int {
	return 4
}

// NodeR is an interface
type NodeR interface {
	Pos() int               // Pos is a method
	End12() int             // End12 is a method
	Name(offset int) string // Sub-name
}

// Person is a type representing a person
// with a name, age, and shoe size
type Person struct {
	Name     string               // Name of the person
	Age      int     `json:"Tag"` // Age of the person
	ShoeSize float32              // Shoe size of the person
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

func (p PeopleByShoeSize) Less(i, j int) (a bool) {
	a = (p[i].ShoeSize < p[j].ShoeSize)
	return
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

func Testing() (E2 int, p string) {
	fmt.Println(E2)

	E2, B2 := 1, 2
	fmt.Println(E2, B2)

	p = "Hello"

	{
		E2 := 99
		B2 := 199
		fmt.Println(E2, B2)
	}

	{
		E2 = 100
		B2 := 200
		fmt.Println(E2, B2)
	}

	fmt.Println(E2, B2)

	return
}

func main() {
	Testing()

	// Hello World SJ3
	x := "Hello, 世界 \123\1123"
	fmt.Println(x) // Where am I?

	// Person slice
	people := []Person{ // EOL Comment
		{
			Name:     "Person1", // Name
			Age:      26,        // Age
			ShoeSize: 8,         // ShoeSize
		}, // Between types comment
		{
			Name:     "Person2",
			Age:      21,
			ShoeSize: 4,
		},
		{
			Name:     "Person3", // Person 3
			Age:      15,
			ShoeSize: 9,
		},
		{
			Name:     "Person4",
			Age:      45, // Person 4 age
			ShoeSize: 15,
		},
		{
			Name:     "Person5",
			Age:      25,
			ShoeSize: 8.5,
		}} // End of type comment

	// Test
	fmt.Println(people)

	sort.Sort(PeopleByShoeSize(people))
	fmt.Println(people)

	sort.Sort(PeopleByAge(people))
	fmt.Println(people)
}

// Post code comments
