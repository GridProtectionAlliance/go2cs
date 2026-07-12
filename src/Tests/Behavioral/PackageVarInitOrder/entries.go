package main

// CROSS-FILE dependency: registry is declared in registry.go. This file sorts
// alphabetically BEFORE registry.go, so a naive C# conversion would run this
// static field initializer while registry is still nil (C# executes static
// field initializers per file in compiler input order) — a NullReference at
// type initialization. Mirrors syscall's procSetFilePointerEx <- modkernel32.
var entryName = registry.add("alpha")

// Depends on a RELOCATED var (entryName): must relocate too, even though the
// reference is same-file and backward — the dependency's value is only
// assigned in the ordered ctor, after every field initializer has run.
var entryUpper = entryName + "!"

// TRANSITIVE cross-file dependency through a package function: describe
// (main.go) reads names (registry.go).
var stdinName = describe(0)

// Func-literal (IIFE) initializer: the literal's body reads base (main.go) —
// cross-file through the literal, and the literal EXECUTES at init time.
var computed = func() int { return base * 2 }()
