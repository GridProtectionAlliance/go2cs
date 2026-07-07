// A shared library package for the cross-package import behavioral tests: importing another package
// (a separate C# assembly) and using its exported surface. Uses a non-go2cs single-segment module so
// dir == module == package name, keeping the producer/consumer C# namespace + class mapping aligned
// (a `go2cs/`-prefixed module would hit the asymmetric prefix strip and mismatch).
package CrossPkgLib

// Celsius is an exported named numeric type (Phase 1: plain exported type + function + method).
// Precision is an exported UNTYPED constant - it emits as a `static readonly UntypedInt`
// wrapper in C#, the shape behind the census F10 switch-tag lowering (goarch.PtrSize).
const Precision = 2

// Sep is an exported UNTYPED RUNE constant - referenced in a string() conversion it
// renders as the static readonly Untyped wrapper, needing the default-type hop.
const Sep = ':'

type Celsius float64

// Temperature is an exported package-level TYPE ALIAS over Celsius (Phase 2: the GoTypeAlias /
// ImportedTypeAliases round-trip — the library records `[assembly: GoTypeAlias("Temperature", …)]`
// in its package_info.cs, which a consumer parses to emit a `global using` for `CrossPkgLib.Temperature`).
type Temperature = Celsius

// Boiling returns a Celsius value.
func Boiling() Celsius { return 100 }

// Freezing returns the alias type, so a consumer can name the imported alias.
func Freezing() Temperature { return 0 }

// Add is an exported method on the exported type.
func (c Celsius) Add(d Celsius) Celsius { return c + d }

// Sensor is an exported struct with exported fields and methods (Phase 3: cross-assembly struct
// field access + method call).
type Sensor struct {
	Name string
	Temp Celsius
}

// Hot reports whether the sensor reads above 50°C.
func (s Sensor) Hot() bool { return s.Temp > 50 }

// Label returns the sensor name; it also makes Sensor satisfy the Labeled interface.
func (s Sensor) Label() string { return s.Name }

// Calibrate is a POINTER-receiver method — promoted through a cross-package pointer embed it has
// no generated forwarder (method promotion is syntax-resolved); the converter emits the explicit
// hop `p.Sensor.Value.Calibrate(…)` at such call sites (the runtime Δrtype.Uncommon shape).
func (s *Sensor) Calibrate(d Celsius) { s.Temp += d }

// Labeled is an exported interface (Phase 3: cross-assembly interface satisfaction / duck typing —
// Sensor implements it in this assembly and a consumer assigns a Sensor to a Labeled).
type Labeled interface {
	Label() string
}

// Interface-satisfaction assertion (idiomatic Go): records, in THIS package/assembly, that Sensor
// implements Labeled — the impl glue can only be added to Sensor in its own assembly, so a consumer
// in another assembly relies on the library having witnessed the satisfaction here.
var _ Labeled = Sensor{}

// Describe takes the interface, so the consumer can pass a Sensor across the assembly boundary.
func Describe(l Labeled) string { return l.Label() }

// LabeledOf records the POINTER-sourced Sensor->Labeled pair IN THIS package, so its
// exported SensorжLabeled adapter exists — the consumer's same-simple-name LOCAL Labeled
// interface must NOT pick it up (the existence key qualifies the interface side; image's
// Paletted->image.Image record satisfying a Paletted->draw.Image cast, CS1503).
func LabeledOf(s *Sensor) Labeled { return s }

// Meter is an exported struct whose only method has a POINTER receiver and mutates state - a
// consumer that embeds *Meter satisfies its interfaces purely by promotion (the reflectlite
// rtype/*abi.Type shape), and mutations through any promoted path land on the shared object.
type Meter struct {
	count int
}

// Bump increments and returns the count (pointer receiver - promoted through pointer embeds).
func (m *Meter) Bump() int {
	m.count++
	return m.count
}

// NewMeter returns a fresh *Meter (count is unexported, so consumers need a constructor).
func NewMeter() *Meter { return &Meter{} }

// Reporter is implemented by *Meter ONLY (pointer receiver): the pointer-sourced
// GoImplement record generates the PUBLIC Meter-adapter this package exports.
type Reporter interface {
	Report() string
}

func (m *Meter) Report() string { return "count" }

// Alarm implements error by POINTER receiver: the adapter must be PUBLIC even though
// `error` is lowercase (the golib interface is public metadata - os PathError CS0122 x40).
type Alarm struct {
	Msg string
}

func (a *Alarm) Error() string { return a.Msg }

// AsErr converts INSIDE the package, creating the pointer-implement record for error.
func AsErr(a *Alarm) error { return a }

// AsReporter converts INSIDE the package, creating the pointer-implement record.
func AsReporter(m *Meter) Reporter { return m }


// Cache is a GENERIC struct with a pointer-receiver method - consumers embed *Cache[T]
// (the unique.uniqueMap shape: the embed's member name must come from the BASE type with
// the bracket-strip running before the dot-strip) and call through a raw box local (the
// X.Value hop ahead of the cross-package pointer-embed hop).
type Cache[T any] struct{ Hits int }

func (c *Cache[T]) Bump() int { c.Hits++; return c.Hits }

// Wrap is an exported GENERIC FUNCTION. A consumer that EXPLICITLY instantiates it through a
// package selector — CrossPkgLib.Wrap[int](5) — is the shape where the base selector (the
// generic-function-VALUE path in convSelectorExpr) AND the [int] index (convIndexExpr) both
// wanted to append the type arguments, doubling to CrossPkgLib.Wrap<int><int>
// (CS1525/CS0119/CS8124 across encoding/gob, xml, asn1, json, text/template, debug/*, unique).
func Wrap[T any](v T) []T { return []T{v} }

// Pair is a TWO-type-parameter generic function: an explicit CrossPkgLib.Pair[string, int]
// instantiation is an IndexListExpr (the concurrent.NewHashTrieMap[K, V] shape) — the same
// double-append through the list form (convIndexListExpr).
func Pair[K comparable, V any](k K, v V) V { return v }

// Sift is an exported METHODLESS FUNC TYPE that COLLIDES with the Sift METHOD below (the go/ast
// `type Filter func(...)` vs `(CommentMap).Filter` shape). The collision renames the type to ΔSift,
// but a methodless func type is rendered INLINE as its base delegate — no named ΔSift type is ever
// emitted. So the producer must NOT export a `[GoTypeAlias("Sift", "ΔSift")]`: a consumer that names
// CrossPkgLib.Sift must render it inline (Func<nint, bool>), not through a generated global-using at
// the nonexistent go.…ΔSift (go/doc's `ast.Filter`, CS0426).
type Sift func(int) bool

// Sift (the method) forces the type-vs-method collision that renames the Sift TYPE.
func (s Sensor) Sift(f Sift) bool { return f(int(s.Temp)) }

// Node is a plain exported struct used as the cross-package element type inside Resolver's signature
// (the go/ast Object shape).
type Node struct {
	ID int
}

// Resolver is a METHODLESS FUNC TYPE whose signature NAMES a cross-package type — a map value and a
// result are `*Node` — mirroring go/ast's `type Importer func(imports map[string]*Object, path
// string) (pkg *Object, err error)`. When a consumer passes its matching package-level func to
// Resolve below as a METHOD GROUP, the converter wraps it in Resolver's collapsed base delegate
// (`new Func<map<@string, ж<…Node>>, @string, (ж<…Node>, error)>(fn)`). That delegate must render
// through the structural getCSTypeName→iifeDelegateType path so the cross-package element keeps its
// file-local alias; the old string path (convertToCSTypeName∘getTypeName) mangled a SLASH-BEARING
// element package into a naive namespace form (go/doc's `go.ast.Object`, CS0234, plus a method-group
// CS0123 against the error-typed delegate). This single-segment producer compiles either way, but the
// emitted delegate FORM — unnamed vs Go-named result tuple — is the byte-golden that guards the routing.
type Resolver func(imports map[string]*Node, path string) (*Node, error)

// Resolve takes the named func type as a parameter, so a consumer passes its matching package-level
// func as a method group (the ast.NewPackage(fset, files, simpleImporter, nil) shape).
func Resolve(r Resolver, path string) (*Node, error) { return r(nil, path) }

// Probe/Sampler: the lib never converts *Probe to Sampler itself, so no exported adapter
// record exists - a consumer's pointer conversion records the pair LOCALLY and emits its
// own ProbeжSampler adapter with metadata-bound forwarding (fmt Fscan(os.Stdin, ...),
// CS1503 x3).
type Probe struct{ Hits int }

func (p *Probe) Sample() int { p.Hits++; return p.Hits }

type Sampler interface{ Sample() int }

// Sealed and Rated share Label without subsuming each other: a consumer interface
// satisfying BOTH inherits both and must RE-DECLARE the shared member (C# member lookup
// through two bases carrying the same signature is ambiguous - CS0121).
type Sealed interface {
	Label() string
	Seal() string
}

type Rated interface {
	Label() string
	Rating() int
}

// Ticks is an exported named INTEGER type (uintptr-based) - consumers define types over it
// (the registry Key over syscall.Handle shape).
// Status (the type) collides with the Sensor.Status method below, so it is exported
// DELTA-RENAMED - a consumer referencing it inside a func-typed global signature must
// use the recorded alias, not the raw qualified name (internal/poll hook_windows CS0426).
type Status struct {
	Code int
}

func (s Sensor) Status() int { return int(s.Temp) }

// Grade (the type) collides with the Sensor.Grade method - a renamed foreign NUMERIC for
// conversion-target and var-decl alias routing (internal/poll DupCloseOnExec/sockaddrToRaw).
type Grade int

func (s Sensor) Grade() int { return 1 }


// snapshot is UNEXPORTED but exposed by the exported var Latest and the exported func
// Peek - Go consumers hold the value and call its exported surface, so the C# type is
// publicized (CS0052/CS0050, internal/poll ErrNetClosing).
type snapshot struct {
	At int
}

var Latest = snapshot{At: 42}

func Peek() snapshot { return Latest }

type Ticks uintptr

// Device embeds Sensor BY VALUE - a consumer embedding Device promotes Sensor fields
// and methods through TWO cross-package levels (reflect structType-abi.StructType-abi.Type).
type Device struct {
	Sensor
	Serial int
}

// Marker is a struct whose FIELD is also named Marker, and the name ALSO collides with the Marker()
// method below — a type+field+method triple-collision. C# Δ-renames the TYPE to ΔMarker (type-vs-
// method) and DOUBLE-marks the FIELD to ΔΔMarker (field-vs-renamed-type). A cross-package access of
// the field must apply the same double (internal/trace's Label field, testtrace). Accessed via an
// inferred-type value so the consumer never names the renamed type.
type Marker struct {
	Marker string
}

// Marker (method) forces the type-vs-method collision that renames the Marker TYPE.
func (s Sensor) Marker() string { return s.Name }

// MakeMarker returns a Marker so a consumer reads its field via an inferred-type value.
func MakeMarker(s string) Marker { return Marker{Marker: s} }

// Emitter is an exported interface SEALED to this package by the UNEXPORTED marker method emitNode()
// (Go's ast.Expr.exprNode() / text/template/parse.Node.tree()/writeTo() shape). A consumer in ANOTHER
// assembly can HOLD an Emitter and call the exported Emit(), but cannot see or forward the unexported
// emitNode — its C# implementation is an INTERNAL extension here, so the pointer adapter generated at
// the consumer's cast site cannot bind `m_box.Value.emitNode()` (CS1061). Go bars calling a sealing
// marker from outside its package, so that adapter must STUB the (still-required) member instead.
type Emitter interface {
	Emit() string
	emitNode()
}

// Leaf implements Emitter DIRECTLY in this package (both the exported Emit and the sealing marker).
type Leaf struct {
	Text string
}

func (l *Leaf) Emit() string { return l.Text }

func (l *Leaf) emitNode() {}

// NewLeaf returns the CONCRETE *Leaf, so a consumer assigning it to an Emitter casts across the
// assembly boundary — the adapter is generated in the CONSUMER assembly, where emitNode is inaccessible.
func NewLeaf(text string) *Leaf { return &Leaf{Text: text} }
