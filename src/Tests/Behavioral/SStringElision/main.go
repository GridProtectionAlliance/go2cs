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

	// (7) UNNAMED comparison against a plain-string VARIABLE (`string(tag) == want`) — now ELIGIBLE:
	//     reading a variable cannot mutate tag, and the mixed sstring/@string operators compare the
	//     views byte-for-byte with no heap copy. This is the second `string(tag)` over the same
	//     never-written source, so the repeated-conversion hoist lifts BOTH (6) and (7) to one temp.
	want := "v2"
	if string(tag) == want {
		fmt.Println("wanted")
	}

	// (8) REPEATED conversion inside a LOOP over a never-written source: the same string(word) is
	//     compared against several literals each iteration. The hoisting pre-pass lifts it to ONE
	//     function-scope `sstring` temp reused by every comparison, instead of re-materializing the
	//     view (3x per iteration) — the loop/tokenizer idiom this optimization targets.
	fmt.Println("classify:", classify([]byte("rust")))

	// (9) REPEATED conversion NOT in a loop — two comparisons of the same never-written source still
	//     hoist to one temp (the second would otherwise re-materialize the view).
	fmt.Println("pick:", pick([]byte("b")))

	// (10) DIFFERENT sub-slices of the same never-written source must NOT collapse into one hoisted
	//      temp — string(buf[:3]) and string(buf[3:6]) are distinct views, so each stays an inline
	//      (sstring) comparison. (The net/http is408Message idiom; only a BARE-identifier source hoists.)
	fmt.Println("prefix:", prefix([]byte("GET /x")))

	// (11) MIXED named-local vs plain-string VARIABLE — `s == want` uses the sstring/@string operator
	//      (byte-compare, no heap copy of s). The common `string(header) == expected` idiom.
	fmt.Println("matchVar:", matchVar([]byte("go2cs"), "go2cs"))

	// (12) MIXED named-local vs struct FIELD (a selector — a pure read) — same mixed operator.
	fmt.Println("matchField:", matchField([]byte("prod"), config{name: "prod"}))

	// (13) TWO conversions compared (`string(a) == string(b)`) — both are zero-copy sstring views,
	//      compared sstring==sstring, neither copied to the heap.
	fmt.Println("twoConv:", matchTwoConversions([]byte("abc"), []byte("abc")))

	// (14) NEGATIVE — the other comparison operand is a function CALL, which could mutate the source
	//      between the (lazy) view and the compare, so the conversion stays a heap @string copy.
	fmt.Println("callOperand:", staysHeapCallOperand([]byte("y"), func() string { return "y" }))
}

type config struct {
	name string
}

// matchVar compares a named-local stack string against a plain-string VARIABLE. The named local is
// non-escaping and its source is never written, so `s` is a stack sstring; the mixed sstring/@string
// comparison operator makes `s == want` compile and run with no heap copy.
func matchVar(b []byte, want string) bool {
	s := string(b)
	return s == want
}

// matchField compares a named-local stack string against a struct FIELD (a selector). A field read
// cannot mutate the source, so `s` stays a stack sstring and the mixed operator applies.
func matchField(b []byte, cfg config) bool {
	s := string(b)
	return s == cfg.name
}

// matchTwoConversions compares two `string(bytes)` conversions directly — each is a zero-copy view,
// so the comparison is sstring == sstring with no heap allocation on either side.
func matchTwoConversions(a, b []byte) bool {
	return string(a) == string(b)
}

// staysHeapCallOperand is a NEGATIVE case: `string(a) == next()` has a function CALL as the other
// operand. Go evaluates string(a) as a copy before next() runs, but a stack view would be read only
// at the comparison — after next() could have mutated a — so the conversion must stay a heap @string.
func staysHeapCallOperand(a []byte, next func() string) bool {
	return string(a) == next()
}

func returnedString() string {
	b := []byte("returned")
	r := string(b)
	return r
}

// classify compares the same never-written []byte against several keywords inside a loop — the
// converter hoists `string(word)` to a single `sstring` temp reused by all three comparisons.
func classify(word []byte) int {
	total := 0
	for i := 0; i < 2; i++ {
		if string(word) == "go" {
			total++
		}
		if string(word) == "rust" {
			total += 10
		}
		if string(word) == "c" {
			total += 100
		}
	}
	return total
}

// pick compares the same never-written []byte against two keywords in straight-line code — two uses
// is enough to hoist (the second reuses the temp rather than rebuilding the view).
func pick(tag []byte) string {
	if string(tag) == "a" {
		return "alpha"
	}
	if string(tag) == "b" {
		return "bravo"
	}
	return "other"
}

// prefix converts two DIFFERENT sub-slices of the same source — the source is a bare identifier only
// at the root, but the conversion operand (buf[:3] / buf[3:6]) differs, so the two views must stay
// separate inline conversions. Hoisting is gated on a bare-identifier operand precisely to avoid
// collapsing these into one temp (which would compare buf[:3] against " /x").
func prefix(buf []byte) bool {
	if len(buf) < 6 {
		return false
	}
	if string(buf[:3]) != "GET" {
		return false
	}
	return string(buf[3:6]) == " /x"
}
