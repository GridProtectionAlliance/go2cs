package main

import "fmt"

// Describer is implemented by *Setting via a POINTER receiver, so *Setting satisfies Describer
// but a Setting value does not — mirroring log/slog's Leveler, satisfied only by *LevelVar.
type Describer interface {
	Describe() string
}

// Setting has a POINTER-receiver Describe, so only its pointer implements Describer.
type Setting struct {
	name  string
	value int
}

func (s *Setting) Describe() string {
	return fmt.Sprintf("%s=%d", s.name, s.value)
}

// holder's first field is the INTERFACE type Describer. Constructing holder with a *Setting places
// a pointer-to-interface conversion at a STRUCT-LITERAL argument position — the case that previously
// recorded no GoImplement<Setting, Describer>(Pointer=true), so the box was passed bare to the
// interface-typed field (CS1503).
type holder struct {
	d     Describer
	label string
}

// A package-global Setting, addressed with & below — mirroring slog's `&logLoggerLevel`.
var globalSetting = Setting{name: "verbosity", value: 3}

func main() {
	// Positional struct literal (the &handlerWriter{h, &logLoggerLevel, pc} shape): field d
	// (Describer) receives *Setting via &globalSetting.
	h := &holder{&globalSetting, "positional"}
	fmt.Println(h.label, h.d.Describe())

	// Keyed struct literal — exercises the keyed branch of the same field-interface routing.
	local := Setting{name: "count", value: 7}
	h2 := holder{d: &local, label: "keyed"}
	fmt.Println(h2.label, h2.d.Describe())
}
