// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for canonical struct tags.

// package structtag -- go2cs converted at 2022 March 13 06:43:18 UTC
// import "cmd/vet/testdata/structtag" ==> using structtag = go.cmd.vet.testdata.structtag_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\structtag\structtag.go
namespace go.cmd.vet.testdata;

using System.ComponentModel;
public static partial class structtag_package {

public partial struct StructTagTest {
    [Description("hello")]
    public nint A; // ERROR "`hello` not compatible with reflect.StructTag.Get: bad syntax for struct tag pair"
}

} // end structtag_package
