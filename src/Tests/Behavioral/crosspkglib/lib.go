// A shared library package for the cross-package import behavioral tests: importing another package
// (a separate C# assembly) and using its exported surface. Uses a plain lowercase single-segment
// module so dir == module == package name (the common Go layout), keeping the producer/consumer C#
// namespace + class mapping aligned.
package crosspkglib

// Celsius is an exported named numeric type (Phase 1: plain exported type + function + method).
type Celsius float64

// Temperature is an exported package-level TYPE ALIAS over Celsius (Phase 2: the GoTypeAlias /
// ImportedTypeAliases round-trip — the library records `[assembly: GoTypeAlias("Temperature", …)]`
// in its package_info.cs, which a consumer parses to emit a `global using` for `crosspkglib.Temperature`).
type Temperature = Celsius

// Boiling returns a Celsius value.
func Boiling() Celsius { return 100 }

// Freezing returns the alias type, so a consumer can name the imported alias.
func Freezing() Temperature { return 0 }

// Add is an exported method on the exported type.
func (c Celsius) Add(d Celsius) Celsius { return c + d }
