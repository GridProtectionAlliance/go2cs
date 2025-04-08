This is a slightly modified version of the Go `internal/abi` standard library package. Code modifications were made so that code compiles and runs in the pattern of other behavorial tests, e.g., changing package name to 'main' and adding a main function for some simple test output.

This package is included in the behavioral tests because it has many complex conversion steps that need regression testing, e.g.:
* Method and type name collisions, which are supported in Go, but not C#
* Complex pointer conversion operations, including pointer to slice conversions
* Extensive use of untyped integers and named types for arrays and integer types
* Comparison of dynamically declared structure types
* Extensive bitwise operations with named type values

WARNING: Note that even though this code compiles, many of the functions will not safely run in C#. Code in this package operates on expected memory layout of Go types -- _this will in no way match the layout the same structures that have been converted to C#_.

In the actual converted standard library of this package, publically accessibly API functions and types have been manually modified to safely operate in a behavorially similar fashion in C#, e.g., by using reflrection.
