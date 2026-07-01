// consumer_composite.go references the unsafe.Pointer type ONLY as a COMPOSITE element
// (`[]unsafe.Pointer`, inferred), with no scalar unsafe.Pointer and no unsafe import. The
// multi-name `var` forces the converter to emit the explicit element type `slice<@unsafe.Pointer>`,
// which — like the scalar case — resolves only through the `@unsafe` alias. This isolates the
// composite-element path: getTypeName sees a *types.Slice (not a direct *types.Basic), so it must
// match unsafe.Pointer by the type's string form, not just a direct Basic, to supply the alias.
package main

func compositeLen() int {
	var base int64 = 9
	var ptrs, k = makePtrs(&base), 0 // ptrs : []unsafe.Pointer (inferred) -> slice<@unsafe.Pointer>
	return len(ptrs) + k             // 1
}
