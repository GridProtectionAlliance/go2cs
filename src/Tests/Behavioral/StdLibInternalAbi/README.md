This a slightly modified version of the Go `internal/abi` standard library package. Modifications are simply to make code compile and run in the pattern of other behavorial tests.

This package is included in the behavioral tests because it has many complex conversion needs that need regression testing, e.g.:
* Method and type name collisions, which are supported in Go, but not C#
* Complex pointer conversion operations, including pointer to slice conversions
* Extensive use of untyped integers and named types for arrays and integer types
* Comparison of dynamically declared structure types
* Extensive bitwise operations with named type values

WARNING: Note that even though this code compiles, many of the functions will not safely run in C#. Code in this package operates on expected memory layout of Go types -- _this will in no way match the layout the same structures that have been converted to C#_.

In the actual converted standard library of this package, publically accessibly API functions and types have been manually modified to safely operate in a behavorially similar fashion in C#, e.g., by using reflrection.
