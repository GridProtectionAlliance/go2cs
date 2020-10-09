// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 04:45:31 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_arm64.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)64L;

        // arm64 doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv and should not be changed after they are
        // initialized.


        // arm64 doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv and should not be changed after they are
        // initialized.
        public static ulong HWCap = default;
        public static ulong HWCap2 = default;

        // HWCAP/HWCAP2 bits. These are exposed by Linux.
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


        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"evtstrm",Feature:&ARM64.HasEVTSTRM}, {Name:"aes",Feature:&ARM64.HasAES}, {Name:"pmull",Feature:&ARM64.HasPMULL}, {Name:"sha1",Feature:&ARM64.HasSHA1}, {Name:"sha2",Feature:&ARM64.HasSHA2}, {Name:"crc32",Feature:&ARM64.HasCRC32}, {Name:"atomics",Feature:&ARM64.HasATOMICS}, {Name:"fphp",Feature:&ARM64.HasFPHP}, {Name:"asimdhp",Feature:&ARM64.HasASIMDHP}, {Name:"cpuid",Feature:&ARM64.HasCPUID}, {Name:"asimdrdm",Feature:&ARM64.HasASIMDRDM}, {Name:"jscvt",Feature:&ARM64.HasJSCVT}, {Name:"fcma",Feature:&ARM64.HasFCMA}, {Name:"lrcpc",Feature:&ARM64.HasLRCPC}, {Name:"dcpop",Feature:&ARM64.HasDCPOP}, {Name:"sha3",Feature:&ARM64.HasSHA3}, {Name:"sm3",Feature:&ARM64.HasSM3}, {Name:"sm4",Feature:&ARM64.HasSM4}, {Name:"asimddp",Feature:&ARM64.HasASIMDDP}, {Name:"sha512",Feature:&ARM64.HasSHA512}, {Name:"sve",Feature:&ARM64.HasSVE}, {Name:"asimdfhm",Feature:&ARM64.HasASIMDFHM}, {Name:"fp",Feature:&ARM64.HasFP,Required:true}, {Name:"asimd",Feature:&ARM64.HasASIMD,Required:true} }); 

            // HWCAP feature bits
            ARM64.HasFP = isSet(HWCap, hwcap_FP);
            ARM64.HasASIMD = isSet(HWCap, hwcap_ASIMD);
            ARM64.HasEVTSTRM = isSet(HWCap, hwcap_EVTSTRM);
            ARM64.HasAES = isSet(HWCap, hwcap_AES);
            ARM64.HasPMULL = isSet(HWCap, hwcap_PMULL);
            ARM64.HasSHA1 = isSet(HWCap, hwcap_SHA1);
            ARM64.HasSHA2 = isSet(HWCap, hwcap_SHA2);
            ARM64.HasCRC32 = isSet(HWCap, hwcap_CRC32);
            ARM64.HasATOMICS = isSet(HWCap, hwcap_ATOMICS);
            ARM64.HasFPHP = isSet(HWCap, hwcap_FPHP);
            ARM64.HasASIMDHP = isSet(HWCap, hwcap_ASIMDHP);
            ARM64.HasCPUID = isSet(HWCap, hwcap_CPUID);
            ARM64.HasASIMDRDM = isSet(HWCap, hwcap_ASIMDRDM);
            ARM64.HasJSCVT = isSet(HWCap, hwcap_JSCVT);
            ARM64.HasFCMA = isSet(HWCap, hwcap_FCMA);
            ARM64.HasLRCPC = isSet(HWCap, hwcap_LRCPC);
            ARM64.HasDCPOP = isSet(HWCap, hwcap_DCPOP);
            ARM64.HasSHA3 = isSet(HWCap, hwcap_SHA3);
            ARM64.HasSM3 = isSet(HWCap, hwcap_SM3);
            ARM64.HasSM4 = isSet(HWCap, hwcap_SM4);
            ARM64.HasASIMDDP = isSet(HWCap, hwcap_ASIMDDP);
            ARM64.HasSHA512 = isSet(HWCap, hwcap_SHA512);
            ARM64.HasSVE = isSet(HWCap, hwcap_SVE);
            ARM64.HasASIMDFHM = isSet(HWCap, hwcap_ASIMDFHM);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
