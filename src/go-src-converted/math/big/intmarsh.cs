// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements encoding/decoding of Ints.

// package big -- go2cs converted at 2022 March 13 05:32:06 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\intmarsh.go
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;


// Gob codec version. Permits backward-compatible changes to the encoding.

public static partial class big_package {

private static readonly byte intGobVersion = 1;

// GobEncode implements the gob.GobEncoder interface.


// GobEncode implements the gob.GobEncoder interface.
private static (slice<byte>, error) GobEncode(this ptr<Int> _addr_x) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Int x = ref _addr_x.val;

    if (x == null) {
        return (null, error.As(null!)!);
    }
    var buf = make_slice<byte>(1 + len(x.abs) * _S); // extra byte for version and sign bit
    var i = x.abs.bytes(buf) - 1; // i >= 0
    var b = intGobVersion << 1; // make space for sign bit
    if (x.neg) {
        b |= 1;
    }
    buf[i] = b;
    return (buf[(int)i..], error.As(null!)!);
}

// GobDecode implements the gob.GobDecoder interface.
private static error GobDecode(this ptr<Int> _addr_z, slice<byte> buf) {
    ref Int z = ref _addr_z.val;

    if (len(buf) == 0) { 
        // Other side sent a nil or default value.
        z.val = new Int();
        return error.As(null!)!;
    }
    var b = buf[0];
    if (b >> 1 != intGobVersion) {
        return error.As(fmt.Errorf("Int.GobDecode: encoding version %d not supported", b >> 1))!;
    }
    z.neg = b & 1 != 0;
    z.abs = z.abs.setBytes(buf[(int)1..]);
    return error.As(null!)!;
}

// MarshalText implements the encoding.TextMarshaler interface.
private static (slice<byte>, error) MarshalText(this ptr<Int> _addr_x) {
    slice<byte> text = default;
    error err = default!;
    ref Int x = ref _addr_x.val;

    if (x == null) {
        return ((slice<byte>)"<nil>", error.As(null!)!);
    }
    return (x.abs.itoa(x.neg, 10), error.As(null!)!);
}

// UnmarshalText implements the encoding.TextUnmarshaler interface.
private static error UnmarshalText(this ptr<Int> _addr_z, slice<byte> text) {
    ref Int z = ref _addr_z.val;

    {
        var (_, ok) = z.setFromScanner(bytes.NewReader(text), 0);

        if (!ok) {
            return error.As(fmt.Errorf("math/big: cannot unmarshal %q into a *big.Int", text))!;
        }
    }
    return error.As(null!)!;
}

// The JSON marshalers are only here for API backward compatibility
// (programs that explicitly look for these two methods). JSON works
// fine with the TextMarshaler only.

// MarshalJSON implements the json.Marshaler interface.
private static (slice<byte>, error) MarshalJSON(this ptr<Int> _addr_x) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Int x = ref _addr_x.val;

    return x.MarshalText();
}

// UnmarshalJSON implements the json.Unmarshaler interface.
private static error UnmarshalJSON(this ptr<Int> _addr_z, slice<byte> text) {
    ref Int z = ref _addr_z.val;
 
    // Ignore null, like in the main JSON package.
    if (string(text) == "null") {
        return error.As(null!)!;
    }
    return error.As(z.UnmarshalText(text))!;
}

} // end big_package
