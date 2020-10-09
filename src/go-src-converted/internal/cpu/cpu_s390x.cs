// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 04:45:31 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_s390x.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)256L;

        // bitIsSet reports whether the bit at index is set. The bit index
        // is in big endian order, so bit index 0 is the leftmost bit.


        // bitIsSet reports whether the bit at index is set. The bit index
        // is in big endian order, so bit index 0 is the leftmost bit.
        private static bool bitIsSet(slice<ulong> bits, ulong index)
        {
            return bits[index / 64L] & ((1L << (int)(63L)) >> (int)((index % 64L))) != 0L;
        }

        // function is the function code for the named function.
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

 
        // KDSA function codes
        private static readonly function ecdsaVerifyP256 = (function)1L; // NIST P256
        private static readonly function ecdsaVerifyP384 = (function)2L; // NIST P384
        private static readonly function ecdsaVerifyP521 = (function)3L; // NIST P521
        private static readonly function ecdsaSignP256 = (function)9L; // NIST P256
        private static readonly function ecdsaSignP384 = (function)10L; // NIST P384
        private static readonly function ecdsaSignP521 = (function)11L; // NIST P521
        private static readonly function eddsaVerifyEd25519 = (function)32L; // Curve25519
        private static readonly function eddsaVerifyEd448 = (function)36L; // Curve448
        private static readonly function eddsaSignEd25519 = (function)40L; // Curve25519
        private static readonly function eddsaSignEd448 = (function)44L; // Curve448

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

 
        // mandatory facilities
        private static readonly facility zarch = (facility)1L; // z architecture mode is active
        private static readonly facility stflef = (facility)7L; // store-facility-list-extended
        private static readonly facility ldisp = (facility)18L; // long-displacement
        private static readonly facility eimm = (facility)21L; // extended-immediate

        // miscellaneous facilities
        private static readonly facility dfp = (facility)42L; // decimal-floating-point
        private static readonly facility etf3eh = (facility)30L; // extended-translation 3 enhancement

        // cryptography facilities
        private static readonly facility msa = (facility)17L; // message-security-assist
        private static readonly facility msa3 = (facility)76L; // message-security-assist extension 3
        private static readonly facility msa4 = (facility)77L; // message-security-assist extension 4
        private static readonly facility msa5 = (facility)57L; // message-security-assist extension 5
        private static readonly facility msa8 = (facility)146L; // message-security-assist extension 8
        private static readonly facility msa9 = (facility)155L; // message-security-assist extension 9

        // vector facilities
        private static readonly facility vxe = (facility)135L; // vector-enhancements 1

        // Note: vx and highgprs are excluded because they require
        // kernel support and so must be fetched from HWCAP.

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

        // The following feature detection functions are defined in cpu_s390x.s.
        // They are likely to be expensive to call so the results should be cached.
        private static facilityList stfle()
;
        private static queryResult kmQuery()
;
        private static queryResult kmcQuery()
;
        private static queryResult kmctrQuery()
;
        private static queryResult kmaQuery()
;
        private static queryResult kimdQuery()
;
        private static queryResult klmdQuery()
;
        private static queryResult kdsaQuery()
;

        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"zarch",Feature:&S390X.HasZARCH}, {Name:"stfle",Feature:&S390X.HasSTFLE}, {Name:"ldisp",Feature:&S390X.HasLDISP}, {Name:"msa",Feature:&S390X.HasMSA}, {Name:"eimm",Feature:&S390X.HasEIMM}, {Name:"dfp",Feature:&S390X.HasDFP}, {Name:"etf3eh",Feature:&S390X.HasETF3EH}, {Name:"vx",Feature:&S390X.HasVX}, {Name:"vxe",Feature:&S390X.HasVXE}, {Name:"kdsa",Feature:&S390X.HasKDSA} });

            function aes = new slice<function>(new function[] { aes128, aes192, aes256 });
            var facilities = stfle();

            S390X.HasZARCH = facilities.Has(zarch);
            S390X.HasSTFLE = facilities.Has(stflef);
            S390X.HasLDISP = facilities.Has(ldisp);
            S390X.HasEIMM = facilities.Has(eimm);
            S390X.HasDFP = facilities.Has(dfp);
            S390X.HasETF3EH = facilities.Has(etf3eh);
            S390X.HasMSA = facilities.Has(msa);

            if (S390X.HasMSA)
            {>>MARKER:FUNCTION_kdsaQuery_BLOCK_PREFIX<< 
                // cipher message
                var km = kmQuery();
                var kmc = kmcQuery();
                S390X.HasAES = km.Has(aes);
                S390X.HasAESCBC = kmc.Has(aes);
                if (facilities.Has(msa4))
                {>>MARKER:FUNCTION_klmdQuery_BLOCK_PREFIX<<
                    var kmctr = kmctrQuery();
                    S390X.HasAESCTR = kmctr.Has(aes);
                }

                if (facilities.Has(msa8))
                {>>MARKER:FUNCTION_kimdQuery_BLOCK_PREFIX<<
                    var kma = kmaQuery();
                    S390X.HasAESGCM = kma.Has(aes);
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
                S390X.HasKDSA = facilities.Has(msa9); // elliptic curves
                if (S390X.HasKDSA)
                {>>MARKER:FUNCTION_kmaQuery_BLOCK_PREFIX<<
                    var kdsa = kdsaQuery();
                    S390X.HasECDSA = kdsa.Has(ecdsaVerifyP256, ecdsaSignP256, ecdsaVerifyP384, ecdsaSignP384, ecdsaVerifyP521, ecdsaSignP521);
                    S390X.HasEDDSA = kdsa.Has(eddsaVerifyEd25519, eddsaSignEd25519, eddsaVerifyEd448, eddsaSignEd448);
                }

            }

            if (S390X.HasVX)
            {>>MARKER:FUNCTION_kmctrQuery_BLOCK_PREFIX<<
                S390X.HasVXE = facilities.Has(vxe);
            }

        }
    }
}}
