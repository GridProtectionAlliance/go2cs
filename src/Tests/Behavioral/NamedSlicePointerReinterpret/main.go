package main

import "fmt"

// A named slice type over []byte — the log/slog/internal/buffer `type Buffer []byte` shape.
type Buf []byte

func main() {
	// Pointer reinterpret from *[]byte (the underlying) to *Buf (the named wrapper). A bare
	// (ж<Buf>)(Ꮡ(b)) cast is CS0030 (unrelated ж instantiations); the converter constructs a
	// wrapper box `Ꮡ(new Buf(b))`. Aliasing with the original `b` is not preserved (the Ꮡ(value)
	// box copies), so write AND read through the SAME pointer p — never read back b.
	b := make([]byte, 0, 8)
	p := (*Buf)(&b)
	*p = append(*p, 'h', 'i')
	*p = append(*p, '!')
	fmt.Println(string(*p)) // hi!
	fmt.Println(len(*p))    // 3

	// The slog/buffer shape: the reinterpret inside a closure that returns the *Buf (its
	// sync.Pool New func does `return (*Buffer)(&b)`).
	makeBuf := func() *Buf {
		s := make([]byte, 0, 4)
		return (*Buf)(&s)
	}
	q := makeBuf()
	*q = append(*q, 'x', 'y')
	fmt.Println(string(*q)) // xy
}
