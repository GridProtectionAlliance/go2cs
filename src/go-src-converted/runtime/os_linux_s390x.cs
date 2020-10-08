// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_s390x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // bit masks taken from bits/hwcap.h
        private static readonly long _HWCAP_S390_VX = (long)2048L; // vector facility

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) // CPU capability bit flags
                cpu.S390X.HasVX = val & _HWCAP_S390_VX != 0L;
            
        }

        private static void osArchInit()
        {
        }
    }
}
