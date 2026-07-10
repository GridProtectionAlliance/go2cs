package main

import "fmt"

// A named func type WITH a method renders as a distinct C# delegate type; its
// VALUE passed to a STRUCTURAL func parameter needs the delegate re-wrap
// (net/http h2_bundle's scheduleHandler shape).
type Handler func(int, string) string

func (h Handler) tag() string {
	return "handler"
}

// A METHODLESS named func type collapses to the structural delegate itself —
// the control: no wrap is emitted for it.
type Plain func(int, string) string

func invoke(f func(int, string) string, n int, s string) string {
	return f(n, s)
}

func describe(n int, s string) string {
	return fmt.Sprintf("%d-%s", n, s)
}

func main() {
	var h Handler = describe
	// Named-with-method delegate VALUE at a structural func param (the fix).
	fmt.Println(invoke(h, 1, "a"))
	fmt.Println(h.tag())

	// Methodless named value — collapses to the same structural delegate (control).
	var p Plain = describe
	fmt.Println(invoke(p, 2, "b"))

	// Method group and func literal convert natively (controls).
	fmt.Println(invoke(describe, 3, "c"))
	fmt.Println(invoke(func(n int, s string) string { return s + "!" }, 4, "d"))

	// The reverse (structural value into a NAMED param position) already wraps —
	// interplay check that both directions coexist.
	var h2 Handler = func(n int, s string) string { return fmt.Sprintf("%s/%d", s, n) }
	fmt.Println(invoke(h2, 5, "e"))

	// The h2_bundle shape: a `:=` local declared from a METHOD VALUE takes the
	// matching package named delegate in C# (go/types keeps it structural), gets
	// reassigned from a named-delegate-returning func, and is passed to the
	// structural parameter — the re-wrap must fire for it too.
	sc := &screen{prefix: "#"}
	handler := sc.render
	fmt.Println(invoke(handler, 6, "f"))
	handler = makeHandler("T:")
	fmt.Println(invoke(handler, 7, "g"))
}

type screen struct {
	prefix string
}

func (s *screen) render(n int, msg string) string {
	return fmt.Sprintf("%s%d-%s", s.prefix, n, msg)
}

func makeHandler(tag string) Handler {
	return func(n int, s string) string {
		return fmt.Sprintf("%s%d%s", tag, n, s)
	}
}
