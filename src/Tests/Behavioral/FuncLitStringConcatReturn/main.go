package main

import "fmt"

func main() {
	// A func literal whose string result mixes a literal+var concat return arm with
	// fmt.Sprintf return arms — the concat arm must not break C# lambda return-type
	// inference (CS8917) when emitted alongside @string-typed sibling returns.
	pick := func(v any) string {
		switch t := v.(type) {
		case string:
			return "string:" + t
		case int:
			return fmt.Sprintf("int:%d", t)
		default:
			return fmt.Sprintf("other:%v", t)
		}
	}

	fmt.Println(pick("abc"))
	fmt.Println(pick(42))
	fmt.Println(pick(true))

	// Minimal shape: literal+param concat in one arm, Sprintf in the other.
	wrap := func(s string, quote bool) string {
		if quote {
			return "q:" + s
		}
		return fmt.Sprintf("p:%s", s)
	}

	fmt.Println(wrap("x", true))
	fmt.Println(wrap("y", false))

	// Concat-only func literal: every return arm is a literal+var concat.
	tag := func(s string) string {
		if len(s) == 0 {
			return "tag:" + "empty"
		}
		return "tag:" + s
	}

	fmt.Println(tag("z"))
	fmt.Println(tag(""))

	// Literal on the right-hand side of the concat.
	bang := func(s string) string {
		if len(s) > 1 {
			return s + "!"
		}
		return fmt.Sprintf("%s?", s)
	}

	fmt.Println(bang("hi"))
	fmt.Println(bang("h"))

	// Same arm mix through the `var` declaration form, which converts along a
	// different path (explicit delegate type) than the `:=` define above.
	var pad = func(s string, wide bool) string {
		if wide {
			return "  " + s
		}
		return fmt.Sprintf("[%s]", s)
	}

	fmt.Println(pad("v", true))
	fmt.Println(pad("v", false))

	// MULTI-result sibling (internal/fuzz fuzzOnce): a tuple whose string ELEMENT is a
	// bare "" literal on some arms. Inside a tuple the literal emits as a bare C# string
	// (u8 spans cannot be tuple elements), so an arm with no nil and no string var —
	// `return entry, []byte{…}, ""` — counted as fully typed and suppressed the explicit
	// tuple return type, inferring a C# string element where the Go result is string
	// (@string): the destructured errMsg then had no `!=` against a u8 literal (CS0019).
	fuzzish := func(entry int) (dur int, cov []byte, errMsg string) {
		if entry < 0 {
			msg := fmt.Sprintf("bad entry %d", entry)
			return entry, nil, msg
		}
		if entry == 0 {
			return entry, nil, ""
		}
		return entry, []byte{byte(entry)}, ""
	}

	d1, c1, e1 := fuzzish(-1)
	fmt.Println(d1, c1, e1 != "", e1)
	d2, c2, e2 := fuzzish(0)
	fmt.Println(d2, c2, e2 != "", e2)
	d3, c3, e3 := fuzzish(2)
	fmt.Println(d3, c3, e3 != "", e3)

	// Unnamed multi-result variant (internal/coverage/decodecounter sget): the "" element
	// rides an arm whose other element is a typed non-nil expression.
	sget := func(ok bool) (string, error) {
		if !ok {
			return "", fmt.Errorf("no string")
		}
		return "found", nil
	}

	s1, err1 := sget(false)
	fmt.Println(s1 == "", err1 != nil)
	s2, err2 := sget(true)
	fmt.Println(s2, err2 == nil)
}
