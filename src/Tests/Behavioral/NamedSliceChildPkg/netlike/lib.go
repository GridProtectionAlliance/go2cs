// netlike plays net/http in the GoType descriptor collision guard: a package living at a NESTED
// namespace (go.NamedSliceChildPkg) that imports BOTH a top-level package — IoLike, giving this
// file the alias `using IoLike = IoLike_package;`, NOT Δ-renamed here because
// go.NamedSliceChildPkg has no IoLike child namespace — AND its FsLike subpackage. The named-slice
// descriptor over the subpackage's type then STARTS with the alias-colliding segment
// (`[]IoLike.FsLike_package.Info`, the `type fileInfoDirs []fs.FileInfo` shape from net/http's
// fs.go). TypeGenerator's source-alias substitution must skip it: rewriting the segment maps the
// descriptor to the nonexistent go.IoLike_package.FsLike_package.Info (CS0426 ×48 in net/http).
package netlike

import (
	"IoLike"
	"IoLike/FsLike"
)

// InfoList plays net/http's fileInfoDirs — a named slice of a foreign SUBPACKAGE type.
type InfoList []FsLike.Info

// Describe imports the ROOT package too, creating the colliding file alias.
func Describe() string {
	return IoLike.Version()
}

// Build constructs the named slice from subpackage values.
func Build() InfoList {
	return InfoList{
		FsLike.NewInfo("alpha", 3),
		FsLike.NewInfo("beta", 5),
	}
}

// ElementName reads an element through the named slice.
func ElementName(infos InfoList, i int) string {
	return infos[i].Name
}

// TotalSize keeps the named slice in a signature position and ranges it.
func TotalSize(infos InfoList) int {
	sum := 0
	for _, info := range infos {
		sum += info.Size
	}
	return sum
}
