// Inferno's libkern/vlrt-arm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/libkern/vlrt-arm.c
//
//         Copyright © 1994-1999 Lucent Technologies Inc. All rights reserved.
//         Revisions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com).  All rights reserved.
//         Portions Copyright 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// +build arm 386 mips mipsle

// package runtime -- go2cs converted at 2020 October 08 03:24:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vlrt.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long sign32 = (long)1L << (int)((32L - 1L));
        private static readonly long sign64 = (long)1L << (int)((64L - 1L));


        private static ulong float64toint64(double d)
        {
            ulong y = default;

            _d2v(_addr_y, d);
            return ;
        }

        private static ulong float64touint64(double d)
        {
            ulong y = default;

            _d2v(_addr_y, d);
            return ;
        }

        private static double int64tofloat64(long y)
        {
            if (y < 0L)
            {
                return -uint64tofloat64(-uint64(y));
            }

            return uint64tofloat64(uint64(y));

        }

        private static double uint64tofloat64(ulong y)
        {
            var hi = float64(uint32(y >> (int)(32L)));
            var lo = float64(uint32(y));
            var d = hi * (1L << (int)(32L)) + lo;
            return d;
        }

        private static void _d2v(ptr<ulong> _addr_y, double d)
        {
            ref ulong y = ref _addr_y.val;

            ptr<ptr<ulong>> x = new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_d));

            var xhi = uint32(x >> (int)(32L)) & 0xfffffUL | 0x100000UL;
            var xlo = uint32(x);
            long sh = 1075L - int32(uint32(x >> (int)(52L)) & 0x7ffUL);

            uint ylo = default;            uint yhi = default;

            if (sh >= 0L)
            {
                sh = uint32(sh); 
                /* v = (hi||lo) >> sh */
                if (sh < 32L)
                {
                    if (sh == 0L)
                    {
                        ylo = xlo;
                        yhi = xhi;
                    }
                    else
                    {
                        ylo = xlo >> (int)(sh) | xhi << (int)((32L - sh));
                        yhi = xhi >> (int)(sh);
                    }

                }
                else
                {
                    if (sh == 32L)
                    {
                        ylo = xhi;
                    }
                    else if (sh < 64L)
                    {
                        ylo = xhi >> (int)((sh - 32L));
                    }

                }

            }
            else
            { 
                /* v = (hi||lo) << -sh */
                sh = uint32(-sh);
                if (sh <= 11L)
                {
                    ylo = xlo << (int)(sh);
                    yhi = xhi << (int)(sh) | xlo >> (int)((32L - sh));
                }
                else
                { 
                    /* overflow */
                    yhi = uint32(d); /* causes something awful */
                }

            }

            if (x & sign64 != 0L)
            {
                if (ylo != 0L)
                {
                    ylo = -ylo;
                    yhi = ~yhi;
                }
                else
                {
                    yhi = -yhi;
                }

            }

            y = uint64(yhi) << (int)(32L) | uint64(ylo);

        }
        private static ulong uint64div(ulong n, ulong d)
        { 
            // Check for 32 bit operands
            if (uint32(n >> (int)(32L)) == 0L && uint32(d >> (int)(32L)) == 0L)
            {
                if (uint32(d) == 0L)
                {
                    panicdivide();
                }

                return uint64(uint32(n) / uint32(d));

            }

            var (q, _) = dodiv(n, d);
            return q;

        }

        private static ulong uint64mod(ulong n, ulong d)
        { 
            // Check for 32 bit operands
            if (uint32(n >> (int)(32L)) == 0L && uint32(d >> (int)(32L)) == 0L)
            {
                if (uint32(d) == 0L)
                {
                    panicdivide();
                }

                return uint64(uint32(n) % uint32(d));

            }

            var (_, r) = dodiv(n, d);
            return r;

        }

        private static long int64div(long n, long d)
        { 
            // Check for 32 bit operands
            if (int64(int32(n)) == n && int64(int32(d)) == d)
            {
                if (int32(n) == -0x80000000UL && int32(d) == -1L)
                { 
                    // special case: 32-bit -0x80000000 / -1 = -0x80000000,
                    // but 64-bit -0x80000000 / -1 = 0x80000000.
                    return 0x80000000UL;

                }

                if (int32(d) == 0L)
                {
                    panicdivide();
                }

                return int64(int32(n) / int32(d));

            }

            var nneg = n < 0L;
            var dneg = d < 0L;
            if (nneg)
            {
                n = -n;
            }

            if (dneg)
            {
                d = -d;
            }

            var (uq, _) = dodiv(uint64(n), uint64(d));
            var q = int64(uq);
            if (nneg != dneg)
            {
                q = -q;
            }

            return q;

        }

        //go:nosplit
        private static long int64mod(long n, long d)
        { 
            // Check for 32 bit operands
            if (int64(int32(n)) == n && int64(int32(d)) == d)
            {
                if (int32(d) == 0L)
                {
                    panicdivide();
                }

                return int64(int32(n) % int32(d));

            }

            var nneg = n < 0L;
            if (nneg)
            {
                n = -n;
            }

            if (d < 0L)
            {
                d = -d;
            }

            var (_, ur) = dodiv(uint64(n), uint64(d));
            var r = int64(ur);
            if (nneg)
            {
                r = -r;
            }

            return r;

        }

        //go:noescape
        private static uint _mul64by32(ptr<ulong> lo64, ulong a, uint b)
;

        //go:noescape
        private static uint _div64by32(ulong a, uint b, ptr<uint> r)
;

        //go:nosplit
        private static (ulong, ulong) dodiv(ulong n, ulong d)
        {
            ulong q = default;
            ulong r = default;

            if (GOARCH == "arm")
            {>>MARKER:FUNCTION__div64by32_BLOCK_PREFIX<< 
                // arm doesn't have a division instruction, so
                // slowdodiv is the best that we can do.
                return slowdodiv(n, d);

            }

            if (GOARCH == "mips" || GOARCH == "mipsle")
            {>>MARKER:FUNCTION__mul64by32_BLOCK_PREFIX<< 
                // No _div64by32 on mips and using only _mul64by32 doesn't bring much benefit
                return slowdodiv(n, d);

            }

            if (d > n)
            {
                return (0L, n);
            }

            if (uint32(d >> (int)(32L)) != 0L)
            {
                var t = uint32(n >> (int)(32L)) / uint32(d >> (int)(32L));
                ref ulong lo64 = ref heap(out ptr<ulong> _addr_lo64);
                var hi32 = _mul64by32(_addr_lo64, d, t);
                if (hi32 != 0L || lo64 > n)
                {
                    return slowdodiv(n, d);
                }

                return (uint64(t), n - lo64);

            } 

            // d is 32 bit
            uint qhi = default;
            if (uint32(n >> (int)(32L)) >= uint32(d))
            {
                if (uint32(d) == 0L)
                {
                    panicdivide();
                }

                qhi = uint32(n >> (int)(32L)) / uint32(d);
                n -= uint64(uint32(d) * qhi) << (int)(32L);

            }
            else
            {
                qhi = 0L;
            }

            ref uint rlo = ref heap(out ptr<uint> _addr_rlo);
            var qlo = _div64by32(n, uint32(d), _addr_rlo);
            return (uint64(qhi) << (int)(32L) + uint64(qlo), uint64(rlo));

        }

        //go:nosplit
        private static (ulong, ulong) slowdodiv(ulong n, ulong d)
        {
            ulong q = default;
            ulong r = default;

            if (d == 0L)
            {
                panicdivide();
            } 

            // Set up the divisor and find the number of iterations needed.
            var capn = n;
            if (n >= sign64)
            {
                capn = sign64;
            }

            long i = 0L;
            while (d < capn)
            {
                d <<= 1L;
                i++;
            }


            while (i >= 0L)
            {
                q <<= 1L;
                if (n >= d)
                {
                    n -= d;
                    q |= 1L;
                i--;
                }

                d >>= 1L;

            }

            return (q, n);

        }

        // Floating point control word values for GOARCH=386 GO386=387.
        // Bits 0-5 are bits to disable floating-point exceptions.
        // Bits 8-9 are the precision control:
        //   0 = single precision a.k.a. float32
        //   2 = double precision a.k.a. float64
        // Bits 10-11 are the rounding mode:
        //   0 = round to nearest (even on a tie)
        //   3 = round toward zero
        private static ushort controlWord64 = 0x3fUL + 2L << (int)(8L) + 0L << (int)(10L);        private static ulong controlWord32 = 0x3fUL + 0L << (int)(8L) + 0L << (int)(10L);        private static ulong controlWord64trunc = 0x3fUL + 2L << (int)(8L) + 3L << (int)(10L);
    }
}
