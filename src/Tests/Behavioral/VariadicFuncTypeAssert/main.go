package main

import "fmt"

func main() {
	// Declared through an explicitly-typed func var: the value boxed into `any` then
	// carries the golib variadic delegate type the assertion target lowers to. (A func
	// literal assigned DIRECTLY to `any` boxes C#'s synthesized anonymous delegate — a
	// separate latent defect, out of this guard's scope.)
	var fn func(string, ...any) = func(format string, args ...any) {
		fmt.Printf(format+"\n", args...)
	}
	var logf any = fn

	// Positive assert: a VARIADIC func type as the assertion target must lower to the
	// params-Span delegate form (net/http transport.go logf shape).
	if fn, ok := logf.(func(string, ...any)); ok {
		fn("value=%v flag=%v", 42, true)
	} else {
		fmt.Println("no match")
	}

	// Negative assert: a non-func value must not match the variadic func target.
	var notFn any = "plain"
	if _, ok := notFn.(func(string, ...any)); ok {
		fmt.Println("unexpected match")
	} else {
		fmt.Println("no match for string")
	}

	// A NON-variadic anonymous func target renders identically on both paths.
	var plain any = func(s string) string { return s + "!" }
	if fn, ok := plain.(func(string) string); ok {
		fmt.Println(fn("ok"))
	}
}
