// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2020 August 29 08:18:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_ppc64x.go
// For go:linkname
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // ppc64x doesn't have a 'cpuid' instruction equivalent and relies on
        // HWCAP/HWCAP2 bits for hardware capabilities.

        //go:linkname cpu_hwcap internal/cpu.ppc64x_hwcap
        //go:linkname cpu_hwcap2 internal/cpu.ppc64x_hwcap2
        private static ulong cpu_hwcap = default;
        private static ulong cpu_hwcap2 = default;

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) 
                cpu_hwcap = uint(val);
            else if (tag == _AT_HWCAP2) 
                cpu_hwcap2 = uint(val);
                    }
    }
}
