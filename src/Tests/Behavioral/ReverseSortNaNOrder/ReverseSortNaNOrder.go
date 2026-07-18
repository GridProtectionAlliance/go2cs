package main

import (
	"fmt"
	"math"
)

// Interface mirrors sort.Interface — the dispatch under test flows through it.
type Interface interface {
	Len() int
	Less(i, j int) bool
	Swap(i, j int)
}

// Float64Slice mirrors sort.Float64Slice: a NaN-aware Less over concrete float64.
type Float64Slice []float64

func (x Float64Slice) Len() int { return len(x) }
func (x Float64Slice) Less(i, j int) bool {
	return x[i] < x[j] || (fisNaN(x[i]) && !fisNaN(x[j]))
}
func (x Float64Slice) Swap(i, j int) { x[i], x[j] = x[j], x[i] }

// fisNaN is sort's local isNaN: concrete float64 self-inequality.
func fisNaN(f float64) bool { return f != f }

// reverse mirrors sort's reverse: an embedded Interface with a Less OVERRIDE.
// The override must win over the promoted embedded method through interface dispatch.
type reverse struct {
	Interface
}

func (r reverse) Less(i, j int) bool {
	return r.Interface.Less(j, i)
}

func Reverse(data Interface) Interface {
	return &reverse{data}
}

// insertionSort dispatches Len/Less/Swap through the interface value.
func insertionSort(data Interface) {
	n := data.Len()
	for i := 1; i < n; i++ {
		for j := i; j > 0 && data.Less(j, j-1); j-- {
			data.Swap(j, j-1)
		}
	}
}

// ordered mirrors cmp.Ordered's operator surface for the generic legs.
type ordered interface {
	~int | ~float32 | ~float64 | ~string
}

// isNaN mirrors cmp.isNaN: GENERIC self-inequality — the emission routes through
// golib equality, which must follow Go's IEEE `==` (NaN unequal to itself).
func isNaN[T ordered](x T) bool { return x != x }

// less mirrors cmp.Less: NaN orders before any non-NaN.
func less[T ordered](x, y T) bool { return (isNaN(x) && !isNaN(y)) || x < y }

// eq exercises the plain `comparable` generic equality path.
func eq[T comparable](a, b T) bool { return a == b }

func main() {
	nan := math.NaN()

	// Reverse (embed-override) dispatch: descending sort through Reverse.
	ints := Float64Slice{74, 59, 238, -784, 9845, 959, 905, 0, 42}
	insertionSort(Reverse(ints))
	fmt.Println(ints)

	// NaN-aware ascending sort through the concrete Less.
	floats := Float64Slice{74.3, 59.0, math.Inf(1), 238.2, -784.0, 2.3, nan, nan, math.Inf(-1), 905}
	insertionSort(floats)
	fmt.Println(floats)

	// Generic NaN probes (the cmp.isNaN / cmp.Less shapes).
	fmt.Println(isNaN(nan), isNaN(1.5), isNaN(float32(math.NaN())), isNaN(7), isNaN("x"))
	fmt.Println(less(nan, -784.0), less(-784.0, nan), less(nan, nan), less(1.0, 2.0))

	// Generic `comparable` equality: IEEE semantics for every float-bearing kind.
	fmt.Println(eq(nan, nan), eq(1.5, 1.5))
	fmt.Println(eq(float32(math.NaN()), float32(math.NaN())), eq(float32(1.5), float32(1.5)))
	fmt.Println(eq(complex(nan, 0), complex(nan, 0)), eq(complex(1, 2), complex(1, 2)))
	fmt.Println(eq(complex64(complex(nan, 0)), complex64(complex(nan, 0))), eq(complex64(complex(1, 2)), complex64(complex(1, 2))))

	// Boxed (interface) equality: Go compares dynamic values with the type's `==`.
	var bx any = nan
	var by any = nan
	fmt.Println(bx == by, any(1.5) == any(1.5))
}
