// Guards the collision-renamed dot-import alias fix. Dot-importing "sync" — a package with a
// child namespace (go.sync, contributed by sync/atomic) — makes the converter collision-rename
// the package qualifier to the shadow marker (Δsync). A type reached through the dot import is
// emitted qualified (Δsync.Mutex), so the file-local canonical alias that visitFile SUPPLIES for
// the reference must carry the SAME rename, or the type binds an alias that was never emitted
// (CS0246). Before the fix the supplied alias stayed the bare "using sync = sync_package;" while
// the type reference rendered "Δsync.Mutex" — an undefined alias.
package main

import (
	"fmt"
	. "sync"
)

// The *Mutex parameter forces the dot-imported type into a qualified position.
func ready(mu *Mutex) bool {
	return mu != nil
}

func main() {
	var mu Mutex
	fmt.Println("mutex ready:", ready(&mu))
}
