package main

import "fmt"

type Signed interface {
	~int | ~int8 | ~int16 | ~int32 | ~int64
}

type Unsigned interface {
	~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
}

type Integer interface {
	Signed | Unsigned
}

type Float interface {
	~float32 | ~float64
}

type Ordered interface {
	Integer | Float | ~string
}

// Min returns the smaller of two comparable values
func Min[T Ordered](a, b T) T {
	if a < b {
		return a
	}
	return b
}

// Convert converts a value from one type to another using a conversion function
func Convert[T, U any](value T, converter func(T) U) U {
	return converter(value)
}

func main() {
	// CallExpr with explicit type argument for a single type parameter
	minValue := Min[int](42, 17)
	fmt.Printf("Min value: %d\n", minValue)

	// CallExpr with explicit type arguments for multiple type parameters
	// (Notice the explicit specification of both types)
	strLength := Convert[string, int]("hello", func(s string) int {
		return len(s)
	})
	fmt.Printf("String length: %d\n", strLength)
}
