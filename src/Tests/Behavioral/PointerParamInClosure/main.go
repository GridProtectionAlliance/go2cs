// Regression test: a pointer parameter dereferenced inside a closure (defer, goroutine-style
// func literal, or any lambda).
//
// The converter dereferences a pointer parameter in the function body through a
// `ref var p = ref Ꮡp.Value` ref-local. A ref-local cannot be captured by a C# lambda
// (CS8175 "cannot use ref local inside an anonymous method"), so `*p = …` inside a deferred or
// nested closure failed to compile. The fix dereferences the heap box parameter directly
// (`Ꮡp.Value`) inside a lambda, which is a capturable reference type — while leaving the direct
// (non-lambda) body deref as the efficient ref-local.
package main

import "fmt"

// write through a pointer parameter inside a deferred closure.
func setViaDefer(p *int) {
	defer func() {
		*p = 42
	}()
}

// read+write a pointer parameter inside a nested (immediately used) closure.
func bumpInClosure(p *int) {
	add := func() {
		*p = *p + 1
	}
	add()
	add()
}

// mix: a direct body deref AND a deferred deref of the same pointer parameter.
func mixed(p *int) {
	*p = 5 // direct deref (uses the ref-local)
	defer func() {
		*p = *p * 10 // deref inside a closure (uses the box)
	}()
}

func main() {
	var a int
	setViaDefer(&a)
	fmt.Println(a) // 42

	b := 10
	bumpInClosure(&b)
	fmt.Println(b) // 12

	c := 0
	mixed(&c)
	fmt.Println(c) // 50 (5, then defer *10)
}
