// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Ints.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte intGobVersion = 1;

// GobEncode implements the [encoding/gob.GobEncoder] interface.
[GoRecv] public static (slice<byte>, error) GobEncode(this ref ΔInt x) {
    if (x == nil) {
        return (default!, default!);
    }
    var buf = new slice<byte>(1 + len(x.abs) * _S);
    // extra byte for version and sign bit
    nint i = x.abs.bytes(buf) - 1;
    // i >= 0
    var b = intGobVersion << (int)(1);
    // make space for sign bit
    if (x.neg) {
        b |= (byte)(1);
    }
    buf[i] = b;
    return (buf[(int)(i)..], default!);
}

// GobDecode implements the [encoding/gob.GobDecoder] interface.
[GoRecv] public static error GobDecode(this ref ΔInt z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new ΔInt(nil);
        return default!;
    }
    var b = buf[0];
    if (b >> (int)(1) != intGobVersion) {
        return fmt.Errorf("Int.GobDecode: encoding version %d not supported"u8, b >> (int)(1));
    }
    z.neg = (byte)(b & 1) != 0;
    z.abs = z.abs.setBytes(buf[1..]);
    return default!;
}

// MarshalText implements the [encoding.TextMarshaler] interface.
[GoRecv] public static (slice<byte> text, error err) MarshalText(this ref ΔInt x) {
    slice<byte> text = default!;
    error err = default!;

    if (x == nil) {
        return (slice<byte>("<nil>"), default!);
    }
    return (x.abs.itoa(x.neg, 10), default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
[GoRecv] public static error UnmarshalText(this ref ΔInt z, slice<byte> text) {
    {
        var (Δ_, ok) = z.setFromScanner(~bytes.NewReader(text), 0); if (!ok) {
            return fmt.Errorf("math/big: cannot unmarshal %q into a *big.Int"u8, text);
        }
    }
    return default!;
}

// The JSON marshalers are only here for API backward compatibility
// (programs that explicitly look for these two methods). JSON works
// fine with the TextMarshaler only.

// MarshalJSON implements the [encoding/json.Marshaler] interface.
[GoRecv] public static (slice<byte>, error) MarshalJSON(this ref ΔInt x) {
    if (x == nil) {
        return (slice<byte>("null"), default!);
    }
    return (x.abs.itoa(x.neg, 10), default!);
}

// UnmarshalJSON implements the [encoding/json.Unmarshaler] interface.
[GoRecv] public static error UnmarshalJSON(this ref ΔInt z, slice<byte> text) {
    // Ignore null, like in the main JSON package.
    if (((@string)text) == "null"u8) {
        return default!;
    }
    return z.UnmarshalText(text);
}

} // end big_package
