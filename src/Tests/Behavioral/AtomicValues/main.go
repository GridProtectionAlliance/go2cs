package main

import (
	"fmt"
	"sync/atomic"
)

func main() {
	// Scalar typed atomic value (receiver-capture / direct-box path).
	var n atomic.Int32
	n.Store(10)
	fmt.Println("add:", n.Add(5))                    // 15
	fmt.Println("load:", n.Load())                   // 15
	fmt.Println("swap:", n.Swap(100))                // 15 (old)
	fmt.Println("cas ok:", n.CompareAndSwap(100, 7)) // true
	fmt.Println("cas no:", n.CompareAndSwap(100, 8)) // false
	fmt.Println("final:", n.Load())                  // 7

	// Generic atomic.Pointer[T] (direct-ж receiver + managed slot).
	var p atomic.Pointer[int]
	fmt.Println("ptr nil:", p.Load() == nil) // true
	a := 1
	p.Store(&a)
	fmt.Println("ptr load:", *p.Load())                 // 1
	b := 2
	fmt.Println("ptr cas no:", p.CompareAndSwap(&b, &b)) // false (current is &a)
	fmt.Println("ptr cas ok:", p.CompareAndSwap(&a, &b)) // true
	fmt.Println("ptr final:", *p.Load())                // 2
	old := p.Swap(&a)
	fmt.Println("ptr swap:", *old, *p.Load()) // 2 1
}
