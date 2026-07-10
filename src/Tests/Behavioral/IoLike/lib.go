// IoLike plays stdlib `io` in the GoType descriptor collision guard: a ROOT package whose name is
// also the FIRST namespace segment of its own subpackage (IoLike/FsLike ↔ io/fs). A consumer that
// imports BOTH gets a file alias `using IoLike = IoLike_package;` while a named-slice descriptor
// over the subpackage's type roots itself `IoLike.FsLike_package.…` — the shape whose alias
// substitution the generator must skip (see NamedSliceChildPkg).
package IoLike

// Version gives the consumer a reason to import the ROOT package (creating the file alias).
func Version() string {
	return "iolike-v1"
}
