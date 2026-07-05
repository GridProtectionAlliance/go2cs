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

// Seq2Like mirrors Go 1.23 iter.Seq2: a GENERIC DEFINED FUNCTION TYPE. Its type parameters
// live on the NAMED type (not the signature), so the emitted C# delegate must carry them --
// `delegate bool Seq2Like<K, V>(...)` -- or the declaration is non-generic while uses say
// Seq2Like<K, V> (CS0308/CS0246, the iter package wave-1 errors).
type Seq2Like[K, V any] func(K, V) bool

func consume[K, V any](s Seq2Like[K, V], k K, v V) bool {
	return s(k, v)
}

// describe is a generic FUNCTION. Calling it with a QUALIFIED (cross-package) type argument —
// `describe[fmt.Stringer]` — is the reflect.TypeFor[encoding.BinaryMarshaler] shape (gob): a
// generic-function IndexExpr whose type argument is a SelectorExpr, not a bare Ident. The
// Ident-only type-argument check kept the Go bracket form while the call machinery also
// appended the C# `<…>`, emitting `describe[fmt.Stringer]<fmt.Stringer>(…)` (CS1525).
func describe[T any](label string) string {
	var zero T
	_ = zero
	return label
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

	// Generic defined function type: declare, instantiate, invoke
	pair := Seq2Like[string, int](func(k string, v int) bool { return len(k) == v })
	fmt.Println(consume(pair, "four", 4), pair("nope", 3))

	// Generic-function call with a cross-package qualified type argument (SelectorExpr).
	fmt.Println(describe[fmt.Stringer]("stringer"))
}
