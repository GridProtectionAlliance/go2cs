// Package vlib is a major-version (v2) module: its import path ends in /v2 but the
// package — and its converted C# class — is named for the PARENT segment, mirroring
// the standard library's math/rand/v2 (package rand).
package vlib

// Source is a small PRNG source interface (mirrors math/rand/v2.Source).
type Source interface {
	Uint64() uint64
}

// PCG is a tiny deterministic xorshift generator; *PCG implements Source.
type PCG struct {
	state uint64
}

func NewPCG(seed uint64) *PCG {
	return &PCG{state: seed}
}

func (p *PCG) Uint64() uint64 {
	p.state ^= p.state << 13
	p.state ^= p.state >> 7
	p.state ^= p.state << 17
	return p.state
}

// Rand draws from a Source (mirrors math/rand/v2.Rand).
type Rand struct {
	src Source
}

func New(src Source) *Rand {
	return &Rand{src: src}
}

func (r *Rand) IntN(n int) int {
	return int(r.src.Uint64() % uint64(n))
}
