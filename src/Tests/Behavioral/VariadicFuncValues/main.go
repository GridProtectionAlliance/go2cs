package main

import "fmt"

// A VARIADIC function TYPE used as a VALUE — the go/types `comparable(…, reportf)` shape. The
// three lowerings must agree: the delegate type a parameter/var renders as (Actionꓸꓸꓸ/Funcꓸꓸꓸ,
// `params Span<T>` tail), the form a variadic func literal and a named function (method group)
// convert to, and calls through the value passing loose Go-style arguments, an empty tail, or a
// spread slice.

// gather is a NAMED variadic function passed as a value (method-group conversion).
func gather(prefix string, vals ...int) string {
	total := 0

	for _, v := range vals {
		total += v
	}

	return fmt.Sprintf("%s:%d(%d)", prefix, total, len(vals))
}

// apply calls a variadic func VALUE with loose args, an empty tail, and a spread.
func apply(f func(prefix string, vals ...int) string) {
	fmt.Println(f("loose", 1, 2, 3))
	fmt.Println(f("empty"))

	nums := []int{4, 5}
	fmt.Println(f("spread", nums...))
}

// report exercises the void printf-callback shape (go/types' reportf) with any elements.
func report(emit func(format string, args ...any)) {
	emit("%s=%d", "x", 7)
	emit("bare")
}

func main() {
	// A named func satisfies the variadic func-typed parameter.
	apply(gather)

	// A variadic func literal satisfies it too.
	apply(func(prefix string, vals ...int) string {
		return fmt.Sprintf("%s|%d", prefix, len(vals))
	})

	// A variadic func-typed VAR: declared (nil), nil-compared, assigned the named func, called.
	var f func(prefix string, vals ...int) string

	if f == nil {
		fmt.Println("nil func value")
	}

	f = gather
	fmt.Println(f("var", 10))

	report(func(format string, args ...any) {
		fmt.Printf(format+"\n", args...)
	})
}
