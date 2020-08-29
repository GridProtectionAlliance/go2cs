// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo

// package net -- go2cs converted at 2020 August 29 08:25:12 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_windows.go

using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private partial struct addrinfoErrno // : long
        {
        }

        private static @string Error(this addrinfoErrno eai)
        {
            return "<nil>";
        }
        private static bool Temporary(this addrinfoErrno eai)
        {
            return false;
        }
        private static bool Timeout(this addrinfoErrno eai)
        {
            return false;
        }
    }
}
