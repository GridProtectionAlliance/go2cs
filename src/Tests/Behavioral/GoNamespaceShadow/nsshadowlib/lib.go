package nsshadow

import (
	"math"
	"math/rand"
)

// Add returns the sum of two ints.
func Add(x, y int) int {
	return x + y
}

// Max8 returns math.MaxInt8. The math import's using alias is emitted INSIDE this
// package's go.go namespace, so it must be global::-rooted to compile.
func Max8() int {
	return math.MaxInt8
}

// Pad returns a pseudo-random int in [0, 1) — always 0. Importing both math and
// math/rand forces the rand using onto the root-qualified path.
func Pad() int {
	return rand.Intn(1)
}
