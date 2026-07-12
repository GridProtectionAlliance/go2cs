package main

import "fmt"

func main() {
	// (1) ELIGIBLE — compared only against a string literal; source never mutated.
	//     Emitted as a stack-only sstring (a zero-copy view over name's bytes).
	name := []byte("go2cs")
	s := string(name)
	if s == "go2cs" {
		fmt.Println("match")
	}

	// (2) ELIGIBLE — used only via byte index and len; source never mutated. sstring.
	digits := []byte("2468")
	d := string(digits)
	fmt.Println(int(d[0]) + int(d[3]) + len(d))

	// (3) INELIGIBLE — the source is mutated AFTER the conversion, so the string must be an
	//     independent heap @string copy, not a view that would observe the mutation.
	scratch := []byte("AB")
	t := string(scratch)
	scratch[0] = 'X'
	if t == "AB" {
		fmt.Println("copy-safe")
	}

	// (4) INELIGIBLE — escapes by being passed to fmt.Println. Stays @string.
	u := string([]byte("printed"))
	fmt.Println(u)

	// (5) INELIGIBLE — the local escapes via return. Stays @string.
	fmt.Println(returnedString())
}

func returnedString() string {
	b := []byte("returned")
	r := string(b)
	return r
}
