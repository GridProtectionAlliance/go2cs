// Locks in the go2cs handling of a DEFINED type over an array-backed defined type
// (`type pallocBits pageBits`, `type pageBits [4]uint64` — runtime mpallocbits.go) and of the
// builtin `copy` from a DEFINED slice type (`type pMask []uint32` — runtime proc.go). The
// generator implements IArray<elem> on the outer wrapper as a view over m_value (golib
// `len(IArray)` and indexing bind — CS1503 otherwise), routed through an ensure-allocated view
// so writes land in the wrapper's OWN storage (the historical lost-writes trap: the underlying
// array wrapper's backing is lazily allocated, and a value-copy path would allocate on the copy).
// golib gains `copy(in slice<T1>, ISlice<T2>)` so a named-slice source binds (generic inference
// cannot reach the slice/slice overload from a wrapper). All values verified against Go,
// including write-through via the (*pageBits)(b) reinterpret and post-copy independence.
// Adversarial additions (all found by a skeptic on the first cut): the pointer-receiver fill loop
// on a VIRGIN wrapper (`b.val[i] = v` emission — val must route through the ensuring view or lazy
// allocation lands on a temp and every write is lost), and copy() with a NONZERO-Low source or
// destination (both indexers are slice-relative — adding Low double-offsets).
//
// Also: the EMPTY composite of a named-over-array (`tb{}` — runtime string.go's `*buf = tmpBuf{}`)
// and of a named slice (`pm{}`). The generic named-composite `nil` filler landed INSIDE the
// element literal (`new tb(new byte[]{nil}.array())` — CS0029 NilType→byte). An empty array
// composite is Go's zero `[N]T` — a zeroed FIXED-LENGTH backing (`new byte[4].array()`, not a
// length-0 `{}` literal); an empty slice composite is empty but NON-NIL.
package main

import "fmt"

type pageBits [4]uint64
type pallocBits pageBits
type pm []uint32
type tb [4]byte

// the runtime string.go `*buf = tmpBuf{}` shape: zero the pointee via an empty composite
func zeroTB(buf *tb) { *buf = tb{} }

func (b *pageBits) set(i uint, v uint64) { b[i] = v }
func (b *pageBits) get(i uint) uint64    { return b[i] }

// REPRO 3: pointer-receiver fill loop on a VIRGIN wrapper (the mpallocbits shape)
func (b *pallocBits) fill() {
	for i := 0; i < len(b); i++ {
		b[i] = uint64(i*10 + 1)
	}
}

func main() {
	// REPRO 3: virgin fill + reinterpret read-back + reinterpret write-back
	var e pallocBits
	e.fill()
	fmt.Println((*pageBits)(&e).get(0), (*pageBits)(&e).get(3)) // 1 31
	(*pageBits)(&e).set(1, 99)
	fmt.Println(e[1]) // 99

	// REPRO 1: copy from an array-backed sub-slice pm (nonzero Low, SHARED backing)
	arr := [6]uint32{10, 20, 30, 40, 50, 60}
	p := pm(arr[2:5])
	d := make([]uint32, 3)
	n := copy(d, p)
	fmt.Println(n, d[0], d[1], d[2]) // 3 30 40 50

	// REPRO 2: copy INTO an array-backed sub-slice dst (nonzero Low)
	var arr2 [6]uint32
	n2 := copy(arr2[3:], pm{7, 8, 9})
	fmt.Println(n2, arr2[3], arr2[4], arr2[5], arr2[0]) // 3 7 8 9 0

	// OVERLAPPING copy through the named-slice source (Go copy has memmove semantics): the pm view
	// and the destination share arr3's backing with a forward overlap.
	arr3 := [6]uint32{1, 2, 3, 4, 5, 6}
	ov := pm(arr3[0:4])
	n3 := copy(arr3[2:6], ov)
	fmt.Println(n3, arr3[2], arr3[3], arr3[4], arr3[5]) // 4 1 2 3 4

	// original probes: direct write + reinterpret write + copy independence
	var b pallocBits
	b[0] = 5
	(*pageBits)(&b).set(2, 30)
	fmt.Println(len(b), b[0]+b[2]) // 4 35
	src := pm{1, 2, 3, 4}
	dd := make([]uint32, 3)
	fmt.Println(copy(dd, src), dd[2]) // 3 3
	src[0] = 100
	fmt.Println(src[0], dd[0]) // 100 1

	// EMPTY composites: named-over-array = zero [N]T (full length, zeroed, writable);
	// named slice = empty but non-nil
	t := tb{}
	fmt.Println(len(t), t[0], t[3]) // 4 0 0
	t[2] = 9
	fmt.Println(t[2]) // 9
	// (nil-vs-empty distinction is NOT probed: golib's slice nil-compare conflates a nil slice
	// with an empty-but-allocated one — a separate, pre-existing model latent)
	w := pm{}
	fmt.Println(len(w), cap(w)) // 0 0
	w = append(w, 42)
	fmt.Println(len(w), w[0]) // 1 42

	// zeroing an existing wrapper THROUGH A POINTER via the empty composite (the string.go shape)
	zeroTB(&t)
	fmt.Println(t[2], len(t)) // 0 4
}
