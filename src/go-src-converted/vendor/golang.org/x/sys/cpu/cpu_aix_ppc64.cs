// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix,ppc64

// package cpu -- go2cs converted at 2020 October 08 05:01:48 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_aix_ppc64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long cacheLineSize = (long)128L;



 
        // getsystemcfg constants
        private static readonly long _SC_IMPL = (long)2L;
        private static readonly ulong _IMPL_POWER8 = (ulong)0x10000UL;
        private static readonly ulong _IMPL_POWER9 = (ulong)0x20000UL;


        private static void init()
        {
            var impl = getsystemcfg(_SC_IMPL);
            if (impl & _IMPL_POWER8 != 0L)
            {
                PPC64.IsPOWER8 = true;
            }

            if (impl & _IMPL_POWER9 != 0L)
            {
                PPC64.IsPOWER9 = true;
            }

            Initialized = true;

        }

        private static ulong getsystemcfg(long label)
        {
            ulong n = default;

            var (r0, _) = callgetsystemcfg(label);
            n = uint64(r0);
            return ;
        }
    }
}}}}}
