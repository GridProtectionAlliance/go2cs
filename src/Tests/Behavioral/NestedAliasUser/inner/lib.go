// Nested-namespace exported type-alias guard (the internal/fuzz CorpusEntry shape).
//
// This package lives at a NESTED C# namespace: it is the `inner` subpackage of module
// NestedAliasUser, so the converter emits namespace `go.NestedAliasUser`, class `inner_package`. It
// exports a type ALIAS to an ANONYMOUS STRUCT; the converter lifts that anon struct to a nested type
// of `inner_package` (Entryᴛ1). Before the visitTypeSpec nested-namespace fix, the same-package
// alias-target qualifier was built from the bare `inner_package` class alone, dropping the
// `NestedAliasUser` namespace segment — so this package's OWN `global using Entry = ...` line and its
// every use (plus the consumer's imported alias) pointed at a nonexistent `go.inner_package.Entryᴛ1`
// (CS0234, the internal/fuzz 60-error shape). The fix prepends the namespace segments between the
// root and the class, so the target resolves to `go.NestedAliasUser.inner_package.Entryᴛ1`.
package inner

// Entry is an exported type ALIAS to an anonymous struct — the internal/fuzz CorpusEntry shape.
type Entry = struct {
	Name  string
	Data  []byte
	Count int
}

// NewEntry constructs an Entry — a SAME-PACKAGE use of the alias, one of the CS0234 sites the fix
// clears (the alias must resolve to go.NestedAliasUser.inner_package.Entryᴛ1).
func NewEntry(name string, count int) Entry {
	return Entry{Name: name, Count: count}
}

// Total sums Count over a slice of the alias — another same-package use of Entry, in both a
// signature and a range body.
func Total(entries []Entry) int {
	sum := 0
	for _, e := range entries {
		sum += e.Count
	}
	return sum
}
