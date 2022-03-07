// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asn1 implements parsing of DER-encoded ASN.1 data structures,
// as defined in ITU-T Rec X.690.
//
// See also ``A Layman's Guide to a Subset of ASN.1, BER, and DER,''
// http://luca.ntop.org/Teaching/Appunti/asn1.html.
// package asn1 -- go2cs converted at 2022 March 06 22:19:43 UTC
// import "encoding/asn1" ==> using asn1 = go.encoding.asn1_package
// Original source: C:\Program Files\Go\src\encoding\asn1\asn1.go
// ASN.1 is a syntax for specifying abstract objects and BER, DER, PER, XER etc
// are different encoding formats for those objects. Here, we'll be dealing
// with DER, the Distinguished Encoding Rules. DER is used in X.509 because
// it's fast to parse and, unlike BER, has a unique encoding for every object.
// When calculating hashes over objects, it's important that the resulting
// bytes be the same at both ends and DER removes this margin of error.
//
// ASN.1 is very complex and this package doesn't attempt to implement
// everything by any means.

using errors = go.errors_package;
using fmt = go.fmt_package;
using math = go.math_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using time = go.time_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;

namespace go.encoding;

public static partial class asn1_package {

    // A StructuralError suggests that the ASN.1 data is valid, but the Go type
    // which is receiving it doesn't match.
public partial struct StructuralError {
    public @string Msg;
}

public static @string Error(this StructuralError e) {
    return "asn1: structure error: " + e.Msg;
}

// A SyntaxError suggests that the ASN.1 data is invalid.
public partial struct SyntaxError {
    public @string Msg;
}

public static @string Error(this SyntaxError e) {
    return "asn1: syntax error: " + e.Msg;
}

// We start by dealing with each of the primitive types in turn.

// BOOLEAN

private static (bool, error) parseBool(slice<byte> bytes) {
    bool ret = default;
    error err = default!;

    if (len(bytes) != 1) {
        err = new SyntaxError("invalid boolean");
        return ;
    }
    switch (bytes[0]) {
        case 0: 
            ret = false;
            break;
        case 0xff: 
            ret = true;
            break;
        default: 
            err = new SyntaxError("invalid boolean");
            break;
    }

    return ;

}

// INTEGER

// checkInteger returns nil if the given bytes are a valid DER-encoded
// INTEGER and an error otherwise.
private static error checkInteger(slice<byte> bytes) {
    if (len(bytes) == 0) {
        return error.As(new StructuralError("empty integer"))!;
    }
    if (len(bytes) == 1) {
        return error.As(null!)!;
    }
    if ((bytes[0] == 0 && bytes[1] & 0x80 == 0) || (bytes[0] == 0xff && bytes[1] & 0x80 == 0x80)) {
        return error.As(new StructuralError("integer not minimally-encoded"))!;
    }
    return error.As(null!)!;

}

// parseInt64 treats the given bytes as a big-endian, signed integer and
// returns the result.
private static (long, error) parseInt64(slice<byte> bytes) {
    long ret = default;
    error err = default!;

    err = checkInteger(bytes);
    if (err != null) {
        return ;
    }
    if (len(bytes) > 8) { 
        // We'll overflow an int64 in this case.
        err = new StructuralError("integer too large");
        return ;

    }
    for (nint bytesRead = 0; bytesRead < len(bytes); bytesRead++) {
        ret<<=8;
        ret |= int64(bytes[bytesRead]);
    } 

    // Shift up and down in order to sign extend the result.
    ret<<=64 - uint8(len(bytes)) * 8;
    ret>>=64 - uint8(len(bytes)) * 8;
    return ;

}

// parseInt treats the given bytes as a big-endian, signed integer and returns
// the result.
private static (int, error) parseInt32(slice<byte> bytes) {
    int _p0 = default;
    error _p0 = default!;

    {
        var err = checkInteger(bytes);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    var (ret64, err) = parseInt64(bytes);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (ret64 != int64(int32(ret64))) {
        return (0, error.As(new StructuralError("integer too large"))!);
    }
    return (int32(ret64), error.As(null!)!);

}

private static var bigOne = big.NewInt(1);

// parseBigInt treats the given bytes as a big-endian, signed integer and returns
// the result.
private static (ptr<big.Int>, error) parseBigInt(slice<byte> bytes) {
    ptr<big.Int> _p0 = default!;
    error _p0 = default!;

    {
        var err = checkInteger(bytes);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    ptr<big.Int> ret = @new<big.Int>();
    if (len(bytes) > 0 && bytes[0] & 0x80 == 0x80) { 
        // This is a negative number.
        var notBytes = make_slice<byte>(len(bytes));
        foreach (var (i) in notBytes) {
            notBytes[i] = ~bytes[i];
        }        ret.SetBytes(notBytes);
        ret.Add(ret, bigOne);
        ret.Neg(ret);
        return (_addr_ret!, error.As(null!)!);

    }
    ret.SetBytes(bytes);
    return (_addr_ret!, error.As(null!)!);

}

// BIT STRING

// BitString is the structure to use when you want an ASN.1 BIT STRING type. A
// bit string is padded up to the nearest byte in memory and the number of
// valid bits is recorded. Padding bits will be zero.
public partial struct BitString {
    public slice<byte> Bytes; // bits packed into bytes.
    public nint BitLength; // length in bits.
}

// At returns the bit at the given index. If the index is out of range it
// returns false.
public static nint At(this BitString b, nint i) {
    if (i < 0 || i >= b.BitLength) {
        return 0;
    }
    var x = i / 8;
    nint y = 7 - uint(i % 8);
    return int(b.Bytes[x] >> (int)(y)) & 1;

}

// RightAlign returns a slice where the padding bits are at the beginning. The
// slice may share memory with the BitString.
public static slice<byte> RightAlign(this BitString b) {
    var shift = uint(8 - (b.BitLength % 8));
    if (shift == 8 || len(b.Bytes) == 0) {
        return b.Bytes;
    }
    var a = make_slice<byte>(len(b.Bytes));
    a[0] = b.Bytes[0] >> (int)(shift);
    for (nint i = 1; i < len(b.Bytes); i++) {
        a[i] = b.Bytes[i - 1] << (int)((8 - shift));
        a[i] |= b.Bytes[i] >> (int)(shift);
    }

    return a;

}

// parseBitString parses an ASN.1 bit string from the given byte slice and returns it.
private static (BitString, error) parseBitString(slice<byte> bytes) {
    BitString ret = default;
    error err = default!;

    if (len(bytes) == 0) {
        err = new SyntaxError("zero length BIT STRING");
        return ;
    }
    var paddingBits = int(bytes[0]);
    if (paddingBits > 7 || len(bytes) == 1 && paddingBits > 0 || bytes[len(bytes) - 1] & ((1 << (int)(bytes[0])) - 1) != 0) {
        err = new SyntaxError("invalid padding bits in BIT STRING");
        return ;
    }
    ret.BitLength = (len(bytes) - 1) * 8 - paddingBits;
    ret.Bytes = bytes[(int)1..];
    return ;

}

// NULL

// NullRawValue is a RawValue with its Tag set to the ASN.1 NULL type tag (5).
public static RawValue NullRawValue = new RawValue(Tag:TagNull);

// NullBytes contains bytes representing the DER-encoded ASN.1 NULL type.
public static byte NullBytes = new slice<byte>(new byte[] { TagNull, 0 });

// OBJECT IDENTIFIER

// An ObjectIdentifier represents an ASN.1 OBJECT IDENTIFIER.
public partial struct ObjectIdentifier { // : slice<nint>
}

// Equal reports whether oi and other represent the same identifier.
public static bool Equal(this ObjectIdentifier oi, ObjectIdentifier other) {
    if (len(oi) != len(other)) {
        return false;
    }
    for (nint i = 0; i < len(oi); i++) {
        if (oi[i] != other[i]) {
            return false;
        }
    }

    return true;

}

public static @string String(this ObjectIdentifier oi) {
    @string s = default;

    foreach (var (i, v) in oi) {
        if (i > 0) {
            s += ".";
        }
        s += strconv.Itoa(v);

    }    return s;

}

// parseObjectIdentifier parses an OBJECT IDENTIFIER from the given bytes and
// returns it. An object identifier is a sequence of variable length integers
// that are assigned in a hierarchy.
private static (ObjectIdentifier, error) parseObjectIdentifier(slice<byte> bytes) {
    ObjectIdentifier s = default;
    error err = default!;

    if (len(bytes) == 0) {
        err = new SyntaxError("zero length OBJECT IDENTIFIER");
        return ;
    }
    s = make_slice<nint>(len(bytes) + 1); 

    // The first varint is 40*value1 + value2:
    // According to this packing, value1 can take the values 0, 1 and 2 only.
    // When value1 = 0 or value1 = 1, then value2 is <= 39. When value1 = 2,
    // then there are no restrictions on value2.
    var (v, offset, err) = parseBase128Int(bytes, 0);
    if (err != null) {
        return ;
    }
    if (v < 80) {
        s[0] = v / 40;
        s[1] = v % 40;
    }
    else
 {
        s[0] = 2;
        s[1] = v - 80;
    }
    nint i = 2;
    while (offset < len(bytes)) {
        v, offset, err = parseBase128Int(bytes, offset);
        if (err != null) {
            return ;
        i++;
        }
        s[i] = v;

    }
    s = s[(int)0..(int)i];
    return ;

}

// ENUMERATED

// An Enumerated is represented as a plain int.
public partial struct Enumerated { // : nint
}

// FLAG

// A Flag accepts any data and is set to true if present.
public partial struct Flag { // : bool
}

// parseBase128Int parses a base-128 encoded int from the given offset in the
// given byte slice. It returns the value and the new offset.
private static (nint, nint, error) parseBase128Int(slice<byte> bytes, nint initOffset) {
    nint ret = default;
    nint offset = default;
    error err = default!;

    offset = initOffset;
    long ret64 = default;
    for (nint shifted = 0; offset < len(bytes); shifted++) { 
        // 5 * 7 bits per byte == 35 bits of data
        // Thus the representation is either non-minimal or too large for an int32
        if (shifted == 5) {
            err = new StructuralError("base 128 integer too large");
            return ;
        }
        ret64<<=7;
        var b = bytes[offset]; 
        // integers should be minimally encoded, so the leading octet should
        // never be 0x80
        if (shifted == 0 && b == 0x80) {
            err = new SyntaxError("integer is not minimally encoded");
            return ;
        }
        ret64 |= int64(b & 0x7f);
        offset++;
        if (b & 0x80 == 0) {
            ret = int(ret64); 
            // Ensure that the returned value fits in an int on all platforms
            if (ret64 > math.MaxInt32) {
                err = new StructuralError("base 128 integer too large");
            }

            return ;

        }
    }
    err = new SyntaxError("truncated base 128 integer");
    return ;

}

// UTCTime

private static (time.Time, error) parseUTCTime(slice<byte> bytes) {
    time.Time ret = default;
    error err = default!;

    var s = string(bytes);

    @string formatStr = "0601021504Z0700";
    ret, err = time.Parse(formatStr, s);
    if (err != null) {
        formatStr = "060102150405Z0700";
        ret, err = time.Parse(formatStr, s);
    }
    if (err != null) {
        return ;
    }
    {
        var serialized = ret.Format(formatStr);

        if (serialized != s) {
            err = fmt.Errorf("asn1: time did not serialize back to the original value and may be invalid: given %q, but serialized as %q", s, serialized);
            return ;
        }
    }


    if (ret.Year() >= 2050) { 
        // UTCTime only encodes times prior to 2050. See https://tools.ietf.org/html/rfc5280#section-4.1.2.5.1
        ret = ret.AddDate(-100, 0, 0);

    }
    return ;

}

// parseGeneralizedTime parses the GeneralizedTime from the given byte slice
// and returns the resulting time.
private static (time.Time, error) parseGeneralizedTime(slice<byte> bytes) {
    time.Time ret = default;
    error err = default!;

    const @string formatStr = "20060102150405Z0700";

    var s = string(bytes);

    ret, err = time.Parse(formatStr, s);

    if (err != null) {
        return ;
    }
    {
        var serialized = ret.Format(formatStr);

        if (serialized != s) {
            err = fmt.Errorf("asn1: time did not serialize back to the original value and may be invalid: given %q, but serialized as %q", s, serialized);
        }
    }


    return ;

}

// NumericString

// parseNumericString parses an ASN.1 NumericString from the given byte array
// and returns it.
private static (@string, error) parseNumericString(slice<byte> bytes) {
    @string ret = default;
    error err = default!;

    foreach (var (_, b) in bytes) {
        if (!isNumeric(b)) {
            return ("", error.As(new SyntaxError("NumericString contains invalid character"))!);
        }
    }    return (string(bytes), error.As(null!)!);

}

// isNumeric reports whether the given b is in the ASN.1 NumericString set.
private static bool isNumeric(byte b) {
    return '0' <= b && b <= '9' || b == ' ';
}

// PrintableString

// parsePrintableString parses an ASN.1 PrintableString from the given byte
// array and returns it.
private static (@string, error) parsePrintableString(slice<byte> bytes) {
    @string ret = default;
    error err = default!;

    foreach (var (_, b) in bytes) {
        if (!isPrintable(b, allowAsterisk, allowAmpersand)) {
            err = new SyntaxError("PrintableString contains invalid character");
            return ;
        }
    }    ret = string(bytes);
    return ;

}

private partial struct asteriskFlag { // : bool
}
private partial struct ampersandFlag { // : bool
}

private static readonly asteriskFlag allowAsterisk = true;
private static readonly asteriskFlag rejectAsterisk = false;

private static readonly ampersandFlag allowAmpersand = true;
private static readonly ampersandFlag rejectAmpersand = false;


// isPrintable reports whether the given b is in the ASN.1 PrintableString set.
// If asterisk is allowAsterisk then '*' is also allowed, reflecting existing
// practice. If ampersand is allowAmpersand then '&' is allowed as well.
private static bool isPrintable(byte b, asteriskFlag asterisk, ampersandFlag ampersand) {
    return 'a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || '0' <= b && b <= '9' || '\'' <= b && b <= ')' || '+' <= b && b <= '/' || b == ' ' || b == ':' || b == '=' || b == '?' || (bool(asterisk) && b == '*') || (bool(ampersand) && b == '&');
}

// IA5String

// parseIA5String parses an ASN.1 IA5String (ASCII string) from the given
// byte slice and returns it.
private static (@string, error) parseIA5String(slice<byte> bytes) {
    @string ret = default;
    error err = default!;

    foreach (var (_, b) in bytes) {
        if (b >= utf8.RuneSelf) {
            err = new SyntaxError("IA5String contains invalid character");
            return ;
        }
    }    ret = string(bytes);
    return ;

}

// T61String

// parseT61String parses an ASN.1 T61String (8-bit clean string) from the given
// byte slice and returns it.
private static (@string, error) parseT61String(slice<byte> bytes) {
    @string ret = default;
    error err = default!;

    return (string(bytes), error.As(null!)!);
}

// UTF8String

// parseUTF8String parses an ASN.1 UTF8String (raw UTF-8) from the given byte
// array and returns it.
private static (@string, error) parseUTF8String(slice<byte> bytes) {
    @string ret = default;
    error err = default!;

    if (!utf8.Valid(bytes)) {
        return ("", error.As(errors.New("asn1: invalid UTF-8 string"))!);
    }
    return (string(bytes), error.As(null!)!);

}

// BMPString

// parseBMPString parses an ASN.1 BMPString (Basic Multilingual Plane of
// ISO/IEC/ITU 10646-1) from the given byte slice and returns it.
private static (@string, error) parseBMPString(slice<byte> bmpString) {
    @string _p0 = default;
    error _p0 = default!;

    if (len(bmpString) % 2 != 0) {
        return ("", error.As(errors.New("pkcs12: odd-length BMP string"))!);
    }
    {
        var l = len(bmpString);

        if (l >= 2 && bmpString[l - 1] == 0 && bmpString[l - 2] == 0) {
            bmpString = bmpString[..(int)l - 2];
        }
    }


    var s = make_slice<ushort>(0, len(bmpString) / 2);
    while (len(bmpString) > 0) {
        s = append(s, uint16(bmpString[0]) << 8 + uint16(bmpString[1]));
        bmpString = bmpString[(int)2..];
    }

    return (string(utf16.Decode(s)), error.As(null!)!);

}

// A RawValue represents an undecoded ASN.1 object.
public partial struct RawValue {
    public nint Class;
    public nint Tag;
    public bool IsCompound;
    public slice<byte> Bytes;
    public slice<byte> FullBytes; // includes the tag and length
}

// RawContent is used to signal that the undecoded, DER data needs to be
// preserved for a struct. To use it, the first field of the struct must have
// this type. It's an error for any of the other fields to have this type.
public partial struct RawContent { // : slice<byte>
}

// Tagging

// parseTagAndLength parses an ASN.1 tag and length pair from the given offset
// into a byte slice. It returns the parsed data and the new offset. SET and
// SET OF (tag 17) are mapped to SEQUENCE and SEQUENCE OF (tag 16) since we
// don't distinguish between ordered and unordered objects in this code.
private static (tagAndLength, nint, error) parseTagAndLength(slice<byte> bytes, nint initOffset) {
    tagAndLength ret = default;
    nint offset = default;
    error err = default!;

    offset = initOffset; 
    // parseTagAndLength should not be called without at least a single
    // byte to read. Thus this check is for robustness:
    if (offset >= len(bytes)) {
        err = errors.New("asn1: internal error in parseTagAndLength");
        return ;
    }
    var b = bytes[offset];
    offset++;
    ret.@class = int(b >> 6);
    ret.isCompound = b & 0x20 == 0x20;
    ret.tag = int(b & 0x1f); 

    // If the bottom five bits are set, then the tag number is actually base 128
    // encoded afterwards
    if (ret.tag == 0x1f) {
        ret.tag, offset, err = parseBase128Int(bytes, offset);
        if (err != null) {
            return ;
        }
        if (ret.tag < 0x1f) {
            err = new SyntaxError("non-minimal tag");
            return ;
        }
    }
    if (offset >= len(bytes)) {
        err = new SyntaxError("truncated tag or length");
        return ;
    }
    b = bytes[offset];
    offset++;
    if (b & 0x80 == 0) { 
        // The length is encoded in the bottom 7 bits.
        ret.length = int(b & 0x7f);

    }
    else
 { 
        // Bottom 7 bits give the number of length bytes to follow.
        var numBytes = int(b & 0x7f);
        if (numBytes == 0) {
            err = new SyntaxError("indefinite length found (not DER)");
            return ;
        }
        ret.length = 0;
        for (nint i = 0; i < numBytes; i++) {
            if (offset >= len(bytes)) {
                err = new SyntaxError("truncated tag or length");
                return ;
            }
            b = bytes[offset];
            offset++;
            if (ret.length >= 1 << 23) { 
                // We can't shift ret.length up without
                // overflowing.
                err = new StructuralError("length too large");
                return ;

            }

            ret.length<<=8;
            ret.length |= int(b);
            if (ret.length == 0) { 
                // DER requires that lengths be minimal.
                err = new StructuralError("superfluous leading zeros in length");
                return ;

            }

        } 
        // Short lengths must be encoded in short form.
        if (ret.length < 0x80) {
            err = new StructuralError("non-minimal length");
            return ;
        }
    }
    return ;

}

// parseSequenceOf is used for SEQUENCE OF and SET OF values. It tries to parse
// a number of ASN.1 values from the given byte slice and returns them as a
// slice of Go values of the given type.
private static (reflect.Value, error) parseSequenceOf(slice<byte> bytes, reflect.Type sliceType, reflect.Type elemType) {
    reflect.Value ret = default;
    error err = default!;

    var (matchAny, expectedTag, compoundType, ok) = getUniversalType(elemType);
    if (!ok) {
        err = new StructuralError("unknown Go type for slice");
        return ;
    }
    nint numElements = 0;
    {
        nint offset__prev1 = offset;

        nint offset = 0;

        while (offset < len(bytes)) {
            tagAndLength t = default;
            t, offset, err = parseTagAndLength(bytes, offset);
            if (err != null) {
                return ;
            }

            if (t.tag == TagIA5String || t.tag == TagGeneralString || t.tag == TagT61String || t.tag == TagUTF8String || t.tag == TagNumericString || t.tag == TagBMPString) 
                // We pretend that various other string types are
                // PRINTABLE STRINGs so that a sequence of them can be
                // parsed into a []string.
                t.tag = TagPrintableString;
            else if (t.tag == TagGeneralizedTime || t.tag == TagUTCTime) 
                // Likewise, both time types are treated the same.
                t.tag = TagUTCTime;
                        if (!matchAny && (t.@class != ClassUniversal || t.isCompound != compoundType || t.tag != expectedTag)) {
                err = new StructuralError("sequence tag mismatch");
                return ;
            }

            if (invalidLength(offset, t.length, len(bytes))) {
                err = new SyntaxError("truncated sequence");
                return ;
            }

            offset += t.length;
            numElements++;

        }

        offset = offset__prev1;
    }
    ret = reflect.MakeSlice(sliceType, numElements, numElements);
    fieldParameters @params = new fieldParameters();
    offset = 0;
    for (nint i = 0; i < numElements; i++) {
        offset, err = parseField(ret.Index(i), bytes, offset, params);
        if (err != null) {
            return ;
        }
    }
    return ;

}

private static var bitStringType = reflect.TypeOf(new BitString());private static var objectIdentifierType = reflect.TypeOf(new ObjectIdentifier());private static var enumeratedType = reflect.TypeOf(Enumerated(0));private static var flagType = reflect.TypeOf(Flag(false));private static var timeType = reflect.TypeOf(new time.Time());private static var rawValueType = reflect.TypeOf(new RawValue());private static var rawContentsType = reflect.TypeOf(RawContent(null));private static var bigIntType = reflect.TypeOf(@new<big.Int>());

// invalidLength reports whether offset + length > sliceLength, or if the
// addition would overflow.
private static bool invalidLength(nint offset, nint length, nint sliceLength) {
    return offset + length < offset || offset + length > sliceLength;
}

// parseField is the main parsing function. Given a byte slice and an offset
// into the array, it will try to parse a suitable ASN.1 value out and store it
// in the given Value.
private static (nint, error) parseField(reflect.Value v, slice<byte> bytes, nint initOffset, fieldParameters @params) {
    nint offset = default;
    error err = default!;

    offset = initOffset;
    var fieldType = v.Type(); 

    // If we have run out of data, it may be that there are optional elements at the end.
    if (offset == len(bytes)) {
        if (!setDefaultValue(v, params)) {
            err = new SyntaxError("sequence truncated");
        }
        return ;

    }
    {
        var ifaceType = fieldType;

        if (ifaceType.Kind() == reflect.Interface && ifaceType.NumMethod() == 0) {
            tagAndLength t = default;
            t, offset, err = parseTagAndLength(bytes, offset);
            if (err != null) {
                return ;
            }
            if (invalidLength(offset, t.length, len(bytes))) {
                err = new SyntaxError("data truncated");
                return ;
            }
            var result = default;
            if (!t.isCompound && t.@class == ClassUniversal) {
                var innerBytes = bytes[(int)offset..(int)offset + t.length];

                if (t.tag == TagPrintableString) 
                    result, err = parsePrintableString(innerBytes);
                else if (t.tag == TagNumericString) 
                    result, err = parseNumericString(innerBytes);
                else if (t.tag == TagIA5String) 
                    result, err = parseIA5String(innerBytes);
                else if (t.tag == TagT61String) 
                    result, err = parseT61String(innerBytes);
                else if (t.tag == TagUTF8String) 
                    result, err = parseUTF8String(innerBytes);
                else if (t.tag == TagInteger) 
                    result, err = parseInt64(innerBytes);
                else if (t.tag == TagBitString) 
                    result, err = parseBitString(innerBytes);
                else if (t.tag == TagOID) 
                    result, err = parseObjectIdentifier(innerBytes);
                else if (t.tag == TagUTCTime) 
                    result, err = parseUTCTime(innerBytes);
                else if (t.tag == TagGeneralizedTime) 
                    result, err = parseGeneralizedTime(innerBytes);
                else if (t.tag == TagOctetString) 
                    result = innerBytes;
                else if (t.tag == TagBMPString) 
                    result, err = parseBMPString(innerBytes);
                else                 
            }

            offset += t.length;
            if (err != null) {
                return ;
            }

            if (result != null) {
                v.Set(reflect.ValueOf(result));
            }

            return ;

        }
    }


    var (t, offset, err) = parseTagAndLength(bytes, offset);
    if (err != null) {
        return ;
    }
    if (@params.@explicit) {
        var expectedClass = ClassContextSpecific;
        if (@params.application) {
            expectedClass = ClassApplication;
        }
        if (offset == len(bytes)) {
            err = new StructuralError("explicit tag has no child");
            return ;
        }
        if (t.@class == expectedClass && t.tag == @params.tag && (t.length == 0 || t.isCompound).val) {
            if (fieldType == rawValueType) { 
                // The inner element should not be parsed for RawValues.
            }
            else if (t.length > 0) {
                t, offset, err = parseTagAndLength(bytes, offset);
                if (err != null) {
                    return ;
                }
            }
            else
 {
                if (fieldType != flagType) {
                    err = new StructuralError("zero length explicit tag was not an asn1.Flag");
                    return ;
                }
                v.SetBool(true);
                return ;
            }

        }
        else
 { 
            // The tags didn't match, it might be an optional element.
            var ok = setDefaultValue(v, params);
            if (ok) {
                offset = initOffset;
            }
            else
 {
                err = new StructuralError("explicitly tagged member didn't match");
            }

            return ;

        }
    }
    var (matchAny, universalTag, compoundType, ok1) = getUniversalType(fieldType);
    if (!ok1) {
        err = new StructuralError(fmt.Sprintf("unknown Go type: %v",fieldType));
        return ;
    }
    if (universalTag == TagPrintableString) {
        if (t.@class == ClassUniversal) {

            if (t.tag == TagIA5String || t.tag == TagGeneralString || t.tag == TagT61String || t.tag == TagUTF8String || t.tag == TagNumericString || t.tag == TagBMPString) 
                universalTag = t.tag;
            
        }
        else if (@params.stringType != 0) {
            universalTag = @params.stringType;
        }
    }
    if (universalTag == TagUTCTime && t.tag == TagGeneralizedTime && t.@class == ClassUniversal) {
        universalTag = TagGeneralizedTime;
    }
    if (@params.set) {
        universalTag = TagSet;
    }
    var matchAnyClassAndTag = matchAny;
    expectedClass = ClassUniversal;
    var expectedTag = universalTag;

    if (!@params.@explicit && @params.tag != null) {
        expectedClass = ClassContextSpecific;
        expectedTag = @params.tag.val;
        matchAnyClassAndTag = false;
    }
    if (!@params.@explicit && @params.application && @params.tag != null) {
        expectedClass = ClassApplication;
        expectedTag = @params.tag.val;
        matchAnyClassAndTag = false;
    }
    if (!@params.@explicit && @params.@private && @params.tag != null) {
        expectedClass = ClassPrivate;
        expectedTag = @params.tag.val;
        matchAnyClassAndTag = false;
    }
    if (!matchAnyClassAndTag && (t.@class != expectedClass || t.tag != expectedTag) || (!matchAny && t.isCompound != compoundType)) { 
        // Tags don't match. Again, it could be an optional element.
        ok = setDefaultValue(v, params);
        if (ok) {
            offset = initOffset;
        }
        else
 {
            err = new StructuralError(fmt.Sprintf("tags don't match (%d vs %+v) %+v %s @%d",expectedTag,t,params,fieldType.Name(),offset));
        }
        return ;

    }
    if (invalidLength(offset, t.length, len(bytes))) {
        err = new SyntaxError("data truncated");
        return ;
    }
    innerBytes = bytes[(int)offset..(int)offset + t.length];
    offset += t.length; 

    // We deal with the structures defined in this package first.
    switch (v.Addr().Interface().type()) {
        case ptr<RawValue> v:
            v.val = new RawValue(t.class,t.tag,t.isCompound,innerBytes,bytes[initOffset:offset]);
            return ;
            break;
        case ptr<ObjectIdentifier> v:
            v.val, err = parseObjectIdentifier(innerBytes);
            return ;
            break;
        case ptr<BitString> v:
            v.val, err = parseBitString(innerBytes);
            return ;
            break;
        case ptr<time.Time> v:
            if (universalTag == TagUTCTime) {
                v.val, err = parseUTCTime(innerBytes);
                return ;
            }
            v.val, err = parseGeneralizedTime(innerBytes);
            return ;
            break;
        case ptr<Enumerated> v:
            var (parsedInt, err1) = parseInt32(innerBytes);
            if (err1 == null) {
                v.val = Enumerated(parsedInt);
            }
            err = err1;
            return ;
            break;
        case ptr<Flag> v:
            v.val = true;
            return ;
            break;
        case ptr<ptr<big.Int>> v:
            (parsedInt, err1) = parseBigInt(innerBytes);
            if (err1 == null) {
                v.val = parsedInt;
            }
            err = err1;
            return ;
            break;
    }
    {
        var val = v;


        if (val.Kind() == reflect.Bool) 
            var (parsedBool, err1) = parseBool(innerBytes);
            if (err1 == null) {
                val.SetBool(parsedBool);
            }
            err = err1;
            return ;
        else if (val.Kind() == reflect.Int || val.Kind() == reflect.Int32 || val.Kind() == reflect.Int64) 
            if (val.Type().Size() == 4) {
                (parsedInt, err1) = parseInt32(innerBytes);
                if (err1 == null) {
                    val.SetInt(int64(parsedInt));
                }
                err = err1;
            }
            else
 {
                (parsedInt, err1) = parseInt64(innerBytes);
                if (err1 == null) {
                    val.SetInt(parsedInt);
                }
                err = err1;
            }

            return ; 
            // TODO(dfc) Add support for the remaining integer types
        else if (val.Kind() == reflect.Struct) 
            var structType = fieldType;

            {
                nint i__prev1 = i;

                for (nint i = 0; i < structType.NumField(); i++) {
                    if (!structType.Field(i).IsExported()) {
                        err = new StructuralError("struct contains unexported fields");
                        return ;
                    }
                }


                i = i__prev1;
            }

            if (structType.NumField() > 0 && structType.Field(0).Type == rawContentsType) {
                var bytes = bytes[(int)initOffset..(int)offset];
                val.Field(0).Set(reflect.ValueOf(RawContent(bytes)));
            }

            nint innerOffset = 0;
            {
                nint i__prev1 = i;

                for (i = 0; i < structType.NumField(); i++) {
                    var field = structType.Field(i);
                    if (i == 0 && field.Type == rawContentsType) {
                        continue;
                    }
                    innerOffset, err = parseField(val.Field(i), innerBytes, innerOffset, parseFieldParameters(field.Tag.Get("asn1")));
                    if (err != null) {
                        return ;
                    }
                } 
                // We allow extra bytes at the end of the SEQUENCE because
                // adding elements to the end has been used in X.509 as the
                // version numbers have increased.


                i = i__prev1;
            } 
            // We allow extra bytes at the end of the SEQUENCE because
            // adding elements to the end has been used in X.509 as the
            // version numbers have increased.
            return ;
        else if (val.Kind() == reflect.Slice) 
            var sliceType = fieldType;
            if (sliceType.Elem().Kind() == reflect.Uint8) {
                val.Set(reflect.MakeSlice(sliceType, len(innerBytes), len(innerBytes)));
                reflect.Copy(val, reflect.ValueOf(innerBytes));
                return ;
            }
            var (newSlice, err1) = parseSequenceOf(innerBytes, sliceType, sliceType.Elem());
            if (err1 == null) {
                val.Set(newSlice);
            }
            err = err1;
            return ;
        else if (val.Kind() == reflect.String) 
            @string v = default;

            if (universalTag == TagPrintableString) 
                v, err = parsePrintableString(innerBytes);
            else if (universalTag == TagNumericString) 
                v, err = parseNumericString(innerBytes);
            else if (universalTag == TagIA5String) 
                v, err = parseIA5String(innerBytes);
            else if (universalTag == TagT61String) 
                v, err = parseT61String(innerBytes);
            else if (universalTag == TagUTF8String) 
                v, err = parseUTF8String(innerBytes);
            else if (universalTag == TagGeneralString) 
                // GeneralString is specified in ISO-2022/ECMA-35,
                // A brief review suggests that it includes structures
                // that allow the encoding to change midstring and
                // such. We give up and pass it as an 8-bit string.
                v, err = parseT61String(innerBytes);
            else if (universalTag == TagBMPString) 
                v, err = parseBMPString(innerBytes);
            else 
                err = new SyntaxError(fmt.Sprintf("internal error: unknown string type %d",universalTag));
                        if (err == null) {
                val.SetString(v);
            }

            return ;

    }
    err = new StructuralError("unsupported: "+v.Type().String());
    return ;

}

// canHaveDefaultValue reports whether k is a Kind that we will set a default
// value for. (A signed integer, essentially.)
private static bool canHaveDefaultValue(reflect.Kind k) {

    if (k == reflect.Int || k == reflect.Int8 || k == reflect.Int16 || k == reflect.Int32 || k == reflect.Int64) 
        return true;
        return false;

}

// setDefaultValue is used to install a default value, from a tag string, into
// a Value. It is successful if the field was optional, even if a default value
// wasn't provided or it failed to install it into the Value.
private static bool setDefaultValue(reflect.Value v, fieldParameters @params) {
    bool ok = default;

    if (!@params.optional) {
        return ;
    }
    ok = true;
    if (@params.defaultValue == null) {
        return ;
    }
    if (canHaveDefaultValue(v.Kind())) {
        v.SetInt(@params.defaultValue.val);
    }
    return ;

}

// Unmarshal parses the DER-encoded ASN.1 data structure b
// and uses the reflect package to fill in an arbitrary value pointed at by val.
// Because Unmarshal uses the reflect package, the structs
// being written to must use upper case field names. If val
// is nil or not a pointer, Unmarshal returns an error.
//
// After parsing b, any bytes that were leftover and not used to fill
// val will be returned in rest. When parsing a SEQUENCE into a struct,
// any trailing elements of the SEQUENCE that do not have matching
// fields in val will not be included in rest, as these are considered
// valid elements of the SEQUENCE and not trailing data.
//
// An ASN.1 INTEGER can be written to an int, int32, int64,
// or *big.Int (from the math/big package).
// If the encoded value does not fit in the Go type,
// Unmarshal returns a parse error.
//
// An ASN.1 BIT STRING can be written to a BitString.
//
// An ASN.1 OCTET STRING can be written to a []byte.
//
// An ASN.1 OBJECT IDENTIFIER can be written to an
// ObjectIdentifier.
//
// An ASN.1 ENUMERATED can be written to an Enumerated.
//
// An ASN.1 UTCTIME or GENERALIZEDTIME can be written to a time.Time.
//
// An ASN.1 PrintableString, IA5String, or NumericString can be written to a string.
//
// Any of the above ASN.1 values can be written to an interface{}.
// The value stored in the interface has the corresponding Go type.
// For integers, that type is int64.
//
// An ASN.1 SEQUENCE OF x or SET OF x can be written
// to a slice if an x can be written to the slice's element type.
//
// An ASN.1 SEQUENCE or SET can be written to a struct
// if each of the elements in the sequence can be
// written to the corresponding element in the struct.
//
// The following tags on struct fields have special meaning to Unmarshal:
//
//    application specifies that an APPLICATION tag is used
//    private     specifies that a PRIVATE tag is used
//    default:x   sets the default value for optional integer fields (only used if optional is also present)
//    explicit    specifies that an additional, explicit tag wraps the implicit one
//    optional    marks the field as ASN.1 OPTIONAL
//    set         causes a SET, rather than a SEQUENCE type to be expected
//    tag:x       specifies the ASN.1 tag number; implies ASN.1 CONTEXT SPECIFIC
//
// When decoding an ASN.1 value with an IMPLICIT tag into a string field,
// Unmarshal will default to a PrintableString, which doesn't support
// characters such as '@' and '&'. To force other encodings, use the following
// tags:
//
//    ia5     causes strings to be unmarshaled as ASN.1 IA5String values
//    numeric causes strings to be unmarshaled as ASN.1 NumericString values
//    utf8    causes strings to be unmarshaled as ASN.1 UTF8String values
//
// If the type of the first field of a structure is RawContent then the raw
// ASN1 contents of the struct will be stored in it.
//
// If the name of a slice type ends with "SET" then it's treated as if
// the "set" tag was set on it. This results in interpreting the type as a
// SET OF x rather than a SEQUENCE OF x. This can be used with nested slices
// where a struct tag cannot be given.
//
// Other ASN.1 types are not supported; if it encounters them,
// Unmarshal returns a parse error.
public static (slice<byte>, error) Unmarshal(slice<byte> b, object val) {
    slice<byte> rest = default;
    error err = default!;

    return UnmarshalWithParams(b, val, "");
}

// An invalidUnmarshalError describes an invalid argument passed to Unmarshal.
// (The argument to Unmarshal must be a non-nil pointer.)
private partial struct invalidUnmarshalError {
    public reflect.Type Type;
}

private static @string Error(this ptr<invalidUnmarshalError> _addr_e) {
    ref invalidUnmarshalError e = ref _addr_e.val;

    if (e.Type == null) {
        return "asn1: Unmarshal recipient value is nil";
    }
    if (e.Type.Kind() != reflect.Ptr) {
        return "asn1: Unmarshal recipient value is non-pointer " + e.Type.String();
    }
    return "asn1: Unmarshal recipient value is nil " + e.Type.String();

}

// UnmarshalWithParams allows field parameters to be specified for the
// top-level element. The form of the params is the same as the field tags.
public static (slice<byte>, error) UnmarshalWithParams(slice<byte> b, object val, @string @params) {
    slice<byte> rest = default;
    error err = default!;

    var v = reflect.ValueOf(val);
    if (v.Kind() != reflect.Ptr || v.IsNil()) {
        return (null, error.As(addr(new invalidUnmarshalError(reflect.TypeOf(val)))!)!);
    }
    var (offset, err) = parseField(v.Elem(), b, 0, parseFieldParameters(params));
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (b[(int)offset..], error.As(null!)!);

}

} // end asn1_package
