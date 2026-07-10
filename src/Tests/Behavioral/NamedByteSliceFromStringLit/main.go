package main

import "fmt"

// A named type over []byte converted from a string LITERAL must take the
// underlying-slice hop (net/http sniff.go's signature-table shape): the u8
// span render has no conversion to the [GoType] wrapper.
type htmlSig []byte

// The rune sibling: the conversion decodes code points, so length is counted
// in RUNES, not bytes — a correctness probe of the slice materialization.
type runeSig []rune

func sigLen(s htmlSig) int {
	return len(s)
}

func main() {
	// Direct conversion of a literal.
	sig := htmlSig("<!DOCTYPE HTML")
	fmt.Println(len(sig), string(sig))

	// Conversion inside a composite literal element (the sniff.go table shape).
	sigs := []htmlSig{
		htmlSig("<HTML"),
		htmlSig("<!--"),
	}
	for _, s := range sigs {
		fmt.Println(len(s), string(s))
	}

	// Element reads through the named wrapper.
	fmt.Println(sig[0], sig[1], sigs[0][0])

	// Conversion at argument position.
	fmt.Println(sigLen(htmlSig("<HEAD")))

	// The rune-slice sibling: multi-byte content, rune-counted length.
	r := runeSig("héllo, 世界")
	fmt.Println(len(r), string(r))
	fmt.Println(r[1], r[7])

	// Control: a plain []byte conversion keeps its existing form.
	plain := []byte("<?xml")
	fmt.Println(len(plain), string(plain))
}
