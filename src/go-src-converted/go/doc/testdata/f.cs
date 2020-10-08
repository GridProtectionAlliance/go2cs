// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The package f is a go/doc test for functions and factory methods.
// package f -- go2cs converted at 2020 October 08 04:02:53 UTC
// import "go/doc.f" ==> using f = go.go.doc.f_package
// Original source: C:\Go\src\go\doc\testdata\f.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class f_package
    {
        // ----------------------------------------------------------------------------
        // Factory functions for non-exported types must not get lost.
        private partial struct @private
        {
        }

        // Exported must always be visible. Was issue 2824.
        public static private Exported()
        {
        }
    }
}}
