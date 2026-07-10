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
}
