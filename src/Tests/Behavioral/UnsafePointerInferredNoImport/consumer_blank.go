// consumer_blank.go reproduces the runtime symtabinl.go variant: a BLANK import of unsafe
// (`_ "unsafe"`, which binds the C# alias to `_`, not `@unsafe`) plus an unsafe.Pointer
// value obtained purely by inference. Like main.go it must still receive the `@unsafe`
// alias for the emitted `@unsafe.Pointer` type. It is not `package main`'s entry point; it
// only contributes a compiled function so the blank-import + inferred-type path is
// physically exercised (not merely described).
package main

import _ "unsafe" // blank import: side effects only — NO using is emitted (a `using _` alias hijacks C# discards)

func blankImportInfersUnsafe() bool {
	var base int64 = 7
	p := ptrOf(&base) // p : unsafe.Pointer (inferred) -> @unsafe.Pointer, only a blank unsafe import
	return p == nil   // false
}

func pair() (int, bool) { return 21, true }

// discardInBlankImportFile proves a `_` DISCARD works in a file with a blank import: the old
// `using _ = unsafe_package;` emission hijacked `_` file-wide, so a deconstruction discard
// bound the namespace alias instead (CS0118 + CS0029 — runtime tracetime.go's
// `(w, _) = w.ensure(…)`).
func discardInBlankImportFile() int {
	n, _ := pair()
	x := 0
	x, _ = pair() // the tracetime shape: EXISTING var + discard in one deconstruction
	return n + x // 42
}
