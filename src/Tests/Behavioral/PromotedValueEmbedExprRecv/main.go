// PromotedValueEmbedExprRecv guards the cross-package promoted-method call whose receiver is a
// pointer-typed EXPRESSION — a type-assert/call chain, not an ident — reaching an EXPORTED method
// promoted through the library type's UNEXPORTED VALUE embed. The shape of go/internal/gcimporter's
// `pkg.Scope().Lookup(name).(*types.TypeName).Type()`: the &-machinery has no addressable base, so
// the receiver renders as the raw box, and before the fix the converter spelled the promotion hop
// through the embed field (`Ꮡ(x.@common).Name()`) — internal cross-assembly (CS1061). The call must
// bind the generated public `Name(this ж<Widget>)` forwarder on the box itself.
package main

import (
	"fmt"

	"PromotedValueEmbedLib"
)

// registry returns the widget as an ANY, so the read in main must type-assert AND call in one
// expression — the receiver is a pointer-typed expression, never a local.
func registry() map[string]any {
	return map[string]any{"w": PromotedValueEmbedLib.New("boxed")}
}

func main() {
	m := registry()
	// Assert-then-call chain (the gcimporter shape): the receiver is the assert expression.
	fmt.Println(m["w"].(*PromotedValueEmbedLib.Widget).Name())
	// Call-chain receiver: the constructor result is the receiver, again never a local.
	fmt.Println(PromotedValueEmbedLib.New("direct").Name())
}
