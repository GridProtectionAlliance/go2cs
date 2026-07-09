// Regression test: a struct VALUE-embedding an interface whose converted C# type name is
// Δ-renamed (shadow-marker collision rename). log/slog declares `type Handler interface`
// AND `func (l *Logger) Handler()` — the method/type name collision makes the converter
// rename the TYPE to ΔHandler, while the struct field embedding it keeps the markerless Go
// embed name (`public ΔHandler Handler;`). testing/slogtest's `type wrapper struct
// { slog.Handler; … }` then broke in BOTH generated wrapper forms: the ImplementGenerator
// derived the promoted-forwarder FIELD name from the interface TYPE name, emitting bare
// `ΔHandler.Enabled(…)` in the value partial struct (CS0103) and `m_box.Value.ΔHandler.…`
// in the pointer adapter (CS1061). The field name must be the Δ-stripped simple name.
package main

import "fmt"

type Logger struct {
	name string
}

// Method named Handler collides with the type name below — the converter Δ-renames the TYPE
// (ΔHandler) but keeps the markerless name for struct fields that embed it.
func (l Logger) Handler() string { return "logger:" + l.name }

type Handler interface {
	Enabled(level int) bool
	Handle(msg string) string
	WithName(name string) string
}

type baseHandler struct {
	level int
}

func (b baseHandler) Enabled(level int) bool      { return level >= b.level }
func (b baseHandler) Handle(msg string) string    { return "base:" + msg }
func (b baseHandler) WithName(name string) string { return "base-name:" + name }

// wrapper VALUE-embeds the Δ-renamed interface and overrides ONE method (Handle);
// Enabled/WithName promote through the embedded interface field in both wrapper forms.
type wrapper struct {
	Handler
	prefix string
}

func (w wrapper) Handle(msg string) string { return w.prefix + w.Handler.Handle(msg) }

func main() {
	l := Logger{name: "test"}
	fmt.Println(l.Handler())

	base := baseHandler{level: 2}

	// VALUE cast → partial-struct implementation (promoted members forward through the field).
	var hv Handler = wrapper{Handler: base, prefix: "wrap:"}
	fmt.Println(hv.Enabled(1), hv.Enabled(3))
	fmt.Println(hv.Handle("msg"))
	fmt.Println(hv.WithName("n"))

	// POINTER cast → ж-adapter implementation (promoted members forward through m_box.Value).
	hp := &wrapper{Handler: base, prefix: "ptr:"}
	var hpi Handler = hp
	fmt.Println(hpi.Enabled(2))
	fmt.Println(hpi.Handle("msg"))
	fmt.Println(hpi.WithName("p"))
}
