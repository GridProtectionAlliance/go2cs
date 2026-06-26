package main

// Features is a package-global variable of an *anonymous* struct type, mirroring
// how the Go runtime declares internal/cpu.X86. The converter lifts this anonymous
// struct to a named C# type (e.g. "Featuresᴛ1") while visiting THIS file; the
// address of its fields is taken in main.go, which exercises cross-file resolution
// of that lifted type name. (Regression guard for the &cpu.X86.HasADX fix.)
var Features struct {
	HasFast bool
	HasWide bool
	Level   int
}

// option pairs a name with a pointer into a Features field, mirroring the
// internal/cpu.doinit options table built from "&X86.HasXXX" field addresses.
type option struct {
	name string
	flag *bool
}
