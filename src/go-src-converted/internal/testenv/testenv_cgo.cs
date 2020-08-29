// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo

// package testenv -- go2cs converted at 2020 August 29 10:11:08 UTC
// import "internal/testenv" ==> using testenv = go.@internal.testenv_package
// Original source: C:\Go\src\internal\testenv\testenv_cgo.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class testenv_package
    {
        private static void init()
        {
            haveCGO = true;
        }
    }
}}
