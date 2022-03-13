// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cryptobyte contains types that help with parsing and constructing
// length-prefixed, binary messages, including ASN.1 DER. (The asn1 subpackage
// contains useful ASN.1 constants.)
//
// The String type is for parsing. It wraps a []byte slice and provides helper
// functions for consuming structures, value by value.
//
// The Builder type is for constructing messages. It providers helper functions
// for appending values and also for appending length-prefixed submessages –
// without having to worry about calculating the length prefix ahead of time.
//
// See the documentation and examples for the Builder and String types to get
// started.

// package cryptobyte -- go2cs converted at 2022 March 13 06:44:42 UTC
// import "vendor/golang.org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang.org.x.crypto.cryptobyte_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\cryptobyte\string.go
namespace go.vendor.golang.org.x.crypto;

public static partial class cryptobyte_package { // import "golang.org/x/crypto/cryptobyte"

// String represents a string of bytes. It provides methods for parsing
// fixed-length and length-prefixed values from it.
public partial struct String { // : slice<byte>
}

// read advances a String by n bytes and returns them. If less than n bytes
// remain, it returns nil.
private static slice<byte> read(this ptr<String> _addr_s, nint n) {
    ref String s = ref _addr_s.val;

    if (len(s.val) < n || n < 0) {
        return null;
    }
    var v = (s.val)[..(int)n];
    s.val = (s.val)[(int)n..];
    return v;
}

// Skip advances the String by n byte and reports whether it was successful.
private static bool Skip(this ptr<String> _addr_s, nint n) {
    ref String s = ref _addr_s.val;

    return s.read(n) != null;
}

// ReadUint8 decodes an 8-bit value into out and advances over it.
// It reports whether the read was successful.
private static bool ReadUint8(this ptr<String> _addr_s, ptr<byte> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref byte @out = ref _addr_@out.val;

    var v = s.read(1);
    if (v == null) {
        return false;
    }
    out.val = uint8(v[0]);
    return true;
}

// ReadUint16 decodes a big-endian, 16-bit value into out and advances over it.
// It reports whether the read was successful.
private static bool ReadUint16(this ptr<String> _addr_s, ptr<ushort> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref ushort @out = ref _addr_@out.val;

    var v = s.read(2);
    if (v == null) {
        return false;
    }
    out.val = uint16(v[0]) << 8 | uint16(v[1]);
    return true;
}

// ReadUint24 decodes a big-endian, 24-bit value into out and advances over it.
// It reports whether the read was successful.
private static bool ReadUint24(this ptr<String> _addr_s, ptr<uint> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref uint @out = ref _addr_@out.val;

    var v = s.read(3);
    if (v == null) {
        return false;
    }
    out.val = uint32(v[0]) << 16 | uint32(v[1]) << 8 | uint32(v[2]);
    return true;
}

// ReadUint32 decodes a big-endian, 32-bit value into out and advances over it.
// It reports whether the read was successful.
private static bool ReadUint32(this ptr<String> _addr_s, ptr<uint> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref uint @out = ref _addr_@out.val;

    var v = s.read(4);
    if (v == null) {
        return false;
    }
    out.val = uint32(v[0]) << 24 | uint32(v[1]) << 16 | uint32(v[2]) << 8 | uint32(v[3]);
    return true;
}

private static bool readUnsigned(this ptr<String> _addr_s, ptr<uint> _addr_@out, nint length) {
    ref String s = ref _addr_s.val;
    ref uint @out = ref _addr_@out.val;

    var v = s.read(length);
    if (v == null) {
        return false;
    }
    uint result = default;
    for (nint i = 0; i < length; i++) {
        result<<=8;
        result |= uint32(v[i]);
    }
    out.val = result;
    return true;
}

private static bool readLengthPrefixed(this ptr<String> _addr_s, nint lenLen, ptr<String> _addr_outChild) {
    ref String s = ref _addr_s.val;
    ref String outChild = ref _addr_outChild.val;

    var lenBytes = s.read(lenLen);
    if (lenBytes == null) {
        return false;
    }
    uint length = default;
    foreach (var (_, b) in lenBytes) {
        length = length << 8;
        length = length | uint32(b);
    }    var v = s.read(int(length));
    if (v == null) {
        return false;
    }
    outChild = v;
    return true;
}

// ReadUint8LengthPrefixed reads the content of an 8-bit length-prefixed value
// into out and advances over it. It reports whether the read was successful.
private static bool ReadUint8LengthPrefixed(this ptr<String> _addr_s, ptr<String> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref String @out = ref _addr_@out.val;

    return s.readLengthPrefixed(1, out);
}

// ReadUint16LengthPrefixed reads the content of a big-endian, 16-bit
// length-prefixed value into out and advances over it. It reports whether the
// read was successful.
private static bool ReadUint16LengthPrefixed(this ptr<String> _addr_s, ptr<String> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref String @out = ref _addr_@out.val;

    return s.readLengthPrefixed(2, out);
}

// ReadUint24LengthPrefixed reads the content of a big-endian, 24-bit
// length-prefixed value into out and advances over it. It reports whether
// the read was successful.
private static bool ReadUint24LengthPrefixed(this ptr<String> _addr_s, ptr<String> _addr_@out) {
    ref String s = ref _addr_s.val;
    ref String @out = ref _addr_@out.val;

    return s.readLengthPrefixed(3, out);
}

// ReadBytes reads n bytes into out and advances over them. It reports
// whether the read was successful.
private static bool ReadBytes(this ptr<String> _addr_s, ptr<slice<byte>> _addr_@out, nint n) {
    ref String s = ref _addr_s.val;
    ref slice<byte> @out = ref _addr_@out.val;

    var v = s.read(n);
    if (v == null) {
        return false;
    }
    out.val = v;
    return true;
}

// CopyBytes copies len(out) bytes into out and advances over them. It reports
// whether the copy operation was successful
private static bool CopyBytes(this ptr<String> _addr_s, slice<byte> @out) {
    ref String s = ref _addr_s.val;

    var n = len(out);
    var v = s.read(n);
    if (v == null) {
        return false;
    }
    return copy(out, v) == n;
}

// Empty reports whether the string does not contain any bytes.
public static bool Empty(this String s) {
    return len(s) == 0;
}

} // end cryptobyte_package
