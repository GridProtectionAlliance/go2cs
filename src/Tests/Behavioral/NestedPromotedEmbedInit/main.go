package main

import "fmt"

// flags is EMBEDDED into formatter (a promoted embed). In C# the embed becomes
// a readonly ж<flags> box that only a constructor allocates — so formatter's
// zero value is broken unless formatter is constructed, not left `default`.
type flags struct {
	width int
	name  string
}

type formatter struct {
	flags          // promoted embed — box must be constructed
	pad  [3]byte   // fixed-array field — `= new(3)` backing must be constructed
}

// printer holds a formatter as a PLAIN (non-embed) FIELD. `new(printer)` must
// construct that nested formatter so its embed box and array backing are non-nil.
// Without the go2cs-gen zero-value fix, printer's parameterless ctor left `f`
// as default(formatter) — a null embed box — and `p.f.width` NRE'd on first
// touch (this mirrors fmt's pp{ fmt fmt } where fmt embeds fmtFlags).
type printer struct {
	f formatter
}

func main() {
	// new(printer): @new<printer>() runs printer's parameterless ctor, which must construct
	// the nested formatter (embed box + array backing), else p.f.width NREs.
	p := new(printer)
	p.f.width = 7    // promoted field through the embed box
	p.f.name = "hi"
	p.f.pad[0] = 65  // fixed-array backing
	fmt.Println(p.f.width, p.f.name, p.f.pad[0])

	// &printer{}: the composite literal lowers to printer's NilType ctor, which likewise
	// constructs the nested formatter.
	r := &printer{}
	r.f.width = 9
	r.f.pad[2] = 90
	fmt.Println(r.f.width, r.f.pad[2])
}
