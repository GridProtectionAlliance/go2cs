// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips64 mips64le

// package cpu -- go2cs converted at 2020 October 08 03:19:09 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_mips64x.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)32L;

        // This is initialized by archauxv and should not be changed after it is
        // initialized.


        // This is initialized by archauxv and should not be changed after it is
        // initialized.
        public static ulong HWCap = default;

        // HWCAP bits. These are exposed by the Linux kernel 5.4.
 
        // CPU features
        private static readonly long hwcap_MIPS_MSA = (long)1L << (int)(1L);


        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"msa",Feature:&MIPS64X.HasMSA} }); 

            // HWCAP feature bits
            MIPS64X.HasMSA = isSet(HWCap, hwcap_MIPS_MSA);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
