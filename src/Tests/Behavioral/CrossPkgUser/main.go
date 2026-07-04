// Cross-package import guard: importing another package (a separate C# assembly) and using its
// exported surface — Phase 1 (plain type + function + method), Phase 2 (an exported type ALIAS,
// exercising the GoTypeAlias / <ImportedTypeAliases> round-trip), and Phase 3 (exported struct field
// access + cross-assembly interface satisfaction).
package main

import (
	"fmt"

	"CrossPkgLib"
)

// CheckFunc is a FUNC-TYPED package var whose signature names a cross-package
// DELTA-RENAMED type (the internal/poll hook_windows shape) - the delegate type must
// render the recorded alias (CrossPkgLibꓸStatus), not the raw qualified name.
var CheckFunc func(CrossPkgLib.Status) int = func(st CrossPkgLib.Status) int {
	return st.Code * 2
}

func main() {
	b := CrossPkgLib.Boiling()
	r := b.Add(10)
	fmt.Println(float64(b))
	fmt.Println(float64(r))

	// Phase 2: name the imported exported type ALIAS `CrossPkgLib.Temperature`.
	var t CrossPkgLib.Temperature = CrossPkgLib.Freezing()
	t = t.Add(32)
	fmt.Println(float64(t))

	// Phase 3: exported struct — field access + method call across the assembly boundary.
	s := CrossPkgLib.Sensor{Name: "kitchen", Temp: CrossPkgLib.Boiling()}
	fmt.Println(s.Name, float64(s.Temp), s.Hot())

	// Phase 3: cross-assembly interface satisfaction — a Sensor (lib assembly) is a Labeled.
	var l CrossPkgLib.Labeled = s
	fmt.Println(l.Label())
	fmt.Println(CrossPkgLib.Describe(s))

	// Phase 4: promoted fields through a cross-package POINTER embed — read AND write-through
	// (the accessor is a true ref through the embed: a write via p.Temp mutates the target s2).
	// NOTE: promoted METHOD calls (p.Hot()) are a separate, still-open cross-package gap (the
	// method-promotion path is also syntax-resolved); zero runtime sites need it — call the method
	// through the embed explicitly.
	s2 := CrossPkgLib.Sensor{Name: "attic", Temp: 20}
	p := probe{Sensor: &s2, id: 7}
	fmt.Println(p.Name, float64(p.Temp), p.id) // attic 20 7
	p.Temp = 75
	fmt.Println(float64(s2.Temp), s2.Hot()) // 75 true — write-through observed via the target

	// Phase 4b: a promoted POINTER-RECEIVER METHOD through the cross-package pointer embed —
	// `p.Calibrate(5)` has no generated forwarder (method promotion is syntax-resolved), so the
	// converter emits the explicit hop `p.Sensor.Value.Calibrate(…)`; the write reaches the target.
	// (A promoted VALUE-receiver method call, p.Hot(), remains a documented open gap — call
	// through the embed explicitly.)
	p.Calibrate(5)
	fmt.Println(float64(s2.Temp)) // 80 — the promoted pointer-receiver write reached s2

	// Phase 4: promoted fields through a cross-package VALUE embed.
	g := tagged{Sensor: CrossPkgLib.Sensor{Name: "cellar", Temp: 5}, n: 3}
	fmt.Println(g.Name, float64(g.Temp), g.n) // cellar 5 3
	g.Temp = 60
	fmt.Println(float64(g.Temp), g.Sensor.Hot()) // 60 true

	// Phase 5: the reflectlite rtype shape. counter embeds *CrossPkgLib.Meter; the local Meter
	// INTERFACE is Δ-renamed (collides with tagged's Meter method), but the embed FIELD keeps its
	// unrenamed struct-scoped name — the promoted-call hop must render `c.Meter.Value.Bump()`,
	// not `c.ΔMeter...` (CS1061). The interface value is satisfied purely by promotion, so the
	// generated implementation forwards through the same hop (CS1929); all paths share the box.
	c := counter{Meter: CrossPkgLib.NewMeter()}
	fmt.Println(c.Bump())        // 1 — promoted call through the hop
	var m Meter = c
	fmt.Println(m.Bump())        // 2 — through the generated interface impl, same object
	fmt.Println(c.Bump())        // 3 — aliasing: all bumps landed on one Meter
	fmt.Println(g.Meter())       // tagged-meter - the colliding METHOD still works

	// Phase 6: the registry Key shape - a LOCAL defined type over a CROSS-PACKAGE named
	// numeric (type reading CrossPkgLib.Celsius). Its [GoType] wrapper keeps the NAMED
	// base, so a conversion back to that base must be the ONE-STEP wrapper operator
	// (CrossPkgLib.Celsius(rd)) - the underlying hop would chain two user conversions
	// (reading->Celsius->float64, CS0030 - registry value.go syscall.Handle(k)).
	rd := reading(CrossPkgLib.Boiling())
	cback := CrossPkgLib.Celsius(rd)
	fmt.Println(float64(cback), cback == CrossPkgLib.Boiling())
	st1, err1 := stampOrErr(false)
	st2, err2 := stampOrErr(true)
	fmt.Println(st1 == st2, err1 != nil, st2 == bigStamp, err2 == nil)

	// Phase 7: TWO-LEVEL cross-package VALUE embed (rig embeds Device embeds Sensor):
	// promoted-field reads/writes resolve through the referenced wrappers' generated
	// ref-properties (reflect census F1 metadata-fallback shape).
	rg := rig{Device: CrossPkgLib.Device{Sensor: CrossPkgLib.Sensor{Name: "deep", Temp: 55}, Serial: 9}, id: 1}
	fmt.Println(float64(rg.Temp), rg.Serial, rg.id)
	rg.Temp = 66
	fmt.Println(float64(rg.Device.Sensor.Temp))
	// (a PROMOTED METHOD call through the two-level cross-package embed - rg.Hot() -
	// remains the documented open sibling gap: shim discovery is syntax-based and the
	// referenced assembly contributes no syntax; call through the embed explicitly)
	fmt.Println(rg.Device.Sensor.Hot())
	// A promoted POINTER-RECEIVER method through the two-level VALUE-embed chain IS
	// converter-emitted (census F7): the hop descends per level -
	// Ꮡrg.of(rig.ᏑDevice).of(CrossPkgLib.Device.ᏑSensor).Calibrate(...) - mirroring
	// reflect's sliceType -> abi.SliceType -> abi.Type Common()/Uncommon() sites (CS1929 x4).
	rg.Calibrate(3)
	fmt.Println(float64(rg.Device.Sensor.Temp))

	// census F10: a switch whose TAG is a cross-package untyped constant (the goarch.PtrSize
	// shape, reflect abi.go) - the static readonly UntypedInt wrapper cannot govern a C#
	// switch nor an `is` constant pattern (CS9135 x2); the if-else lowering compares ==.
	switch CrossPkgLib.Precision {
	case 1:
		fmt.Println("coarse")
	case 2:
		fmt.Println("fine")
	default:
		fmt.Println("unknown")
	}

	// string(<cross-package untyped rune const>) - the wrapper needs the default-type hop
	// (string(utf8.RuneError), time format.cs CS0030).
	fmt.Println("a" + string(CrossPkgLib.Sep) + "b")

	fmt.Println(CheckFunc(CrossPkgLib.Status{Code: 21}), CrossPkgLib.Sensor{Temp: 9}.Status())
}

// reading mirrors registry Key: a defined type whose written base is a cross-package named type.
type reading CrossPkgLib.Celsius

// stamp mirrors registry Key over an INTEGER base: its wrapper has only stamp<->Ticks operators
// (no numeric bridge), so a beyond-int32 const must chain through the base
// (unchecked((stamp)(CrossPkgLib.Ticks)2147483649)) and a constant return ()
// needs the same hop.
type stamp CrossPkgLib.Ticks

const bigStamp stamp = 0x80000001

func stampOrErr(ok bool) (stamp, error) {
	if !ok {
		return 0, fmt.Errorf("no stamp")
	}
	return bigStamp, nil
}

// Phase 4: field promotion through a CROSS-PACKAGE embed. In a real MSBuild build the library
// arrives as a METADATA reference (never a CompilationReference), so the generator's syntax-based
// member resolution finds nothing and the promoted-field accessors were silently EMPTY — every
// `p.Name`/`p.Temp` on the embedding struct was CS1061 (runtime hits this on
// `type rtype struct { *abi.Type }`, t.TFlag/t.Str/t.Kind_). The generator now falls back to the
// semantic model (public instance fields via the type's metadata symbol), emitting true-ref
// accessors through the embed (`ref Sensor.Value.Temp` for a pointer embed), so writes through the
// promoted name reach the embedded target.

// probe embeds *CrossPkgLib.Sensor (POINTER embed — the rtype shape).
type probe struct {
	*CrossPkgLib.Sensor
	id int
}

// tagged embeds CrossPkgLib.Sensor BY VALUE (the same resolution failure hit value embeds).
type tagged struct {
	CrossPkgLib.Sensor
	n int
}

// Meter is a LOCAL package-level type sharing its name with the method tagged.Meter below - the
// type-vs-method collision Δ-renames THIS type (ΔMeter). The counter embed field below is ALSO
// named Meter (from *CrossPkgLib.Meter) but is struct-scoped and declared UNRENAMED - the
// embedded-pointer hop emission must not apply the package-level rename to the field
// (reflectlite's rtype.Type vs the ΔType interface, CS1061).
type Meter interface {
	Bump() int
}

// Meter (the method) forces the collision that Δ-renames the Meter interface above.
func (t tagged) Meter() string { return "tagged-meter" }

// counter satisfies the local Meter interface PURELY by promotion through a cross-package
// pointer embed - the generated interface implementation must forward through the hop
// (this.Meter.Value.Bump()), since counter has no Bump of its own (reflectlite's
// GoImplement<rtype, ΔType> Size/Kind shape, CS1929).
type counter struct {
	*CrossPkgLib.Meter
}

// rig embeds Device BY VALUE - two-level cross-package promotion.
type rig struct {
	CrossPkgLib.Device
	id int
}
