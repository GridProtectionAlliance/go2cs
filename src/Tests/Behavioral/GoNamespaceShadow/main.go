// Namespace-root-shadow guard: importing a package whose path starts with "go/"
// (namespace go.go) makes C# bind the bare leading "go" of other root-qualified
// using targets to that sibling namespace instead of the root (CS0234), so the
// converter must emit them with global:: — both in this importer (go.go in the
// import CLOSURE) and inside the go/* package itself (namespace go.go).
package main

import (
	"fmt"
	"math"
	"math/rand"

	"go/nsshadow"
)

func main() {
	fmt.Println(nsshadow.Add(math.MaxInt8, rand.Intn(1)))
	fmt.Println(nsshadow.Max8() + nsshadow.Pad())
}
