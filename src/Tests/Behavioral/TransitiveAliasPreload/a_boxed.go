// Guards the package-level imported-alias PRELOAD: importedTypeAliases is package-global but was
// populated INCREMENTALLY (per import, in sorted-filename order), so a foreign Δ-RENAMED type reached
// TRANSITIVELY — through a value whose package THIS file does not itself import — rendered its raw
// (nonexistent) name when this file converted before any file that DOES import that package. This is the
// go/printer comment.go `slash := list[0].Slash` shape (a token.Pos read through ast.Comment, with only
// go/ast imported; comment.go sorts first, so `slash`'s heap box rendered `go.token_package.Pos` instead
// of the alias `tokenꓸPos` = go.go.token_package.ΔPos — CS0426).
//
// a_boxed.go sorts BEFORE z_main.go. It reads a CrossPkgLib.Status TRANSITIVELY — through CrossPkgBox.Box's
// field — WITHOUT importing CrossPkgLib. `return &s` escapes `s`, so it is heap-boxed and its element TYPE
// (CrossPkgLib.Status, Δ-renamed to ΔStatus, alias CrossPkgLibꓸStatus) is rendered HERE. Because z_main.go
// (which DOES import CrossPkgLib) sorts AFTER this file, WITHOUT the preload the alias is not yet loaded
// when this file converts and the box renders the nonexistent raw `go.CrossPkgLib_package.Status` (CS0426).
package main

import "CrossPkgBox"

func peekPtr(code int) any {
	b := CrossPkgBox.New(code)
	s := b.S
	return &s
}
