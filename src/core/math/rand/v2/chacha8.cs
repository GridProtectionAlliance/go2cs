// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using errors = errors_package;
using byteorder = @internal.byteorder_package;
using chacha8rand = @internal.chacha8rand_package;
using @internal;

partial class rand_package {

// A ChaCha8 is a ChaCha8-based cryptographically strong
// random number generator.
[GoType] partial struct ChaCha8 {
    internal @internal.chacha8rand_package.State state;
    // The last readLen bytes of readBuf are still to be consumed by Read.
    internal array<byte> readBuf = new(8);
    internal nint readLen; // 0 <= readLen <= 8
}

// NewChaCha8 returns a new ChaCha8 seeded with the given seed.
public static ж<ChaCha8> NewChaCha8(array<byte> seed) {
    seed = seed.Clone();

    var c = @new<ChaCha8>();
    (~c).state.Init(seed);
    return c;
}

// Seed resets the ChaCha8 to behave the same way as NewChaCha8(seed).
[GoRecv] public static void Seed(this ref ChaCha8 c, array<byte> seed) {
    seed = seed.Clone();

    c.state.Init(seed);
    c.readLen = 0;
    c.readBuf = new byte[]{}.array();
}

// Uint64 returns a uniformly distributed random uint64 value.
[GoRecv] public static uint64 Uint64(this ref ChaCha8 c) {
    while (ᐧ) {
        var (x, ok) = c.state.Next();
        if (ok) {
            return x;
        }
        c.state.Refill();
    }
}

// Read reads exactly len(p) bytes into p.
// It always returns len(p) and a nil error.
//
// If calls to Read and Uint64 are interleaved, the order in which bits are
// returned by the two is undefined, and Read may return bits generated before
// the last call to Uint64.
[GoRecv] public static (nint n, error err) Read(this ref ChaCha8 c, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (c.readLen > 0) {
        n = copy(p, c.readBuf[(int)(len(c.readBuf) - c.readLen)..]);
        c.readLen -= n;
        p = p[(int)(n)..];
    }
    while (len(p) >= 8) {
        byteorder.LePutUint64(p, c.Uint64());
        p = p[8..];
        n += 8;
    }
    if (len(p) > 0) {
        byteorder.LePutUint64(c.readBuf[..], c.Uint64());
        n += copy(p, c.readBuf[..]);
        c.readLen = 8 - len(p);
    }
    return (n, err);
}

// UnmarshalBinary implements the encoding.BinaryUnmarshaler interface.
[GoRecv] public static error UnmarshalBinary(this ref ChaCha8 c, slice<byte> data) {
    var (data, ok) = cutPrefix(data, slice<byte>("readbuf:"));
    if (ok) {
        slice<byte> buf = default!;
        (buf, data, ok) = readUint8LengthPrefixed(data);
        if (!ok) {
            return errors.New("invalid ChaCha8 Read buffer encoding"u8);
        }
        c.readLen = copy(c.readBuf[(int)(len(c.readBuf) - len(buf))..], buf);
    }
    return chacha8rand.Unmarshal(Ꮡ(c.state), data);
}

internal static (slice<byte> after, bool found) cutPrefix(slice<byte> s, slice<byte> prefix) {
    slice<byte> after = default!;
    bool found = default!;

    if (len(s) < len(prefix) || ((@string)(s[..(int)(len(prefix))])) != ((@string)prefix)) {
        return (s, false);
    }
    return (s[(int)(len(prefix))..], true);
}

internal static (slice<byte> buf, slice<byte> rest, bool ok) readUint8LengthPrefixed(slice<byte> b) {
    slice<byte> buf = default!;
    slice<byte> rest = default!;
    bool ok = default!;

    if (len(b) == 0 || len(b) < ((nint)(1 + b[0]))) {
        return (default!, default!, false);
    }
    return (b[1..(int)(1 + b[0])], b[(int)(1 + b[0])..], true);
}

// MarshalBinary implements the encoding.BinaryMarshaler interface.
[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref ChaCha8 c) {
    if (c.readLen > 0) {
        var @out = slice<byte>("readbuf:");
        @out = append(@out, ((uint8)c.readLen));
        @out = append(@out, c.readBuf[(int)(len(c.readBuf) - c.readLen)..].ꓸꓸꓸ);
        return (append(@out, chacha8rand.Marshal(Ꮡ(c.state)).ꓸꓸꓸ), default!);
    }
    return (chacha8rand.Marshal(Ꮡ(c.state)), default!);
}

} // end rand_package
