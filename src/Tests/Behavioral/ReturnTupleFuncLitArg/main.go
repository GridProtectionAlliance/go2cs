package main

import "fmt"

// handler mirrors net/http's HandlerFunc: a defined func type WITH a method, so a
// conversion `handler(func literal)` keeps the named type (it does not collapse to the
// base delegate) and takes the type-CONVERSION render path.
type handler func(msg string)

func (h handler) invoke(msg string) { h(msg) }

func wrap(h handler) handler { return h }

// A func literal inside a named-func-type CONVERSION inside a return TUPLE: the
// conversion route does not thread the call-argument capture channel, so the literal's
// capture-snapshot declarations (a captured SLICE gets a snapshot, as net/http's
// allowedMethods does) must hoist before the `return`, not emit inline in the expression
// (net/http findHandler / traceviewer MainHandler shape).
func makeHandlers(prefix string) (handler, string, error) {
	allowed := []string{prefix + "-a", prefix + "-b"}
	return handler(func(msg string) {
		fmt.Println(allowed[0]+":"+msg, len(allowed))
	}), "label", nil
}

// The same conversion shape with a SINGLE result.
func makeSingle(tag string) handler {
	counts := map[string]int{"x": len(tag)}
	return handler(func(msg string) {
		fmt.Println(counts["x"], msg)
	})
}

// A PLAIN call argument in a return threads the existing capture channel — the
// already-working sibling shape stays guarded too.
func makePlain(n int) handler {
	limits := []int{n, n * 2}
	return wrap(func(msg string) {
		fmt.Println(limits[1], msg)
	})
}

func main() {
	h, label, err := makeHandlers("go")
	fmt.Println(label, err)
	h.invoke("ping")

	s := makeSingle("four")
	s("pong")

	p := makePlain(3)
	p("plain")
}
