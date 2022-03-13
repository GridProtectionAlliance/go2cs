// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for untagged struct literals.

// package composite -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata/composite" ==> using composite = go.cmd.vet.testdata.composite_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\composite\composite.go
namespace go.cmd.vet.testdata;

using flag = flag_package;

public static partial class composite_package {

// Testing is awkward because we need to reference things from a separate package
// to trigger the warnings.

private static flag.Flag goodStructLiteral = new flag.Flag(Name:"Name",Usage:"Usage",);

private static flag.Flag badStructLiteral = new flag.Flag("Name","Usage",nil,"DefValue",);

} // end composite_package
