// Cross-package guard for the nested-namespace exported type-alias fix. This top-level `package main`
// (C# namespace `go`) imports the `inner` subpackage — which lives at the NESTED namespace
// `go.NestedAliasUser` — and references its exported anon-struct type ALIAS `inner.Entry` across the
// assembly boundary. The imported alias must carry the FULL namespace
// (go.NestedAliasUser.inner_package.Entryᴛ1); a dropped `NestedAliasUser` segment is CS0234 — the
// internal/fuzz CorpusEntry consumed by testing / testing/internal/testdeps shape.
package main

import (
	"fmt"

	"NestedAliasUser/inner"
)

func main() {
	e := inner.NewEntry("alpha", 3)
	fmt.Println(e.Name, e.Count) // alpha 3

	// Name the imported alias directly — forces the consumer's imported global-using alias for Entry.
	e2 := inner.Entry{Name: "beta", Data: []byte("xy"), Count: 5}
	fmt.Println(e2.Name, len(e2.Data), e2.Count) // beta 2 5

	fmt.Println(inner.Total([]inner.Entry{e, e2})) // 8
}
