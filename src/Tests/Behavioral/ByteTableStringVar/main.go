// Guards the byte-table string: a Go string is a BYTE sequence, and a table whose bytes are not
// valid UTF-8 cannot round-trip through any C# string literal. Both `"…"` (UTF-16) and `"…"u8`
// RE-ENCODE their content, so every byte >= 0x80 becomes two bytes and every index past it
// shifts — the table silently decodes to the wrong values while compiling perfectly.
//
// Both declaration forms are covered, because they reach the byte-array machinery by DIFFERENT
// routes: a CONST folds to a single value (checked with utf8.ValidString), while a VAR renders
// its initializer expression and each literal is checked on its own. The concatenated form is
// the one that matters — it is how Go source actually writes these tables (encoding/hex's
// reverseHexTable, math/bits' rev8tab).
//
// The valid-UTF-8 declarations are controls: they must KEEP their readable string literal form,
// and their len() is their UTF-8 BYTE count, not their rune count.
package main

import "fmt"

const constTable = "" +
	"\xff\x00\x80\x01" +
	"\xfe\x7f\x0a\xff"

var varTable = "" +
	"\xff\x00\x80\x01" +
	"\xfe\x7f\x0a\xff"

// A single (non-concatenated) literal, and an explicitly typed var.
var varSingle = "\xff\x80\x01"
var varTyped string = "" + "\xff\x80" + "\x7f"

// CONTROLS: valid UTF-8 must keep the readable literal form.
const constText = "" + "hello " + "world"

var varText = "" + "café " + "白鵬翔"

func dump(name string, s string) {
	fmt.Print(name, " len=", len(s), " bytes=")

	for i := 0; i < len(s); i++ {
		fmt.Print(s[i], " ")
	}

	fmt.Println()
}

func main() {
	dump("constTable", constTable)
	dump("varTable", varTable)
	dump("varSingle", varSingle)
	dump("varTyped", varTyped)

	// Indexing the table is the operation that silently broke: a re-encoded table returns
	// 0xC3/0xC2 lead bytes instead of the intended value.
	fmt.Println("index", constTable[2], varTable[2], constTable[4], varTable[4])

	// A function-LOCAL var takes the same route as a package-level one.
	local := "" + "\xff\x80" + "\x02"
	dump("local", local)

	// A []byte conversion of a concatenated non-UTF-8 literal.
	b := []byte("" + "\xff\x80")
	fmt.Println("bytes", len(b), b[0], b[1])

	// Controls: readable text, byte-counted length.
	dump("constText", constText)
	fmt.Println("varText", varText, len(varText))
}
