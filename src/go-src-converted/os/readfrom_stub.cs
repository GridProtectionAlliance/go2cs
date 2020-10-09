// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux

// package os -- go2cs converted at 2020 October 09 05:07:20 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\readfrom_stub.go
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (long, bool, error) readFrom(this ptr<File> _addr_f, io.Reader r)
        {
            long n = default;
            bool handled = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            return (0L, false, error.As(null!)!);
        }
    }
}
