// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package asn1 -- go2cs converted at 2022 March 06 22:19:47 UTC
// import "encoding/asn1" ==> using asn1 = go.encoding.asn1_package
// Original source: C:\Program Files\Go\src\encoding\asn1\marshal.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.encoding;

public static partial class asn1_package {

private static encoder byte00Encoder = encoder.As(byteEncoder(0x00))!;private static encoder byteFFEncoder = encoder.As(byteEncoder(0xff))!;

// encoder represents an ASN.1 element that is waiting to be marshaled.
private partial interface encoder {
    nint Len(); // Encode encodes this element by writing Len() bytes to dst.
    nint Encode(slice<byte> dst);
}

private partial struct byteEncoder { // : byte
}

private static nint Len(this byteEncoder c) {
    return 1;
}

private static void Encode(this byteEncoder c, slice<byte> dst) {
    dst[0] = byte(c);
}

private partial struct bytesEncoder { // : slice<byte>
}

private static nint Len(this bytesEncoder b) {
    return len(b);
}

private static void Encode(this bytesEncoder b, slice<byte> dst) => func((_, panic, _) => {
    if (copy(dst, b) != len(b)) {
        panic("internal error");
    }
});

private partial struct stringEncoder { // : @string
}

private static nint Len(this stringEncoder s) {
    return len(s);
}

private static void Encode(this stringEncoder s, slice<byte> dst) => func((_, panic, _) => {
    if (copy(dst, s) != len(s)) {
        panic("internal error");
    }
});

private partial struct multiEncoder { // : slice<encoder>
}

private static nint Len(this multiEncoder m) {
    nint size = default;
    foreach (var (_, e) in m) {
        size += e.Len();
    }    return size;
}

private static void Encode(this multiEncoder m, slice<byte> dst) {
    nint off = default;
    foreach (var (_, e) in m) {
        e.Encode(dst[(int)off..]);
        off += e.Len();
    }
}

private partial struct setEncoder { // : slice<encoder>
}

private static nint Len(this setEncoder s) {
    nint size = default;
    foreach (var (_, e) in s) {
        size += e.Len();
    }    return size;
}

private static void Encode(this setEncoder s, slice<byte> dst) { 
    // Per X690 Section 11.6: The encodings of the component values of a
    // set-of value shall appear in ascending order, the encodings being
    // compared as octet strings with the shorter components being padded
    // at their trailing end with 0-octets.
    //
    // First we encode each element to its TLV encoding and then use
    // octetSort to get the ordering expected by X690 DER rules before
    // writing the sorted encodings out to dst.
    var l = make_slice<slice<byte>>(len(s));
    foreach (var (i, e) in s) {
        l[i] = make_slice<byte>(e.Len());
        e.Encode(l[i]);
    }    sort.Slice(l, (i, j) => { 
        // Since we are using bytes.Compare to compare TLV encodings we
        // don't need to right pad s[i] and s[j] to the same length as
        // suggested in X690. If len(s[i]) < len(s[j]) the length octet of
        // s[i], which is the first determining byte, will inherently be
        // smaller than the length octet of s[j]. This lets us skip the
        // padding step.
        return bytes.Compare(l[i], l[j]) < 0;

    });

    nint off = default;
    foreach (var (_, b) in l) {
        copy(dst[(int)off..], b);
        off += len(b);
    }
}

private partial struct taggedEncoder {
    public array<byte> scratch;
    public encoder tag;
    public encoder body;
}

private static nint Len(this ptr<taggedEncoder> _addr_t) {
    ref taggedEncoder t = ref _addr_t.val;

    return t.tag.Len() + t.body.Len();
}

private static void Encode(this ptr<taggedEncoder> _addr_t, slice<byte> dst) {
    ref taggedEncoder t = ref _addr_t.val;

    t.tag.Encode(dst);
    t.body.Encode(dst[(int)t.tag.Len()..]);
}

private partial struct int64Encoder { // : long
}

private static nint Len(this int64Encoder i) {
    nint n = 1;

    while (i > 127) {
        n++;
        i>>=8;
    }

    while (i < -128) {
        n++;
        i>>=8;
    }

    return n;
}

private static void Encode(this int64Encoder i, slice<byte> dst) {
    var n = i.Len();

    for (nint j = 0; j < n; j++) {
        dst[j] = byte(i >> (int)(uint((n - 1 - j) * 8)));
    }
}

private static nint base128IntLength(long n) {
    if (n == 0) {
        return 1;
    }
    nint l = 0;
    {
        var i = n;

        while (i > 0) {
            l++;
            i>>=7;
        }
    }

    return l;

}

private static slice<byte> appendBase128Int(slice<byte> dst, long n) {
    var l = base128IntLength(n);

    for (var i = l - 1; i >= 0; i--) {
        var o = byte(n >> (int)(uint(i * 7)));
        o &= 0x7f;
        if (i != 0) {
            o |= 0x80;
        }
        dst = append(dst, o);

    }

    return dst;

}

private static (encoder, error) makeBigInt(ptr<big.Int> _addr_n) {
    encoder _p0 = default;
    error _p0 = default!;
    ref big.Int n = ref _addr_n.val;

    if (n == null) {
        return (null, error.As(new StructuralError("empty integer"))!);
    }
    if (n.Sign() < 0) { 
        // A negative number has to be converted to two's-complement
        // form. So we'll invert and subtract 1. If the
        // most-significant-bit isn't set then we'll need to pad the
        // beginning with 0xff in order to keep the number negative.
        ptr<big.Int> nMinus1 = @new<big.Int>().Neg(n);
        nMinus1.Sub(nMinus1, bigOne);
        var bytes = nMinus1.Bytes();
        foreach (var (i) in bytes) {
            bytes[i] ^= 0xff;
        }        if (len(bytes) == 0 || bytes[0] & 0x80 == 0) {
            return (multiEncoder(new slice<encoder>(new encoder[] { encoder.As(byteFFEncoder)!, encoder.As(bytesEncoder(bytes))! })), error.As(null!)!);
        }
        return (bytesEncoder(bytes), error.As(null!)!);

    }
    else if (n.Sign() == 0) { 
        // Zero is written as a single 0 zero rather than no bytes.
        return (byte00Encoder, error.As(null!)!);

    }
    else
 {
        bytes = n.Bytes();
        if (len(bytes) > 0 && bytes[0] & 0x80 != 0) { 
            // We'll have to pad this with 0x00 in order to stop it
            // looking like a negative number.
            return (multiEncoder(new slice<encoder>(new encoder[] { encoder.As(byte00Encoder)!, encoder.As(bytesEncoder(bytes))! })), error.As(null!)!);

        }
        return (bytesEncoder(bytes), error.As(null!)!);

    }
}

private static slice<byte> appendLength(slice<byte> dst, nint i) {
    var n = lengthLength(i);

    while (n > 0) {
        dst = append(dst, byte(i >> (int)(uint((n - 1) * 8))));
        n--;
    }

    return dst;

}

private static nint lengthLength(nint i) {
    nint numBytes = default;

    numBytes = 1;
    while (i > 255) {
        numBytes++;
        i>>=8;
    }
    return ;
}

private static slice<byte> appendTagAndLength(slice<byte> dst, tagAndLength t) {
    var b = uint8(t.@class) << 6;
    if (t.isCompound) {
        b |= 0x20;
    }
    if (t.tag >= 31) {
        b |= 0x1f;
        dst = append(dst, b);
        dst = appendBase128Int(dst, int64(t.tag));
    }
    else
 {
        b |= uint8(t.tag);
        dst = append(dst, b);
    }
    if (t.length >= 128) {
        var l = lengthLength(t.length);
        dst = append(dst, 0x80 | byte(l));
        dst = appendLength(dst, t.length);
    }
    else
 {
        dst = append(dst, byte(t.length));
    }
    return dst;

}

private partial struct bitStringEncoder { // : BitString
}

private static nint Len(this bitStringEncoder b) {
    return len(b.Bytes) + 1;
}

private static void Encode(this bitStringEncoder b, slice<byte> dst) => func((_, panic, _) => {
    dst[0] = byte((8 - b.BitLength % 8) % 8);
    if (copy(dst[(int)1..], b.Bytes) != len(b.Bytes)) {
        panic("internal error");
    }
});

private partial struct oidEncoder { // : slice<nint>
}

private static nint Len(this oidEncoder oid) {
    var l = base128IntLength(int64(oid[0] * 40 + oid[1]));
    for (nint i = 2; i < len(oid); i++) {
        l += base128IntLength(int64(oid[i]));
    }
    return l;
}

private static void Encode(this oidEncoder oid, slice<byte> dst) {
    dst = appendBase128Int(dst[..(int)0], int64(oid[0] * 40 + oid[1]));
    for (nint i = 2; i < len(oid); i++) {
        dst = appendBase128Int(dst, int64(oid[i]));
    }
}

private static (encoder, error) makeObjectIdentifier(slice<nint> oid) {
    encoder e = default;
    error err = default!;

    if (len(oid) < 2 || oid[0] > 2 || (oid[0] < 2 && oid[1] >= 40)) {
        return (null, error.As(new StructuralError("invalid object identifier"))!);
    }
    return (oidEncoder(oid), error.As(null!)!);

}

private static (encoder, error) makePrintableString(@string s) {
    encoder e = default;
    error err = default!;

    for (nint i = 0; i < len(s); i++) { 
        // The asterisk is often used in PrintableString, even though
        // it is invalid. If a PrintableString was specifically
        // requested then the asterisk is permitted by this code.
        // Ampersand is allowed in parsing due a handful of CA
        // certificates, however when making new certificates
        // it is rejected.
        if (!isPrintable(s[i], allowAsterisk, rejectAmpersand)) {
            return (null, error.As(new StructuralError("PrintableString contains invalid character"))!);
        }
    }

    return (stringEncoder(s), error.As(null!)!);

}

private static (encoder, error) makeIA5String(@string s) {
    encoder e = default;
    error err = default!;

    for (nint i = 0; i < len(s); i++) {
        if (s[i] > 127) {
            return (null, error.As(new StructuralError("IA5String contains invalid character"))!);
        }
    }

    return (stringEncoder(s), error.As(null!)!);

}

private static (encoder, error) makeNumericString(@string s) {
    encoder e = default;
    error err = default!;

    for (nint i = 0; i < len(s); i++) {
        if (!isNumeric(s[i])) {
            return (null, error.As(new StructuralError("NumericString contains invalid character"))!);
        }
    }

    return (stringEncoder(s), error.As(null!)!);

}

private static encoder makeUTF8String(@string s) {
    return stringEncoder(s);
}

private static slice<byte> appendTwoDigits(slice<byte> dst, nint v) {
    return append(dst, byte('0' + (v / 10) % 10), byte('0' + v % 10));
}

private static slice<byte> appendFourDigits(slice<byte> dst, nint v) {
    array<byte> bytes = new array<byte>(4);
    foreach (var (i) in bytes) {
        bytes[3 - i] = '0' + byte(v % 10);
        v /= 10;
    }    return append(dst, bytes[..]);
}

private static bool outsideUTCRange(time.Time t) {
    var year = t.Year();
    return year < 1950 || year >= 2050;
}

private static (encoder, error) makeUTCTime(time.Time t) {
    encoder e = default;
    error err = default!;

    var dst = make_slice<byte>(0, 18);

    dst, err = appendUTCTime(dst, t);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (bytesEncoder(dst), error.As(null!)!);

}

private static (encoder, error) makeGeneralizedTime(time.Time t) {
    encoder e = default;
    error err = default!;

    var dst = make_slice<byte>(0, 20);

    dst, err = appendGeneralizedTime(dst, t);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (bytesEncoder(dst), error.As(null!)!);

}

private static (slice<byte>, error) appendUTCTime(slice<byte> dst, time.Time t) {
    slice<byte> ret = default;
    error err = default!;

    var year = t.Year();


    if (1950 <= year && year < 2000) 
        dst = appendTwoDigits(dst, year - 1900);
    else if (2000 <= year && year < 2050) 
        dst = appendTwoDigits(dst, year - 2000);
    else 
        return (null, error.As(new StructuralError("cannot represent time as UTCTime"))!);
        return (appendTimeCommon(dst, t), error.As(null!)!);

}

private static (slice<byte>, error) appendGeneralizedTime(slice<byte> dst, time.Time t) {
    slice<byte> ret = default;
    error err = default!;

    var year = t.Year();
    if (year < 0 || year > 9999) {
        return (null, error.As(new StructuralError("cannot represent time as GeneralizedTime"))!);
    }
    dst = appendFourDigits(dst, year);

    return (appendTimeCommon(dst, t), error.As(null!)!);

}

private static slice<byte> appendTimeCommon(slice<byte> dst, time.Time t) {
    var (_, month, day) = t.Date();

    dst = appendTwoDigits(dst, int(month));
    dst = appendTwoDigits(dst, day);

    var (hour, min, sec) = t.Clock();

    dst = appendTwoDigits(dst, hour);
    dst = appendTwoDigits(dst, min);
    dst = appendTwoDigits(dst, sec);

    var (_, offset) = t.Zone();


    if (offset / 60 == 0) 
        return append(dst, 'Z');
    else if (offset > 0) 
        dst = append(dst, '+');
    else if (offset < 0) 
        dst = append(dst, '-');
        var offsetMinutes = offset / 60;
    if (offsetMinutes < 0) {
        offsetMinutes = -offsetMinutes;
    }
    dst = appendTwoDigits(dst, offsetMinutes / 60);
    dst = appendTwoDigits(dst, offsetMinutes % 60);

    return dst;

}

private static slice<byte> stripTagAndLength(slice<byte> @in) {
    var (_, offset, err) = parseTagAndLength(in, 0);
    if (err != null) {
        return in;
    }
    return in[(int)offset..];

}

private static (encoder, error) makeBody(reflect.Value value, fieldParameters @params) {
    encoder e = default;
    error err = default!;


    if (value.Type() == flagType) 
        return (bytesEncoder(null), error.As(null!)!);
    else if (value.Type() == timeType) 
        time.Time t = value.Interface()._<time.Time>();
        if (@params.timeType == TagGeneralizedTime || outsideUTCRange(t)) {
            return makeGeneralizedTime(t);
        }
        return makeUTCTime(t);
    else if (value.Type() == bitStringType) 
        return (bitStringEncoder(value.Interface()._<BitString>()), error.As(null!)!);
    else if (value.Type() == objectIdentifierType) 
        return makeObjectIdentifier(value.Interface()._<ObjectIdentifier>());
    else if (value.Type() == bigIntType) 
        return makeBigInt(value.Interface()._<ptr<big.Int>>());
        {
        var v = value;


        if (v.Kind() == reflect.Bool) 
            if (v.Bool()) {
                return (byteFFEncoder, error.As(null!)!);
            }
            return (byte00Encoder, error.As(null!)!);
        else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
            return (int64Encoder(v.Int()), error.As(null!)!);
        else if (v.Kind() == reflect.Struct) 
            t = v.Type();

            {
                nint i__prev1 = i;

                for (nint i = 0; i < t.NumField(); i++) {
                    if (!t.Field(i).IsExported()) {
                        return (null, error.As(new StructuralError("struct contains unexported fields"))!);
                    }
                }


                i = i__prev1;
            }

            nint startingField = 0;

            var n = t.NumField();
            if (n == 0) {
                return (bytesEncoder(null), error.As(null!)!);
            } 

            // If the first element of the structure is a non-empty
            // RawContents, then we don't bother serializing the rest.
            if (t.Field(0).Type == rawContentsType) {
                var s = v.Field(0);
                if (s.Len() > 0) {
                    var bytes = s.Bytes();
                    /* The RawContents will contain the tag and
                                     * length fields but we'll also be writing
                                     * those ourselves, so we strip them out of
                                     * bytes */
                    return (bytesEncoder(stripTagAndLength(bytes)), error.As(null!)!);

                }

                startingField = 1;

            }

            {
                var n1 = n - startingField;

                switch (n1) {
                    case 0: 
                        return (bytesEncoder(null), error.As(null!)!);
                        break;
                    case 1: 
                        return makeField(v.Field(startingField), parseFieldParameters(t.Field(startingField).Tag.Get("asn1")));
                        break;
                    default: 
                        var m = make_slice<encoder>(n1);
                        {
                            nint i__prev1 = i;

                            for (i = 0; i < n1; i++) {
                                m[i], err = makeField(v.Field(i + startingField), parseFieldParameters(t.Field(i + startingField).Tag.Get("asn1")));
                                if (err != null) {
                                    return (null, error.As(err)!);
                                }
                            }


                            i = i__prev1;
                        }

                        return (multiEncoder(m), error.As(null!)!);

                        break;
                }
            }
        else if (v.Kind() == reflect.Slice) 
            var sliceType = v.Type();
            if (sliceType.Elem().Kind() == reflect.Uint8) {
                return (bytesEncoder(v.Bytes()), error.As(null!)!);
            }
            fieldParameters fp = default;

            {
                var l = v.Len();

                switch (l) {
                    case 0: 
                        return (bytesEncoder(null), error.As(null!)!);
                        break;
                    case 1: 
                        return makeField(v.Index(0), fp);
                        break;
                    default: 
                        m = make_slice<encoder>(l);

                        {
                            nint i__prev1 = i;

                            for (i = 0; i < l; i++) {
                                m[i], err = makeField(v.Index(i), fp);
                                if (err != null) {
                                    return (null, error.As(err)!);
                                }
                            }


                            i = i__prev1;
                        }

                        if (@params.set) {
                            return (setEncoder(m), error.As(null!)!);
                        }

                        return (multiEncoder(m), error.As(null!)!);

                        break;
                }
            }
        else if (v.Kind() == reflect.String) 

            if (@params.stringType == TagIA5String) 
                return makeIA5String(v.String());
            else if (@params.stringType == TagPrintableString) 
                return makePrintableString(v.String());
            else if (@params.stringType == TagNumericString) 
                return makeNumericString(v.String());
            else 
                return (makeUTF8String(v.String()), error.As(null!)!);
            
    }

    return (null, error.As(new StructuralError("unknown Go type"))!);

}

private static (encoder, error) makeField(reflect.Value v, fieldParameters @params) {
    encoder e = default;
    error err = default!;

    if (!v.IsValid()) {
        return (null, error.As(fmt.Errorf("asn1: cannot marshal nil value"))!);
    }
    if (v.Kind() == reflect.Interface && v.Type().NumMethod() == 0) {
        return makeField(v.Elem(), params);
    }
    if (v.Kind() == reflect.Slice && v.Len() == 0 && @params.omitEmpty) {
        return (bytesEncoder(null), error.As(null!)!);
    }
    if (@params.optional && @params.defaultValue != null && canHaveDefaultValue(v.Kind())) {
        var defaultValue = reflect.New(v.Type()).Elem();
        defaultValue.SetInt(@params.defaultValue.val);

        if (reflect.DeepEqual(v.Interface(), defaultValue.Interface())) {
            return (bytesEncoder(null), error.As(null!)!);
        }
    }
    if (@params.optional && @params.defaultValue == null) {
        if (reflect.DeepEqual(v.Interface(), reflect.Zero(v.Type()).Interface())) {
            return (bytesEncoder(null), error.As(null!)!);
        }
    }
    if (v.Type() == rawValueType) {
        RawValue rv = v.Interface()._<RawValue>();
        if (len(rv.FullBytes) != 0) {
            return (bytesEncoder(rv.FullBytes), error.As(null!)!);
        }
        ptr<taggedEncoder> t = @new<taggedEncoder>();

        t.tag = bytesEncoder(appendTagAndLength(t.scratch[..(int)0], new tagAndLength(rv.Class,rv.Tag,len(rv.Bytes),rv.IsCompound)));
        t.body = bytesEncoder(rv.Bytes);

        return (t, error.As(null!)!);

    }
    var (matchAny, tag, isCompound, ok) = getUniversalType(v.Type());
    if (!ok || matchAny) {
        return (null, error.As(new StructuralError(fmt.Sprintf("unknown Go type: %v",v.Type())))!);
    }
    if (@params.timeType != 0 && tag != TagUTCTime) {
        return (null, error.As(new StructuralError("explicit time type given to non-time member"))!);
    }
    if (@params.stringType != 0 && tag != TagPrintableString) {
        return (null, error.As(new StructuralError("explicit string type given to non-string member"))!);
    }

    if (tag == TagPrintableString) 
        if (@params.stringType == 0) { 
            // This is a string without an explicit string type. We'll use
            // a PrintableString if the character set in the string is
            // sufficiently limited, otherwise we'll use a UTF8String.
            foreach (var (_, r) in v.String()) {
                if (r >= utf8.RuneSelf || !isPrintable(byte(r), rejectAsterisk, rejectAmpersand)) {
                    if (!utf8.ValidString(v.String())) {
                        return (null, error.As(errors.New("asn1: string not valid UTF-8"))!);
                    }
                    tag = TagUTF8String;
                    break;
                }
            }
        else
        } {
            tag = @params.stringType;
        }
    else if (tag == TagUTCTime) 
        if (@params.timeType == TagGeneralizedTime || outsideUTCRange(v.Interface()._<time.Time>())) {
            tag = TagGeneralizedTime;
        }
        if (@params.set) {
        if (tag != TagSequence) {
            return (null, error.As(new StructuralError("non sequence tagged as set"))!);
        }
        tag = TagSet;

    }
    if (tag == TagSet && !@params.set) {
        @params.set = true;
    }
    t = @new<taggedEncoder>();

    t.body, err = makeBody(v, params);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var bodyLen = t.body.Len();

    var @class = ClassUniversal;
    if (@params.tag != null) {
        if (@params.application) {
            class = ClassApplication;
        }
        else if (@params.@private) {
            class = ClassPrivate;
        }
        else
 {
            class = ClassContextSpecific;
        }
        if (@params.@explicit) {
            t.tag = bytesEncoder(appendTagAndLength(t.scratch[..(int)0], new tagAndLength(ClassUniversal,tag,bodyLen,isCompound)));

            ptr<taggedEncoder> tt = @new<taggedEncoder>();

            tt.body = t;

            tt.tag = bytesEncoder(appendTagAndLength(tt.scratch[..(int)0], new tagAndLength(class:class,tag:*params.tag,length:bodyLen+t.tag.Len(),isCompound:true,)));

            return (tt, error.As(null!)!);
        }
        tag = @params.tag.val;

    }
    t.tag = bytesEncoder(appendTagAndLength(t.scratch[..(int)0], new tagAndLength(class,tag,bodyLen,isCompound)));

    return (t, error.As(null!)!);

}

// Marshal returns the ASN.1 encoding of val.
//
// In addition to the struct tags recognised by Unmarshal, the following can be
// used:
//
//    ia5:         causes strings to be marshaled as ASN.1, IA5String values
//    omitempty:   causes empty slices to be skipped
//    printable:   causes strings to be marshaled as ASN.1, PrintableString values
//    utf8:        causes strings to be marshaled as ASN.1, UTF8String values
//    utc:         causes time.Time to be marshaled as ASN.1, UTCTime values
//    generalized: causes time.Time to be marshaled as ASN.1, GeneralizedTime values
public static (slice<byte>, error) Marshal(object val) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return MarshalWithParams(val, "");
}

// MarshalWithParams allows field parameters to be specified for the
// top-level element. The form of the params is the same as the field tags.
public static (slice<byte>, error) MarshalWithParams(object val, @string @params) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (e, err) = makeField(reflect.ValueOf(val), parseFieldParameters(params));
    if (err != null) {
        return (null, error.As(err)!);
    }
    var b = make_slice<byte>(e.Len());
    e.Encode(b);
    return (b, error.As(null!)!);

}

} // end asn1_package
