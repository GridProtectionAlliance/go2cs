// PromotedEmbedUser is the caller half of the cross-package promoted-box-receiver-method guard. It
// calls Counter's promoted Add/Report (declared on the unexported embedded common) from ANOTHER
// package/assembly, which is what surfaced the CS0117 before the fix.
package main

import (
	"fmt"

	"PromotedEmbedLib"
)

// bump reaches the promoted box-receiver method through a *Counter PARAMETER — the deref-aliased
// `Ꮡc` box form (mirrors cryptotest's writeToHash calling `Ꮡt.Errorf(...)`).
func bump(c *PromotedEmbedLib.Counter, n int) {
	c.Add(n)
}

func main() {
	c := PromotedEmbedLib.New("widget")
	c.Add(3)   // promoted box-receiver method (void), direct call on a *Counter local
	bump(c, 4) // promoted box-receiver method reached through a parameter
	// c.Label is the exported value field (contrast: ordinary cross-package field access);
	// c.Report() is a promoted box-receiver method returning a value.
	fmt.Println(c.Label, c.Report().Sum)
}
