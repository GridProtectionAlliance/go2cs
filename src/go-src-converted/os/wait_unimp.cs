// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:44:40 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\wait_unimp.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // blockUntilWaitable attempts to block until a call to p.Wait will
        // succeed immediately, and returns whether it has done so.
        // It does not actually call p.Wait.
        // This version is used on systems that do not implement waitid,
        // or where we have not implemented it yet.
        private static (bool, error) blockUntilWaitable(this ref Process p)
        {
            return (false, null);
        }
    }
}