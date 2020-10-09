// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,race linux,race freebsd,race

// package unix -- go2cs converted at 2020 October 09 05:56:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\race.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static readonly var raceenabled = true;



        private static void raceAcquire(unsafe.Pointer addr)
        {
            runtime.RaceAcquire(addr);
        }

        private static void raceReleaseMerge(unsafe.Pointer addr)
        {
            runtime.RaceReleaseMerge(addr);
        }

        private static void raceReadRange(unsafe.Pointer addr, long len)
        {
            runtime.RaceReadRange(addr, len);
        }

        private static void raceWriteRange(unsafe.Pointer addr, long len)
        {
            runtime.RaceWriteRange(addr, len);
        }
    }
}}}}}}
