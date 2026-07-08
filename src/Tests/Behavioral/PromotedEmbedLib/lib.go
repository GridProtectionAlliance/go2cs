// PromotedEmbedLib is the library half of a cross-package guard for a pointer-receiver "box-receiver"
// (direct-ж) method that is EXPORTED but promoted through an UNEXPORTED VALUE embed — the
// testing.T.common shape. Reached from another package it produced CS0117 ("does not contain a
// definition for 'Ꮡcommon'"): the converter descended through the embed's Ꮡ<embed> box accessor,
// which is `internal` (matching the unexported embed) and therefore invisible across assemblies.
// The fix pairs a converter change (call the promoted method directly on the receiver box
// cross-package) with a go2cs-gen change (emit a public box-receiver forwarder for such value-embed
// promoted methods). See docs/ConversionStrategies.md "Promoted box-receiver method through an
// unexported value embed".
package PromotedEmbedLib

// Reading is the EXPORTED result type returned by the promoted Report method, so Report's cross-package
// forwarder is `public` (an exported method on the exported Counter returning a public type). A method
// returning a builtin (@string/int) would instead get an `internal` forwarder — the shim scope tracks
// the enclosing struct's exportedness and downgrades a non-public return type, matching every other
// promoted forwarder.
type Reading struct {
	Sum int
}

// common is embedded by VALUE (and is unexported) in Counter. Its exported methods take the ADDRESS
// of a receiver field (`&c.sum`), so the converter emits them as direct-ж (`this ж<common>`)
// primaries — the same box-receiver shape as testing.T.common's Errorf/Helper/Logf.
type common struct {
	sum int
}

// Add is an exported, direct-ж (box-receiver) method promoted through the unexported value embed:
// it takes the address of a receiver field and writes through the pointer (void — the cryptotest
// Errorf/Helper/Logf shape).
func (c *common) Add(n int) {
	p := &c.sum
	*p += n
}

// Report is a second promoted box-receiver method, this one with a value return; it reads the field
// through a pointer to its own receiver field so it too is emitted as a box-receiver primary.
func (c *common) Report() Reading {
	p := &c.sum
	return Reading{Sum: *p}
}

// Counter embeds common by value and adds an exported value field (Label) for contrast — the
// exported field uses the ordinary cross-package field path, not the promoted-embed descent.
type Counter struct {
	common
	Label string
}

// New returns a heap-allocated *Counter so writes through the promoted method persist.
func New(label string) *Counter {
	return &Counter{Label: label}
}
