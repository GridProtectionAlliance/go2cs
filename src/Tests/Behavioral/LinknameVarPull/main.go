// The //go:linkname variable-pull behavioral test. `secret` is declared bodyless here and linkname-
// PULLED from LinknameVarPullLib.secret (a separate package, hence a separate C# assembly). Go aliases
// their storage; the converter emits `secret` as a forwarding property to the provider's now-public
// symbol and queues the provider for a project reference. Printing it proves the cross-assembly pull
// resolves at run time to the provider's value.
package main

import (
	"fmt"
	_ "unsafe" // for go:linkname

	_ "LinknameVarPullLib"
)

//go:linkname secret LinknameVarPullLib.secret
var secret string

func main() {
	fmt.Println(secret)
}
