// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_s390x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint cacheLineSize = 256;



private static void initOptions() {
    options = new slice<option>(new option[] { {Name:"zarch",Feature:&S390X.HasZARCH,Required:true}, {Name:"stfle",Feature:&S390X.HasSTFLE,Required:true}, {Name:"ldisp",Feature:&S390X.HasLDISP,Required:true}, {Name:"eimm",Feature:&S390X.HasEIMM,Required:true}, {Name:"dfp",Feature:&S390X.HasDFP}, {Name:"etf3eh",Feature:&S390X.HasETF3EH}, {Name:"msa",Feature:&S390X.HasMSA}, {Name:"aes",Feature:&S390X.HasAES}, {Name:"aescbc",Feature:&S390X.HasAESCBC}, {Name:"aesctr",Feature:&S390X.HasAESCTR}, {Name:"aesgcm",Feature:&S390X.HasAESGCM}, {Name:"ghash",Feature:&S390X.HasGHASH}, {Name:"sha1",Feature:&S390X.HasSHA1}, {Name:"sha256",Feature:&S390X.HasSHA256}, {Name:"sha3",Feature:&S390X.HasSHA3}, {Name:"sha512",Feature:&S390X.HasSHA512}, {Name:"vx",Feature:&S390X.HasVX}, {Name:"vxe",Feature:&S390X.HasVXE} });
}

// bitIsSet reports whether the bit at index is set. The bit index
// is in big endian order, so bit index 0 is the leftmost bit.
private static bool bitIsSet(slice<ulong> bits, nuint index) {
    return bits[index / 64] & ((1 << 63) >> (int)((index % 64))) != 0;
}

// facility is a bit index for the named facility.
private partial struct facility { // : byte
}

 
// mandatory facilities
private static readonly facility zarch = 1; // z architecture mode is active
private static readonly facility stflef = 7; // store-facility-list-extended
private static readonly facility ldisp = 18; // long-displacement
private static readonly facility eimm = 21; // extended-immediate

// miscellaneous facilities
private static readonly facility dfp = 42; // decimal-floating-point
private static readonly facility etf3eh = 30; // extended-translation 3 enhancement

// cryptography facilities
private static readonly facility msa = 17; // message-security-assist
private static readonly facility msa3 = 76; // message-security-assist extension 3
private static readonly facility msa4 = 77; // message-security-assist extension 4
private static readonly facility msa5 = 57; // message-security-assist extension 5
private static readonly facility msa8 = 146; // message-security-assist extension 8
private static readonly facility msa9 = 155; // message-security-assist extension 9

// vector facilities
private static readonly facility vx = 129; // vector facility
private static readonly facility vxe = 135; // vector-enhancements 1
private static readonly facility vxe2 = 148; // vector-enhancements 2

// facilityList contains the result of an STFLE call.
// Bits are numbered in big endian order so the
// leftmost bit (the MSB) is at index 0.
private partial struct facilityList {
    public array<ulong> bits;
}

// Has reports whether the given facilities are present.
private static bool Has(this ptr<facilityList> _addr_s, params facility[] fs) => func((_, panic, _) => {
    fs = fs.Clone();
    ref facilityList s = ref _addr_s.val;

    if (len(fs) == 0) {
        panic("no facility bits provided");
    }
    foreach (var (_, f) in fs) {
        if (!bitIsSet(s.bits[..], uint(f))) {
            return false;
        }
    }    return true;

});

// function is the code for the named cryptographic function.
private partial struct function { // : byte
}

 
// KM{,A,C,CTR} function codes
private static readonly function aes128 = 18; // AES-128
private static readonly function aes192 = 19; // AES-192
private static readonly function aes256 = 20; // AES-256

// K{I,L}MD function codes
private static readonly function sha1 = 1; // SHA-1
private static readonly function sha256 = 2; // SHA-256
private static readonly function sha512 = 3; // SHA-512
private static readonly function sha3_224 = 32; // SHA3-224
private static readonly function sha3_256 = 33; // SHA3-256
private static readonly function sha3_384 = 34; // SHA3-384
private static readonly function sha3_512 = 35; // SHA3-512
private static readonly function shake128 = 36; // SHAKE-128
private static readonly function shake256 = 37; // SHAKE-256

// KLMD function codes
private static readonly function ghash = 65; // GHASH

// queryResult contains the result of a Query function
// call. Bits are numbered in big endian order so the
// leftmost bit (the MSB) is at index 0.
private partial struct queryResult {
    public array<ulong> bits;
}

// Has reports whether the given functions are present.
private static bool Has(this ptr<queryResult> _addr_q, params function[] fns) => func((_, panic, _) => {
    fns = fns.Clone();
    ref queryResult q = ref _addr_q.val;

    if (len(fns) == 0) {
        panic("no function codes provided");
    }
    foreach (var (_, f) in fns) {
        if (!bitIsSet(q.bits[..], uint(f))) {
            return false;
        }
    }    return true;

});

private static void doinit() {
    initS390Xbase(); 

    // We need implementations of stfle, km and so on
    // to detect cryptographic features.
    if (!haveAsmFunctions()) {
        return ;
    }
    if (S390X.HasMSA) {
        function aes = new slice<function>(new function[] { aes128, aes192, aes256 }); 

        // cipher message
        var km = kmQuery();
        var kmc = kmcQuery();
        S390X.HasAES = km.Has(aes);
        S390X.HasAESCBC = kmc.Has(aes);
        if (S390X.HasSTFLE) {
            var facilities = stfle();
            if (facilities.Has(msa4)) {
                var kmctr = kmctrQuery();
                S390X.HasAESCTR = kmctr.Has(aes);
            }
            if (facilities.Has(msa8)) {
                var kma = kmaQuery();
                S390X.HasAESGCM = kma.Has(aes);
            }
        }
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

} // end cpu_package
