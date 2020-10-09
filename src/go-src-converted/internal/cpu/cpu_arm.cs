// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 04:45:31 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_arm.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)32L;

        // arm doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv() and should not be changed after they are
        // initialized.


        // arm doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv() and should not be changed after they are
        // initialized.
        public static ulong HWCap = default;
        public static ulong HWCap2 = default;

        // HWCAP/HWCAP2 bits. These are exposed by Linux and FreeBSD.
        private static readonly long hwcap_VFPv4 = (long)1L << (int)(16L);
        private static readonly long hwcap_IDIVA = (long)1L << (int)(17L);


        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"vfpv4",Feature:&ARM.HasVFPv4}, {Name:"idiva",Feature:&ARM.HasIDIVA} }); 

            // HWCAP feature bits
            ARM.HasVFPv4 = isSet(HWCap, hwcap_VFPv4);
            ARM.HasIDIVA = isSet(HWCap, hwcap_IDIVA);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
