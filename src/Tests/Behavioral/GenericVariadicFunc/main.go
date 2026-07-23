package main

import "fmt"

// First takes a variadic over a bare type parameter (`...T`). Before the fix, the
// converter emitted a namespace-level `using ꓸꓸꓸT = Span<T>` alias that referenced
// the out-of-scope method type parameter T; it now emits `params Span<T>` inline.
func First[T any](vals ...T) T { return vals[0] }

// Count is a second variadic-type-parameter case (returns the argument count).
func Count[T any](vals ...T) int { return len(vals) }

// DeferredCount threads an inline Span<T> through the ref-based defer execution wrapper.
func DeferredCount[T any](vals ...T) int {
	defer func() {}()
	return len(vals)
}

// Or defines a `comparable`-constrained variadic, mirroring cmp.Or. go2cs maps the
// Go built-in `comparable` constraint to golib's generic `comparable<T>` interface
// (previously it emitted a bare `comparable`, which is a generic type needing a type
// argument). The DEFINITION compiles and its emitted constraint is locked by the
// target-comparison golden. It is intentionally not called: native C# types do not
// yet implement `comparable<T>` (a known golib limitation), so instantiating it with
// int/string would not compile.
func Or[T comparable](vals ...T) T {
	var zero T
	for _, v := range vals {
		if v != zero {
			return v
		}
	}
	return zero
}

func main() {
	// Instantiate the variadic generics across several element types.
	fmt.Println(First(10, 20, 30))
	fmt.Println(First(1.5, 2.5))
	fmt.Println(Count(1, 2, 3, 4))
	fmt.Println(DeferredCount("x", "y"))

	// Strings work too: a Go `string` maps to golib's `@string`, which is a *struct*
	// with a parameterless constructor, so it satisfies the `new()` constraint go2cs
	// adds to generic type parameters (unlike .NET's reference-type `System.String`).
	var s1, s2 string = "go", "2cs"
	fmt.Println(First(s1, s2))

	// Bare string literals also work: go2cs casts a string argument passed to a
	// generic type parameter to `@string` (e.g. `First("A", "B")` becomes
	// `First((@string)"A", (@string)"B")`). Without the cast the literal would be a
	// .NET string and C# would infer the type argument as `string`, failing `new()`.
	fmt.Println(First("A", "B", "C"))
}
