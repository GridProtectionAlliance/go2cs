// BCL-name shadow guard (internal/trace/traceviewer's `type Range struct` shape). A Go type named
// after a BCL type becomes a SIBLING member of the package class, shadowing the bare BCL name in
// every generated partial. The named-string and named-slice wrapper indexers take System.Range —
// the templates must global::-qualify it (and every other BCL reference) or ViewType.g.cs binds
// the Go Range (CS1503).
package main

import "fmt"

// Range shadows System.Range as a sibling member of main_package.
type Range struct {
	Start, End int
}

// ViewType is a named string type — its wrapper forwards @string's indexers (TV-1's ViewType.g.cs).
type ViewType string

// byteView is a named slice type — its wrapper's sub-slice indexer takes System.Range too.
type byteView []byte

func main() {
	r := Range{Start: 1, End: 4}
	v := ViewType("abcdef")

	sub := v[r.Start:r.End]
	fmt.Println(sub)
	fmt.Println(len(sub))
	fmt.Println(v[0])

	raw := []byte{120, 121, 122, 119}
	b := byteView(raw)
	bs := b[r.Start:r.End]
	fmt.Println(len(bs))
	fmt.Println(bs[0])
	fmt.Println(r.End - r.Start)
}
