package main

import (
	"fmt"
)

type Signed interface {
	~int | ~int8 | ~int16 | ~int32 | ~int64
}

type Unsigned interface {
	~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
}

type Integer interface {
	Signed | Unsigned
}

type Point []int32

func (p Point) String() string {
	return fmt.Sprintf("%d", p)
}

// Scale returns a copy of s with each element multiplied by c.
func Scale[S ~[]E, E Integer](s S, c E) S {
	r := make(S, len(s))
	for i, v := range s {
		r[i] = v * c
	}
	return r
}

// ScaleAndPrint doubles a Point and prints it.
func ScaleAndPrint(p Point) {
	r := Scale(p, 2)
	fmt.Println(r.String())
}

// Twice runs the constrained S back through ANOTHER generic function: Go infers the inner
// Scale's [S, E] through core types, but C# cannot re-infer a type parameter that appears only
// in constraints (CS0411 — the slices.Sort -> pdqsort chain), so the converter renders the
// resolved instantiation explicitly: `Scale<S, E>(s, 2)`.
func Twice[S ~[]E, E Integer](s S, c E) S {
	return Scale(s, c) // constrained S/E flow through: C# needs Scale<S, E>(s, c)
}

// CopyClearMinMax drives golib's interface-typed builtin overloads through the ~[]E constraint:
// copy with an ISlice-boxed dst must write into the caller's backing array, clear must zero it,
// and 2-arg min/max bind the IComparisonOperators overload (a constrained E has no IComparable).
func CopyClearMinMax[S ~[]E, E Integer](dst, src S) (E, E) {
	copy(dst, src)
	lo := min(dst[0], src[1])
	hi := max(dst[0], src[1])
	clear(src)
	return lo, hi
}

// SumHalves recurses over SUB-SLICES of the constrained S (the pdqsort shape): s[lo:hi] on a
// type parameter must yield S again SHARING backing (golib subslice<S, E> via S.Wrap), and
// append through the constraint must yield S (golib append<S, T>).
func SumHalves[S ~[]E, E Integer](s S) E {
	if len(s) == 1 {
		s[0] += s[0] // write through the sub-slice view -- must land in the caller's array
		return s[0]
	}
	mid := len(s) / 2
	return SumHalves(s[:mid]) + SumHalves(s[mid:])
}

// AppendKeep grows a constrained S through append, preserving its type.
func AppendKeep[S ~[]E, E Integer](s S, v E) S {
	out := append(s, v)
	return out
}

// setFirst takes a CONCRETE []E parameter.
func setFirst[E Integer](s []E, v E) {
	if len(s) > 0 {
		s[0] = v
	}
}

// PassSlice passes the constrained S where []E is expected (Go assignability -- the slices
// package's rotateRight/pdqsort helper chain): the emitted `new slice<E>(s)` materialization
// SHARES backing, so the helper's write lands in the caller's array.
func PassSlice[S ~[]E, E Integer](s S, v E) {
	setFirst(s, v)
}

// Grades is a named map type satisfying ~map[K]V.
type Grades map[string]int

// EqualMaps mirrors maps.Equal: comma-ok indexing through a ~map[K]V constrained M (the
// IMap<K, V> two-value indexer), with comparable-erased K/V equality routed through AreEqual.
func EqualMaps[M ~map[K]V, K, V comparable](m1, m2 M) bool {
	if len(m1) != len(m2) {
		return false
	}
	for k, v1 := range m1 {
		if v2, ok := m2[k]; !ok || v1 != v2 {
			return false
		}
	}
	return true
}

// Seq mirrors Go 1.23 iter.Seq: a GENERIC defined function type ranged over with
// range-over-func semantics (break must stop the producer via yield returning false).
type Seq[V any] func(yield func(V) bool)

// KVSeq mirrors iter.Seq2 (two-value range-over-func).
type KVSeq[K, V any] func(yield func(K, V) bool)

func countdown(n int) Seq[int] {
	return func(yield func(int) bool) {
		for i := n; i > 0; i-- {
			if !yield(i) {
				return
			}
		}
	}
}

func letters() KVSeq[string, int] {
	return func(yield func(string, int) bool) {
		_ = yield("a", 1) && yield("b", 2)
	}
}

// CloneOrNil mirrors maps.Clone: the nil-preserve guard (`m == nil` — Go's only legal map
// comparison, IMap.IsNil in C#) plus make/range/index-set through the constraint.
func CloneOrNil[M ~map[K]V, K, V comparable](m M) M {
	if m == nil {
		return nil
	}
	out := make(M, len(m))
	for k, v := range m {
		out[k] = v
	}
	return out
}

// DropKey mirrors maps.DeleteFunc's constrained delete.
func DropKey[M ~map[K]V, K, V comparable](m M, key K) {
	delete(m, key)
}

func main() {
	var p Point
	p = []int32{1, 2, 3}
	ScaleAndPrint(p)
	fmt.Println(Twice(Point{3, 4}, 2).String())
	dst2, src2 := Point{0, 0, 0}, Point{7, 8, 9}
	lo, hi := CopyClearMinMax(dst2, src2)
	fmt.Println(dst2.String(), src2.String(), lo, hi)
	q := Point{1, 2, 3, 4}
	total := SumHalves(q)
	fmt.Println(q.String(), total)
	grown := AppendKeep(Point{5, 6}, 7)
	fmt.Println(grown.String())
	pt := Point{1, 2}
	PassSlice(pt, 42)
	fmt.Println(pt.String())
	g1 := Grades{"a": 1, "b": 2}
	g2 := Grades{"a": 1, "b": 2}
	g3 := Grades{"a": 9}
	fmt.Println(EqualMaps(g1, g2), EqualMaps(g1, g3))
	sum := 0
	for v := range countdown(5) {
		if v == 2 {
			break // range-over-func break: yield returns false, producer stops
		}
		sum += v
	}
	fmt.Println(sum)
	for k, v := range letters() {
		fmt.Println(k, v)
	}
	var nilG Grades
	fmt.Println(CloneOrNil(nilG) == nil)
	cl := CloneOrNil(g1)
	fmt.Println(len(cl), cl["a"])
	DropKey(g1, "a")
	fmt.Println(len(g1))
	i16, ok16 := bsearchLike([]uint16{2, 4, 8}, uint16(4))
	i32, ok32 := bsearchLike([]uint32{10, 20, 30}, uint32(25))
	fmt.Println(i16, ok16, i32, ok32, halve(uint16(6)), halve(uint32(100)))
	fmt.Println(halveN(int32(10)), halveN(int64(-3)))
	ssum := int64(0)
	for v := range seqOf[int64](int32(4)) {
		ssum += v
	}
	fmt.Println(ssum)
	fmt.Println(growShrink(uintptr(1), uintptr(20)), growShrink(uint32(2), uint32(9)))
}

// growShrink instantiates a numeric-union constraint with U = uintptr: the golib uintptr
// STRUCT must DECLARE the generic-math interfaces the union maps to - matching operators
// alone never satisfy a where-clause (CS0315 x9 at reflect's rangeNum<uintptr, uint64>).
func growShrink[U ~uint32 | ~uintptr](v, lim U) U {
	for v < lim {
		v = v*2 + 1
	}
	return (v % lim) >> 1
}

// seqOf mirrors reflect's rangeNum[T, N]: TWO type parameters where the call supplies only T
// explicitly and N is inferred from the argument, returning a generic named type parameterized
// by T ALONE. The emitted C# call must carry the CALLEE's full resolved instantiation
// (info.Instances - both arguments), not the result Seq[T]'s single argument (CS0305 x11,
// reflect iter.go).
func seqOf[T ~int64, N ~int32 | ~int64](n N) Seq[T] {
	return func(yield func(T) bool) {
		for i := T(0); i < T(n); i = i + 1 {
			if !yield(i) {
				return
			}
		}
	}
}

// bsearchLike mirrors strconv's bsearch: a ~uint16|~uint32 union whose lifted Integer set
// must use the BCL shift shape IShiftOperators<E, int, E> (ushort/uint lack the self-typed form).
func bsearchLike[S ~[]E, E ~uint16 | ~uint32](s S, v E) (int, bool) {
	n := len(s)
	i, j := 0, n
	for i < j {
		h := i + (j-i)>>1
		if s[h] < v {
			i = h + 1
		} else {
			j = h
		}
	}
	return i, i < n && s[i] == v
}

// halveN mirrors rand.N[Int intType]: an integer-constrained type parameter compared
// against an untyped constant (CS0019 - the lifted operator interfaces take Int, not int)
// and converted to/from a basic integer (CS0030 - no cast exists to/from a type parameter;
// routed through golib ConvertToType/ConvertToUInt64, the banked E(100) family).
func halveN[Int ~int32 | ~int64](n Int) Int {
	if n <= 0 {
		return n
	}
	return Int(uint64(n) / 2)
}

// halve exercises a shift ON the type parameter itself: emits `v >> (int)(1)`,
// which binds IShiftOperators<E, int, E>.
func halve[E ~uint16 | ~uint32](v E) E {
	return v >> 1
}