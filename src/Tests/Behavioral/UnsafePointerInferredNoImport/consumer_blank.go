// consumer_blank.go reproduces the runtime symtabinl.go variant: a BLANK import of unsafe
// (`_ "unsafe"`, which binds the C# alias to `_`, not `@unsafe`) plus an unsafe.Pointer
// value obtained purely by inference. Like main.go it must still receive the `@unsafe`
// alias for the emitted `@unsafe.Pointer` type. It is not `package main`'s entry point; it
// only contributes a compiled function so the blank-import + inferred-type path is
// physically exercised (not merely described).
package main

import _ "unsafe" // blank import: emits `using _ = unsafe_package;`, NOT the @unsafe alias

func blankImportInfersUnsafe() bool {
	var base int64 = 7
	p := ptrOf(&base) // p : unsafe.Pointer (inferred) -> @unsafe.Pointer, only a blank unsafe import
	return p == nil   // false
}
