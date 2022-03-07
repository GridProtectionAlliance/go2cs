// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rand -- go2cs converted at 2022 March 06 22:17:22 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\util.go
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;

namespace go.crypto;

public static partial class rand_package {

    // smallPrimes is a list of small, prime numbers that allows us to rapidly
    // exclude some fraction of composite candidates when searching for a random
    // prime. This list is truncated at the point where smallPrimesProduct exceeds
    // a uint64. It does not include two because we ensure that the candidates are
    // odd by construction.
private static byte smallPrimes = new slice<byte>(new byte[] { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 });

// smallPrimesProduct is the product of the values in smallPrimes and allows us
// to reduce a candidate prime by this number and then determine whether it's
// coprime to all the elements of smallPrimes without further big.Int
// operations.
private static ptr<object> smallPrimesProduct = @new<big.Int>().SetUint64((nuint)16294579238595022365UL);

// Prime returns a number, p, of the given size, such that p is prime
// with high probability.
// Prime will return error for any error returned by rand.Read or if bits < 2.
public static (ptr<big.Int>, error) Prime(io.Reader rand, nint bits) {
    ptr<big.Int> p = default!;
    error err = default!;

    if (bits < 2) {
        err = errors.New("crypto/rand: prime size must be at least 2-bit");
        return ;
    }
    var b = uint(bits % 8);
    if (b == 0) {
        b = 8;
    }
    var bytes = make_slice<byte>((bits + 7) / 8);
    p = @new<big.Int>();

    ptr<big.Int> bigMod = @new<big.Int>();

    while (true) {
        _, err = io.ReadFull(rand, bytes);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        bytes[0] &= uint8(int(1 << (int)(b)) - 1); 
        // Don't let the value be too small, i.e, set the most significant two bits.
        // Setting the top two bits, rather than just the top bit,
        // means that when two of these values are multiplied together,
        // the result isn't ever one bit short.
        if (b >= 2) {
            bytes[0] |= 3 << (int)((b - 2));
        }
        else
 { 
            // Here b==1, because b cannot be zero.
            bytes[0] |= 1;
            if (len(bytes) > 1) {
                bytes[1] |= 0x80;
            }

        }
        bytes[len(bytes) - 1] |= 1;

        p.SetBytes(bytes); 

        // Calculate the value mod the product of smallPrimes. If it's
        // a multiple of any of these primes we add two until it isn't.
        // The probability of overflowing is minimal and can be ignored
        // because we still perform Miller-Rabin tests on the result.
        bigMod.Mod(p, smallPrimesProduct);
        var mod = bigMod.Uint64();

NextDelta: 

        // There is a tiny possibility that, by adding delta, we caused
        // the number to be one bit too long. Thus we check BitLen
        // here.
        {
            var delta = uint64(0);

            while (delta < 1 << 20) {
                var m = mod + delta;
                foreach (var (_, prime) in smallPrimes) {
                    if (m % uint64(prime) == 0 && (bits > 6 || m != uint64(prime))) {
                        _continueNextDelta = true;
                        break;
                    }

                delta += 2;
                }
                if (delta > 0) {
                    bigMod.SetUint64(delta);
                    p.Add(p, bigMod);
                }

                break;

            } 

            // There is a tiny possibility that, by adding delta, we caused
            // the number to be one bit too long. Thus we check BitLen
            // here.

        } 

        // There is a tiny possibility that, by adding delta, we caused
        // the number to be one bit too long. Thus we check BitLen
        // here.
        if (p.ProbablyPrime(20) && p.BitLen() == bits) {
            return ;
        }
    }

}

// Int returns a uniform random value in [0, max). It panics if max <= 0.
public static (ptr<big.Int>, error) Int(io.Reader rand, ptr<big.Int> _addr_max) => func((_, panic, _) => {
    ptr<big.Int> n = default!;
    error err = default!;
    ref big.Int max = ref _addr_max.val;

    if (max.Sign() <= 0) {
        panic("crypto/rand: argument to Int is <= 0");
    }
    n = @new<big.Int>();
    n.Sub(max, n.SetUint64(1)); 
    // bitLen is the maximum bit length needed to encode a value < max.
    var bitLen = n.BitLen();
    if (bitLen == 0) { 
        // the only valid result is 0
        return ;

    }
    var k = (bitLen + 7) / 8; 
    // b is the number of bits in the most significant byte of max-1.
    var b = uint(bitLen % 8);
    if (b == 0) {
        b = 8;
    }
    var bytes = make_slice<byte>(k);

    while (true) {
        _, err = io.ReadFull(rand, bytes);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        bytes[0] &= uint8(int(1 << (int)(b)) - 1);

        n.SetBytes(bytes);
        if (n.Cmp(max) < 0) {
            return ;
        }
    }

});

} // end rand_package
