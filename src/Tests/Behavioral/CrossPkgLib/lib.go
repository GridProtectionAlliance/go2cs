// A shared library package for the cross-package import behavioral tests: importing another package
// (a separate C# assembly) and using its exported surface. Uses a non-go2cs single-segment module so
// dir == module == package name, keeping the producer/consumer C# namespace + class mapping aligned
// (a `go2cs/`-prefixed module would hit the asymmetric prefix strip and mismatch).
package CrossPkgLib

// Celsius is an exported named numeric type (Phase 1: plain exported type + function + method).
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

// Ticks is an exported named INTEGER type (uintptr-based) - consumers define types over it
// (the registry Key over syscall.Handle shape).
type Ticks uintptr

// Device embeds Sensor BY VALUE - a consumer embedding Device promotes Sensor fields
// and methods through TWO cross-package levels (reflect structType-abi.StructType-abi.Type).
type Device struct {
	Sensor
	Serial int
}
