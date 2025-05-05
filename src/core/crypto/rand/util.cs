// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using randutil = crypto.@internal.randutil_package;
using errors = errors_package;
using io = io_package;
using big = math.big_package;
using crypto.@internal;
using math;

partial class rand_package {

// Prime returns a number of the given bit length that is prime with high probability.
// Prime will return error for any error returned by [rand.Read] or if bits < 2.
public static (ж<bigꓸInt>, error) Prime(io.Reader rand, nint bits) {
    if (bits < 2) {
        return (default!, errors.New("crypto/rand: prime size must be at least 2-bit"u8));
    }
    randutil.MaybeReadByte(rand);
    nuint b = ((nuint)(bits % 8));
    if (b == 0) {
        b = 8;
    }
    var bytes = new slice<byte>((bits + 7) / 8);
    var p = @new<bigꓸInt>();
    while (ᐧ) {
        {
            var (_, err) = io.ReadFull(rand, bytes); if (err != default!) {
                return (default!, err);
            }
        }
        // Clear bits in the first byte to make sure the candidate has a size <= bits.
        bytes[0] &= (uint8)(((uint8)(((nint)(1 << (int)(b))) - 1)));
        // Don't let the value be too small, i.e, set the most significant two bits.
        // Setting the top two bits, rather than just the top bit,
        // means that when two of these values are multiplied together,
        // the result isn't ever one bit short.
        if (b >= 2){
            bytes[0] |= (byte)(3 << (int)((b - 2)));
        } else {
            // Here b==1, because b cannot be zero.
            bytes[0] |= (byte)(1);
            if (len(bytes) > 1) {
                bytes[1] |= (byte)(128);
            }
        }
        // Make the value odd since an even number this large certainly isn't prime.
        bytes[len(bytes) - 1] |= (byte)(1);
        p.SetBytes(bytes);
        if (p.ProbablyPrime(20)) {
            return (p, default!);
        }
    }
}

// Int returns a uniform random value in [0, max). It panics if max <= 0.
public static (ж<bigꓸInt> n, error err) Int(io.Reader rand, ж<bigꓸInt> Ꮡmax) {
    ж<bigꓸInt> n = default!;
    error err = default!;

    ref var max = ref Ꮡmax.val;
    if (max.Sign() <= 0) {
        throw panic("crypto/rand: argument to Int is <= 0");
    }
    n = @new<bigꓸInt>();
    n.Sub(Ꮡmax, n.SetUint64(1));
    // bitLen is the maximum bit length needed to encode a value < max.
    nint bitLen = n.BitLen();
    if (bitLen == 0) {
        // the only valid result is 0
        return (n, err);
    }
    // k is the maximum byte length needed to encode a value < max.
    nint k = (bitLen + 7) / 8;
    // b is the number of bits in the most significant byte of max-1.
    nuint b = ((nuint)(bitLen % 8));
    if (b == 0) {
        b = 8;
    }
    var bytes = new slice<byte>(k);
    while (ᐧ) {
        (_, err) = io.ReadFull(rand, bytes);
        if (err != default!) {
            return (default!, err);
        }
        // Clear bits in the first byte to increase the probability
        // that the candidate is < max.
        bytes[0] &= (uint8)(((uint8)(((nint)(1 << (int)(b))) - 1)));
        n.SetBytes(bytes);
        if (n.Cmp(Ꮡmax) < 0) {
            return (n, err);
        }
    }
}

} // end rand_package
