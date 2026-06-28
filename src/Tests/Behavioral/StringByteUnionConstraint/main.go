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
}
