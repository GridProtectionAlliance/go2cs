// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows,!race

// package windows -- go2cs converted at 2020 October 08 04:53:46 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\race0.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        private static readonly var raceenabled = (var)false;



        private static void raceAcquire(unsafe.Pointer addr)
        {
        }

        private static void raceReleaseMerge(unsafe.Pointer addr)
        {
        }

        private static void raceReadRange(unsafe.Pointer addr, long len)
        {
        }

        private static void raceWriteRange(unsafe.Pointer addr, long len)
        {
        }
    }
}}}}}}
