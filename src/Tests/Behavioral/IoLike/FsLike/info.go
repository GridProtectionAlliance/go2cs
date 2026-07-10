// FsLike plays stdlib `io/fs`: a SUBPACKAGE of IoLike whose types land at the nested namespace
// `go.IoLike`, class `FsLike_package` — so a foreign named-slice descriptor over Info starts with
// the SAME identifier (`IoLike.`) that the consumer's root-package import aliases.
package FsLike

// Info plays fs.FileInfo — the element type of the consumer's named slice.
type Info struct {
	Name string
	Size int
}

// NewInfo constructs an Info for the consumer.
func NewInfo(name string, size int) Info {
	return Info{Name: name, Size: size}
}
