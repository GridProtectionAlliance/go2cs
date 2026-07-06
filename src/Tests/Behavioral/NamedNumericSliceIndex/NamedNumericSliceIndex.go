// Guards indexing a slice/array by a value whose type is a NAMED wide-integer or a numeric
// TYPE PARAMETER — internal/trace's `dataTable[EI ~uint64]` doing `d.dense[id]`, and its
// `type ProcID int64` indexing spans. Go permits any integer type as an index; the C#
// slice<E>/array<E> indexer takes nint, and neither a named int64/uint64 nor a constrained
// type parameter narrows implicitly (CS1503/CS0030). The converter now casts such an index to
// nint — for a named wide-int via `(nint)(x)` (one user + one built-in conversion), for a type
// parameter via `(nint)(ConvertToUInt64<K>(x))`. A named-over-int (num:nint) and a plain int
// index stay bare (they narrow implicitly), so this must not churn those. Kept to a generic
// FUNCTION (not a generic struct with methods) to isolate the index cast from unrelated
// generic-struct-method limitations.
package main

import "fmt"

type PID int64 // named over a WIDE built-in (int64) — needs the (nint) cast
type rank int  // named over int (num:nint) — narrows implicitly, must stay bare

// lookup indexes a slice by a ~uint64 TYPE PARAMETER (the internal/trace dataTable.get shape),
// including an arithmetic result that is still the type parameter.
func lookup[K ~uint64](dense []string, id K) (string, string) {
	return dense[id], dense[id/2]
}

// bitset mirrors internal/trace dataTable's present-bit test: the type parameter is used as a
// SHIFT COUNT (`1 << (id % 8)`), which the converter coerces to int — a type-parameter operand
// there routes through ConvertToUInt64<K> (CS0030 otherwise).
func bitset[K ~uint64](id K) uint8 {
	return uint8(1) << (id % 8)
}

// pick indexes by a named wide-int (PID over int64) plus an arithmetic result.
func pick(spans []string, p PID) (string, string) {
	return spans[p], spans[p-1]
}

func main() {
	dense := []string{"a", "b", "c", "d", "e", "f"}
	x, y := lookup(dense, uint64(5)) // type-parameter index (K=uint64): 5 and 5/2=2
	fmt.Println(x, y)
	fmt.Println(bitset(uint64(3)), bitset(uint64(10))) // type-param shift count: 1<<3=8, 1<<(10%8)=4

	spans := []string{"zero", "one", "two", "three"}
	a, b := pick(spans, PID(3)) // named-int64 index → (nint) cast
	fmt.Println(a, b)

	names := []string{"lo", "mid", "hi"}
	var r rank = 2
	fmt.Println(names[r]) // named-over-int (num:nint) → stays bare (implicit narrow)
}
