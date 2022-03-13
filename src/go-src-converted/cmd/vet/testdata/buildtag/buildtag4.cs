// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the buildtag checker.

//go:build !(bad || worse)
// +build !bad
// +build !worse

// package testdata -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\buildtag\buildtag4.go
namespace go.cmd.vet;

} // end testdata_package
