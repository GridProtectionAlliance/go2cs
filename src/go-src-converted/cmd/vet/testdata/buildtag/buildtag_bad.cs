// This file contains misplaced or malformed build constraints.
// The Go tool will skip it, because the constraints are invalid.
// It serves only to test the tag checker during make test.

// Mention +build // ERROR "possible malformed \+build comment"

// +build !!bang // ERROR "invalid double negative in build constraint"
// +build @#$ // ERROR "invalid non-alphanumeric build constraint"

// +build toolate // ERROR "build comment must appear before package clause and be followed by a blank line"
// package bad -- go2cs converted at 2020 August 29 10:10:40 UTC
// import "cmd/vet/testdata.bad" ==> using bad = go.cmd.vet.testdata.bad_package
// Original source: C:\Go\src\cmd\vet\testdata\buildtag\buildtag_bad.go
    }

