// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using math = math_package;
using big = math.big_package;
using bits = math.bits_package;
using strconv = strconv_package;
using strings = strings_package;
using encoding;
using math;

partial class x509_package {

internal static error errInvalidOID = errors.New("invalid oid"u8);

// An OID represents an ASN.1 OBJECT IDENTIFIER.
[GoType] partial struct OID {
    internal slice<byte> der;
}

// ParseOID parses a Object Identifier string, represented by ASCII numbers separated by dots.
public static (OID, error) ParseOID(@string oid) {
    OID o = default!;
    return (o, o.unmarshalOIDText(oid));
}

internal static (OID, bool) newOIDFromDER(slice<byte> der) {
    if (len(der) == 0 || (byte)(der[len(der) - 1] & 128) != 0) {
        return (new OID(nil), false);
    }
    nint start = 0;
    foreach (var (i, v) in der) {
        // ITU-T X.690, section 8.19.2:
        // The subidentifier shall be encoded in the fewest possible octets,
        // that is, the leading octet of the subidentifier shall not have the value 0x80.
        if (i == start && v == 128) {
            return (new OID(nil), false);
        }
        if ((byte)(v & 128) == 0) {
            start = i + 1;
        }
    }
    return (new OID(der), true);
}

// OIDFromInts creates a new OID using ints, each integer is a separate component.
public static (OID, error) OIDFromInts(slice<uint64> oid) {
    if (len(oid) < 2 || oid[0] > 2 || (oid[0] < 2 && oid[1] >= 40)) {
        return (new OID(nil), errInvalidOID);
    }
    nint length = base128IntLength(oid[0] * 40 + oid[1]);
    foreach (var (_, v) in oid[2..]) {
        length += base128IntLength(v);
    }
    var der = new slice<byte>(0, length);
    der = appendBase128Int(der, oid[0] * 40 + oid[1]);
    foreach (var (_, v) in oid[2..]) {
        der = appendBase128Int(der, v);
    }
    return (new OID(der), default!);
}

internal static nint base128IntLength(uint64 n) {
    if (n == 0) {
        return 1;
    }
    return (bits.Len64(n) + 6) / 7;
}

internal static slice<byte> appendBase128Int(slice<byte> dst, uint64 n) {
    for (nint i = base128IntLength(n) - 1; i >= 0; i--) {
        var o = ((byte)(n >> (int)(((nuint)(i * 7)))));
        o &= (byte)(127);
        if (i != 0) {
            o |= (byte)(128);
        }
        dst = append(dst, o);
    }
    return dst;
}

internal static nint base128BigIntLength(ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.val;

    if (n.Cmp(big.NewInt(0)) == 0) {
        return 1;
    }
    return (n.BitLen() + 6) / 7;
}

internal static slice<byte> appendBase128BigInt(slice<byte> dst, ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.val;

    if (n.Cmp(big.NewInt(0)) == 0) {
        return append(dst, 0);
    }
    for (nint i = base128BigIntLength(Ꮡn) - 1; i >= 0; i--) {
        var o = ((byte)big.NewInt(0).Rsh(Ꮡn, ((nuint)i) * 7).Bits()[0]);
        o &= (byte)(127);
        if (i != 0) {
            o |= (byte)(128);
        }
        dst = append(dst, o);
    }
    return dst;
}

// MarshalText implements [encoding.TextMarshaler]
public static (slice<byte>, error) MarshalText(this OID o) {
    return (slice<byte>(o.String()), default!);
}

// UnmarshalText implements [encoding.TextUnmarshaler]
[GoRecv] public static error UnmarshalText(this ref OID o, slice<byte> text) {
    return o.unmarshalOIDText(((@string)text));
}

[GoRecv] internal static error unmarshalOIDText(this ref OID o, @string oid) {
    // (*big.Int).SetString allows +/- signs, but we don't want
    // to allow them in the string representation of Object Identifier, so
    // reject such encodings.
    foreach (var (_, c) in oid) {
        var isDigit = c >= (rune)'0' && c <= (rune)'9';
        if (!isDigit && c != (rune)'.') {
            return errInvalidOID;
        }
    }
    @string firstNum = default!;
    @string secondNum = default!;
    bool nextComponentExists = default!;
    (firstNum, oid, nextComponentExists) = strings.Cut(oid, "."u8);
    if (!nextComponentExists) {
        return errInvalidOID;
    }
    (secondNum, oid, nextComponentExists) = strings.Cut(oid, "."u8);
    ж<bigꓸInt> first = big.NewInt(0);
    ж<bigꓸInt> second = big.NewInt(0);
    {
        var (_, ok) = first.SetString(firstNum, 10); if (!ok) {
            return errInvalidOID;
        }
    }
    {
        var (_, ok) = second.SetString(secondNum, 10); if (!ok) {
            return errInvalidOID;
        }
    }
    if (first.Cmp(big.NewInt(2)) > 0 || (first.Cmp(big.NewInt(2)) < 0 && second.Cmp(big.NewInt(40)) >= 0)) {
        return errInvalidOID;
    }
    var firstComponent = first.Mul(first, big.NewInt(40));
    firstComponent.Add(firstComponent, second);
    var der = appendBase128BigInt(new slice<byte>(0, 32), firstComponent);
    while (nextComponentExists) {
        @string strNum = default!;
        (strNum, oid, nextComponentExists) = strings.Cut(oid, "."u8);
        var (b, ok) = big.NewInt(0).SetString(strNum, 10);
        if (!ok) {
            return errInvalidOID;
        }
        der = appendBase128BigInt(der, b);
    }
    o.der = der;
    return default!;
}

// MarshalBinary implements [encoding.BinaryMarshaler]
public static (slice<byte>, error) MarshalBinary(this OID o) {
    return (bytes.Clone(o.der), default!);
}

// UnmarshalBinary implements [encoding.BinaryUnmarshaler]
[GoRecv] public static error UnmarshalBinary(this ref OID o, slice<byte> b) {
    var (oid, ok) = newOIDFromDER(bytes.Clone(b));
    if (!ok) {
        return errInvalidOID;
    }
    o = oid;
    return default!;
}

// Equal returns true when oid and other represents the same Object Identifier.
public static bool Equal(this OID oid, OID other) {
    // There is only one possible DER encoding of
    // each unique Object Identifier.
    return bytes.Equal(oid.der, other.der);
}

internal static (nint ret, nint offset, bool failed) parseBase128Int(slice<byte> bytes, nint initOffset) {
    nint ret = default!;
    nint offset = default!;
    bool failed = default!;

    offset = initOffset;
    int64 ret64 = default!;
    for (nint shifted = 0; offset < len(bytes); shifted++) {
        // 5 * 7 bits per byte == 35 bits of data
        // Thus the representation is either non-minimal or too large for an int32
        if (shifted == 5) {
            failed = true;
            return (ret, offset, failed);
        }
        ret64 <<= (UntypedInt)(7);
        var b = bytes[offset];
        // integers should be minimally encoded, so the leading octet should
        // never be 0x80
        if (shifted == 0 && b == 128) {
            failed = true;
            return (ret, offset, failed);
        }
        ret64 |= (int64)(((int64)((byte)(b & 127))));
        offset++;
        if ((byte)(b & 128) == 0) {
            ret = ((nint)ret64);
            // Ensure that the returned value fits in an int on all platforms
            if (ret64 > math.MaxInt32) {
                failed = true;
            }
            return (ret, offset, failed);
        }
    }
    failed = true;
    return (ret, offset, failed);
}

// EqualASN1OID returns whether an OID equals an asn1.ObjectIdentifier. If
// asn1.ObjectIdentifier cannot represent the OID specified by oid, because
// a component of OID requires more than 31 bits, it returns false.
public static bool EqualASN1OID(this OID oid, asn1.ObjectIdentifier other) {
    if (len(other) < 2) {
        return false;
    }
    var (v, offset, failed) = parseBase128Int(oid.der, 0);
    if (failed) {
        // This should never happen, since we've already parsed the OID,
        // but just in case.
        return false;
    }
    if (v < 80){
        nint a = v / 40;
        nint b = v % 40;
        if (other[0] != a || other[1] != b) {
            return false;
        }
    } else {
        nint a = 2;
        nint b = v - 80;
        if (other[0] != a || other[1] != b) {
            return false;
        }
    }
    nint i = 2;
    for (; offset < len(oid.der); i++) {
        (v, offset, failed) = parseBase128Int(oid.der, offset);
        if (failed) {
            // Again, shouldn't happen, since we've already parsed
            // the OID, but better safe than sorry.
            return false;
        }
        if (i >= len(other) || v != other[i]) {
            return false;
        }
    }
    return i == len(other);
}

// Strings returns the string representation of the Object Identifier.
public static @string String(this OID oid) {
    strings.Builder b = default!;
    b.Grow(32);
    static readonly UntypedInt valSize = 64; // size in bits of val.
    static readonly UntypedInt bitsPerByte = 7;
    static readonly UntypedInt maxValSafeShift = /* (1 << (valSize - bitsPerByte)) - 1 */ 144115188075855871;
    nint start = 0;
    uint64 val = ((uint64)0);
    slice<byte> numBuf = new slice<byte>(0, 21);
    ж<bigꓸInt> bigVal = default!;
    bool overflow = default!;
    foreach (var (i, v) in oid.der) {
        var curVal = (byte)(v & 127);
        var valEnd = (byte)(v & 128) == 0;
        if (valEnd) {
            if (start != 0) {
                b.WriteByte((rune)'.');
            }
        }
        if (!overflow && val > maxValSafeShift) {
            if (bigVal == nil) {
                bigVal = @new<bigꓸInt>();
            }
            bigVal = bigVal.SetUint64(val);
            overflow = true;
        }
        if (overflow) {
            bigVal = bigVal.Lsh(bigVal, bitsPerByte).Or(bigVal, big.NewInt(((int64)curVal)));
            if (valEnd) {
                if (start == 0) {
                    b.WriteString("2."u8);
                    bigVal = bigVal.Sub(bigVal, big.NewInt(80));
                }
                numBuf = bigVal.Append(numBuf, 10);
                b.Write(numBuf);
                numBuf = numBuf[..0];
                val = 0;
                start = i + 1;
                overflow = false;
            }
            continue;
        }
        val <<= (UntypedInt)(bitsPerByte);
        val |= (uint64)(((uint64)curVal));
        if (valEnd) {
            if (start == 0){
                if (val < 80){
                    b.Write(strconv.AppendUint(numBuf, val / 40, 10));
                    b.WriteByte((rune)'.');
                    b.Write(strconv.AppendUint(numBuf, val % 40, 10));
                } else {
                    b.WriteString("2."u8);
                    b.Write(strconv.AppendUint(numBuf, val - 80, 10));
                }
            } else {
                b.Write(strconv.AppendUint(numBuf, val, 10));
            }
            val = 0;
            start = i + 1;
        }
    }
    return b.String();
}

internal static (asn1.ObjectIdentifier, bool) toASN1OID(this OID oid) {
    var @out = new slice<nint>(0, len(oid.der) + 1);
    static readonly UntypedInt valSize = 31; // amount of usable bits of val for OIDs.
    static readonly UntypedInt bitsPerByte = 7;
    static readonly UntypedInt maxValSafeShift = /* (1 << (valSize - bitsPerByte)) - 1 */ 16777215;
    nint val = 0;
    foreach (var (_, v) in oid.der) {
        if (val > maxValSafeShift) {
            return (default!, false);
        }
        val <<= (UntypedInt)(bitsPerByte);
        val |= (nint)(((nint)((byte)(v & 127))));
        if ((byte)(v & 128) == 0) {
            if (len(@out) == 0) {
                if (val < 80){
                    @out = append(@out, val / 40);
                    @out = append(@out, val % 40);
                } else {
                    @out = append(@out, 2);
                    @out = append(@out, val - 80);
                }
                val = 0;
                continue;
            }
            @out = append(@out, val);
            val = 0;
        }
    }
    return (@out, true);
}

} // end x509_package
