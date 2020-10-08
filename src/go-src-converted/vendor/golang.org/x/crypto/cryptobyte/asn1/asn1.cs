// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asn1 contains supporting types for parsing and building ASN.1
// messages with the cryptobyte package.
// package asn1 -- go2cs converted at 2020 October 08 05:00:02 UTC
// import "vendor/golang.org/x/crypto/cryptobyte/asn1" ==> using asn1 = go.vendor.golang.org.x.crypto.cryptobyte.asn1_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\cryptobyte\asn1\asn1.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace cryptobyte
{
    public static partial class asn1_package
    { // import "golang.org/x/crypto/cryptobyte/asn1"

        // Tag represents an ASN.1 identifier octet, consisting of a tag number
        // (indicating a type) and class (such as context-specific or constructed).
        //
        // Methods in the cryptobyte package only support the low-tag-number form, i.e.
        // a single identifier octet with bits 7-8 encoding the class and bits 1-6
        // encoding the tag number.
        public partial struct Tag // : byte
        {
        }

        private static readonly ulong classConstructed = (ulong)0x20UL;
        private static readonly ulong classContextSpecific = (ulong)0x80UL;


        // Constructed returns t with the constructed class bit set.
        public static Tag Constructed(this Tag t)
        {
            return t | classConstructed;
        }

        // ContextSpecific returns t with the context-specific class bit set.
        public static Tag ContextSpecific(this Tag t)
        {
            return t | classContextSpecific;
        }

        // The following is a list of standard tag and class combinations.
        public static readonly var BOOLEAN = (var)Tag(1L);
        public static readonly var INTEGER = (var)Tag(2L);
        public static readonly var BIT_STRING = (var)Tag(3L);
        public static readonly var OCTET_STRING = (var)Tag(4L);
        public static readonly var NULL = (var)Tag(5L);
        public static readonly var OBJECT_IDENTIFIER = (var)Tag(6L);
        public static readonly var ENUM = (var)Tag(10L);
        public static readonly var UTF8String = (var)Tag(12L);
        public static readonly var SEQUENCE = (var)Tag(16L | classConstructed);
        public static readonly var SET = (var)Tag(17L | classConstructed);
        public static readonly var PrintableString = (var)Tag(19L);
        public static readonly var T61String = (var)Tag(20L);
        public static readonly var IA5String = (var)Tag(22L);
        public static readonly var UTCTime = (var)Tag(23L);
        public static readonly var GeneralizedTime = (var)Tag(24L);
        public static readonly var GeneralString = (var)Tag(27L);

    }
}}}}}}
