// Regression test: a Go interpreted string literal that carries `\xHH` raw-byte escapes.
//
// Go's `\x` escape is EXACTLY two hex digits denoting one raw byte; C#'s `\x` escape is a GREEDY
// 1-to-4-hex-digit code-UNIT escape. Re-emitting the Go token verbatim as a C# string literal both
// (a) mis-parses `\xdb` followed by ASCII "5""0" (the token `\xdb50`) as the single UTF-16 code unit
// U+DB50 — a lone high surrogate that cannot UTF-8-encode into a golib @string (CS9026) — and
// (b) silently widens every byte >= 0x80 to two UTF-8 bytes, so @string byte indexing (s[i], len)
// would not match Go. This is exactly time/tzdata's embedded zip blob. The converter must emit such
// a literal as a byte-array-backed @string so the exact bytes survive.
package main

import "fmt"

// data mixes the zip magic ("PK\x03\x04"), the surrogate-forming `\xdb50` sequence, high bytes
// (0xff, 0x92), a NUL, and trailing ASCII "LMT" — the byte-exactness of all of which is asserted.
const data = "\x50\x4b\x03\x04\xdb50\xff\x92\x00LMT"

// get4 reads a little-endian uint32 the way time/tzdata's get4s does, proving byte indexing matches Go.
func get4(s string, i int) int {
	return int(s[i]) | int(s[i+1])<<8 | int(s[i+2])<<16 | int(s[i+3])<<24
}

func main() {
	fmt.Println(len(data))
	for i := 0; i < len(data); i++ {
		fmt.Printf("%d ", data[i])
	}
	fmt.Println()
	fmt.Println(get4(data, 0))
}
