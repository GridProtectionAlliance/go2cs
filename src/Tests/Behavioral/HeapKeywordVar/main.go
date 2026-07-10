package main

import "fmt"

func set(p *int) { *p = 42 }

// Local variables whose names are C# keywords (base, as, event, lock) AND whose addresses are
// taken, so they are heap-boxed. The box declaration must use the sanitized var name (`@base`)
// for the local and a valid box identifier (`Ꮡbase`, not the invalid `Ꮡ@base`) consistently at
// the declaration and every `&base` reference. The runtime package hits this (signal_windows
// `base`, cgocall `as`).
// decimal is an UNEXPORTED type whose name is a C# keyword (strconv's shape): the emitted
// struct is internal `@decimal`, and its exported-looking methods must CLAMP to internal -
// the '@' escape previously read as exported in getAccess, emitting a public method with an
// internal receiver parameter type (CS0051).
// null is a C# keyword used as a PACKAGE-LEVEL var whose address is taken: the global's
// heap-box backing FIELD must compose the box name with the escape stripped (`Ꮡnull`, not
// the invalid two-token `Ꮡ@null`) while the ref property keeps the escaped name (`@null`),
// matching every `&null` use site — net/rpc/jsonrpc's `var null = json.RawMessage([]byte("null"))`.
var null = []byte("null")

type decimal struct {
	d int
}

func (a *decimal) String() string {
	return fmt.Sprint(a.d)
}

func (a *decimal) Assign(v int) {
	a.d = v
}

func main() {
	var base int
	var as int
	var event int

	set(&base)
	set(&as)
	set(&event)

	base += 1
	fmt.Println(base, as, event)

	var dec decimal
	dec.Assign(7)
	fmt.Println(dec.String())

	// Write through the global's pointer, then read the global itself — the box backs the
	// original storage, so the mutation is visible both ways.
	p := &null
	*p = append(*p, '!')
	fmt.Println(string(null), string(*p))
}
