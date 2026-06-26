// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Rats.
namespace go.math;

using errors = errors_package;
using fmt = fmt_package;
using byteorder = @internal.byteorder_package;
using math = math_package;
using @internal;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte ratGobVersion = 1;

// GobEncode implements the [encoding/gob.GobEncoder] interface.
[GoRecv] public static (slice<byte>, error) GobEncode(this ref ΔRat x) {
    if (x == nil) {
        return (default!, default!);
    }
    var buf = new slice<byte>(1 + 4 + (len(x.a.abs) + len(x.b.abs)) * _S);
    // extra bytes for version and sign bit (1), and numerator length (4)
    nint i = x.b.abs.bytes(buf);
    nint j = x.a.abs.bytes(buf[..(int)(i)]);
    nint n = i - j;
    if (((nint)((uint32)n)) != n) {
        // this should never happen
        return (default!, errors.New("Rat.GobEncode: numerator too large"u8));
    }
    byteorder.BePutUint32(buf[(int)(j - 4)..(int)(j)], ((uint32)n));
    j -= 1 + 4;
    var b = ratGobVersion << (int)(1);
    // make space for sign bit
    if (x.a.neg) {
        b |= (byte)(1);
    }
    buf[j] = b;
    return (buf[(int)(j)..], default!);
}

// GobDecode implements the [encoding/gob.GobDecoder] interface.
[GoRecv] public static error GobDecode(this ref ΔRat z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new ΔRat(nil);
        return default!;
    }
    if (len(buf) < 5) {
        return errors.New("Rat.GobDecode: buffer too small"u8);
    }
    var b = buf[0];
    if (b >> (int)(1) != ratGobVersion) {
        return fmt.Errorf("Rat.GobDecode: encoding version %d not supported"u8, b >> (int)(1));
    }
    static readonly UntypedInt j = /* 1 + 4 */ 5;
    var ln = byteorder.BeUint32(buf[(int)(j - 4)..(int)(j)]);
    if (((uint64)ln) > math.MaxInt - j) {
        return errors.New("Rat.GobDecode: invalid length"u8);
    }
    nint i = j + ((nint)ln);
    if (len(buf) < i) {
        return errors.New("Rat.GobDecode: buffer too small"u8);
    }
    z.a.neg = (byte)(b & 1) != 0;
    z.a.abs = z.a.abs.setBytes(buf[(int)(j)..(int)(i)]);
    z.b.abs = z.b.abs.setBytes(buf[(int)(i)..]);
    return default!;
}

// MarshalText implements the [encoding.TextMarshaler] interface.
[GoRecv] public static (slice<byte> text, error err) MarshalText(this ref ΔRat x) {
    slice<byte> text = default!;
    error err = default!;

    if (x.IsInt()) {
        return x.a.MarshalText();
    }
    return (x.marshal(), default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
[GoRecv] public static error UnmarshalText(this ref ΔRat z, slice<byte> text) {
    // TODO(gri): get rid of the []byte/string conversion
    {
        var (Δ_, ok) = z.SetString(((@string)text)); if (!ok) {
            return fmt.Errorf("math/big: cannot unmarshal %q into a *big.Rat"u8, text);
        }
    }
    return default!;
}

} // end big_package
