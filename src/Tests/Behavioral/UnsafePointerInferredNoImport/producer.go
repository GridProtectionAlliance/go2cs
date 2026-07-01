// UnsafePointerInferredNoImport guards emission of the `using @unsafe = unsafe_package;`
// alias in a file that references the unsafe.Pointer TYPE only through inference and
// therefore never imports `unsafe` under a usable name. `unsafe.Pointer` renders as
// `@unsafe.Pointer`, which resolves only via that alias; the alias was previously emitted
// solely from a NAMED import, so a consumer file that infers an unsafe.Pointer local
// (`p := ptrOf(&base)`) while importing only `fmt` failed to compile (CS0246) — exactly the
// runtime preempt.go / symtabinl.go pattern (a var whose inferred type is funcdata()'s
// unsafe.Pointer return, in a file that does not import unsafe). This producer file DOES
// import unsafe; main.go (the consumer) does not. It also covers the COMPOSITE-element form
// (`[]unsafe.Pointer` inferred without an unsafe import), which renders the element as
// `@unsafe.Pointer` through the type string path and equally needs the alias.
//
// Checks are on lengths and nil comparisons of REAL (non-nil) pointers only: the
// unsafe.Pointer managed round-trip / address arithmetic — and a nil unsafe.Pointer's uintptr
// round-trip — are separate accepted runtime limitations, so the test stays away from them and
// its output is deterministic (the point under test is that the consumer files COMPILE).
package main

import "unsafe"

//go:noinline
func ptrOf(x *int64) unsafe.Pointer { return unsafe.Pointer(x) }

//go:noinline
func isNil(p unsafe.Pointer) bool { return p == nil }

//go:noinline
func makePtrs(x *int64) []unsafe.Pointer { return []unsafe.Pointer{unsafe.Pointer(x)} }
