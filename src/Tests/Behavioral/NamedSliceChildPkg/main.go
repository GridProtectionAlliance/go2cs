// GoType descriptor vs generator alias-substitution guard (net/http's fs.go shape,
// `type fileInfoDirs []fs.FileInfo`). The interesting package is the netlike SUBPACKAGE — a
// consumer at a nested namespace whose file alias for IoLike collides with the leading namespace
// segment of its named-slice descriptor (see netlike/lib.go). This main exercises the named slice
// across the assembly boundary: construction, len, element access, sub-slice (the wrapper keeps
// the named type), and a signature position.
package main

import (
	"fmt"

	"NamedSliceChildPkg/netlike"
)

func main() {
	fmt.Println(netlike.Describe())

	infos := netlike.Build()
	fmt.Println(len(infos))
	fmt.Println(netlike.ElementName(infos, 0))
	fmt.Println(netlike.TotalSize(infos))

	// Sub-slice keeps the named type (the wrapper's System.Range indexer).
	tail := infos[1:]
	fmt.Println(netlike.TotalSize(tail))
}
