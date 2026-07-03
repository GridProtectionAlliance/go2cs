// PointerParamCapturedInClosure guards a deref'd pointer PARAMETER (or receiver) that
// is captured by a closure. A pointer param is emitted as the box `ж<T> Ꮡp` aliased to
// `ref var p = ref Ꮡp.Value`; a C# closure cannot capture that ref-local (CS8175). Inside
// a lambda the converter references it through the box instead — a value use becomes
// `Ꮡp.Value.field`, an address use `Ꮡp`. Mirrors runtime closures that capture a `*maptype`
// / `*m` / `*p` parameter (e.g. map.go's markBucketsEmpty using `t.BucketSize`).
package main

import "fmt"

type config struct {
	size int
	tag  int
}

// scaler captures the pointer param c by value-field use inside the returned closure.
func scaler(c *config) func(int) int {
	return func(x int) int {
		return x*c.size + c.tag
	}
}

// mutate captures c and writes through it from inside the closure.
func mutate(c *config) func() {
	return func() { c.size++ }
}

func main() {
	c := &config{size: 10, tag: 3}
	f := scaler(c)
	fmt.Println(f(5)) // 53

	inc := mutate(c)
	inc()
	inc()
	fmt.Println(c.size)  // 12
	fmt.Println(f(2))    // 2*12 + 3 = 27
}
