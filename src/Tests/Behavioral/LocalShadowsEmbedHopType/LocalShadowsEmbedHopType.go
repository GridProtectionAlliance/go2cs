// Guards the box-accessor qualification when a LOCAL VARIABLE shadows a package-level TYPE whose
// name the EMITTER spells on its own. Reaching a method promoted through a chain of embedded value
// structs emits one `.of(<Owner>.Ꮡ<embed>)` hop per link, and `<Owner>` is a bare type name — which
// C# binds to any same-named variable in scope, over the whole enclosing block, regardless of where
// the declaration sits. Go has no such conflict: inside the shadowed scope the type is simply not
// reachable by that name, so every bare occurrence the emitter produces is unambiguously the type.
//
// Found in vendor/golang.org/x/crypto/internal/poly1305 under `-tags purego`, whose `mac_noasm.go`
// declares `type mac struct{ macGeneric }` while `func (h *MAC) Sum(b []byte)` declares
// `var mac [TagSize]byte` — so `h.mac.Sum(&mac)` emitted `Ꮡ(h.mac).of(mac.ᏑmacGeneric)`, binding
// `mac` to the `array<byte>` local (CS1061). The accessor type is now package-qualified
// (`<pkg>_package.acc.Ꮡinner`) exactly at the shadowed sites. Sum/Verify below declare the shadowing
// local and must qualify; Write does not and must stay bare — that discrimination is the guard.
package main

import "fmt"

// inner carries the promoted methods; it is reached only through an embed hop.
type inner struct {
	total int
}

func (g *inner) Add(p []byte) {
	for _, b := range p {
		g.total += int(b)
	}
}

func (g *inner) Store(out *[4]byte) {
	out[0] = byte(g.total)
	out[1] = byte(g.total >> 8)
	out[2] = byte(g.total >> 16)
	out[3] = byte(g.total >> 24)
}

// acc is the intermediate embed — the type name the emitter spells as the hop qualifier.
type acc struct{ inner }

// deep adds a second hop so the multi-link `.of(...).of(...)` chain is covered too.
type deep struct{ acc }

type Hash struct {
	acc
	finalized bool
}

type DeepHash struct {
	deep
}

// Write declares NO local named `acc` — the hop qualifier must stay the bare `acc`.
func (h *Hash) Write(p []byte) {
	h.acc.Add(p)
}

// Sum declares a local named `acc`, shadowing the type: the hop qualifier must be package-qualified.
func (h *Hash) Sum(b []byte) []byte {
	var acc [4]byte
	h.acc.Store(&acc)
	h.finalized = true
	return append(b, acc[:]...)
}

// Verify shadows the type from a nested block and shadows the SECOND-hop type name as well, so the
// qualification cannot key off the immediately-enclosing scope or off a single hop position.
func (d *DeepHash) Verify(expected []byte) bool {
	acc := 0
	{
		var deep [4]byte
		d.deep.Store(&deep)
		for i, b := range deep {
			if i < len(expected) && b == expected[i] {
				acc++
			}
		}
	}
	return acc == len(expected)
}

func main() {
	h := &Hash{}
	h.Write([]byte("go2cs"))
	fmt.Println(h.Sum(nil))
	fmt.Println(h.finalized)

	d := &DeepHash{}
	d.deep.Add([]byte("go2cs"))
	var out [4]byte
	d.deep.Store(&out)
	fmt.Println(out)
	fmt.Println(d.Verify(out[:]))
	fmt.Println(d.Verify([]byte{0, 0}))
}
