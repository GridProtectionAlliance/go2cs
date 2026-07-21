// Guards the Go METHOD-EXPRESSION form `(*T).M` (the unbound method as a func value, the receiver
// lifted to the first parameter) when T is FOREIGN to the emitting package through a DOT-import —
// the reduced bufio_test `(*Reader).ReadBytes` / `(*Reader).ReadSlice` shape. The method's
// [GoRecv] extension static lives in the DEFINING package's C# class, so the bare method name is
// not in scope as a method group (a `using static` imports an extension method only for
// `recv.M()` invocation, never as a bare group) -> CS0103 before the fix. The static form must be
// qualified with the method's OWN package alias (`mep.Read`); the old source-spelling peel missed
// it because a dot-imported type is a BARE ident (`Reader`), not a `pkg.T` selector.
package main

import (
	"fmt"

	. "MethodExprDotImport/mep"
)

func main() {
	readers := []func(*Reader, byte) (string, error){
		(*Reader).Read,
		(*Reader).Peek,
	}

	r := &Reader{Name: "go2cs"}

	for _, read := range readers {
		s, _ := read(r, ',')
		fmt.Println(s)
	}
}
