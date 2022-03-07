// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements encoding/decoding of Rats.

// package big -- go2cs converted at 2022 March 06 22:18:06 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\ratmarsh.go
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;

namespace go.math;

public static partial class big_package {

    // Gob codec version. Permits backward-compatible changes to the encoding.
private static readonly byte ratGobVersion = 1;

// GobEncode implements the gob.GobEncoder interface.


// GobEncode implements the gob.GobEncoder interface.
private static (slice<byte>, error) GobEncode(this ptr<Rat> _addr_x) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Rat x = ref _addr_x.val;

    if (x == null) {
        return (null, error.As(null!)!);
    }
    var buf = make_slice<byte>(1 + 4 + (len(x.a.abs) + len(x.b.abs)) * _S); // extra bytes for version and sign bit (1), and numerator length (4)
    var i = x.b.abs.bytes(buf);
    var j = x.a.abs.bytes(buf[..(int)i]);
    var n = i - j;
    if (int(uint32(n)) != n) { 
        // this should never happen
        return (null, error.As(errors.New("Rat.GobEncode: numerator too large"))!);

    }
    binary.BigEndian.PutUint32(buf[(int)j - 4..(int)j], uint32(n));
    j -= 1 + 4;
    var b = ratGobVersion << 1; // make space for sign bit
    if (x.a.neg) {
        b |= 1;
    }
    buf[j] = b;
    return (buf[(int)j..], error.As(null!)!);

}

// GobDecode implements the gob.GobDecoder interface.
private static error GobDecode(this ptr<Rat> _addr_z, slice<byte> buf) {
    ref Rat z = ref _addr_z.val;

    if (len(buf) == 0) { 
        // Other side sent a nil or default value.
        z.val = new Rat();
        return error.As(null!)!;

    }
    var b = buf[0];
    if (b >> 1 != ratGobVersion) {
        return error.As(fmt.Errorf("Rat.GobDecode: encoding version %d not supported", b >> 1))!;
    }
    const nint j = 1 + 4;

    var i = j + binary.BigEndian.Uint32(buf[(int)j - 4..(int)j]);
    z.a.neg = b & 1 != 0;
    z.a.abs = z.a.abs.setBytes(buf[(int)j..(int)i]);
    z.b.abs = z.b.abs.setBytes(buf[(int)i..]);
    return error.As(null!)!;

}

// MarshalText implements the encoding.TextMarshaler interface.
private static (slice<byte>, error) MarshalText(this ptr<Rat> _addr_x) {
    slice<byte> text = default;
    error err = default!;
    ref Rat x = ref _addr_x.val;

    if (x.IsInt()) {
        return x.a.MarshalText();
    }
    return (x.marshal(), error.As(null!)!);

}

// UnmarshalText implements the encoding.TextUnmarshaler interface.
private static error UnmarshalText(this ptr<Rat> _addr_z, slice<byte> text) {
    ref Rat z = ref _addr_z.val;
 
    // TODO(gri): get rid of the []byte/string conversion
    {
        var (_, ok) = z.SetString(string(text));

        if (!ok) {
            return error.As(fmt.Errorf("math/big: cannot unmarshal %q into a *big.Rat", text))!;
        }
    }

    return error.As(null!)!;

}

} // end big_package
