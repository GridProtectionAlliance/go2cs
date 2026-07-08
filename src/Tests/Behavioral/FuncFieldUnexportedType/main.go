package main

import "fmt"

// hkdfState is an UNEXPORTED type reachable only through the func RESULT of a public var's map
// value type (mirrors crypto/internal/hpke's hkdfKDF exposed via SupportedKDFs). A public field
// whose type embeds this through a func signature forces it to be at least as accessible as the
// field, so the converter must publicize it or the emitted C# fails CS0052.
type hkdfState struct {
	name string
	size int
}

func (s *hkdfState) describe() string {
	return fmt.Sprintf("%s/%d", s.name, s.size)
}

// cfg is an UNEXPORTED type reachable only through a func PARAMETER of a public var's slice
// element type — guards the params side of the signature walk as well as the results side.
type cfg struct {
	verbose bool
}

// SupportedStates is a PUBLIC package-level var of type map[uint16]func() *hkdfState. The map
// value is a func whose RESULT exposes the unexported hkdfState — exactly the hpke shape.
var SupportedStates = map[uint16]func() *hkdfState{
	0x0001: func() *hkdfState { return &hkdfState{"sha256", 32} },
	0x0002: func() *hkdfState { return &hkdfState{"sha512", 64} },
}

// Appliers is a PUBLIC package-level var of type []func(*cfg). The slice element is a func whose
// PARAMETER exposes the unexported cfg.
var Appliers = []func(*cfg){
	func(c *cfg) { c.verbose = true },
}

func main() {
	for _, id := range []uint16{0x0001, 0x0002} {
		ctor := SupportedStates[id]
		s := ctor()
		fmt.Println(s.describe())
	}

	c := &cfg{}
	for _, apply := range Appliers {
		apply(c)
	}
	fmt.Println(c.verbose)
}
