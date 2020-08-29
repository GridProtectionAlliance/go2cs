// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux dragonfly openbsd solaris

// package tar -- go2cs converted at 2020 August 29 08:45:23 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\stat_actime1.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        private static time.Time statAtime(ref syscall.Stat_t st)
        {
            return time.Unix(st.Atim.Unix());
        }

        private static time.Time statCtime(ref syscall.Stat_t st)
        {
            return time.Unix(st.Ctim.Unix());
        }
    }
}}
