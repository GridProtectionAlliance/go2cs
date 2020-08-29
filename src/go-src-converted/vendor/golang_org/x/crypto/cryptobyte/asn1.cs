// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cryptobyte -- go2cs converted at 2020 August 29 10:11:19 UTC
// import "vendor/golang_org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang_org.x.crypto.cryptobyte_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\cryptobyte\asn1.go
using encoding_asn1 = go.encoding.asn1_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using time = go.time_package;

using asn1 = go.golang_org.x.crypto.cryptobyte.asn1_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class cryptobyte_package
    {
        // This file contains ASN.1-related methods for String and Builder.

        // Builder

        // AddASN1Int64 appends a DER-encoded ASN.1 INTEGER.
        private static void AddASN1Int64(this ref Builder b, long v)
        {
            b.addASN1Signed(asn1.INTEGER, v);
        }

        // AddASN1Enum appends a DER-encoded ASN.1 ENUMERATION.
        private static void AddASN1Enum(this ref Builder b, long v)
        {
            b.addASN1Signed(asn1.ENUM, v);
        }

        private static void addASN1Signed(this ref Builder b, asn1.Tag tag, long v)
        {
            b.AddASN1(tag, c =>
            {
                long length = 1L;
                {
                    var i__prev1 = i;

                    var i = v;

                    while (i >= 0x80UL || i < -0x80UL)
                    {
                        length++;
                        i >>= 8L;
                    }


                    i = i__prev1;
                }

                while (length > 0L)
                {
                    i = v >> (int)(uint((length - 1L) * 8L)) & 0xffUL;
                    c.AddUint8(uint8(i));
                    length--;
                }

            });
        }

        // AddASN1Uint64 appends a DER-encoded ASN.1 INTEGER.
        private static void AddASN1Uint64(this ref Builder b, ulong v)
        {
            b.AddASN1(asn1.INTEGER, c =>
            {
                long length = 1L;
                {
                    var i__prev1 = i;

                    var i = v;

                    while (i >= 0x80UL)
                    {
                        length++;
                        i >>= 8L;
                    }


                    i = i__prev1;
                }

                while (length > 0L)
                {
                    i = v >> (int)(uint((length - 1L) * 8L)) & 0xffUL;
                    c.AddUint8(uint8(i));
                    length--;
                }

            });
        }

        // AddASN1BigInt appends a DER-encoded ASN.1 INTEGER.
        private static void AddASN1BigInt(this ref Builder b, ref big.Int n)
        {
            if (b.err != null)
            {
                return;
            }
            b.AddASN1(asn1.INTEGER, c =>
            {
                if (n.Sign() < 0L)
                { 
                    // A negative number has to be converted to two's-complement form. So we
                    // invert and subtract 1. If the most-significant-bit isn't set then
                    // we'll need to pad the beginning with 0xff in order to keep the number
                    // negative.
                    ptr<big.Int> nMinus1 = @new<big.Int>().Neg(n);
                    nMinus1.Sub(nMinus1, bigOne);
                    var bytes = nMinus1.Bytes();
                    foreach (var (i) in bytes)
                    {
                        bytes[i] ^= 0xffUL;
                    }
                    if (bytes[0L] & 0x80UL == 0L)
                    {
                        c.add(0xffUL);
                    }
                    c.add(bytes);
                }
                else if (n.Sign() == 0L)
                {
                    c.add(0L);
                }
                else
                {
                    bytes = n.Bytes();
                    if (bytes[0L] & 0x80UL != 0L)
                    {
                        c.add(0L);
                    }
                    c.add(bytes);
                }
            });
        }

        // AddASN1OctetString appends a DER-encoded ASN.1 OCTET STRING.
        private static void AddASN1OctetString(this ref Builder b, slice<byte> bytes)
        {
            b.AddASN1(asn1.OCTET_STRING, c =>
            {
                c.AddBytes(bytes);
            });
        }

        private static readonly @string generalizedTimeFormatStr = "20060102150405Z0700";

        // AddASN1GeneralizedTime appends a DER-encoded ASN.1 GENERALIZEDTIME.


        // AddASN1GeneralizedTime appends a DER-encoded ASN.1 GENERALIZEDTIME.
        private static void AddASN1GeneralizedTime(this ref Builder b, time.Time t)
        {
            if (t.Year() < 0L || t.Year() > 9999L)
            {
                b.err = fmt.Errorf("cryptobyte: cannot represent %v as a GeneralizedTime", t);
                return;
            }
            b.AddASN1(asn1.GeneralizedTime, c =>
            {
                c.AddBytes((slice<byte>)t.Format(generalizedTimeFormatStr));
            });
        }

        // AddASN1BitString appends a DER-encoded ASN.1 BIT STRING. This does not
        // support BIT STRINGs that are not a whole number of bytes.
        private static void AddASN1BitString(this ref Builder b, slice<byte> data)
        {
            b.AddASN1(asn1.BIT_STRING, b =>
            {
                b.AddUint8(0L);
                b.AddBytes(data);
            });
        }

        private static void addBase128Int(this ref Builder b, long n)
        {
            long length = default;
            if (n == 0L)
            {
                length = 1L;
            }
            else
            {
                {
                    var i__prev1 = i;

                    var i = n;

                    while (i > 0L)
                    {
                        length++;
                        i >>= 7L;
                    }


                    i = i__prev1;
                }
            }
            {
                var i__prev1 = i;

                for (i = length - 1L; i >= 0L; i--)
                {
                    var o = byte(n >> (int)(uint(i * 7L)));
                    o &= 0x7fUL;
                    if (i != 0L)
                    {
                        o |= 0x80UL;
                    }
                    b.add(o);
                }


                i = i__prev1;
            }
        }

        private static bool isValidOID(encoding_asn1.ObjectIdentifier oid)
        {
            if (len(oid) < 2L)
            {
                return false;
            }
            if (oid[0L] > 2L || (oid[0L] <= 1L && oid[1L] >= 40L))
            {
                return false;
            }
            foreach (var (_, v) in oid)
            {
                if (v < 0L)
                {
                    return false;
                }
            }
            return true;
        }

        private static void AddASN1ObjectIdentifier(this ref Builder b, encoding_asn1.ObjectIdentifier oid)
        {
            b.AddASN1(asn1.OBJECT_IDENTIFIER, b =>
            {
                if (!isValidOID(oid))
                {
                    b.err = fmt.Errorf("cryptobyte: invalid OID: %v", oid);
                    return;
                }
                b.addBase128Int(int64(oid[0L]) * 40L + int64(oid[1L]));
                foreach (var (_, v) in oid[2L..])
                {
                    b.addBase128Int(int64(v));
                }
            });
        }

        private static void AddASN1Boolean(this ref Builder b, bool v)
        {
            b.AddASN1(asn1.BOOLEAN, b =>
            {
                if (v)
                {
                    b.AddUint8(0xffUL);
                }
                else
                {
                    b.AddUint8(0L);
                }
            });
        }

        private static void AddASN1NULL(this ref Builder b)
        {
            b.add(uint8(asn1.NULL), 0L);
        }

        // MarshalASN1 calls encoding_asn1.Marshal on its input and appends the result if
        // successful or records an error if one occurred.
        private static void MarshalASN1(this ref Builder b, object v)
        { 
            // NOTE(martinkr): This is somewhat of a hack to allow propagation of
            // encoding_asn1.Marshal errors into Builder.err. N.B. if you call MarshalASN1 with a
            // value embedded into a struct, its tag information is lost.
            if (b.err != null)
            {
                return;
            }
            var (bytes, err) = encoding_asn1.Marshal(v);
            if (err != null)
            {
                b.err = err;
                return;
            }
            b.AddBytes(bytes);
        }

        // AddASN1 appends an ASN.1 object. The object is prefixed with the given tag.
        // Tags greater than 30 are not supported and result in an error (i.e.
        // low-tag-number form only). The child builder passed to the
        // BuilderContinuation can be used to build the content of the ASN.1 object.
        private static void AddASN1(this ref Builder b, asn1.Tag tag, BuilderContinuation f)
        {
            if (b.err != null)
            {
                return;
            } 
            // Identifiers with the low five bits set indicate high-tag-number format
            // (two or more octets), which we don't support.
            if (tag & 0x1fUL == 0x1fUL)
            {
                b.err = fmt.Errorf("cryptobyte: high-tag number identifier octects not supported: 0x%x", tag);
                return;
            }
            b.AddUint8(uint8(tag));
            b.addLengthPrefixed(1L, true, f);
        }

        // String

        private static bool ReadASN1Boolean(this ref String s, ref bool @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.INTEGER) || len(bytes) != 1L)
            {
                return false;
            }
            switch (bytes[0L])
            {
                case 0L: 
                    out.Value = false;
                    break;
                case 0xffUL: 
                    out.Value = true;
                    break;
                default: 
                    return false;
                    break;
            }

            return true;
        }

        private static var bigIntType = reflect.TypeOf((big.Int.Value)(null)).Elem();

        // ReadASN1Integer decodes an ASN.1 INTEGER into out and advances. If out does
        // not point to an integer or to a big.Int, it panics. It returns true on
        // success and false on error.
        private static bool ReadASN1Integer(this ref String _s, object @out) => func(_s, (ref String s, Defer _, Panic panic, Recover __) =>
        {
            if (reflect.TypeOf(out).Kind() != reflect.Ptr)
            {
                panic("out is not a pointer");
            }

            if (reflect.ValueOf(out).Elem().Kind() == reflect.Int || reflect.ValueOf(out).Elem().Kind() == reflect.Int8 || reflect.ValueOf(out).Elem().Kind() == reflect.Int16 || reflect.ValueOf(out).Elem().Kind() == reflect.Int32 || reflect.ValueOf(out).Elem().Kind() == reflect.Int64) 
                long i = default;
                if (!s.readASN1Int64(ref i) || reflect.ValueOf(out).Elem().OverflowInt(i))
                {
                    return false;
                }
                reflect.ValueOf(out).Elem().SetInt(i);
                return true;
            else if (reflect.ValueOf(out).Elem().Kind() == reflect.Uint || reflect.ValueOf(out).Elem().Kind() == reflect.Uint8 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint16 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint32 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint64) 
                ulong u = default;
                if (!s.readASN1Uint64(ref u) || reflect.ValueOf(out).Elem().OverflowUint(u))
                {
                    return false;
                }
                reflect.ValueOf(out).Elem().SetUint(u);
                return true;
            else if (reflect.ValueOf(out).Elem().Kind() == reflect.Struct) 
                if (reflect.TypeOf(out).Elem() == bigIntType)
                {
                    return s.readASN1BigInt(out._<ref big.Int>());
                }
                        panic("out does not point to an integer type");
        });

        private static bool checkASN1Integer(slice<byte> bytes)
        {
            if (len(bytes) == 0L)
            { 
                // An INTEGER is encoded with at least one octet.
                return false;
            }
            if (len(bytes) == 1L)
            {
                return true;
            }
            if (bytes[0L] == 0L && bytes[1L] & 0x80UL == 0L || bytes[0L] == 0xffUL && bytes[1L] & 0x80UL == 0x80UL)
            { 
                // Value is not minimally encoded.
                return false;
            }
            return true;
        }

        private static var bigOne = big.NewInt(1L);

        private static bool readASN1BigInt(this ref String s, ref big.Int @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.INTEGER) || !checkASN1Integer(bytes))
            {
                return false;
            }
            if (bytes[0L] & 0x80UL == 0x80UL)
            { 
                // Negative number.
                var neg = make_slice<byte>(len(bytes));
                foreach (var (i, b) in bytes)
                {
                    neg[i] = ~b;
                }
            else
                @out.SetBytes(neg);
                @out.Add(out, bigOne);
                @out.Neg(out);
            }            {
                @out.SetBytes(bytes);
            }
            return true;
        }

        private static bool readASN1Int64(this ref String s, ref long @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Signed(out, bytes))
            {
                return false;
            }
            return true;
        }

        private static bool asn1Signed(ref long @out, slice<byte> n)
        {
            var length = len(n);
            if (length > 8L)
            {
                return false;
            }
            for (long i = 0L; i < length; i++)
            {
                out.Value <<= 8L;
                out.Value |= int64(n[i]);
            } 
            // Shift up and down in order to sign extend the result.
 
            // Shift up and down in order to sign extend the result.
            out.Value <<= 64L - uint8(length) * 8L;
            out.Value >>= 64L - uint8(length) * 8L;
            return true;
        }

        private static bool readASN1Uint64(this ref String s, ref ulong @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Unsigned(out, bytes))
            {
                return false;
            }
            return true;
        }

        private static bool asn1Unsigned(ref ulong @out, slice<byte> n)
        {
            var length = len(n);
            if (length > 9L || length == 9L && n[0L] != 0L)
            { 
                // Too large for uint64.
                return false;
            }
            if (n[0L] & 0x80UL != 0L)
            { 
                // Negative number.
                return false;
            }
            for (long i = 0L; i < length; i++)
            {
                out.Value <<= 8L;
                out.Value |= uint64(n[i]);
            }

            return true;
        }

        // ReadASN1Enum decodes an ASN.1 ENUMERATION into out and advances. It returns
        // true on success and false on error.
        private static bool ReadASN1Enum(this ref String s, ref long @out)
        {
            String bytes = default;
            long i = default;
            if (!s.ReadASN1(ref bytes, asn1.ENUM) || !checkASN1Integer(bytes) || !asn1Signed(ref i, bytes))
            {
                return false;
            }
            if (int64(int(i)) != i)
            {
                return false;
            }
            out.Value = int(i);
            return true;
        }

        private static bool readBase128Int(this ref String s, ref long @out)
        {
            long ret = 0L;
            for (long i = 0L; len(s.Value) > 0L; i++)
            {
                if (i == 4L)
                {
                    return false;
                }
                ret <<= 7L;
                var b = s.read(1L)[0L];
                ret |= int(b & 0x7fUL);
                if (b & 0x80UL == 0L)
                {
                    out.Value = ret;
                    return true;
                }
            }

            return false; // truncated
        }

        // ReadASN1ObjectIdentifier decodes an ASN.1 OBJECT IDENTIFIER into out and
        // advances. It returns true on success and false on error.
        private static bool ReadASN1ObjectIdentifier(this ref String s, ref encoding_asn1.ObjectIdentifier @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.OBJECT_IDENTIFIER) || len(bytes) == 0L)
            {
                return false;
            } 

            // In the worst case, we get two elements from the first byte (which is
            // encoded differently) and then every varint is a single byte long.
            var components = make_slice<long>(len(bytes) + 1L); 

            // The first varint is 40*value1 + value2:
            // According to this packing, value1 can take the values 0, 1 and 2 only.
            // When value1 = 0 or value1 = 1, then value2 is <= 39. When value1 = 2,
            // then there are no restrictions on value2.
            long v = default;
            if (!bytes.readBase128Int(ref v))
            {
                return false;
            }
            if (v < 80L)
            {
                components[0L] = v / 40L;
                components[1L] = v % 40L;
            }
            else
            {
                components[0L] = 2L;
                components[1L] = v - 80L;
            }
            long i = 2L;
            while (len(bytes) > 0L)
            {
                if (!bytes.readBase128Int(ref v))
                {
                    return false;
                i++;
                }
                components[i] = v;
            }

            out.Value = components[..i];
            return true;
        }

        // ReadASN1GeneralizedTime decodes an ASN.1 GENERALIZEDTIME into out and
        // advances. It returns true on success and false on error.
        private static bool ReadASN1GeneralizedTime(this ref String s, ref time.Time @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.GeneralizedTime))
            {
                return false;
            }
            var t = string(bytes);
            var (res, err) = time.Parse(generalizedTimeFormatStr, t);
            if (err != null)
            {
                return false;
            }
            {
                var serialized = res.Format(generalizedTimeFormatStr);

                if (serialized != t)
                {
                    return false;
                }

            }
            out.Value = res;
            return true;
        }

        // ReadASN1BitString decodes an ASN.1 BIT STRING into out and advances. It
        // returns true on success and false on error.
        private static bool ReadASN1BitString(this ref String s, ref encoding_asn1.BitString @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.BIT_STRING) || len(bytes) == 0L)
            {
                return false;
            }
            var paddingBits = uint8(bytes[0L]);
            bytes = bytes[1L..];
            if (paddingBits > 7L || len(bytes) == 0L && paddingBits != 0L || len(bytes) > 0L && bytes[len(bytes) - 1L] & (1L << (int)(paddingBits) - 1L) != 0L)
            {
                return false;
            }
            @out.BitLength = len(bytes) * 8L - int(paddingBits);
            @out.Bytes = bytes;
            return true;
        }

        // ReadASN1BitString decodes an ASN.1 BIT STRING into out and advances. It is
        // an error if the BIT STRING is not a whole number of bytes. This function
        // returns true on success and false on error.
        private static bool ReadASN1BitStringAsBytes(this ref String s, ref slice<byte> @out)
        {
            String bytes = default;
            if (!s.ReadASN1(ref bytes, asn1.BIT_STRING) || len(bytes) == 0L)
            {
                return false;
            }
            var paddingBits = uint8(bytes[0L]);
            if (paddingBits != 0L)
            {
                return false;
            }
            out.Value = bytes[1L..];
            return true;
        }

        // ReadASN1Bytes reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It returns true on success and false on error.
        private static bool ReadASN1Bytes(this ref String s, ref slice<byte> @out, asn1.Tag tag)
        {
            return s.ReadASN1((String.Value)(out), tag);
        }

        // ReadASN1 reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It returns true on success and false on error.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadASN1(this ref String s, ref String @out, asn1.Tag tag)
        {
            asn1.Tag t = default;
            if (!s.ReadAnyASN1(out, ref t) || t != tag)
            {
                return false;
            }
            return true;
        }

        // ReadASN1Element reads the contents of a DER-encoded ASN.1 element (including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It returns true on success and false on error.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadASN1Element(this ref String s, ref String @out, asn1.Tag tag)
        {
            asn1.Tag t = default;
            if (!s.ReadAnyASN1Element(out, ref t) || t != tag)
            {
                return false;
            }
            return true;
        }

        // ReadAnyASN1 reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, sets outTag to its tag, and advances. It
        // returns true on success and false on error.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadAnyASN1(this ref String s, ref String @out, ref asn1.Tag outTag)
        {
            return s.readASN1(out, outTag, true);
        }

        // ReadAnyASN1Element reads the contents of a DER-encoded ASN.1 element
        // (including tag and length bytes) into out, sets outTag to is tag, and
        // advances. It returns true on success and false on error.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadAnyASN1Element(this ref String s, ref String @out, ref asn1.Tag outTag)
        {
            return s.readASN1(out, outTag, false);
        }

        // PeekASN1Tag returns true if the next ASN.1 value on the string starts with
        // the given tag.
        public static bool PeekASN1Tag(this String s, asn1.Tag tag)
        {
            if (len(s) == 0L)
            {
                return false;
            }
            return asn1.Tag(s[0L]) == tag;
        }

        // SkipASN1 reads and discards an ASN.1 element with the given tag.
        private static bool SkipASN1(this ref String s, asn1.Tag tag)
        {
            String unused = default;
            return s.ReadASN1(ref unused, tag);
        }

        // ReadOptionalASN1 attempts to read the contents of a DER-encoded ASN.1
        // element (not including tag and length bytes) tagged with the given tag into
        // out. It stores whether an element with the tag was found in outPresent,
        // unless outPresent is nil. It returns true on success and false on error.
        private static bool ReadOptionalASN1(this ref String s, ref String @out, ref bool outPresent, asn1.Tag tag)
        {
            var present = s.PeekASN1Tag(tag);
            if (outPresent != null)
            {
                outPresent.Value = present;
            }
            if (present && !s.ReadASN1(out, tag))
            {
                return false;
            }
            return true;
        }

        // SkipOptionalASN1 advances s over an ASN.1 element with the given tag, or
        // else leaves s unchanged.
        private static bool SkipOptionalASN1(this ref String s, asn1.Tag tag)
        {
            if (!s.PeekASN1Tag(tag))
            {
                return true;
            }
            String unused = default;
            return s.ReadASN1(ref unused, tag);
        }

        // ReadOptionalASN1Integer attempts to read an optional ASN.1 INTEGER
        // explicitly tagged with tag into out and advances. If no element with a
        // matching tag is present, it writes defaultValue into out instead. If out
        // does not point to an integer or to a big.Int, it panics. It returns true on
        // success and false on error.
        private static bool ReadOptionalASN1Integer(this ref String _s, object @out, asn1.Tag tag, object defaultValue) => func(_s, (ref String s, Defer _, Panic panic, Recover __) =>
        {
            if (reflect.TypeOf(out).Kind() != reflect.Ptr)
            {
                panic("out is not a pointer");
            }
            bool present = default;
            String i = default;
            if (!s.ReadOptionalASN1(ref i, ref present, tag))
            {
                return false;
            }
            if (!present)
            {

                if (reflect.ValueOf(out).Elem().Kind() == reflect.Int || reflect.ValueOf(out).Elem().Kind() == reflect.Int8 || reflect.ValueOf(out).Elem().Kind() == reflect.Int16 || reflect.ValueOf(out).Elem().Kind() == reflect.Int32 || reflect.ValueOf(out).Elem().Kind() == reflect.Int64 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint || reflect.ValueOf(out).Elem().Kind() == reflect.Uint8 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint16 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint32 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint64) 
                    reflect.ValueOf(out).Elem().Set(reflect.ValueOf(defaultValue));
                else if (reflect.ValueOf(out).Elem().Kind() == reflect.Struct) 
                    if (reflect.TypeOf(out).Elem() != bigIntType)
                    {
                        panic("invalid integer type");
                    }
                    if (reflect.TypeOf(defaultValue).Kind() != reflect.Ptr || reflect.TypeOf(defaultValue).Elem() != bigIntType)
                    {
                        panic("out points to big.Int, but defaultValue does not");
                    }
                    out._<ref big.Int>().Set(defaultValue._<ref big.Int>());
                else 
                    panic("invalid integer type");
                                return true;
            }
            if (!i.ReadASN1Integer(out) || !i.Empty())
            {
                return false;
            }
            return true;
        });

        // ReadOptionalASN1OctetString attempts to read an optional ASN.1 OCTET STRING
        // explicitly tagged with tag into out and advances. If no element with a
        // matching tag is present, it writes defaultValue into out instead. It returns
        // true on success and false on error.
        private static bool ReadOptionalASN1OctetString(this ref String s, ref slice<byte> @out, ref bool outPresent, asn1.Tag tag)
        {
            bool present = default;
            String child = default;
            if (!s.ReadOptionalASN1(ref child, ref present, tag))
            {
                return false;
            }
            if (outPresent != null)
            {
                outPresent.Value = present;
            }
            if (present)
            {
                String oct = default;
                if (!child.ReadASN1(ref oct, asn1.OCTET_STRING) || !child.Empty())
                {
                    return false;
                }
                out.Value = oct;
            }
            else
            {
                out.Value = null;
            }
            return true;
        }

        // ReadOptionalASN1Boolean sets *out to the value of the next ASN.1 BOOLEAN or,
        // if the next bytes are not an ASN.1 BOOLEAN, to the value of defaultValue.
        private static bool ReadOptionalASN1Boolean(this ref String s, ref bool @out, bool defaultValue)
        {
            bool present = default;
            String child = default;
            if (!s.ReadOptionalASN1(ref child, ref present, asn1.BOOLEAN))
            {
                return false;
            }
            if (!present)
            {
                out.Value = defaultValue;
                return true;
            }
            return s.ReadASN1Boolean(out);
        }

        private static bool readASN1(this ref String _s, ref String _@out, ref asn1.Tag _outTag, bool skipHeader) => func(_s, _@out, _outTag, (ref String s, ref String @out, ref asn1.Tag outTag, Defer _, Panic panic, Recover __) =>
        {
            if (len(s.Value) < 2L)
            {
                return false;
            }
            var tag = (s.Value)[0L];
            var lenByte = (s.Value)[1L];

            if (tag & 0x1fUL == 0x1fUL)
            { 
                // ITU-T X.690 section 8.1.2
                //
                // An identifier octet with a tag part of 0x1f indicates a high-tag-number
                // form identifier with two or more octets. We only support tags less than
                // 31 (i.e. low-tag-number form, single octet identifier).
                return false;
            }
            if (outTag != null)
            {
                outTag.Value = asn1.Tag(tag);
            } 

            // ITU-T X.690 section 8.1.3
            //
            // Bit 8 of the first length byte indicates whether the length is short- or
            // long-form.
            uint length = default;            uint headerLen = default; // length includes headerLen
 // length includes headerLen
            if (lenByte & 0x80UL == 0L)
            { 
                // Short-form length (section 8.1.3.4), encoded in bits 1-7.
                length = uint32(lenByte) + 2L;
                headerLen = 2L;
            }
            else
            { 
                // Long-form length (section 8.1.3.5). Bits 1-7 encode the number of octets
                // used to encode the length.
                var lenLen = lenByte & 0x7fUL;
                uint len32 = default;

                if (lenLen == 0L || lenLen > 4L || len(s.Value) < int(2L + lenLen))
                {
                    return false;
                }
                var lenBytes = String((s.Value)[2L..2L + lenLen]);
                if (!lenBytes.readUnsigned(ref len32, int(lenLen)))
                {
                    return false;
                } 

                // ITU-T X.690 section 10.1 (DER length forms) requires encoding the length
                // with the minimum number of octets.
                if (len32 < 128L)
                { 
                    // Length should have used short-form encoding.
                    return false;
                }
                if (len32 >> (int)(((lenLen - 1L) * 8L)) == 0L)
                { 
                    // Leading octet is 0. Length should have been at least one byte shorter.
                    return false;
                }
                headerLen = 2L + uint32(lenLen);
                if (headerLen + len32 < len32)
                { 
                    // Overflow.
                    return false;
                }
                length = headerLen + len32;
            }
            if (uint32(int(length)) != length || !s.ReadBytes(new ptr<ref slice<byte>>(out), int(length)))
            {
                return false;
            }
            if (skipHeader && !@out.Skip(int(headerLen)))
            {
                panic("cryptobyte: internal error");
            }
            return true;
        });
    }
}}}}}
