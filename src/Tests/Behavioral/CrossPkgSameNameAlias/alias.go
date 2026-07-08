// package atomic deliberately shares its name with the imported stdlib "sync/atomic" package — the
// html/template (package template) importing text/template (also package template) shape — to guard
// cross-package type-ALIAS resolution when the consumer and the imported package have the SAME Go
// package name. getFullTypeName must compare package IDENTITY, not name: a name-only guard treats the
// foreign atomic.Int32 as same-package and falls through to the t.String() slash-strip, dropping BOTH
// the `sync` path segment and the `_package` class — emitting `global using Int32 = go.atomic.Int32;`
// (CS0234) instead of the fully-qualified `go.sync.atomic_package.Int32`.
package atomic

import "sync/atomic"

// Int32 aliases the SAME-NAMED foreign package's Int32 (the `type FuncMap = template.FuncMap` shape).
// Its `global using` RHS must be the FULLY cross-package-qualified name, not a dropped-segment form.
type Int32 = atomic.Int32

// Wrap names the SAME-NAMED foreign package's type in a signature + body position (the getTypeName path,
// which already keyed on package identity and rendered correctly) alongside the alias RHS above, so both
// routes are exercised and stay in agreement.
func Wrap(v *atomic.Int32) *Int32 { return v }
