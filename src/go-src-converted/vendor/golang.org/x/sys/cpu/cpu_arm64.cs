// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 06:07:53 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_arm64.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long cacheLineSize = (long)64L;



        private static void init()
        {
            switch (runtime.GOOS)
            {
                case "android": 
                    // Android and iOS don't seem to allow reading these registers.
                    // Fake the minimal features expected by
                    // TestARM64minimalFeatures.

                case "darwin": 
                    // Android and iOS don't seem to allow reading these registers.
                    // Fake the minimal features expected by
                    // TestARM64minimalFeatures.
                    ARM64.HasASIMD = true;
                    ARM64.HasFP = true;
                    break;
                case "linux": 
                    doinit();
                    break;
                default: 
                    readARM64Registers();
                    break;
            }

        }

        private static void readARM64Registers()
        {
            Initialized = true; 

            // ID_AA64ISAR0_EL1
            var isar0 = getisar0();

            switch (extractBits(isar0, 4L, 7L))
            {
                case 1L: 
                    ARM64.HasAES = true;
                    break;
                case 2L: 
                    ARM64.HasAES = true;
                    ARM64.HasPMULL = true;
                    break;
            }

            switch (extractBits(isar0, 8L, 11L))
            {
                case 1L: 
                    ARM64.HasSHA1 = true;
                    break;
            }

            switch (extractBits(isar0, 12L, 15L))
            {
                case 1L: 
                    ARM64.HasSHA2 = true;
                    break;
                case 2L: 
                    ARM64.HasSHA2 = true;
                    ARM64.HasSHA512 = true;
                    break;
            }

            switch (extractBits(isar0, 16L, 19L))
            {
                case 1L: 
                    ARM64.HasCRC32 = true;
                    break;
            }

            switch (extractBits(isar0, 20L, 23L))
            {
                case 2L: 
                    ARM64.HasATOMICS = true;
                    break;
            }

            switch (extractBits(isar0, 28L, 31L))
            {
                case 1L: 
                    ARM64.HasASIMDRDM = true;
                    break;
            }

            switch (extractBits(isar0, 32L, 35L))
            {
                case 1L: 
                    ARM64.HasSHA3 = true;
                    break;
            }

            switch (extractBits(isar0, 36L, 39L))
            {
                case 1L: 
                    ARM64.HasSM3 = true;
                    break;
            }

            switch (extractBits(isar0, 40L, 43L))
            {
                case 1L: 
                    ARM64.HasSM4 = true;
                    break;
            }

            switch (extractBits(isar0, 44L, 47L))
            {
                case 1L: 
                    ARM64.HasASIMDDP = true;
                    break;
            } 

            // ID_AA64ISAR1_EL1
            var isar1 = getisar1();

            switch (extractBits(isar1, 0L, 3L))
            {
                case 1L: 
                    ARM64.HasDCPOP = true;
                    break;
            }

            switch (extractBits(isar1, 12L, 15L))
            {
                case 1L: 
                    ARM64.HasJSCVT = true;
                    break;
            }

            switch (extractBits(isar1, 16L, 19L))
            {
                case 1L: 
                    ARM64.HasFCMA = true;
                    break;
            }

            switch (extractBits(isar1, 20L, 23L))
            {
                case 1L: 
                    ARM64.HasLRCPC = true;
                    break;
            } 

            // ID_AA64PFR0_EL1
            var pfr0 = getpfr0();

            switch (extractBits(pfr0, 16L, 19L))
            {
                case 0L: 
                    ARM64.HasFP = true;
                    break;
                case 1L: 
                    ARM64.HasFP = true;
                    ARM64.HasFPHP = true;
                    break;
            }

            switch (extractBits(pfr0, 20L, 23L))
            {
                case 0L: 
                    ARM64.HasASIMD = true;
                    break;
                case 1L: 
                    ARM64.HasASIMD = true;
                    ARM64.HasASIMDHP = true;
                    break;
            }

            switch (extractBits(pfr0, 32L, 35L))
            {
                case 1L: 
                    ARM64.HasSVE = true;
                    break;
            }

        }

        private static ulong extractBits(ulong data, ulong start, ulong end)
        {
            return (uint)(data >> (int)(start)) & ((1L << (int)((end - start + 1L))) - 1L);
        }
    }
}}}}}
