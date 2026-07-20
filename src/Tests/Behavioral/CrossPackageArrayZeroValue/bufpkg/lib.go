// bufpkg plays internal/chacha8rand in the cross-package zero-value guard: it declares a struct
// whose zero value is only usable once its FIXED ARRAY fields have a backing. A go2cs array<T>
// uses a null backing array as its zero-value discriminator, so the array field's `= new(N)`
// initializer — which only an explicitly declared constructor runs — is what makes the zero value
// real. The consumer (../main.go) reaches this type across an assembly boundary, where the
// generator sees it as compiled METADATA rather than syntax.
package bufpkg

// State plays chacha8rand.State — fixed arrays plus plain scalars.
type State struct {
	Buf  [4]uint64
	Seed [2]uint64
	N    uint32
}

// Nested plays a struct that reaches a fixed array only THROUGH another struct, so the
// needs-construction walk has to recurse to find it.
type Nested struct {
	Inner State
	Label string
}

// Fill writes through the array field via the pointer receiver.
func (s *State) Fill() {
	for i := range s.Buf {
		s.Buf[i] = uint64(i) + 1
	}
	s.N = uint32(len(s.Buf))
}

// FillArray takes the address of a fixed array — the shape that PINS the backing array
// (chacha8's `block(&s.seed, &s.buf, ...)`), which throws on a null backing rather than
// merely reading a wrong value.
func FillArray(buf *[4]uint64) {
	for i := range buf {
		buf[i] = uint64(i) * 3
	}
}

// Sum ranges the array field through a pointer to the struct.
func Sum(s *State) uint64 {
	total := uint64(0)
	for _, v := range s.Buf {
		total += v
	}
	return total
}
