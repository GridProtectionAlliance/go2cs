// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue13742 -- go2cs converted at 2020 August 29 08:47:13 UTC
// import "go/doc.issue13742" ==> using issue13742 = go.go.doc.issue13742_package
// Original source: C:\Go\src\go\doc\testdata\issue13742.go
using ast = go.go.ast_package;
using ast = go.go.ast_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class issue13742_package
    {
        // Both F0 and G0 should appear as functions.
        public static void F0(Node _p0)
        {
        }
        public static Node G0()
        {
            return null;
        }

        // Both F1 and G1 should appear as functions.
        public static void F1(ast.Node _p0)
        {
        }
        public static ast.Node G1()
        {
            return null;
        }
    }
}}
