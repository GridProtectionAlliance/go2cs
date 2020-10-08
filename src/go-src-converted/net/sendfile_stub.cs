// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin js,wasm netbsd openbsd

// package net -- go2cs converted at 2020 October 08 03:34:11 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sendfile_stub.go
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error, bool) sendFile(ptr<netFD> _addr_c, io.Reader r)
        {
            long n = default;
            error err = default!;
            bool handled = default;
            ref netFD c = ref _addr_c.val;

            return (0L, error.As(null!)!, false);
        }
    }
}
