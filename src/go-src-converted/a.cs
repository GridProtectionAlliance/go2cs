// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue13566

// package a -- go2cs converted at 2020 October 08 04:55:37 UTC
// import "golang.org/x/tools/go/internal/gcimporter.a" ==> using a = go.golang.org.x.tools.go.@internal.gcimporter.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\testdata\a.go
using json = go.encoding.json_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class a_package
    {
        public partial struct A
        {
            public ptr<A> a;
            public json.RawMessage json;
        }
    }
}}}}}}
