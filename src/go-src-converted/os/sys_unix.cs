// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 08 03:45:19 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_unix.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // supportsCloseOnExec reports whether the platform supports the
        // O_CLOEXEC flag.
        // On Darwin, the O_CLOEXEC flag was introduced in OS X 10.7 (Darwin 11.0.0).
        // See https://support.apple.com/kb/HT1633.
        // On FreeBSD, the O_CLOEXEC flag was introduced in version 8.3.
        private static readonly var supportsCloseOnExec = (var)true;

    }
}
