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

//go:build arm || 386 || mips || mipsle
// +build arm 386 mips mipsle

// package runtime -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vlrt.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

private static readonly nint sign32 = 1 << (int)((32 - 1));
private static readonly nint sign64 = 1 << (int)((64 - 1));

private static ulong float64toint64(double d) {
    ulong y = default;

    _d2v(_addr_y, d);
    return ;
}

private static ulong float64touint64(double d) {
    ulong y = default;

    _d2v(_addr_y, d);
    return ;
}

private static double int64tofloat64(long y) {
    if (y < 0) {
        return -uint64tofloat64(-uint64(y));
    }
    return uint64tofloat64(uint64(y));
}

private static double uint64tofloat64(ulong y) {
    var hi = float64(uint32(y >> 32));
    var lo = float64(uint32(y));
    var d = hi * (1 << 32) + lo;
    return d;
}

private static void _d2v(ptr<ulong> _addr_y, double d) {
    ref ulong y = ref _addr_y.val;

    ptr<ptr<ulong>> x = new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_d));

    var xhi = uint32(x >> 32) & 0xfffff | 0x100000;
    var xlo = uint32(x);
    nint sh = 1075 - int32(uint32(x >> 52) & 0x7ff);

    uint ylo = default;    uint yhi = default;

    if (sh >= 0) {
        sh = uint32(sh); 
        /* v = (hi||lo) >> sh */
        if (sh < 32) {
            if (sh == 0) {
                ylo = xlo;
                yhi = xhi;
            }
            else
 {
                ylo = xlo >> (int)(sh) | xhi << (int)((32 - sh));
                yhi = xhi >> (int)(sh);
            }
        }
        else
 {
            if (sh == 32) {
                ylo = xhi;
            }
            else if (sh < 64) {
                ylo = xhi >> (int)((sh - 32));
            }
        }
    }
    else
 { 
        /* v = (hi||lo) << -sh */
        sh = uint32(-sh);
        if (sh <= 11) {
            ylo = xlo << (int)(sh);
            yhi = xhi << (int)(sh) | xlo >> (int)((32 - sh));
        }
        else
 { 
            /* overflow */
            yhi = uint32(d); /* causes something awful */
        }
    }
    if (x & sign64 != 0) {
        if (ylo != 0) {
            ylo = -ylo;
            yhi = ~yhi;
        }
        else
 {
            yhi = -yhi;
        }
    }
    y = uint64(yhi) << 32 | uint64(ylo);
}
private static ulong uint64div(ulong n, ulong d) { 
    // Check for 32 bit operands
    if (uint32(n >> 32) == 0 && uint32(d >> 32) == 0) {
        if (uint32(d) == 0) {
            panicdivide();
        }
        return uint64(uint32(n) / uint32(d));
    }
    var (q, _) = dodiv(n, d);
    return q;
}

private static ulong uint64mod(ulong n, ulong d) { 
    // Check for 32 bit operands
    if (uint32(n >> 32) == 0 && uint32(d >> 32) == 0) {
        if (uint32(d) == 0) {
            panicdivide();
        }
        return uint64(uint32(n) % uint32(d));
    }
    var (_, r) = dodiv(n, d);
    return r;
}

private static long int64div(long n, long d) { 
    // Check for 32 bit operands
    if (int64(int32(n)) == n && int64(int32(d)) == d) {
        if (int32(n) == -0x80000000 && int32(d) == -1) { 
            // special case: 32-bit -0x80000000 / -1 = -0x80000000,
            // but 64-bit -0x80000000 / -1 = 0x80000000.
            return 0x80000000;
        }
        if (int32(d) == 0) {
            panicdivide();
        }
        return int64(int32(n) / int32(d));
    }
    var nneg = n < 0;
    var dneg = d < 0;
    if (nneg) {
        n = -n;
    }
    if (dneg) {
        d = -d;
    }
    var (uq, _) = dodiv(uint64(n), uint64(d));
    var q = int64(uq);
    if (nneg != dneg) {
        q = -q;
    }
    return q;
}

//go:nosplit
private static long int64mod(long n, long d) { 
    // Check for 32 bit operands
    if (int64(int32(n)) == n && int64(int32(d)) == d) {
        if (int32(d) == 0) {
            panicdivide();
        }
        return int64(int32(n) % int32(d));
    }
    var nneg = n < 0;
    if (nneg) {
        n = -n;
    }
    if (d < 0) {
        d = -d;
    }
    var (_, ur) = dodiv(uint64(n), uint64(d));
    var r = int64(ur);
    if (nneg) {
        r = -r;
    }
    return r;
}

//go:noescape
private static uint _mul64by32(ptr<ulong> lo64, ulong a, uint b);

//go:noescape
private static uint _div64by32(ulong a, uint b, ptr<uint> r);

//go:nosplit
private static (ulong, ulong) dodiv(ulong n, ulong d) {
    ulong q = default;
    ulong r = default;

    if (GOARCH == "arm") {>>MARKER:FUNCTION__div64by32_BLOCK_PREFIX<< 
        // arm doesn't have a division instruction, so
        // slowdodiv is the best that we can do.
        return slowdodiv(n, d);
    }
    if (GOARCH == "mips" || GOARCH == "mipsle") {>>MARKER:FUNCTION__mul64by32_BLOCK_PREFIX<< 
        // No _div64by32 on mips and using only _mul64by32 doesn't bring much benefit
        return slowdodiv(n, d);
    }
    if (d > n) {
        return (0, n);
    }
    if (uint32(d >> 32) != 0) {
        var t = uint32(n >> 32) / uint32(d >> 32);
        ref ulong lo64 = ref heap(out ptr<ulong> _addr_lo64);
        var hi32 = _mul64by32(_addr_lo64, d, t);
        if (hi32 != 0 || lo64 > n) {
            return slowdodiv(n, d);
        }
        return (uint64(t), n - lo64);
    }
    uint qhi = default;
    if (uint32(n >> 32) >= uint32(d)) {
        if (uint32(d) == 0) {
            panicdivide();
        }
        qhi = uint32(n >> 32) / uint32(d);
        n -= uint64(uint32(d) * qhi) << 32;
    }
    else
 {
        qhi = 0;
    }
    ref uint rlo = ref heap(out ptr<uint> _addr_rlo);
    var qlo = _div64by32(n, uint32(d), _addr_rlo);
    return (uint64(qhi) << 32 + uint64(qlo), uint64(rlo));
}

//go:nosplit
private static (ulong, ulong) slowdodiv(ulong n, ulong d) {
    ulong q = default;
    ulong r = default;

    if (d == 0) {
        panicdivide();
    }
    var capn = n;
    if (n >= sign64) {
        capn = sign64;
    }
    nint i = 0;
    while (d < capn) {
        d<<=1;
        i++;
    }

    while (i >= 0) {
        q<<=1;
        if (n >= d) {
            n -= d;
            q |= 1;
        i--;
        }
        d>>=1;
    }
    return (q, n);
}

// Floating point control word values.
// Bits 0-5 are bits to disable floating-point exceptions.
// Bits 8-9 are the precision control:
//   0 = single precision a.k.a. float32
//   2 = double precision a.k.a. float64
// Bits 10-11 are the rounding mode:
//   0 = round to nearest (even on a tie)
//   3 = round toward zero
private static ushort controlWord64 = 0x3f + 2 << 8 + 0 << 10;private static ushort controlWord64trunc = 0x3f + 2 << 8 + 3 << 10;

} // end runtime_package
