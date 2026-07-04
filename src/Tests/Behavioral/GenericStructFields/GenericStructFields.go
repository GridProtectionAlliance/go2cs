package main

import "fmt"

// Result is a generic type to represent a computation result
type Result[T any] struct {
	Value T
	Error error
}

// Container is a struct with fields that use generic types
type Container struct {
	IntResult    Result[int]
	StringResult Result[string]
	FloatValues  []Result[float64]       // Slice of generic type
	Mappings     map[string]Result[bool] // Map with generic value type
}

// tag is a generic struct EMBEDDED (by value) into wrapped below
type tag[T any] struct {
	label T
}

func (t tag[T]) show() T { return t.label }

// wrapped embeds the generic tag[T]: the embed field emits under the BASE name (`tag`)
// with promoted accessors — a generic embed was silently DROPPED from the struct
// entirely (internal/concurrent's entry[K,V] embedding node[K,V], CS0117 x12), and the
// generated accessors needed the type-parameterized instance param + bare member access
// (CS0305).
type wrapped[T any] struct {
	tag[T]
	count int
}

func main() {
	// Create a Container with various generic field types
	container := Container{
		IntResult: Result[int]{
			Value: 42,
			Error: nil,
		},
		StringResult: Result[string]{
			Value: "success",
			Error: nil,
		},
		FloatValues: []Result[float64]{
			{Value: 3.14, Error: nil},
			{Value: 2.71, Error: nil},
		},
		Mappings: map[string]Result[bool]{
			"completed": {Value: true, Error: nil},
			"verified":  {Value: false, Error: fmt.Errorf("verification pending")},
		},
	}

	// Access fields that use generic types
	fmt.Printf("Int result: %d\n", container.IntResult.Value)
	fmt.Printf("String result: %s\n", container.StringResult.Value)
	fmt.Printf("First float value: %f\n", container.FloatValues[0].Value)
	fmt.Printf("Completion status: %t\n", container.Mappings["completed"].Value)

	// GENERIC type sharing its name with a METHOD (internal/concurrent: type indirect[K,V]
	// vs node.indirect()): the collision rename must reach instantiation USES, not just the
	// declaration (the field/var below were emitted raw while the decl renamed - CS0246 x33).
	var pl pool[int]
	pl.items = append(pl.items, 7, 8)
	v, ok := pl.take()
	fmt.Println(v, ok, keeper{}.pool())

	// NOTE: unqualified promoted METHOD calls through a generic embed (w.show()) are a
	// separate banked root — the receiver-wrapper generation resolves the embedded type
	// by exact name and misses generic instantiations. Qualified calls work.
	w := wrapped[string]{tag: tag[string]{label: "gen"}, count: 2}
	fmt.Println(w.label, w.tag.show(), w.count) // gen gen 2
}

type pool[T any] struct{ items []T }

func (p *pool[T]) take() (T, bool) {
	if len(p.items) == 0 {
		var zero T
		return zero, false
	}
	v := p.items[len(p.items)-1]
	p.items = p.items[:len(p.items)-1]
	return v, true
}

type keeper struct{}

// pool (the method) forces the type-vs-method collision with the generic type above.
func (k keeper) pool() string { return "kept" }
