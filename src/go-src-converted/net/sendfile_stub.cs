// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin nacl netbsd openbsd

// package net -- go2cs converted at 2020 August 29 08:27:18 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sendfile_stub.go
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error, bool) sendFile(ref netFD c, io.Reader r)
        {
            return (0L, null, false);
        }
    }
}
