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

	// (6) UNNAMED comparison operand — the string() temp is created and consumed within the
	//     comparison, so it never escapes and its source cannot be mutated first: emitted as a
	//     zero-copy (sstring)x view with no local and no escape/mutation analysis.
	tag := []byte("v2")
	if string(tag) == "v2" {
		fmt.Println("tagged")
	}

	// (7) UNNAMED comparison against a VARIABLE (not a literal) — stays @string, since the other
	//     operand is not a string literal (mixed sstring==@string is deliberately avoided).
	want := "v2"
	if string(tag) == want {
		fmt.Println("wanted")
	}
}

func returnedString() string {
	b := []byte("returned")
	r := string(b)
	return r
}
