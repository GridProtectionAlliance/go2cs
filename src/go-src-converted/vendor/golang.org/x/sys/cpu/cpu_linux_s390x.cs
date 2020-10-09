// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 06:07:55 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_s390x.go

using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long cacheLineSize = (long)256L;



 
        // bit mask values from /usr/include/bits/hwcap.h
        private static readonly long hwcap_ZARCH = (long)2L;
        private static readonly long hwcap_STFLE = (long)4L;
        private static readonly long hwcap_MSA = (long)8L;
        private static readonly long hwcap_LDISP = (long)16L;
        private static readonly long hwcap_EIMM = (long)32L;
        private static readonly long hwcap_DFP = (long)64L;
        private static readonly long hwcap_ETF3EH = (long)256L;
        private static readonly long hwcap_VX = (long)2048L;
        private static readonly long hwcap_VXE = (long)8192L;


        // bitIsSet reports whether the bit at index is set. The bit index
        // is in big endian order, so bit index 0 is the leftmost bit.
        private static bool bitIsSet(slice<ulong> bits, ulong index)
        {
            return bits[index / 64L] & ((1L << (int)(63L)) >> (int)((index % 64L))) != 0L;
        }

        // function is the code for the named cryptographic function.
        private partial struct function // : byte
        {
        }

 
        // KM{,A,C,CTR} function codes
        private static readonly function aes128 = (function)18L; // AES-128
        private static readonly function aes192 = (function)19L; // AES-192
        private static readonly function aes256 = (function)20L; // AES-256

        // K{I,L}MD function codes
        private static readonly function sha1 = (function)1L; // SHA-1
        private static readonly function sha256 = (function)2L; // SHA-256
        private static readonly function sha512 = (function)3L; // SHA-512
        private static readonly function sha3_224 = (function)32L; // SHA3-224
        private static readonly function sha3_256 = (function)33L; // SHA3-256
        private static readonly function sha3_384 = (function)34L; // SHA3-384
        private static readonly function sha3_512 = (function)35L; // SHA3-512
        private static readonly function shake128 = (function)36L; // SHAKE-128
        private static readonly function shake256 = (function)37L; // SHAKE-256

        // KLMD function codes
        private static readonly function ghash = (function)65L; // GHASH

        // queryResult contains the result of a Query function
        // call. Bits are numbered in big endian order so the
        // leftmost bit (the MSB) is at index 0.
        private partial struct queryResult
        {
            public array<ulong> bits;
        }

        // Has reports whether the given functions are present.
        private static bool Has(this ptr<queryResult> _addr_q, params function[] fns) => func((_, panic, __) =>
        {
            fns = fns.Clone();
            ref queryResult q = ref _addr_q.val;

            if (len(fns) == 0L)
            {
                panic("no function codes provided");
            }

            foreach (var (_, f) in fns)
            {
                if (!bitIsSet(q.bits[..], uint(f)))
                {
                    return false;
                }

            }
            return true;

        });

        // facility is a bit index for the named facility.
        private partial struct facility // : byte
        {
        }

 
        // cryptography facilities
        private static readonly facility msa4 = (facility)77L; // message-security-assist extension 4
        private static readonly facility msa8 = (facility)146L; // message-security-assist extension 8

        // facilityList contains the result of an STFLE call.
        // Bits are numbered in big endian order so the
        // leftmost bit (the MSB) is at index 0.
        private partial struct facilityList
        {
            public array<ulong> bits;
        }

        // Has reports whether the given facilities are present.
        private static bool Has(this ptr<facilityList> _addr_s, params facility[] fs) => func((_, panic, __) =>
        {
            fs = fs.Clone();
            ref facilityList s = ref _addr_s.val;

            if (len(fs) == 0L)
            {
                panic("no facility bits provided");
            }

            foreach (var (_, f) in fs)
            {
                if (!bitIsSet(s.bits[..], uint(f)))
                {
                    return false;
                }

            }
            return true;

        });

        private static void doinit()
        { 
            // test HWCAP bit vector
            Func<ulong, bool> has = featureMask =>
            {
                return hwCap & featureMask == featureMask;
            } 

            // mandatory
; 

            // mandatory
            S390X.HasZARCH = has(hwcap_ZARCH); 

            // optional
            S390X.HasSTFLE = has(hwcap_STFLE);
            S390X.HasLDISP = has(hwcap_LDISP);
            S390X.HasEIMM = has(hwcap_EIMM);
            S390X.HasETF3EH = has(hwcap_ETF3EH);
            S390X.HasDFP = has(hwcap_DFP);
            S390X.HasMSA = has(hwcap_MSA);
            S390X.HasVX = has(hwcap_VX);
            if (S390X.HasVX)
            {
                S390X.HasVXE = has(hwcap_VXE);
            } 

            // We need implementations of stfle, km and so on
            // to detect cryptographic features.
            if (!haveAsmFunctions())
            {
                return ;
            } 

            // optional cryptographic functions
            if (S390X.HasMSA)
            {
                function aes = new slice<function>(new function[] { aes128, aes192, aes256 }); 

                // cipher message
                var km = kmQuery();
                var kmc = kmcQuery();
                S390X.HasAES = km.Has(aes);
                S390X.HasAESCBC = kmc.Has(aes);
                if (S390X.HasSTFLE)
                {
                    var facilities = stfle();
                    if (facilities.Has(msa4))
                    {
                        var kmctr = kmctrQuery();
                        S390X.HasAESCTR = kmctr.Has(aes);
                    }

                    if (facilities.Has(msa8))
                    {
                        var kma = kmaQuery();
                        S390X.HasAESGCM = kma.Has(aes);
                    }

                } 

                // compute message digest
                var kimd = kimdQuery(); // intermediate (no padding)
                var klmd = klmdQuery(); // last (padding)
                S390X.HasSHA1 = kimd.Has(sha1) && klmd.Has(sha1);
                S390X.HasSHA256 = kimd.Has(sha256) && klmd.Has(sha256);
                S390X.HasSHA512 = kimd.Has(sha512) && klmd.Has(sha512);
                S390X.HasGHASH = kimd.Has(ghash); // KLMD-GHASH does not exist
                function sha3 = new slice<function>(new function[] { sha3_224, sha3_256, sha3_384, sha3_512, shake128, shake256 });
                S390X.HasSHA3 = kimd.Has(sha3) && klmd.Has(sha3);

            }

        }
    }
}}}}}
