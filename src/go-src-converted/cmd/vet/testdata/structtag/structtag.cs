// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for canonical struct tags.

// package structtag -- go2cs converted at 2020 October 09 06:05:12 UTC
// import "cmd/vet/testdata/structtag" ==> using structtag = go.cmd.vet.testdata.structtag_package
// Original source: C:\Go\src\cmd\vet\testdata\structtag\structtag.go

using static go.builtin;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class structtag_package
    {
        public partial struct StructTagTest
        {
            [Description("hello")]
            public long A; // ERROR "`hello` not compatible with reflect.StructTag.Get: bad syntax for struct tag pair"
        }
    }
}}}}
