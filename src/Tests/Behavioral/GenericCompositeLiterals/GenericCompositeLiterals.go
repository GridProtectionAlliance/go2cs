package main

import "fmt"

// Queue is a generic queue implementation
type Queue[T any] struct {
	items []T
}

func (q *Queue[T]) Enqueue(item T) {
	q.items = append(q.items, item)
}

func (q *Queue[T]) Dequeue() (T, bool) {
	var zero T
	if len(q.items) == 0 {
		return zero, false
	}

	item := q.items[0]
	q.items = q.items[1:]
	return item, true
}

func (q *Queue[T]) Size() int {
	return len(q.items)
}

func main() {
	// Empty composite literal with generic type (CompositeLit)
	intQueue := Queue[int]{}
	intQueue.Enqueue(10)
	intQueue.Enqueue(20)
	fmt.Printf("Int queue size: %d\n", intQueue.Size())

	// Composite literal with field values
	stringQueue := Queue[string]{
		items: []string{"hello", "world"},
	}
	fmt.Printf("String queue size: %d\n", stringQueue.Size())

	// Composite literal for a generic type within a variable declaration
	var floatQueue = Queue[float64]{
		items: []float64{3.14, 2.71, 1.618},
	}
	fmt.Printf("Float queue size: %d\n", floatQueue.Size())

	// Multiple instantiations in a slice literal
	queues := []interface{}{
		Queue[int]{},
		Queue[string]{},
		Queue[bool]{items: []bool{true, false}},
	}
	fmt.Printf("Number of queues: %d\n", len(queues))
}
