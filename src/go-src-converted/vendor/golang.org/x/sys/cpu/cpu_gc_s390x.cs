// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo

// package cpu -- go2cs converted at 2020 October 08 05:01:49 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_gc_s390x.go

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
            return true;
        }

        // The following feature detection functions are defined in cpu_s390x.s.
        // They are likely to be expensive to call so the results should be cached.
        private static facilityList stfle()
;
        private static queryResult kmQuery()
;
        private static queryResult kmcQuery()
;
        private static queryResult kmctrQuery()
;
        private static queryResult kmaQuery()
;
        private static queryResult kimdQuery()
;
        private static queryResult klmdQuery()
;
    }
}}}}}
