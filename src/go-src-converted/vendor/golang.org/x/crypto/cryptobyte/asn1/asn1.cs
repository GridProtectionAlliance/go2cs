// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asn1 contains supporting types for parsing and building ASN.1
// messages with the cryptobyte package.

// package asn1 -- go2cs converted at 2022 March 13 06:44:42 UTC
// import "vendor/golang.org/x/crypto/cryptobyte/asn1" ==> using asn1 = go.vendor.golang.org.x.crypto.cryptobyte.asn1_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\cryptobyte\asn1\asn1.go
namespace go.vendor.golang.org.x.crypto.cryptobyte;

public static partial class asn1_package { // import "golang.org/x/crypto/cryptobyte/asn1"

// Tag represents an ASN.1 identifier octet, consisting of a tag number
// (indicating a type) and class (such as context-specific or constructed).
//
// Methods in the cryptobyte package only support the low-tag-number form, i.e.
// a single identifier octet with bits 7-8 encoding the class and bits 1-6
// encoding the tag number.
public partial struct Tag { // : byte
}

private static readonly nuint classConstructed = 0x20;
private static readonly nuint classContextSpecific = 0x80;

// Constructed returns t with the constructed class bit set.
public static Tag Constructed(this Tag t) {
    return t | classConstructed;
}

// ContextSpecific returns t with the context-specific class bit set.
public static Tag ContextSpecific(this Tag t) {
    return t | classContextSpecific;
}

// The following is a list of standard tag and class combinations.
public static readonly var BOOLEAN = Tag(1);
public static readonly var INTEGER = Tag(2);
public static readonly var BIT_STRING = Tag(3);
public static readonly var OCTET_STRING = Tag(4);
public static readonly var NULL = Tag(5);
public static readonly var OBJECT_IDENTIFIER = Tag(6);
public static readonly var ENUM = Tag(10);
public static readonly var UTF8String = Tag(12);
public static readonly var SEQUENCE = Tag(16 | classConstructed);
public static readonly var SET = Tag(17 | classConstructed);
public static readonly var PrintableString = Tag(19);
public static readonly var T61String = Tag(20);
public static readonly var IA5String = Tag(22);
public static readonly var UTCTime = Tag(23);
public static readonly var GeneralizedTime = Tag(24);
public static readonly var GeneralString = Tag(27);

} // end asn1_package
