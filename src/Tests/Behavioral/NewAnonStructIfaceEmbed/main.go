// NewAnonStructIfaceEmbed guards the dyn-lift naming of `new(struct{ SomeIface })` — an anonymous
// struct whose ONLY member is an embedded interface, so the lift has no field or parameter name to
// derive from. The shape of go/internal/gccgoimporter's `var reserved = new(struct{ types.Type })`:
// before the fix the package-level var's lift arrived from the call-argument path under builtin
// new's UNNAMED parameter, declaring `partial struct  {` with an EMPTY name and registering "" for
// every reference (`@new<>()`, `GoImplement<, …>`, `new жΔType(…)` — a whole-package syntax
// cascade), while the declaration type fell to the raw `struct{…}` t.String() mangle (`ж<types.Type}>`).
package main

import "fmt"

// badge is the embedded interface — the anonymous struct's only member.
type badge interface {
	label() string
}

type gold struct{}

func (gold) label() string { return "gold" }

// reserved mirrors gccgoimporter's singleton: package-level, type inferred from builtin new over
// the interface-embedding anonymous struct. The lift must take the var's name (reservedᴛ1).
var reserved = new(struct{ badge })

func main() {
	// The pointer converts to the embedded interface through the lifted type's pointer adapter —
	// gccgoimporter wraps reserved into types.Type slots the same way (promoted embedded method).
	var b badge = reserved
	fmt.Println(b != nil)

	// Fill the embedded field and call through the promotion: the adapter must forward to the
	// live embedded value, not a copy.
	reserved.badge = gold{}
	fmt.Println(b.label())

	// The FUNCTION-LOCAL form of the same shape reaches the lift through the call-argument path
	// (builtin new's parameter is unnamed), exercising the empty-name fallback ("type").
	local := new(struct{ badge })
	local.badge = gold{}

	var b2 badge = local
	fmt.Println("local:", b2.label())
}
