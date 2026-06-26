package main

import "fmt"

func main() {
	// Build a table of pointers into the fields of the package-global anonymous
	// struct declared in features.go. Each "&Features.HasXxx" is the address of a
	// field of a package-global value var, declared in a different file — the exact
	// shape that previously mis-converted to "ᏑX86.of(cpu.CacheLinePad}.ᏑHasADX)".
	options := []option{
		{"fast", &Features.HasFast},
		{"wide", &Features.HasWide},
	}

	// Set each feature flag through its field pointer, then read it back through the
	// same pointer (mirrors internal/cpu.doinit assigning *option.flag).
	for _, o := range options {
		*o.flag = true
	}

	for _, o := range options {
		fmt.Printf("%s=%t\n", o.name, *o.flag)
	}

	// Address of a non-bool field, written and read back through the pointer.
	level := &Features.Level
	*level = 3
	fmt.Printf("level=%d\n", *level)
}
