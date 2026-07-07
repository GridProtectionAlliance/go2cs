package main

import "fmt"

// byteSum mirrors internal/bytealg's Rabin-Karp pattern: a function generic over the Go union
// constraint `string | []byte` that indexes elements and reads len. C# cannot express the
// "string OR []byte" union, so the converter emits the read-only IByteSeq<byte> interface (which
// both @string and slice<byte> implement) as the constraint.
func byteSum[T string | []byte](s T) uint32 {
	var h uint32
	for i := 0; i < len(s); i++ {
		h = h*31 + uint32(s[i])
	}
	return h
}

// prefixMatch exercises sub-slicing of the constrained value and conversion of the slice to a
// string (`string(s[:n])`) — the other operations a `string | []byte` body relies on.
func prefixMatch[T string | []byte](s T, sep T) bool {
	n := len(sep)
	if len(s) < n {
		return false
	}
	return string(s[:n]) == string(sep)
}

func main() {
	str := "hello"
	bs := []byte(str)
	hel := "hel"
	helBytes := []byte(hel)

	// Same bytes, both instantiations of the union-constrained generic produce the same hash.
	fmt.Println("sum(string):", byteSum(str))
	fmt.Println("sum([]byte):", byteSum(bs))

	fmt.Println("prefix string:", prefixMatch(str, hel))
	fmt.Println("prefix []byte:", prefixMatch(bs, helBytes))
	fmt.Println("prefix miss:", prefixMatch(str, "xyz"))
	fmt.Println("prefix too long:", prefixMatch(str, "hello world"))
	fmt.Println("last(string):", lastByte(str))
	fmt.Println("last([]byte):", lastByte(bs))
	fmt.Println("sum:", digitSum("12:34"), digitSum([]byte("56:78")))
	fmt.Println("head:", headSum("x98:76"), headSum([]byte("y10:23")))
	fmt.Println("appendRun(string):", string(appendRun(nil, "abcde")))
	fmt.Println("appendRun([]byte):", string(appendRun([]byte("<"), []byte("abcde"))))
}

// lastByte uses the REVERSED union order - `[]byte | string` (time/format.go appendNano):
// the slice term leads, which previously matched the ISlice branch and leaked the raw
// union as the element type (ISlice<byte | string> - CS1003 cascade).
// digitSum mirrors time format_rfc3339 parseStrictRFC3339: a CLOSURE parameter typed as
// the union type param (renders IByteSeq<byte> - a lambda has no where-clause) receiving
// SUB-SLICES of the constrained value, plus the []byte(s) conversion inside (shares for a
// slice instantiation, copies for string).
func digitSum[T []byte | string](s T) int {
	parse := func(part T) int {
		n := 0
		for _, c := range []byte(part) {
			n = n*10 + int(c-'0')
		}
		return n
	}
	return parse(s[0:2]) + parse(s[3:5])
}

// trimHead mirrors time format_rfc3339 parseStrictRFC3339/atoi: a sub-slice of the
// constrained value ASSIGNED BACK to the type-param variable, passed to another
// constrained generic, and RETURNED as the type param - each position needs the
// IByteSeq-to-type-param cast (CS0266/CS0310/CS0029 x6, time).
func trimHead[T []byte | string](s T, n int) T {
	for i := 0; i < n; i++ {
		if len(s) > 1 {
			s = s[1:]
		}
	}
	return s[0:]
}

func headSum[T []byte | string](s T) int {
	return digitSum(trimHead(s, 1))
}

func lastByte[T []byte | string](s T) byte {
	return s[len(s)-1]
}

// appendRun mirrors encoding/json's appendString: a function generic over the union constraint
// `[]byte | string` that SPREADS sub-slices of the constrained value into a variadic append
// (`append(dst, src[lo:hi]...)` and the open-ended `append(dst, src[lo:]...)`). Each sub-slice is
// typed as the union type parameter again, so the spread must reach the IByteSeq<byte> spread
// member through the constraint. The converter emits `((Bytes)(src[lo..hi])).ꓸꓸꓸ`, and a bare
// type-parameter value has no spread member of its own, so the `ꓸꓸꓸ` must be declared on the
// constraint interface (golib IByteSeq<T>) for the cast-through to bind (CS1061 otherwise).
func appendRun[Bytes []byte | string](dst []byte, src Bytes) []byte {
	dst = append(dst, '[')
	if len(src) > 1 {
		dst = append(dst, src[1:len(src)-1]...) // bounded sub-slice of the union value
		dst = append(dst, src[len(src)-1:]...)  // open-ended sub-slice of the union value
	}
	dst = append(dst, ']')
	return dst
}
