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

// gauge locks the general signature path: the DELTA-renamed foreign type as a METHOD
// parameter and result-tuple element (internal/poll fd_windows CS0426 x19).
func gauge(st CrossPkgLib.Status) (CrossPkgLib.Status, int) {
	return CrossPkgLib.Status{Code: st.Code + 1}, st.Code
}

// meterBox locks the struct-FIELD display path: the renamed foreign type as a named
// field's declared type (internal/poll's `Sysfd syscall.Handle`, CS0426).
type meterBox struct {
	st  CrossPkgLib.Status
	sat int
}

// note receives a deferred untyped-const-reference arg - deferǃ inference must see the
// DEFAULT type, not the UntypedInt wrapper (CS0123, poll deferred Seek).
func note(n int) { fmt.Println("noted", n) }

// Tagged aliases the foreign interface: an implementation converted through BOTH the
// alias and the original name must record ONCE (types.Unalias key - os DirEntry alias,
// CS8646 x4 double explicit implementation).
type Tagged = CrossPkgLib.Labeled

type badge struct{ name string }

func (b badge) Label() string { return "badge:" + b.name }

// tick aliases the foreign named numeric: a conversion from a DIFFERING basic must hop
// through the underlying even when the target arrives as a *types.Alias (os FileMode,
// CS0030 removeall_noat).
type tick = CrossPkgLib.Ticks

// namedLabel structurally contains CrossPkgLib.Labeled (a strict superset of its method
// set), so the converter emits C# interface inheritance and the Describe call in main is
// an implicit reference conversion — no adapter, identity preserved (os CopyFS passing an
// fs.File to io.Copy, CS1503).
type namedLabel interface {
	Label() string
	Rank() int
}

type emblem struct {
	name string
	rank int
}

func (e emblem) Label() string { return e.name }
func (e emblem) Rank() int     { return e.rank }

// stamped EMBEDS the foreign CrossPkgLib.Labeled explicitly (zip's fileInfoDirEntry
// embedding fs.FileInfo): a struct recorded against BOTH the derived local interface
// and the foreign base must have the base record pruned — the inheritance tracking
// stores the CANONICAL interface name so the prune's implementation-map lookup matches
// (headerFileInfo implemented fs.FileInfo twice, CS8646 ×6/CS0111 ×2).
type stamped interface {
	CrossPkgLib.Labeled
	Stamp() string
}

type seal struct{ name string }

func (s seal) Label() string { return s.name }
func (s seal) Stamp() string { return "ok:" + s.name }

// certificate satisfies BOTH CrossPkgLib.Sealed and CrossPkgLib.Rated — which share Label
// without subsuming each other — plus CrossPkgLib.Labeled (subsumed by either): the two
// non-subsumed bases are inherited and the shared Label is RE-DECLARED to hide the two
// inherited slots, keeping member lookup through certificate unambiguous (CS0121).
type certificate interface {
	Label() string
	Seal() string
	Rating() int
	Serial() int
}

type cert struct{ id int }

func (c cert) Label() string { return "cert" }
func (c cert) Seal() string  { return "wax" }
func (c cert) Rating() int   { return 9 }
func (c cert) Serial() int   { return c.id }

// holder embeds the FOREIGN GENERIC *CrossPkgLib.Cache[T] (unique.uniqueMap's shape):
// the member emits under the base name `Cache` (bracket-strip before dot-strip - the
// type args' qualified names otherwise win the LastIndex and misname the member), and a
// promoted method call through a raw box local hops X.Value first (CS1061 x4).
type holder[T any] struct {
	*CrossPkgLib.Cache[T]
	name string
}

// relay's pointer receiver satisfies CrossPkgLib.Reporter; getReporter forwards the
// multi-value call as its WHOLE result list, so the converter deconstructs the tuple and
// converts the interface element — C# tuple conversions are not element-wise (os
// SyscallConn returning (*rawConn, error) as (syscall.RawConn, error), CS0266).
type relay struct{ tag string }

func (r *relay) Report() string { return "relay:" + r.tag }

func makeRelay() (*relay, error) { return &relay{tag: "live"}, nil }

func getReporter() (CrossPkgLib.Reporter, error) { return makeRelay() }

func main() {
	defer note(CrossPkgLib.Precision)

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
	g1, c1 := gauge(CrossPkgLib.Status{Code: 5})
	fmt.Println(g1.Code, c1) // 6 5
	mb := meterBox{st: CrossPkgLib.Status{Code: 3}, sat: 1}
	fmt.Println(mb.st.Code + mb.sat) // 4
	fmt.Println(CrossPkgLib.Latest.At, CrossPkgLib.Peek().At) // 42 42

	// Conversion-TARGET and no-value VAR-DECL positions of renamed foreign types route
	// through the recorded alias (CS0426, internal/poll).
	gv := CrossPkgLib.Grade(7)
	var stv CrossPkgLib.Status
	stv.Code = int(gv)
	fmt.Println(stv.Code) // 7
	var l1 CrossPkgLib.Labeled = badge{name: "a"}
	var l2 Tagged = badge{name: "b"}
	fmt.Println(l1.Label(), l2.Label())
	// Cross-package POINTER-to-interface assignment: the conversion references the
	// FOREIGN assembly's public adapter (os err = &PathError{...}, CS0029 x38).
	var rep CrossPkgLib.Reporter
	mtr := CrossPkgLib.NewMeter()
	rep = mtr
	fmt.Println(rep.Report())
	var boom error = &CrossPkgLib.Alarm{Msg: "boom"}
	fmt.Println(boom.Error())
	tk := tick(3 | int(gv))
	fmt.Println(uint64(tk)) // 7

	var nl namedLabel = emblem{name: "gold", rank: 1}
	fmt.Println(CrossPkgLib.Describe(nl), nl.Rank()) // gold 1

	var st stamped = seal{name: "notary"}
	var lb CrossPkgLib.Labeled = seal{name: "base"}
	fmt.Println(st.Label(), st.Stamp(), CrossPkgLib.Describe(st), lb.Label()) // notary ok:notary notary base

	rp, rerr := getReporter()
	fmt.Println(rp.Report(), rerr == nil) // relay:live true

	var ct certificate = cert{id: 42}
	fmt.Println(ct.Label(), ct.Seal(), ct.Rating(), ct.Serial()) // cert wax 9 42
	var sd CrossPkgLib.Sealed = ct
	var rt CrossPkgLib.Rated = ct
	fmt.Println(sd.Label(), rt.Rating()) // cert 9

	// *Probe -> Sampler: no exported adapter in the lib (it never converts the pair) - the
	// conversion records locally and the LOCAL ProbeжSampler adapter wraps the box, so the
	// mutation through the interface aliases pr (reads back 2, not a copy's 0).
	pr := &CrossPkgLib.Probe{}
	var sam CrossPkgLib.Sampler = pr
	fmt.Println(sam.Sample(), sam.Sample(), pr.Hits) // 1 2 2

	h := &holder[int]{Cache: &CrossPkgLib.Cache[int]{}, name: "h"}
	fmt.Println(h.Bump(), h.Bump(), h.name) // 1 2 h
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
