// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Floats.
namespace go.math;

using errors = errors_package;
using fmt = fmt_package;
using byteorder = @internal.byteorder_package;
using @internal;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte floatGobVersion = 1;

// GobEncode implements the [encoding/gob.GobEncoder] interface.
// The [Float] value and all its attributes (precision,
// rounding mode, accuracy) are marshaled.
[GoRecv] public static (slice<byte>, error) GobEncode(this ref Float x) {
    if (x == nil) {
        return (default!, default!);
    }
    // determine max. space (bytes) required for encoding
    nint sz = 1 + 1 + 4;
    // version + mode|acc|form|neg (3+2+2+1bit) + prec
    nint n = 0;
    // number of mantissa words
    if (x.form == finite) {
        // add space for mantissa and exponent
        n = ((nint)((x.prec + (_W - 1)) / _W));
        // required mantissa length in words for given precision
        // actual mantissa slice could be shorter (trailing 0's) or longer (unused bits):
        // - if shorter, only encode the words present
        // - if longer, cut off unused words when encoding in bytes
        //   (in practice, this should never happen since rounding
        //   takes care of it, but be safe and do it always)
        if (len(x.mant) < n) {
            n = len(x.mant);
        }
        // len(x.mant) >= n
        sz += 4 + n * _S;
    }
    // exp + mant
    var buf = new slice<byte>(sz);
    buf[0] = floatGobVersion;
    var b = (byte)((byte)(((byte)((RoundingMode)(x.mode & 7))) << (int)(5) | ((byte)((Accuracy)((x.acc + 1) & 3))) << (int)(3)) | ((byte)((form)(x.form & 3))) << (int)(1));
    if (x.neg) {
        b |= (byte)(1);
    }
    buf[1] = b;
    byteorder.BePutUint32(buf[2..], x.prec);
    if (x.form == finite) {
        byteorder.BePutUint32(buf[6..], ((uint32)x.exp));
        x.mant[(int)(len(x.mant) - n)..].bytes(buf[10..]);
    }
    // cut off unused trailing words
    return (buf, default!);
}

// GobDecode implements the [encoding/gob.GobDecoder] interface.
// The result is rounded per the precision and rounding mode of
// z unless z's precision is 0, in which case z is set exactly
// to the decoded value.
[GoRecv] public static error GobDecode(this ref Float z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new Float(nil);
        return default!;
    }
    if (len(buf) < 6) {
        return errors.New("Float.GobDecode: buffer too small"u8);
    }
    if (buf[0] != floatGobVersion) {
        return fmt.Errorf("Float.GobDecode: encoding version %d not supported"u8, buf[0]);
    }
    var oldPrec = z.prec;
    var oldMode = z.mode;
    var b = buf[1];
    z.mode = ((RoundingMode)((byte)((b >> (int)(5)) & 7)));
    z.acc = ((Accuracy)((byte)((b >> (int)(3)) & 3))) - 1;
    z.form = ((form)((byte)((b >> (int)(1)) & 3)));
    z.neg = (byte)(b & 1) != 0;
    z.prec = byteorder.BeUint32(buf[2..]);
    if (z.form == finite) {
        if (len(buf) < 10) {
            return errors.New("Float.GobDecode: buffer too small for finite form float"u8);
        }
        z.exp = ((int32)byteorder.BeUint32(buf[6..]));
        z.mant = z.mant.setBytes(buf[10..]);
    }
    if (oldPrec != 0) {
        z.mode = oldMode;
        z.SetPrec(((nuint)oldPrec));
    }
    {
        @string msg = z.validate0(); if (msg != ""u8) {
            return errors.New("Float.GobDecode: "u8 + msg);
        }
    }
    return default!;
}

// MarshalText implements the [encoding.TextMarshaler] interface.
// Only the [Float] value is marshaled (in full precision), other
// attributes such as precision or accuracy are ignored.
[GoRecv] public static (slice<byte> text, error err) MarshalText(this ref Float x) {
    slice<byte> text = default!;
    error err = default!;

    if (x == nil) {
        return (slice<byte>("<nil>"), default!);
    }
    slice<byte> buf = default!;
    return (x.Append(buf, (rune)'g', -1), default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
// The result is rounded per the precision and rounding mode of z.
// If z's precision is 0, it is changed to 64 before rounding takes
// effect.
[GoRecv] public static error UnmarshalText(this ref Float z, slice<byte> text) {
    // TODO(gri): get rid of the []byte/string conversion
    var (Δ_, Δ_, err) = z.Parse(((@string)text), 0);
    if (err != default!) {
        err = fmt.Errorf("math/big: cannot unmarshal %q into a *big.Float (%v)"u8, text, err);
    }
    return err;
}

} // end big_package
