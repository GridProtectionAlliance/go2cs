// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2020 October 08 03:21:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_ppc64x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) 
                // ppc64x doesn't have a 'cpuid' instruction
                // equivalent and relies on HWCAP/HWCAP2 bits for
                // hardware capabilities.
                cpu.HWCap = uint(val);
            else if (tag == _AT_HWCAP2) 
                cpu.HWCap2 = uint(val);
            
        }

        private static void osArchInit()
        {
        }
    }
}
