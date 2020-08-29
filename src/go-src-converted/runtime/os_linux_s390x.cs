// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_s390x.go
using sys = go.runtime.@internal.sys_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // bit masks taken from bits/hwcap.h
        private static readonly long _HWCAP_S390_VX = 2048L; // vector facility

        // facilities is padded to avoid false sharing.
        private partial struct facilities
        {
            public array<byte> _;
            public bool hasVX; // vector facility
            public array<byte> _;
        }

        // cpu indicates the availability of s390x facilities that can be used in
        // Go assembly but are optional on models supported by Go.
        private static facilities cpu = default;

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) // CPU capability bit flags
                cpu.hasVX = val & _HWCAP_S390_VX != 0L;
                    }
    }
}
