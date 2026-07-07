// CrossPkgBox is an intermediary package for the alias-preload guard: it holds a struct with a FIELD
// of CrossPkgLib's Δ-renamed Status type. A consumer that reads that field WITHOUT importing CrossPkgLib
// reaches Status only TRANSITIVELY — the go/ast(`Comment.Slash: token.Pos`) shape that drives the
// package-level imported-alias preload.
package CrossPkgBox

import "CrossPkgLib"

// Box carries a CrossPkgLib.Status by value.
type Box struct {
	S CrossPkgLib.Status
}

// New builds a Box; the caller need not name CrossPkgLib.
func New(code int) Box {
	return Box{S: CrossPkgLib.Status{Code: code}}
}
