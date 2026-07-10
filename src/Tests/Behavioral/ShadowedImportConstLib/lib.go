// A library whose exported TYPED const collides with a same-named METHOD, so the const is
// Δ-renamed in this package and exported `const:ΔPeak` in its package_info (time's `Second`
// const vs `Time.Second()` method shape). Uses a non-go2cs single-segment module so
// dir == module == package name (matching the CrossPkgLib convention).
package ShadowedImportConstLib

// Span is a named numeric a consumer scales by Peak (the `time.Duration * time.Second` shape).
type Span int64

// Peak (const) collides with the Meter.Peak method below — the CONST is Δ-renamed.
const Peak Span = 60

type Meter struct{ Level int }

// Peak (the method) forces the const-vs-method collision that Δ-renames the Peak const.
func (m Meter) Peak() int { return m.Level + 1 }
