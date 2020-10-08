// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cryptobyte -- go2cs converted at 2020 October 08 05:00:00 UTC
// import "vendor/golang.org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang.org.x.crypto.cryptobyte_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\cryptobyte\asn1.go
using encoding_asn1 = go.encoding.asn1_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using time = go.time_package;

using asn1 = go.golang.org.x.crypto.cryptobyte.asn1_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class cryptobyte_package
    {
        // This file contains ASN.1-related methods for String and Builder.

        // Builder

        // AddASN1Int64 appends a DER-encoded ASN.1 INTEGER.
        private static void AddASN1Int64(this ptr<Builder> _addr_b, long v)
        {
            ref Builder b = ref _addr_b.val;

            b.addASN1Signed(asn1.INTEGER, v);
        }

        // AddASN1Int64WithTag appends a DER-encoded ASN.1 INTEGER with the
        // given tag.
        private static void AddASN1Int64WithTag(this ptr<Builder> _addr_b, long v, asn1.Tag tag)
        {
            ref Builder b = ref _addr_b.val;

            b.addASN1Signed(tag, v);
        }

        // AddASN1Enum appends a DER-encoded ASN.1 ENUMERATION.
        private static void AddASN1Enum(this ptr<Builder> _addr_b, long v)
        {
            ref Builder b = ref _addr_b.val;

            b.addASN1Signed(asn1.ENUM, v);
        }

        private static void addASN1Signed(this ptr<Builder> _addr_b, asn1.Tag tag, long v)
        {
            ref Builder b = ref _addr_b.val;

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
        private static void AddASN1Uint64(this ptr<Builder> _addr_b, ulong v)
        {
            ref Builder b = ref _addr_b.val;

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
        private static void AddASN1BigInt(this ptr<Builder> _addr_b, ptr<big.Int> _addr_n)
        {
            ref Builder b = ref _addr_b.val;
            ref big.Int n = ref _addr_n.val;

            if (b.err != null)
            {
                return ;
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
                    if (len(bytes) == 0L || bytes[0L] & 0x80UL == 0L)
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
        private static void AddASN1OctetString(this ptr<Builder> _addr_b, slice<byte> bytes)
        {
            ref Builder b = ref _addr_b.val;

            b.AddASN1(asn1.OCTET_STRING, c =>
            {
                c.AddBytes(bytes);
            });

        }

        private static readonly @string generalizedTimeFormatStr = (@string)"20060102150405Z0700";

        // AddASN1GeneralizedTime appends a DER-encoded ASN.1 GENERALIZEDTIME.


        // AddASN1GeneralizedTime appends a DER-encoded ASN.1 GENERALIZEDTIME.
        private static void AddASN1GeneralizedTime(this ptr<Builder> _addr_b, time.Time t)
        {
            ref Builder b = ref _addr_b.val;

            if (t.Year() < 0L || t.Year() > 9999L)
            {
                b.err = fmt.Errorf("cryptobyte: cannot represent %v as a GeneralizedTime", t);
                return ;
            }

            b.AddASN1(asn1.GeneralizedTime, c =>
            {
                c.AddBytes((slice<byte>)t.Format(generalizedTimeFormatStr));
            });

        }

        // AddASN1BitString appends a DER-encoded ASN.1 BIT STRING. This does not
        // support BIT STRINGs that are not a whole number of bytes.
        private static void AddASN1BitString(this ptr<Builder> _addr_b, slice<byte> data)
        {
            ref Builder b = ref _addr_b.val;

            b.AddASN1(asn1.BIT_STRING, b =>
            {
                b.AddUint8(0L);
                b.AddBytes(data);
            });

        }

        private static void addBase128Int(this ptr<Builder> _addr_b, long n)
        {
            ref Builder b = ref _addr_b.val;

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

        private static void AddASN1ObjectIdentifier(this ptr<Builder> _addr_b, encoding_asn1.ObjectIdentifier oid)
        {
            ref Builder b = ref _addr_b.val;

            b.AddASN1(asn1.OBJECT_IDENTIFIER, b =>
            {
                if (!isValidOID(oid))
                {
                    b.err = fmt.Errorf("cryptobyte: invalid OID: %v", oid);
                    return ;
                }

                b.addBase128Int(int64(oid[0L]) * 40L + int64(oid[1L]));
                foreach (var (_, v) in oid[2L..])
                {
                    b.addBase128Int(int64(v));
                }

            });

        }

        private static void AddASN1Boolean(this ptr<Builder> _addr_b, bool v)
        {
            ref Builder b = ref _addr_b.val;

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

        private static void AddASN1NULL(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            b.add(uint8(asn1.NULL), 0L);
        }

        // MarshalASN1 calls encoding_asn1.Marshal on its input and appends the result if
        // successful or records an error if one occurred.
        private static void MarshalASN1(this ptr<Builder> _addr_b, object v)
        {
            ref Builder b = ref _addr_b.val;
 
            // NOTE(martinkr): This is somewhat of a hack to allow propagation of
            // encoding_asn1.Marshal errors into Builder.err. N.B. if you call MarshalASN1 with a
            // value embedded into a struct, its tag information is lost.
            if (b.err != null)
            {
                return ;
            }

            var (bytes, err) = encoding_asn1.Marshal(v);
            if (err != null)
            {
                b.err = err;
                return ;
            }

            b.AddBytes(bytes);

        }

        // AddASN1 appends an ASN.1 object. The object is prefixed with the given tag.
        // Tags greater than 30 are not supported and result in an error (i.e.
        // low-tag-number form only). The child builder passed to the
        // BuilderContinuation can be used to build the content of the ASN.1 object.
        private static void AddASN1(this ptr<Builder> _addr_b, asn1.Tag tag, BuilderContinuation f)
        {
            ref Builder b = ref _addr_b.val;

            if (b.err != null)
            {
                return ;
            } 
            // Identifiers with the low five bits set indicate high-tag-number format
            // (two or more octets), which we don't support.
            if (tag & 0x1fUL == 0x1fUL)
            {
                b.err = fmt.Errorf("cryptobyte: high-tag number identifier octects not supported: 0x%x", tag);
                return ;
            }

            b.AddUint8(uint8(tag));
            b.addLengthPrefixed(1L, true, f);

        }

        // String

        // ReadASN1Boolean decodes an ASN.1 BOOLEAN and converts it to a boolean
        // representation into out and advances. It reports whether the read
        // was successful.
        private static bool ReadASN1Boolean(this ptr<String> _addr_s, ptr<bool> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref bool @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.BOOLEAN) || len(bytes) != 1L)
            {
                return false;
            }

            switch (bytes[0L])
            {
                case 0L: 
                    out.val = false;
                    break;
                case 0xffUL: 
                    out.val = true;
                    break;
                default: 
                    return false;
                    break;
            }

            return true;

        }

        private static var bigIntType = reflect.TypeOf((big.Int.val)(null)).Elem();

        // ReadASN1Integer decodes an ASN.1 INTEGER into out and advances. If out does
        // not point to an integer or to a big.Int, it panics. It reports whether the
        // read was successful.
        private static bool ReadASN1Integer(this ptr<String> _addr_s, object @out) => func((_, panic, __) =>
        {
            ref String s = ref _addr_s.val;

            if (reflect.TypeOf(out).Kind() != reflect.Ptr)
            {
                panic("out is not a pointer");
            }


            if (reflect.ValueOf(out).Elem().Kind() == reflect.Int || reflect.ValueOf(out).Elem().Kind() == reflect.Int8 || reflect.ValueOf(out).Elem().Kind() == reflect.Int16 || reflect.ValueOf(out).Elem().Kind() == reflect.Int32 || reflect.ValueOf(out).Elem().Kind() == reflect.Int64) 
                ref long i = ref heap(out ptr<long> _addr_i);
                if (!s.readASN1Int64(_addr_i) || reflect.ValueOf(out).Elem().OverflowInt(i))
                {
                    return false;
                }

                reflect.ValueOf(out).Elem().SetInt(i);
                return true;
            else if (reflect.ValueOf(out).Elem().Kind() == reflect.Uint || reflect.ValueOf(out).Elem().Kind() == reflect.Uint8 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint16 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint32 || reflect.ValueOf(out).Elem().Kind() == reflect.Uint64) 
                ref ulong u = ref heap(out ptr<ulong> _addr_u);
                if (!s.readASN1Uint64(_addr_u) || reflect.ValueOf(out).Elem().OverflowUint(u))
                {
                    return false;
                }

                reflect.ValueOf(out).Elem().SetUint(u);
                return true;
            else if (reflect.ValueOf(out).Elem().Kind() == reflect.Struct) 
                if (reflect.TypeOf(out).Elem() == bigIntType)
                {
                    return s.readASN1BigInt(out._<ptr<big.Int>>());
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

        private static bool readASN1BigInt(this ptr<String> _addr_s, ptr<big.Int> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref big.Int @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.INTEGER) || !checkASN1Integer(bytes))
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

        private static bool readASN1Int64(this ptr<String> _addr_s, ptr<long> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref long @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Signed(_addr_out, bytes))
            {
                return false;
            }

            return true;

        }

        private static bool asn1Signed(ptr<long> _addr_@out, slice<byte> n)
        {
            ref long @out = ref _addr_@out.val;

            var length = len(n);
            if (length > 8L)
            {
                return false;
            }

            for (long i = 0L; i < length; i++)
            {
                out.val <<= 8L;
                out.val |= int64(n[i]);
            } 
            // Shift up and down in order to sign extend the result.
 
            // Shift up and down in order to sign extend the result.
            out.val <<= 64L - uint8(length) * 8L;
            out.val >>= 64L - uint8(length) * 8L;
            return true;

        }

        private static bool readASN1Uint64(this ptr<String> _addr_s, ptr<ulong> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref ulong @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Unsigned(_addr_out, bytes))
            {
                return false;
            }

            return true;

        }

        private static bool asn1Unsigned(ptr<ulong> _addr_@out, slice<byte> n)
        {
            ref ulong @out = ref _addr_@out.val;

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
                out.val <<= 8L;
                out.val |= uint64(n[i]);
            }

            return true;

        }

        // ReadASN1Int64WithTag decodes an ASN.1 INTEGER with the given tag into out
        // and advances. It reports whether the read was successful and resulted in a
        // value that can be represented in an int64.
        private static bool ReadASN1Int64WithTag(this ptr<String> _addr_s, ptr<long> _addr_@out, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref long @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            return s.ReadASN1(_addr_bytes, tag) && checkASN1Integer(bytes) && asn1Signed(_addr_out, bytes);
        }

        // ReadASN1Enum decodes an ASN.1 ENUMERATION into out and advances. It reports
        // whether the read was successful.
        private static bool ReadASN1Enum(this ptr<String> _addr_s, ptr<long> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref long @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            ref long i = ref heap(out ptr<long> _addr_i);
            if (!s.ReadASN1(_addr_bytes, asn1.ENUM) || !checkASN1Integer(bytes) || !asn1Signed(_addr_i, bytes))
            {
                return false;
            }

            if (int64(int(i)) != i)
            {
                return false;
            }

            out.val = int(i);
            return true;

        }

        private static bool readBase128Int(this ptr<String> _addr_s, ptr<long> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref long @out = ref _addr_@out.val;

            long ret = 0L;
            for (long i = 0L; len(s.val) > 0L; i++)
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
                    out.val = ret;
                    return true;
                }

            }

            return false; // truncated
        }

        // ReadASN1ObjectIdentifier decodes an ASN.1 OBJECT IDENTIFIER into out and
        // advances. It reports whether the read was successful.
        private static bool ReadASN1ObjectIdentifier(this ptr<String> _addr_s, ptr<encoding_asn1.ObjectIdentifier> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref encoding_asn1.ObjectIdentifier @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.OBJECT_IDENTIFIER) || len(bytes) == 0L)
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
            ref long v = ref heap(out ptr<long> _addr_v);
            if (!bytes.readBase128Int(_addr_v))
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
                if (!bytes.readBase128Int(_addr_v))
                {
                    return false;
                i++;
                }

                components[i] = v;

            }

            out.val = components[..i];
            return true;

        }

        // ReadASN1GeneralizedTime decodes an ASN.1 GENERALIZEDTIME into out and
        // advances. It reports whether the read was successful.
        private static bool ReadASN1GeneralizedTime(this ptr<String> _addr_s, ptr<time.Time> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref time.Time @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.GeneralizedTime))
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

            out.val = res;
            return true;

        }

        // ReadASN1BitString decodes an ASN.1 BIT STRING into out and advances.
        // It reports whether the read was successful.
        private static bool ReadASN1BitString(this ptr<String> _addr_s, ptr<encoding_asn1.BitString> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref encoding_asn1.BitString @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.BIT_STRING) || len(bytes) == 0L || len(bytes) * 8L / 8L != len(bytes))
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
        // an error if the BIT STRING is not a whole number of bytes. It reports
        // whether the read was successful.
        private static bool ReadASN1BitStringAsBytes(this ptr<String> _addr_s, ptr<slice<byte>> _addr_@out)
        {
            ref String s = ref _addr_s.val;
            ref slice<byte> @out = ref _addr_@out.val;

            ref String bytes = ref heap(out ptr<String> _addr_bytes);
            if (!s.ReadASN1(_addr_bytes, asn1.BIT_STRING) || len(bytes) == 0L)
            {
                return false;
            }

            var paddingBits = uint8(bytes[0L]);
            if (paddingBits != 0L)
            {
                return false;
            }

            out.val = bytes[1L..];
            return true;

        }

        // ReadASN1Bytes reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It reports whether the read was successful.
        private static bool ReadASN1Bytes(this ptr<String> _addr_s, ptr<slice<byte>> _addr_@out, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref slice<byte> @out = ref _addr_@out.val;

            return s.ReadASN1((String.val)(out), tag);
        }

        // ReadASN1 reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It reports whether the read was successful.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadASN1(this ptr<String> _addr_s, ptr<String> _addr_@out, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;

            ref asn1.Tag t = ref heap(out ptr<asn1.Tag> _addr_t);
            if (!s.ReadAnyASN1(out, _addr_t) || t != tag)
            {
                return false;
            }

            return true;

        }

        // ReadASN1Element reads the contents of a DER-encoded ASN.1 element (including
        // tag and length bytes) into out, and advances. The element must match the
        // given tag. It reports whether the read was successful.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadASN1Element(this ptr<String> _addr_s, ptr<String> _addr_@out, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;

            ref asn1.Tag t = ref heap(out ptr<asn1.Tag> _addr_t);
            if (!s.ReadAnyASN1Element(out, _addr_t) || t != tag)
            {
                return false;
            }

            return true;

        }

        // ReadAnyASN1 reads the contents of a DER-encoded ASN.1 element (not including
        // tag and length bytes) into out, sets outTag to its tag, and advances.
        // It reports whether the read was successful.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadAnyASN1(this ptr<String> _addr_s, ptr<String> _addr_@out, ptr<asn1.Tag> _addr_outTag)
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;
            ref asn1.Tag outTag = ref _addr_outTag.val;

            return s.readASN1(out, outTag, true);
        }

        // ReadAnyASN1Element reads the contents of a DER-encoded ASN.1 element
        // (including tag and length bytes) into out, sets outTag to is tag, and
        // advances. It reports whether the read was successful.
        //
        // Tags greater than 30 are not supported (i.e. low-tag-number format only).
        private static bool ReadAnyASN1Element(this ptr<String> _addr_s, ptr<String> _addr_@out, ptr<asn1.Tag> _addr_outTag)
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;
            ref asn1.Tag outTag = ref _addr_outTag.val;

            return s.readASN1(out, outTag, false);
        }

        // PeekASN1Tag reports whether the next ASN.1 value on the string starts with
        // the given tag.
        public static bool PeekASN1Tag(this String s, asn1.Tag tag)
        {
            if (len(s) == 0L)
            {
                return false;
            }

            return asn1.Tag(s[0L]) == tag;

        }

        // SkipASN1 reads and discards an ASN.1 element with the given tag. It
        // reports whether the operation was successful.
        private static bool SkipASN1(this ptr<String> _addr_s, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;

            ref String unused = ref heap(out ptr<String> _addr_unused);
            return s.ReadASN1(_addr_unused, tag);
        }

        // ReadOptionalASN1 attempts to read the contents of a DER-encoded ASN.1
        // element (not including tag and length bytes) tagged with the given tag into
        // out. It stores whether an element with the tag was found in outPresent,
        // unless outPresent is nil. It reports whether the read was successful.
        private static bool ReadOptionalASN1(this ptr<String> _addr_s, ptr<String> _addr_@out, ptr<bool> _addr_outPresent, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;
            ref bool outPresent = ref _addr_outPresent.val;

            var present = s.PeekASN1Tag(tag);
            if (outPresent != null)
            {
                outPresent = present;
            }

            if (present && !s.ReadASN1(out, tag))
            {
                return false;
            }

            return true;

        }

        // SkipOptionalASN1 advances s over an ASN.1 element with the given tag, or
        // else leaves s unchanged. It reports whether the operation was successful.
        private static bool SkipOptionalASN1(this ptr<String> _addr_s, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;

            if (!s.PeekASN1Tag(tag))
            {
                return true;
            }

            ref String unused = ref heap(out ptr<String> _addr_unused);
            return s.ReadASN1(_addr_unused, tag);

        }

        // ReadOptionalASN1Integer attempts to read an optional ASN.1 INTEGER
        // explicitly tagged with tag into out and advances. If no element with a
        // matching tag is present, it writes defaultValue into out instead. If out
        // does not point to an integer or to a big.Int, it panics. It reports
        // whether the read was successful.
        private static bool ReadOptionalASN1Integer(this ptr<String> _addr_s, object @out, asn1.Tag tag, object defaultValue) => func((_, panic, __) =>
        {
            ref String s = ref _addr_s.val;

            if (reflect.TypeOf(out).Kind() != reflect.Ptr)
            {
                panic("out is not a pointer");
            }

            ref bool present = ref heap(out ptr<bool> _addr_present);
            ref String i = ref heap(out ptr<String> _addr_i);
            if (!s.ReadOptionalASN1(_addr_i, _addr_present, tag))
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

                    out._<ptr<big.Int>>().Set(defaultValue._<ptr<big.Int>>());
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
        // matching tag is present, it sets "out" to nil instead. It reports
        // whether the read was successful.
        private static bool ReadOptionalASN1OctetString(this ptr<String> _addr_s, ptr<slice<byte>> _addr_@out, ptr<bool> _addr_outPresent, asn1.Tag tag)
        {
            ref String s = ref _addr_s.val;
            ref slice<byte> @out = ref _addr_@out.val;
            ref bool outPresent = ref _addr_outPresent.val;

            ref bool present = ref heap(out ptr<bool> _addr_present);
            ref String child = ref heap(out ptr<String> _addr_child);
            if (!s.ReadOptionalASN1(_addr_child, _addr_present, tag))
            {
                return false;
            }

            if (outPresent != null)
            {
                outPresent = present;
            }

            if (present)
            {
                ref String oct = ref heap(out ptr<String> _addr_oct);
                if (!child.ReadASN1(_addr_oct, asn1.OCTET_STRING) || !child.Empty())
                {
                    return false;
                }

                out.val = oct;

            }
            else
            {
                out.val = null;
            }

            return true;

        }

        // ReadOptionalASN1Boolean sets *out to the value of the next ASN.1 BOOLEAN or,
        // if the next bytes are not an ASN.1 BOOLEAN, to the value of defaultValue.
        // It reports whether the operation was successful.
        private static bool ReadOptionalASN1Boolean(this ptr<String> _addr_s, ptr<bool> _addr_@out, bool defaultValue)
        {
            ref String s = ref _addr_s.val;
            ref bool @out = ref _addr_@out.val;

            ref bool present = ref heap(out ptr<bool> _addr_present);
            ref String child = ref heap(out ptr<String> _addr_child);
            if (!s.ReadOptionalASN1(_addr_child, _addr_present, asn1.BOOLEAN))
            {
                return false;
            }

            if (!present)
            {
                out.val = defaultValue;
                return true;
            }

            return s.ReadASN1Boolean(out);

        }

        private static bool readASN1(this ptr<String> _addr_s, ptr<String> _addr_@out, ptr<asn1.Tag> _addr_outTag, bool skipHeader) => func((_, panic, __) =>
        {
            ref String s = ref _addr_s.val;
            ref String @out = ref _addr_@out.val;
            ref asn1.Tag outTag = ref _addr_outTag.val;

            if (len(s.val) < 2L)
            {
                return false;
            }

            var tag = (s.val)[0L];
            var lenByte = (s.val)[1L];

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
                outTag = asn1.Tag(tag);
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
                ref uint len32 = ref heap(out ptr<uint> _addr_len32);

                if (lenLen == 0L || lenLen > 4L || len(s.val) < int(2L + lenLen))
                {
                    return false;
                }

                var lenBytes = String((s.val)[2L..2L + lenLen]);
                if (!lenBytes.readUnsigned(_addr_len32, int(lenLen)))
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

            if (int(length) < 0L || !s.ReadBytes(new ptr<ptr<slice<byte>>>(out), int(length)))
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
