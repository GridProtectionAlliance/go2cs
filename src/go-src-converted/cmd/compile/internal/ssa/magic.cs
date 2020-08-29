// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:07 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\magic.go
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // So you want to compute x / c for some constant c?
        // Machine division instructions are slow, so we try to
        // compute this division with a multiplication + a few
        // other cheap instructions instead.
        // (We assume here that c != 0, +/- 1, or +/- 2^i.  Those
        // cases are easy to handle in different ways).

        // Technique from https://gmplib.org/~tege/divcnst-pldi94.pdf

        // First consider unsigned division.
        // Our strategy is to precompute 1/c then do
        //   ⎣x / c⎦ = ⎣x * (1/c)⎦.
        // 1/c is less than 1, so we can't compute it directly in
        // integer arithmetic.  Let's instead compute 2^e/c
        // for a value of e TBD (^ = exponentiation).  Then
        //   ⎣x / c⎦ = ⎣x * (2^e/c) / 2^e⎦.
        // Dividing by 2^e is easy.  2^e/c isn't an integer, unfortunately.
        // So we must approximate it.  Let's call its approximation m.
        // We'll then compute
        //   ⎣x * m / 2^e⎦
        // Which we want to be equal to ⎣x / c⎦ for 0 <= x < 2^n-1
        // where n is the word size.
        // Setting x = c gives us c * m >= 2^e.
        // We'll chose m = ⎡2^e/c⎤ to satisfy that equation.
        // What remains is to choose e.
        // Let m = 2^e/c + delta, 0 <= delta < 1
        //   ⎣x * (2^e/c + delta) / 2^e⎦
        //   ⎣x / c + x * delta / 2^e⎦
        // We must have x * delta / 2^e < 1/c so that this
        // additional term never rounds differently than ⎣x / c⎦ does.
        // Rearranging,
        //   2^e > x * delta * c
        // x can be at most 2^n-1 and delta can be at most 1.
        // So it is sufficient to have 2^e >= 2^n*c.
        // So we'll choose e = n + s, with s = ⎡log2(c)⎤.
        //
        // An additional complication arises because m has n+1 bits in it.
        // Hardware restricts us to n bit by n bit multiplies.
        // We divide into 3 cases:
        //
        // Case 1: m is even.
        //   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
        //   ⎣x / c⎦ = ⎣x * (m/2) / 2^(n+s-1)⎦
        //   ⎣x / c⎦ = ⎣x * (m/2) / 2^n / 2^(s-1)⎦
        //   ⎣x / c⎦ = ⎣⎣x * (m/2) / 2^n⎦ / 2^(s-1)⎦
        //   multiply + shift
        //
        // Case 2: c is even.
        //   ⎣x / c⎦ = ⎣(x/2) / (c/2)⎦
        //   ⎣x / c⎦ = ⎣⎣x/2⎦ / (c/2)⎦
        //     This is just the original problem, with x' = ⎣x/2⎦, c' = c/2, n' = n-1.
        //       s' = s-1
        //       m' = ⎡2^(n'+s')/c'⎤
        //          = ⎡2^(n+s-1)/c⎤
        //          = ⎡m/2⎤
        //   ⎣x / c⎦ = ⎣x' * m' / 2^(n'+s')⎦
        //   ⎣x / c⎦ = ⎣⎣x/2⎦ * ⎡m/2⎤ / 2^(n+s-2)⎦
        //   ⎣x / c⎦ = ⎣⎣⎣x/2⎦ * ⎡m/2⎤ / 2^n⎦ / 2^(s-2)⎦
        //   shift + multiply + shift
        //
        // Case 3: everything else
        //   let k = m - 2^n. k fits in n bits.
        //   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
        //   ⎣x / c⎦ = ⎣x * (2^n + k) / 2^(n+s)⎦
        //   ⎣x / c⎦ = ⎣(x + x * k / 2^n) / 2^s⎦
        //   ⎣x / c⎦ = ⎣(x + ⎣x * k / 2^n⎦) / 2^s⎦
        //   ⎣x / c⎦ = ⎣(x + ⎣x * k / 2^n⎦) / 2^s⎦
        //   ⎣x / c⎦ = ⎣⎣(x + ⎣x * k / 2^n⎦) / 2⎦ / 2^(s-1)⎦
        //   multiply + avg + shift
        //
        // These can be implemented in hardware using:
        //  ⎣a * b / 2^n⎦ - aka high n bits of an n-bit by n-bit multiply.
        //  ⎣(a+b) / 2⎦   - aka "average" of two n-bit numbers.
        //                  (Not just a regular add & shift because the intermediate result
        //                   a+b has n+1 bits in it.  Nevertheless, can be done
        //                   in 2 instructions on x86.)

        // umagicOK returns whether we should strength reduce a n-bit divide by c.
        private static bool umagicOK(ulong n, long c)
        { 
            // Convert from ConstX auxint values to the real uint64 constant they represent.
            var d = uint64(c) << (int)((64L - n)) >> (int)((64L - n)); 

            // Doesn't work for 0.
            // Don't use for powers of 2.
            return d & (d - 1L) != 0L;
        }

        private partial struct umagicData
        {
            public long s; // ⎡log2(c)⎤
            public ulong m; // ⎡2^(n+s)/c⎤ - 2^n
        }

        // umagic computes the constants needed to strength reduce unsigned n-bit divides by the constant uint64(c).
        // The return values satisfy for all 0 <= x < 2^n
        //  floor(x / uint64(c)) = x * (m + 2^n) >> (n+s)
        private static umagicData umagic(ulong n, long c) => func((_, panic, __) =>
        { 
            // Convert from ConstX auxint values to the real uint64 constant they represent.
            var d = uint64(c) << (int)((64L - n)) >> (int)((64L - n));

            ptr<object> C = @new<big.Int>().SetUint64(d);
            var s = C.BitLen();
            var M = big.NewInt(1L);
            M.Lsh(M, n + uint(s)); // 2^(n+s)
            M.Add(M, C); // 2^(n+s)+c
            M.Sub(M, big.NewInt(1L)); // 2^(n+s)+c-1
            M.Div(M, C); // ⎡2^(n+s)/c⎤
            if (M.Bit(int(n)) != 1L)
            {
                panic("n+1st bit isn't set");
            }
            M.SetBit(M, int(n), 0L);
            var m = M.Uint64();
            return new umagicData(s:int64(s),m:m);
        });

        // For signed division, we use a similar strategy.
        // First, we enforce a positive c.
        //   x / c = -(x / (-c))
        // This will require an additional Neg op for c<0.
        //
        // If x is positive we're in a very similar state
        // to the unsigned case above.  We define:
        //   s = ⎡log2(c)⎤-1
        //   m = ⎡2^(n+s)/c⎤
        // Then
        //   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
        // If x is negative we have
        //   ⎡x / c⎤ = ⎣x * m / 2^(n+s)⎦ + 1
        // (TODO: derivation?)
        //
        // The multiply is a bit odd, as it is a signed n-bit value
        // times an unsigned n-bit value.  For n smaller than the
        // word size, we can extend x and m appropriately and use the
        // signed multiply instruction.  For n == word size,
        // we must use the signed multiply high and correct
        // the result by adding x*2^n.
        //
        // Adding 1 if x<0 is done by subtracting x>>(n-1).

        private static bool smagicOK(ulong n, long c)
        {
            if (c < 0L)
            { 
                // Doesn't work for negative c.
                return false;
            } 
            // Doesn't work for 0.
            // Don't use it for powers of 2.
            return c & (c - 1L) != 0L;
        }

        private partial struct smagicData
        {
            public long s; // ⎡log2(c)⎤-1
            public ulong m; // ⎡2^(n+s)/c⎤
        }

        // magic computes the constants needed to strength reduce signed n-bit divides by the constant c.
        // Must have c>0.
        // The return values satisfy for all -2^(n-1) <= x < 2^(n-1)
        //  trunc(x / c) = x * m >> (n+s) + (x < 0 ? 1 : 0)
        private static smagicData smagic(ulong n, long c) => func((_, panic, __) =>
        {
            ptr<object> C = @new<big.Int>().SetInt64(c);
            var s = C.BitLen() - 1L;
            var M = big.NewInt(1L);
            M.Lsh(M, n + uint(s)); // 2^(n+s)
            M.Add(M, C); // 2^(n+s)+c
            M.Sub(M, big.NewInt(1L)); // 2^(n+s)+c-1
            M.Div(M, C); // ⎡2^(n+s)/c⎤
            if (M.Bit(int(n)) != 0L)
            {
                panic("n+1st bit is set");
            }
            if (M.Bit(int(n - 1L)) == 0L)
            {
                panic("nth bit is not set");
            }
            var m = M.Uint64();
            return new smagicData(s:int64(s),m:m);
        });
    }
}}}}
