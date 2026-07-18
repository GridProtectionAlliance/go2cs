package main

import "fmt"

type setting struct {
	name  string
	count int
}

func (s *setting) bump() int {
	s.count++
	return s.count
}

type Setting struct {
	tag string
	*setting
}

func lookup(name string) *setting {
	return &setting{name: name}
}

// The internal/godebug New shape: the embedded pointer is left nil at construction
// and populated later (godebug's Value/once.Do), which must be legal — only a
// dereference through the nil embed panics.
func New(tag string) *Setting {
	return &Setting{tag: tag}
}

func main() {
	s := New("debug")

	// Holding and inspecting a nil embedded pointer is legal.
	fmt.Println(s.setting == nil)

	// Dereferencing through the nil embedded pointer panics (recoverably).
	func() {
		defer func() {
			fmt.Println("recovered:", recover())
		}()
		fmt.Println(s.name) // promoted field read through nil *setting
	}()

	// Assigning a nil pointer variable through the promoted accessor is legal too.
	var p *setting
	s.setting = p
	fmt.Println(s.setting == nil)

	// Post-construction population — the assignment the defect blocked.
	s.setting = lookup("gcpacertrace")
	fmt.Println(s.setting == nil)
	fmt.Println(s.name, s.bump(), s.bump())

	// The embed aliases the same *setting the lookup returned.
	direct := lookup("other")
	s.setting = direct
	s.count = 10
	before := direct.count
	after := s.bump()
	fmt.Println(before, after, direct.count)
}
