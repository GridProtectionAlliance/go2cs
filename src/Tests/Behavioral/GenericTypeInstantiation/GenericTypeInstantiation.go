package main

import "fmt"

// Stack is a generic stack implementation
type Stack[T ~int | ~string] struct {
	elements []T
}

func (s *Stack[T]) Push(element T) {
	s.elements = append(s.elements, element)
}

func (s *Stack[T]) Pop() (T, bool) {
	var zero T
	if len(s.elements) == 0 {
		return zero, false
	}

	index := len(s.elements) - 1
	element := s.elements[index]
	s.elements = s.elements[:index]
	return element, true
}

// Standalone function with two type parameters
func MapElements[T ~int, U any](s *Stack[T], mapper func(T) U) []U {
	result := make([]U, 0, len(s.elements))
	for _, elem := range s.elements {
		result = append(result, mapper(elem))
	}
	return result
}

func main() {
	// Type instantiation with a single type argument (IndexExpr)
	intStack := Stack[int]{}
	intStack.Push(10)
	intStack.Push(20)
	val, _ := intStack.Pop()
	fmt.Printf("Popped from int stack: %d\n", val)

	// Another type instantiation with a different type
	stringStack := Stack[string]{}
	stringStack.Push("hello")
	stringStack.Push("world")
	text, _ := stringStack.Pop()
	fmt.Printf("Popped from string stack: %s\n", text)
}
