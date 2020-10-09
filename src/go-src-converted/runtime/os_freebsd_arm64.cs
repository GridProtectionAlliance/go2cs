// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_freebsd_arm64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long hwcap_FP = (long)1L << (int)(0L);
        private static readonly long hwcap_ASIMD = (long)1L << (int)(1L);
        private static readonly long hwcap_EVTSTRM = (long)1L << (int)(2L);
        private static readonly long hwcap_AES = (long)1L << (int)(3L);
        private static readonly long hwcap_PMULL = (long)1L << (int)(4L);
        private static readonly long hwcap_SHA1 = (long)1L << (int)(5L);
        private static readonly long hwcap_SHA2 = (long)1L << (int)(6L);
        private static readonly long hwcap_CRC32 = (long)1L << (int)(7L);
        private static readonly long hwcap_ATOMICS = (long)1L << (int)(8L);
        private static readonly long hwcap_FPHP = (long)1L << (int)(9L);
        private static readonly long hwcap_ASIMDHP = (long)1L << (int)(10L);
        private static readonly long hwcap_CPUID = (long)1L << (int)(11L);
        private static readonly long hwcap_ASIMDRDM = (long)1L << (int)(12L);
        private static readonly long hwcap_JSCVT = (long)1L << (int)(13L);
        private static readonly long hwcap_FCMA = (long)1L << (int)(14L);
        private static readonly long hwcap_LRCPC = (long)1L << (int)(15L);
        private static readonly long hwcap_DCPOP = (long)1L << (int)(16L);
        private static readonly long hwcap_SHA3 = (long)1L << (int)(17L);
        private static readonly long hwcap_SM3 = (long)1L << (int)(18L);
        private static readonly long hwcap_SM4 = (long)1L << (int)(19L);
        private static readonly long hwcap_ASIMDDP = (long)1L << (int)(20L);
        private static readonly long hwcap_SHA512 = (long)1L << (int)(21L);
        private static readonly long hwcap_SVE = (long)1L << (int)(22L);
        private static readonly long hwcap_ASIMDFHM = (long)1L << (int)(23L);


        private static ulong getisar0()
;
        private static ulong getisar1()
;
        private static ulong getpfr0()
;

        // no hwcap support on FreeBSD aarch64, we need to retrieve the info from
        // ID_AA64ISAR0_EL1, ID_AA64ISAR1_EL1 and ID_AA64PFR0_EL1
        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {
            ulong isar0 = default;            ulong isar1 = default;            ulong pfr0 = default;



            isar0 = getisar0();
            isar1 = getisar1();
            pfr0 = getpfr0(); 

            // ID_AA64ISAR0_EL1
            switch (extractBits(isar0, 4L, 7L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_AES;
                    break;
                case 2L: 
                    cpu.HWCap |= hwcap_PMULL | hwcap_AES;
                    break;
            }

            switch (extractBits(isar0, 8L, 11L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SHA1;
                    break;
            }

            switch (extractBits(isar0, 12L, 15L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SHA2;
                    break;
                case 2L: 
                    cpu.HWCap |= hwcap_SHA2 | hwcap_SHA512;
                    break;
            }

            switch (extractBits(isar0, 16L, 19L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_CRC32;
                    break;
            }

            switch (extractBits(isar0, 20L, 23L))
            {
                case 2L: 
                    cpu.HWCap |= hwcap_ATOMICS;
                    break;
            }

            switch (extractBits(isar0, 28L, 31L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_ASIMDRDM;
                    break;
            }

            switch (extractBits(isar0, 32L, 35L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SHA3;
                    break;
            }

            switch (extractBits(isar0, 36L, 39L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SM3;
                    break;
            }

            switch (extractBits(isar0, 40L, 43L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SM4;
                    break;
            }

            switch (extractBits(isar0, 44L, 47L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_ASIMDDP;
                    break;
            } 

            // ID_AA64ISAR1_EL1
            switch (extractBits(isar1, 0L, 3L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_DCPOP;
                    break;
            }

            switch (extractBits(isar1, 12L, 15L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_JSCVT;
                    break;
            }

            switch (extractBits(isar1, 16L, 19L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_FCMA;
                    break;
            }

            switch (extractBits(isar1, 20L, 23L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_LRCPC;
                    break;
            } 

            // ID_AA64PFR0_EL1
            switch (extractBits(pfr0, 16L, 19L))
            {
                case 0L: 
                    cpu.HWCap |= hwcap_FP;
                    break;
                case 1L: 
                    cpu.HWCap |= hwcap_FP | hwcap_FPHP;
                    break;
            }

            switch (extractBits(pfr0, 20L, 23L))
            {
                case 0L: 
                    cpu.HWCap |= hwcap_ASIMD;
                    break;
                case 1L: 
                    cpu.HWCap |= hwcap_ASIMD | hwcap_ASIMDHP;
                    break;
            }

            switch (extractBits(pfr0, 32L, 35L))
            {
                case 1L: 
                    cpu.HWCap |= hwcap_SVE;
                    break;
            }

        }

        private static ulong extractBits(ulong data, ulong start, ulong end)
        {
            return (uint)(data >> (int)(start)) & ((1L << (int)((end - start + 1L))) - 1L);
        }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed fastrand().
            // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            return nanotime();

        }
    }
}
