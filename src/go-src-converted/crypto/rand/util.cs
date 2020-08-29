// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rand -- go2cs converted at 2020 August 29 08:30:54 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\util.go
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
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
        private static ptr<object> smallPrimesProduct = @new<big.Int>().SetUint64(16294579238595022365UL);

        // Prime returns a number, p, of the given size, such that p is prime
        // with high probability.
        // Prime will return error for any error returned by rand.Read or if bits < 2.
        public static (ref big.Int, error) Prime(io.Reader rand, long bits)
        {
            if (bits < 2L)
            {
                err = errors.New("crypto/rand: prime size must be at least 2-bit");
                return;
            }
            var b = uint(bits % 8L);
            if (b == 0L)
            {
                b = 8L;
            }
            var bytes = make_slice<byte>((bits + 7L) / 8L);
            p = @new<big.Int>();

            ptr<big.Int> bigMod = @new<big.Int>();

            while (true)
            {
                _, err = io.ReadFull(rand, bytes);
                if (err != null)
                {
                    return (null, err);
                } 

                // Clear bits in the first byte to make sure the candidate has a size <= bits.
                bytes[0L] &= uint8(int(1L << (int)(b)) - 1L); 
                // Don't let the value be too small, i.e, set the most significant two bits.
                // Setting the top two bits, rather than just the top bit,
                // means that when two of these values are multiplied together,
                // the result isn't ever one bit short.
                if (b >= 2L)
                {
                    bytes[0L] |= 3L << (int)((b - 2L));
                }
                else
                { 
                    // Here b==1, because b cannot be zero.
                    bytes[0L] |= 1L;
                    if (len(bytes) > 1L)
                    {
                        bytes[1L] |= 0x80UL;
                    }
                } 
                // Make the value odd since an even number this large certainly isn't prime.
                bytes[len(bytes) - 1L] |= 1L;

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
                    var delta = uint64(0L);

                    while (delta < 1L << (int)(20L))
                    {
                        var m = mod + delta;
                        foreach (var (_, prime) in smallPrimes)
                        {
                            if (m % uint64(prime) == 0L && (bits > 6L || m != uint64(prime)))
                            {
                                _continueNextDelta = true;
                                break;
                            }
                        delta += 2L;
                        }
                        if (delta > 0L)
                        {
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
                if (p.ProbablyPrime(20L) && p.BitLen() == bits)
                {
                    return;
                }
            }

        }

        // Int returns a uniform random value in [0, max). It panics if max <= 0.
        public static (ref big.Int, error) Int(io.Reader rand, ref big.Int _max) => func(_max, (ref big.Int max, Defer _, Panic panic, Recover __) =>
        {
            if (max.Sign() <= 0L)
            {
                panic("crypto/rand: argument to Int is <= 0");
            }
            n = @new<big.Int>();
            n.Sub(max, n.SetUint64(1L)); 
            // bitLen is the maximum bit length needed to encode a value < max.
            var bitLen = n.BitLen();
            if (bitLen == 0L)
            { 
                // the only valid result is 0
                return;
            } 
            // k is the maximum byte length needed to encode a value < max.
            var k = (bitLen + 7L) / 8L; 
            // b is the number of bits in the most significant byte of max-1.
            var b = uint(bitLen % 8L);
            if (b == 0L)
            {
                b = 8L;
            }
            var bytes = make_slice<byte>(k);

            while (true)
            {
                _, err = io.ReadFull(rand, bytes);
                if (err != null)
                {
                    return (null, err);
                } 

                // Clear bits in the first byte to increase the probability
                // that the candidate is < max.
                bytes[0L] &= uint8(int(1L << (int)(b)) - 1L);

                n.SetBytes(bytes);
                if (n.Cmp(max) < 0L)
                {
                    return;
                }
            }

        });
    }
}}
