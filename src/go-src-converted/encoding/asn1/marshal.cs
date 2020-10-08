// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package asn1 -- go2cs converted at 2020 October 08 03:36:54 UTC
// import "encoding/asn1" ==> using asn1 = go.encoding.asn1_package
// Original source: C:\Go\src\encoding\asn1\marshal.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class asn1_package
    {
        private static encoder byte00Encoder = encoder.As(byteEncoder(0x00UL))!;        private static encoder byteFFEncoder = encoder.As(byteEncoder(0xffUL))!;

        // encoder represents an ASN.1 element that is waiting to be marshaled.
        private partial interface encoder
        {
            long Len(); // Encode encodes this element by writing Len() bytes to dst.
            long Encode(slice<byte> dst);
        }

        private partial struct byteEncoder // : byte
        {
        }

        private static long Len(this byteEncoder c)
        {
            return 1L;
        }

        private static void Encode(this byteEncoder c, slice<byte> dst)
        {
            dst[0L] = byte(c);
        }

        private partial struct bytesEncoder // : slice<byte>
        {
        }

        private static long Len(this bytesEncoder b)
        {
            return len(b);
        }

        private static void Encode(this bytesEncoder b, slice<byte> dst) => func((_, panic, __) =>
        {
            if (copy(dst, b) != len(b))
            {
                panic("internal error");
            }

        });

        private partial struct stringEncoder // : @string
        {
        }

        private static long Len(this stringEncoder s)
        {
            return len(s);
        }

        private static void Encode(this stringEncoder s, slice<byte> dst) => func((_, panic, __) =>
        {
            if (copy(dst, s) != len(s))
            {
                panic("internal error");
            }

        });

        private partial struct multiEncoder // : slice<encoder>
        {
        }

        private static long Len(this multiEncoder m)
        {
            long size = default;
            foreach (var (_, e) in m)
            {
                size += e.Len();
            }
            return size;

        }

        private static void Encode(this multiEncoder m, slice<byte> dst)
        {
            long off = default;
            foreach (var (_, e) in m)
            {
                e.Encode(dst[off..]);
                off += e.Len();
            }

        }

        private partial struct setEncoder // : slice<encoder>
        {
        }

        private static long Len(this setEncoder s)
        {
            long size = default;
            foreach (var (_, e) in s)
            {
                size += e.Len();
            }
            return size;

        }

        private static void Encode(this setEncoder s, slice<byte> dst)
        { 
            // Per X690 Section 11.6: The encodings of the component values of a
            // set-of value shall appear in ascending order, the encodings being
            // compared as octet strings with the shorter components being padded
            // at their trailing end with 0-octets.
            //
            // First we encode each element to its TLV encoding and then use
            // octetSort to get the ordering expected by X690 DER rules before
            // writing the sorted encodings out to dst.
            var l = make_slice<slice<byte>>(len(s));
            foreach (var (i, e) in s)
            {
                l[i] = make_slice<byte>(e.Len());
                e.Encode(l[i]);
            }
            sort.Slice(l, (i, j) =>
            { 
                // Since we are using bytes.Compare to compare TLV encodings we
                // don't need to right pad s[i] and s[j] to the same length as
                // suggested in X690. If len(s[i]) < len(s[j]) the length octet of
                // s[i], which is the first determining byte, will inherently be
                // smaller than the length octet of s[j]. This lets us skip the
                // padding step.
                return bytes.Compare(l[i], l[j]) < 0L;

            });

            long off = default;
            foreach (var (_, b) in l)
            {
                copy(dst[off..], b);
                off += len(b);
            }

        }

        private partial struct taggedEncoder
        {
            public array<byte> scratch;
            public encoder tag;
            public encoder body;
        }

        private static long Len(this ptr<taggedEncoder> _addr_t)
        {
            ref taggedEncoder t = ref _addr_t.val;

            return t.tag.Len() + t.body.Len();
        }

        private static void Encode(this ptr<taggedEncoder> _addr_t, slice<byte> dst)
        {
            ref taggedEncoder t = ref _addr_t.val;

            t.tag.Encode(dst);
            t.body.Encode(dst[t.tag.Len()..]);
        }

        private partial struct int64Encoder // : long
        {
        }

        private static long Len(this int64Encoder i)
        {
            long n = 1L;

            while (i > 127L)
            {
                n++;
                i >>= 8L;
            }


            while (i < -128L)
            {
                n++;
                i >>= 8L;
            }


            return n;

        }

        private static void Encode(this int64Encoder i, slice<byte> dst)
        {
            var n = i.Len();

            for (long j = 0L; j < n; j++)
            {
                dst[j] = byte(i >> (int)(uint((n - 1L - j) * 8L)));
            }


        }

        private static long base128IntLength(long n)
        {
            if (n == 0L)
            {
                return 1L;
            }

            long l = 0L;
            {
                var i = n;

                while (i > 0L)
                {
                    l++;
                    i >>= 7L;
                }

            }

            return l;

        }

        private static slice<byte> appendBase128Int(slice<byte> dst, long n)
        {
            var l = base128IntLength(n);

            for (var i = l - 1L; i >= 0L; i--)
            {
                var o = byte(n >> (int)(uint(i * 7L)));
                o &= 0x7fUL;
                if (i != 0L)
                {
                    o |= 0x80UL;
                }

                dst = append(dst, o);

            }


            return dst;

        }

        private static (encoder, error) makeBigInt(ptr<big.Int> _addr_n)
        {
            encoder _p0 = default;
            error _p0 = default!;
            ref big.Int n = ref _addr_n.val;

            if (n == null)
            {
                return (null, error.As(new StructuralError("empty integer"))!);
            }

            if (n.Sign() < 0L)
            { 
                // A negative number has to be converted to two's-complement
                // form. So we'll invert and subtract 1. If the
                // most-significant-bit isn't set then we'll need to pad the
                // beginning with 0xff in order to keep the number negative.
                ptr<big.Int> nMinus1 = @new<big.Int>().Neg(n);
                nMinus1.Sub(nMinus1, bigOne);
                var bytes = nMinus1.Bytes();
                foreach (var (i) in bytes)
                {
                    bytes[i] ^= 0xffUL;
                }
                if (len(bytes) == 0L || bytes[0L] & 0x80UL == 0L)
                {
                    return (multiEncoder(new slice<encoder>(new encoder[] { encoder.As(byteFFEncoder)!, encoder.As(bytesEncoder(bytes))! })), error.As(null!)!);
                }

                return (bytesEncoder(bytes), error.As(null!)!);

            }
            else if (n.Sign() == 0L)
            { 
                // Zero is written as a single 0 zero rather than no bytes.
                return (byte00Encoder, error.As(null!)!);

            }
            else
            {
                bytes = n.Bytes();
                if (len(bytes) > 0L && bytes[0L] & 0x80UL != 0L)
                { 
                    // We'll have to pad this with 0x00 in order to stop it
                    // looking like a negative number.
                    return (multiEncoder(new slice<encoder>(new encoder[] { encoder.As(byte00Encoder)!, encoder.As(bytesEncoder(bytes))! })), error.As(null!)!);

                }

                return (bytesEncoder(bytes), error.As(null!)!);

            }

        }

        private static slice<byte> appendLength(slice<byte> dst, long i)
        {
            var n = lengthLength(i);

            while (n > 0L)
            {
                dst = append(dst, byte(i >> (int)(uint((n - 1L) * 8L))));
                n--;
            }


            return dst;

        }

        private static long lengthLength(long i)
        {
            long numBytes = default;

            numBytes = 1L;
            while (i > 255L)
            {
                numBytes++;
                i >>= 8L;
            }

            return ;

        }

        private static slice<byte> appendTagAndLength(slice<byte> dst, tagAndLength t)
        {
            var b = uint8(t.@class) << (int)(6L);
            if (t.isCompound)
            {
                b |= 0x20UL;
            }

            if (t.tag >= 31L)
            {
                b |= 0x1fUL;
                dst = append(dst, b);
                dst = appendBase128Int(dst, int64(t.tag));
            }
            else
            {
                b |= uint8(t.tag);
                dst = append(dst, b);
            }

            if (t.length >= 128L)
            {
                var l = lengthLength(t.length);
                dst = append(dst, 0x80UL | byte(l));
                dst = appendLength(dst, t.length);
            }
            else
            {
                dst = append(dst, byte(t.length));
            }

            return dst;

        }

        private partial struct bitStringEncoder // : BitString
        {
        }

        private static long Len(this bitStringEncoder b)
        {
            return len(b.Bytes) + 1L;
        }

        private static void Encode(this bitStringEncoder b, slice<byte> dst) => func((_, panic, __) =>
        {
            dst[0L] = byte((8L - b.BitLength % 8L) % 8L);
            if (copy(dst[1L..], b.Bytes) != len(b.Bytes))
            {
                panic("internal error");
            }

        });

        private partial struct oidEncoder // : slice<long>
        {
        }

        private static long Len(this oidEncoder oid)
        {
            var l = base128IntLength(int64(oid[0L] * 40L + oid[1L]));
            for (long i = 2L; i < len(oid); i++)
            {
                l += base128IntLength(int64(oid[i]));
            }

            return l;

        }

        private static void Encode(this oidEncoder oid, slice<byte> dst)
        {
            dst = appendBase128Int(dst[..0L], int64(oid[0L] * 40L + oid[1L]));
            for (long i = 2L; i < len(oid); i++)
            {
                dst = appendBase128Int(dst, int64(oid[i]));
            }


        }

        private static (encoder, error) makeObjectIdentifier(slice<long> oid)
        {
            encoder e = default;
            error err = default!;

            if (len(oid) < 2L || oid[0L] > 2L || (oid[0L] < 2L && oid[1L] >= 40L))
            {
                return (null, error.As(new StructuralError("invalid object identifier"))!);
            }

            return (oidEncoder(oid), error.As(null!)!);

        }

        private static (encoder, error) makePrintableString(@string s)
        {
            encoder e = default;
            error err = default!;

            for (long i = 0L; i < len(s); i++)
            { 
                // The asterisk is often used in PrintableString, even though
                // it is invalid. If a PrintableString was specifically
                // requested then the asterisk is permitted by this code.
                // Ampersand is allowed in parsing due a handful of CA
                // certificates, however when making new certificates
                // it is rejected.
                if (!isPrintable(s[i], allowAsterisk, rejectAmpersand))
                {
                    return (null, error.As(new StructuralError("PrintableString contains invalid character"))!);
                }

            }


            return (stringEncoder(s), error.As(null!)!);

        }

        private static (encoder, error) makeIA5String(@string s)
        {
            encoder e = default;
            error err = default!;

            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] > 127L)
                {
                    return (null, error.As(new StructuralError("IA5String contains invalid character"))!);
                }

            }


            return (stringEncoder(s), error.As(null!)!);

        }

        private static (encoder, error) makeNumericString(@string s)
        {
            encoder e = default;
            error err = default!;

            for (long i = 0L; i < len(s); i++)
            {
                if (!isNumeric(s[i]))
                {
                    return (null, error.As(new StructuralError("NumericString contains invalid character"))!);
                }

            }


            return (stringEncoder(s), error.As(null!)!);

        }

        private static encoder makeUTF8String(@string s)
        {
            return stringEncoder(s);
        }

        private static slice<byte> appendTwoDigits(slice<byte> dst, long v)
        {
            return append(dst, byte('0' + (v / 10L) % 10L), byte('0' + v % 10L));
        }

        private static slice<byte> appendFourDigits(slice<byte> dst, long v)
        {
            array<byte> bytes = new array<byte>(4L);
            foreach (var (i) in bytes)
            {
                bytes[3L - i] = '0' + byte(v % 10L);
                v /= 10L;
            }
            return append(dst, bytes[..]);

        }

        private static bool outsideUTCRange(time.Time t)
        {
            var year = t.Year();
            return year < 1950L || year >= 2050L;
        }

        private static (encoder, error) makeUTCTime(time.Time t)
        {
            encoder e = default;
            error err = default!;

            var dst = make_slice<byte>(0L, 18L);

            dst, err = appendUTCTime(dst, t);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (bytesEncoder(dst), error.As(null!)!);

        }

        private static (encoder, error) makeGeneralizedTime(time.Time t)
        {
            encoder e = default;
            error err = default!;

            var dst = make_slice<byte>(0L, 20L);

            dst, err = appendGeneralizedTime(dst, t);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (bytesEncoder(dst), error.As(null!)!);

        }

        private static (slice<byte>, error) appendUTCTime(slice<byte> dst, time.Time t)
        {
            slice<byte> ret = default;
            error err = default!;

            var year = t.Year();


            if (1950L <= year && year < 2000L) 
                dst = appendTwoDigits(dst, year - 1900L);
            else if (2000L <= year && year < 2050L) 
                dst = appendTwoDigits(dst, year - 2000L);
            else 
                return (null, error.As(new StructuralError("cannot represent time as UTCTime"))!);
                        return (appendTimeCommon(dst, t), error.As(null!)!);

        }

        private static (slice<byte>, error) appendGeneralizedTime(slice<byte> dst, time.Time t)
        {
            slice<byte> ret = default;
            error err = default!;

            var year = t.Year();
            if (year < 0L || year > 9999L)
            {
                return (null, error.As(new StructuralError("cannot represent time as GeneralizedTime"))!);
            }

            dst = appendFourDigits(dst, year);

            return (appendTimeCommon(dst, t), error.As(null!)!);

        }

        private static slice<byte> appendTimeCommon(slice<byte> dst, time.Time t)
        {
            var (_, month, day) = t.Date();

            dst = appendTwoDigits(dst, int(month));
            dst = appendTwoDigits(dst, day);

            var (hour, min, sec) = t.Clock();

            dst = appendTwoDigits(dst, hour);
            dst = appendTwoDigits(dst, min);
            dst = appendTwoDigits(dst, sec);

            var (_, offset) = t.Zone();


            if (offset / 60L == 0L) 
                return append(dst, 'Z');
            else if (offset > 0L) 
                dst = append(dst, '+');
            else if (offset < 0L) 
                dst = append(dst, '-');
                        var offsetMinutes = offset / 60L;
            if (offsetMinutes < 0L)
            {
                offsetMinutes = -offsetMinutes;
            }

            dst = appendTwoDigits(dst, offsetMinutes / 60L);
            dst = appendTwoDigits(dst, offsetMinutes % 60L);

            return dst;

        }

        private static slice<byte> stripTagAndLength(slice<byte> @in)
        {
            var (_, offset, err) = parseTagAndLength(in, 0L);
            if (err != null)
            {
                return in;
            }

            return in[offset..];

        }

        private static (encoder, error) makeBody(reflect.Value value, fieldParameters @params)
        {
            encoder e = default;
            error err = default!;


            if (value.Type() == flagType) 
                return (bytesEncoder(null), error.As(null!)!);
            else if (value.Type() == timeType) 
                time.Time t = value.Interface()._<time.Time>();
                if (@params.timeType == TagGeneralizedTime || outsideUTCRange(t))
                {
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
                    if (v.Bool())
                    {
                        return (byteFFEncoder, error.As(null!)!);
                    }

                    return (byte00Encoder, error.As(null!)!);
                else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                    return (int64Encoder(v.Int()), error.As(null!)!);
                else if (v.Kind() == reflect.Struct) 
                    t = v.Type();

                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < t.NumField(); i++)
                        {
                            if (t.Field(i).PkgPath != "")
                            {
                                return (null, error.As(new StructuralError("struct contains unexported fields"))!);
                            }

                        }


                        i = i__prev1;
                    }

                    long startingField = 0L;

                    var n = t.NumField();
                    if (n == 0L)
                    {
                        return (bytesEncoder(null), error.As(null!)!);
                    } 

                    // If the first element of the structure is a non-empty
                    // RawContents, then we don't bother serializing the rest.
                    if (t.Field(0L).Type == rawContentsType)
                    {
                        var s = v.Field(0L);
                        if (s.Len() > 0L)
                        {
                            var bytes = s.Bytes();
                            /* The RawContents will contain the tag and
                                             * length fields but we'll also be writing
                                             * those ourselves, so we strip them out of
                                             * bytes */
                            return (bytesEncoder(stripTagAndLength(bytes)), error.As(null!)!);

                        }

                        startingField = 1L;

                    }

                    {
                        var n1 = n - startingField;

                        switch (n1)
                        {
                            case 0L: 
                                return (bytesEncoder(null), error.As(null!)!);
                                break;
                            case 1L: 
                                return makeField(v.Field(startingField), parseFieldParameters(t.Field(startingField).Tag.Get("asn1")));
                                break;
                            default: 
                                var m = make_slice<encoder>(n1);
                                {
                                    long i__prev1 = i;

                                    for (i = 0L; i < n1; i++)
                                    {
                                        m[i], err = makeField(v.Field(i + startingField), parseFieldParameters(t.Field(i + startingField).Tag.Get("asn1")));
                                        if (err != null)
                                        {
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
                    if (sliceType.Elem().Kind() == reflect.Uint8)
                    {
                        return (bytesEncoder(v.Bytes()), error.As(null!)!);
                    }

                    fieldParameters fp = default;

                    {
                        var l = v.Len();

                        switch (l)
                        {
                            case 0L: 
                                return (bytesEncoder(null), error.As(null!)!);
                                break;
                            case 1L: 
                                return makeField(v.Index(0L), fp);
                                break;
                            default: 
                                m = make_slice<encoder>(l);

                                {
                                    long i__prev1 = i;

                                    for (i = 0L; i < l; i++)
                                    {
                                        m[i], err = makeField(v.Index(i), fp);
                                        if (err != null)
                                        {
                                            return (null, error.As(err)!);
                                        }

                                    }


                                    i = i__prev1;
                                }

                                if (@params.set)
                                {
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

        private static (encoder, error) makeField(reflect.Value v, fieldParameters @params)
        {
            encoder e = default;
            error err = default!;

            if (!v.IsValid())
            {
                return (null, error.As(fmt.Errorf("asn1: cannot marshal nil value"))!);
            } 
            // If the field is an interface{} then recurse into it.
            if (v.Kind() == reflect.Interface && v.Type().NumMethod() == 0L)
            {
                return makeField(v.Elem(), params);
            }

            if (v.Kind() == reflect.Slice && v.Len() == 0L && @params.omitEmpty)
            {
                return (bytesEncoder(null), error.As(null!)!);
            }

            if (@params.optional && @params.defaultValue != null && canHaveDefaultValue(v.Kind()))
            {
                var defaultValue = reflect.New(v.Type()).Elem();
                defaultValue.SetInt(@params.defaultValue.val);

                if (reflect.DeepEqual(v.Interface(), defaultValue.Interface()))
                {
                    return (bytesEncoder(null), error.As(null!)!);
                }

            } 

            // If no default value is given then the zero value for the type is
            // assumed to be the default value. This isn't obviously the correct
            // behavior, but it's what Go has traditionally done.
            if (@params.optional && @params.defaultValue == null)
            {
                if (reflect.DeepEqual(v.Interface(), reflect.Zero(v.Type()).Interface()))
                {
                    return (bytesEncoder(null), error.As(null!)!);
                }

            }

            if (v.Type() == rawValueType)
            {
                RawValue rv = v.Interface()._<RawValue>();
                if (len(rv.FullBytes) != 0L)
                {
                    return (bytesEncoder(rv.FullBytes), error.As(null!)!);
                }

                ptr<taggedEncoder> t = @new<taggedEncoder>();

                t.tag = bytesEncoder(appendTagAndLength(t.scratch[..0L], new tagAndLength(rv.Class,rv.Tag,len(rv.Bytes),rv.IsCompound)));
                t.body = bytesEncoder(rv.Bytes);

                return (t, error.As(null!)!);

            }

            var (matchAny, tag, isCompound, ok) = getUniversalType(v.Type());
            if (!ok || matchAny)
            {
                return (null, error.As(new StructuralError(fmt.Sprintf("unknown Go type: %v",v.Type())))!);
            }

            if (@params.timeType != 0L && tag != TagUTCTime)
            {
                return (null, error.As(new StructuralError("explicit time type given to non-time member"))!);
            }

            if (@params.stringType != 0L && tag != TagPrintableString)
            {
                return (null, error.As(new StructuralError("explicit string type given to non-string member"))!);
            }


            if (tag == TagPrintableString) 
                if (@params.stringType == 0L)
                { 
                    // This is a string without an explicit string type. We'll use
                    // a PrintableString if the character set in the string is
                    // sufficiently limited, otherwise we'll use a UTF8String.
                    foreach (var (_, r) in v.String())
                    {
                        if (r >= utf8.RuneSelf || !isPrintable(byte(r), rejectAsterisk, rejectAmpersand))
                        {
                            if (!utf8.ValidString(v.String()))
                            {
                                return (null, error.As(errors.New("asn1: string not valid UTF-8"))!);
                            }

                            tag = TagUTF8String;
                            break;

                        }

                    }
                else
                }                {
                    tag = @params.stringType;
                }

            else if (tag == TagUTCTime) 
                if (@params.timeType == TagGeneralizedTime || outsideUTCRange(v.Interface()._<time.Time>()))
                {
                    tag = TagGeneralizedTime;
                }

                        if (@params.set)
            {
                if (tag != TagSequence)
                {
                    return (null, error.As(new StructuralError("non sequence tagged as set"))!);
                }

                tag = TagSet;

            } 

            // makeField can be called for a slice that should be treated as a SET
            // but doesn't have params.set set, for instance when using a slice
            // with the SET type name suffix. In this case getUniversalType returns
            // TagSet, but makeBody doesn't know about that so will treat the slice
            // as a sequence. To work around this we set params.set.
            if (tag == TagSet && !@params.set)
            {
                @params.set = true;
            }

            t = @new<taggedEncoder>();

            t.body, err = makeBody(v, params);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var bodyLen = t.body.Len();

            var @class = ClassUniversal;
            if (@params.tag != null)
            {
                if (@params.application)
                {
                    class = ClassApplication;
                }
                else if (@params.@private)
                {
                    class = ClassPrivate;
                }
                else
                {
                    class = ClassContextSpecific;
                }

                if (@params.@explicit)
                {
                    t.tag = bytesEncoder(appendTagAndLength(t.scratch[..0L], new tagAndLength(ClassUniversal,tag,bodyLen,isCompound)));

                    ptr<taggedEncoder> tt = @new<taggedEncoder>();

                    tt.body = t;

                    tt.tag = bytesEncoder(appendTagAndLength(tt.scratch[..0L], new tagAndLength(class:class,tag:*params.tag,length:bodyLen+t.tag.Len(),isCompound:true,)));

                    return (tt, error.As(null!)!);
                } 

                // implicit tag.
                tag = @params.tag.val;

            }

            t.tag = bytesEncoder(appendTagAndLength(t.scratch[..0L], new tagAndLength(class,tag,bodyLen,isCompound)));

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
        public static (slice<byte>, error) Marshal(object val)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            return MarshalWithParams(val, "");
        }

        // MarshalWithParams allows field parameters to be specified for the
        // top-level element. The form of the params is the same as the field tags.
        public static (slice<byte>, error) MarshalWithParams(object val, @string @params)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (e, err) = makeField(reflect.ValueOf(val), parseFieldParameters(params));
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var b = make_slice<byte>(e.Len());
            e.Encode(b);
            return (b, error.As(null!)!);

        }
    }
}}
