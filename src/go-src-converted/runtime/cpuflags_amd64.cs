// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cpuflags_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static bool useAVXmemmove = default;

        private static void init()
        { 
            // Let's remove stepping and reserved fields
            var processor = processorVersionInfo & 0x0FFF3FF0UL;

            var isIntelBridgeFamily = isIntel && processor == 0x206A0UL || processor == 0x206D0UL || processor == 0x306A0UL || processor == 0x306E0UL;

            useAVXmemmove = support_avx && !isIntelBridgeFamily;
        }
    }
}
