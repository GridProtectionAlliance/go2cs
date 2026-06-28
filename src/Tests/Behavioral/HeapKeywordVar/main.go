package main

import "fmt"

func set(p *int) { *p = 42 }

// Local variables whose names are C# keywords (base, as, event, lock) AND whose addresses are
// taken, so they are heap-boxed. The box declaration must use the sanitized var name (`@base`)
// for the local and a valid box identifier (`Ꮡbase`, not the invalid `Ꮡ@base`) consistently at
// the declaration and every `&base` reference. The runtime package hits this (signal_windows
// `base`, cgocall `as`).
func main() {
	var base int
	var as int
	var event int

	set(&base)
	set(&as)
	set(&event)

	base += 1
	fmt.Println(base, as, event)
}
