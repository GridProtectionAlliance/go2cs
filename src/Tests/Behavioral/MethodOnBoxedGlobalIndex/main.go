// MethodOnBoxedGlobalIndex guards a pointer-receiver / capture-mode method called on
// an INDEXED field of an address-taken package global — `tr.tabs[i].bump()` where
// `tr` is heap-boxed (its address is taken elsewhere) and `tabs[i]` is an element of a
// field. The method needs the element's address; it must go through the box-field
// accessor, `Ꮡ(tr.tabs, i).bump()` (slice) / `Ꮡtr.of(T.Ꮡarr).at<E>(i).bump()` (array),
// not a naive `Ꮡ` prefix on `tr.tabs[i]` (which binds to the box variable `Ꮡtr`,
// whose value type has no such field → CS1061). Mirrors runtime's
// `trace.stackTab[gen%2].dump(gen)`. The slice form is exercised at runtime here; the
// array form additionally needs the convUnaryExpr `.at<>()` fix and is compile-checked
// against the runtime (its element-array zero value cannot be indexed at run time — a
// separate, deferred generator limitation).
package main

import "fmt"

type buf struct{ n int }

//go:noinline
func add(p *int, d int) { *p += d }

func (b *buf) bump()    { add(&b.n, 1) } // takes &b.n -> capture-mode
func (b *buf) get() int { return b.n }

type tracer struct {
	tabs []buf
}

var tr tracer

//go:noinline
func keep(t *tracer) { _ = t }

func main() {
	keep(&tr)
	tr.tabs = make([]buf, 2)

	tr.tabs[1].bump() // capture-mode method on boxed-global.field[index]
	tr.tabs[1].bump()
	tr.tabs[0].bump()

	fmt.Println(tr.tabs[0].get(), tr.tabs[1].get()) // 1 2
}
