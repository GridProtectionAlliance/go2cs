// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm64

// package cpu -- go2cs converted at 2020 August 29 08:22:20 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_arm64.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLineSize = 64L;

        // arm64 doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are linknamed in runtime/os_linux_arm64.go and are initialized by
        // archauxv().


        // arm64 doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are linknamed in runtime/os_linux_arm64.go and are initialized by
        // archauxv().
        private static ulong arm64_hwcap = default;
        private static ulong arm64_hwcap2 = default;

        // HWCAP/HWCAP2 bits. These are exposed by Linux.
        private static readonly long _ARM64_FEATURE_HAS_FP = (1L << (int)(0L));
        private static readonly long _ARM64_FEATURE_HAS_ASIMD = (1L << (int)(1L));
        private static readonly long _ARM64_FEATURE_HAS_EVTSTRM = (1L << (int)(2L));
        private static readonly long _ARM64_FEATURE_HAS_AES = (1L << (int)(3L));
        private static readonly long _ARM64_FEATURE_HAS_PMULL = (1L << (int)(4L));
        private static readonly long _ARM64_FEATURE_HAS_SHA1 = (1L << (int)(5L));
        private static readonly long _ARM64_FEATURE_HAS_SHA2 = (1L << (int)(6L));
        private static readonly long _ARM64_FEATURE_HAS_CRC32 = (1L << (int)(7L));
        private static readonly long _ARM64_FEATURE_HAS_ATOMICS = (1L << (int)(8L));

        private static void init()
        { 
            // HWCAP feature bits
            ARM64.HasFP = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_FP);
            ARM64.HasASIMD = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_ASIMD);
            ARM64.HasEVTSTRM = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_EVTSTRM);
            ARM64.HasAES = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_AES);
            ARM64.HasPMULL = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_PMULL);
            ARM64.HasSHA1 = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_SHA1);
            ARM64.HasSHA2 = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_SHA2);
            ARM64.HasCRC32 = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_CRC32);
            ARM64.HasATOMICS = isSet(arm64_hwcap, _ARM64_FEATURE_HAS_ATOMICS);
        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
