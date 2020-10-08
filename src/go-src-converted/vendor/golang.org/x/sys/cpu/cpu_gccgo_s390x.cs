// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gccgo

// package cpu -- go2cs converted at 2020 October 08 05:01:49 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_gccgo_s390x.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // haveAsmFunctions reports whether the other functions in this file can
        // be safely called.
        private static bool haveAsmFunctions()
        {
            return false;
        }

        // TODO(mundaym): the following feature detection functions are currently
        // stubs. See https://golang.org/cl/162887 for how to fix this.
        // They are likely to be expensive to call so the results should be cached.
        private static facilityList stfle() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult kmQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult kmcQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult kmctrQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult kmaQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult kimdQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
        private static queryResult klmdQuery() => func((_, panic, __) =>
        {
            panic("not implemented for gccgo");
        });
    }
}}}}}
