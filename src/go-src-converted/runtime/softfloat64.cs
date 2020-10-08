// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Software IEEE754 64-bit floating point.
// Only referred to (and thus linked in) by arm port
// and by tests in this directory.

// package runtime -- go2cs converted at 2020 October 08 03:23:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\softfloat64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong mantbits64 = (ulong)52L;
        private static readonly ulong expbits64 = (ulong)11L;
        private static readonly long bias64 = (long)-1L << (int)((expbits64 - 1L)) + 1L;

        private static readonly ulong nan64 = (ulong)(1L << (int)(expbits64) - 1L) << (int)(mantbits64) + 1L << (int)((mantbits64 - 1L)); // quiet NaN, 0 payload
        private static readonly ulong inf64 = (ulong)(1L << (int)(expbits64) - 1L) << (int)(mantbits64);
        private static readonly ulong neg64 = (ulong)1L << (int)((expbits64 + mantbits64));

        private static readonly ulong mantbits32 = (ulong)23L;
        private static readonly ulong expbits32 = (ulong)8L;
        private static readonly long bias32 = (long)-1L << (int)((expbits32 - 1L)) + 1L;

        private static readonly uint nan32 = (uint)(1L << (int)(expbits32) - 1L) << (int)(mantbits32) + 1L << (int)((mantbits32 - 1L)); // quiet NaN, 0 payload
        private static readonly uint inf32 = (uint)(1L << (int)(expbits32) - 1L) << (int)(mantbits32);
        private static readonly uint neg32 = (uint)1L << (int)((expbits32 + mantbits32));


        private static (ulong, ulong, long, bool, bool) funpack64(ulong f)
        {
            ulong sign = default;
            ulong mant = default;
            long exp = default;
            bool inf = default;
            bool nan = default;

            sign = f & (1L << (int)((mantbits64 + expbits64)));
            mant = f & (1L << (int)(mantbits64) - 1L);
            exp = int(f >> (int)(mantbits64)) & (1L << (int)(expbits64) - 1L);

            switch (exp)
            {
                case 1L << (int)(expbits64) - 1L: 
                    if (mant != 0L)
                    {
                        nan = true;
                        return ;
                    }

                    inf = true;
                    return ;
                    break;
                case 0L: 
                    // denormalized
                    if (mant != 0L)
                    {
                        exp += bias64 + 1L;
                        while (mant < 1L << (int)(mantbits64))
                        {
                            mant <<= 1L;
                            exp--;
                        }


                    }

                    break;
                default: 
                    // add implicit top bit
                    mant |= 1L << (int)(mantbits64);
                    exp += bias64;
                    break;
            }
            return ;

        }

        private static (uint, uint, long, bool, bool) funpack32(uint f)
        {
            uint sign = default;
            uint mant = default;
            long exp = default;
            bool inf = default;
            bool nan = default;

            sign = f & (1L << (int)((mantbits32 + expbits32)));
            mant = f & (1L << (int)(mantbits32) - 1L);
            exp = int(f >> (int)(mantbits32)) & (1L << (int)(expbits32) - 1L);

            switch (exp)
            {
                case 1L << (int)(expbits32) - 1L: 
                    if (mant != 0L)
                    {
                        nan = true;
                        return ;
                    }

                    inf = true;
                    return ;
                    break;
                case 0L: 
                    // denormalized
                    if (mant != 0L)
                    {
                        exp += bias32 + 1L;
                        while (mant < 1L << (int)(mantbits32))
                        {
                            mant <<= 1L;
                            exp--;
                        }


                    }

                    break;
                default: 
                    // add implicit top bit
                    mant |= 1L << (int)(mantbits32);
                    exp += bias32;
                    break;
            }
            return ;

        }

        private static ulong fpack64(ulong sign, ulong mant, long exp, ulong trunc)
        {
            var mant0 = mant;
            var exp0 = exp;
            var trunc0 = trunc;
            if (mant == 0L)
            {
                return sign;
            }

            while (mant < 1L << (int)(mantbits64))
            {
                mant <<= 1L;
                exp--;
            }

            while (mant >= 4L << (int)(mantbits64))
            {
                trunc |= mant & 1L;
                mant >>= 1L;
                exp++;
            }

            if (mant >= 2L << (int)(mantbits64))
            {
                if (mant & 1L != 0L && (trunc != 0L || mant & 2L != 0L))
                {
                    mant++;
                    if (mant >= 4L << (int)(mantbits64))
                    {
                        mant >>= 1L;
                        exp++;
                    }

                }

                mant >>= 1L;
                exp++;

            }

            if (exp >= 1L << (int)(expbits64) - 1L + bias64)
            {
                return sign ^ inf64;
            }

            if (exp < bias64 + 1L)
            {
                if (exp < bias64 - int(mantbits64))
                {
                    return sign | 0L;
                } 
                // repeat expecting denormal
                mant = mant0;
                exp = exp0;
                trunc = trunc0;
                while (exp < bias64)
                {
                    trunc |= mant & 1L;
                    mant >>= 1L;
                    exp++;
                }

                if (mant & 1L != 0L && (trunc != 0L || mant & 2L != 0L))
                {
                    mant++;
                }

                mant >>= 1L;
                exp++;
                if (mant < 1L << (int)(mantbits64))
                {
                    return sign | mant;
                }

            }

            return sign | uint64(exp - bias64) << (int)(mantbits64) | mant & (1L << (int)(mantbits64) - 1L);

        }

        private static uint fpack32(uint sign, uint mant, long exp, uint trunc)
        {
            var mant0 = mant;
            var exp0 = exp;
            var trunc0 = trunc;
            if (mant == 0L)
            {
                return sign;
            }

            while (mant < 1L << (int)(mantbits32))
            {
                mant <<= 1L;
                exp--;
            }

            while (mant >= 4L << (int)(mantbits32))
            {
                trunc |= mant & 1L;
                mant >>= 1L;
                exp++;
            }

            if (mant >= 2L << (int)(mantbits32))
            {
                if (mant & 1L != 0L && (trunc != 0L || mant & 2L != 0L))
                {
                    mant++;
                    if (mant >= 4L << (int)(mantbits32))
                    {
                        mant >>= 1L;
                        exp++;
                    }

                }

                mant >>= 1L;
                exp++;

            }

            if (exp >= 1L << (int)(expbits32) - 1L + bias32)
            {
                return sign ^ inf32;
            }

            if (exp < bias32 + 1L)
            {
                if (exp < bias32 - int(mantbits32))
                {
                    return sign | 0L;
                } 
                // repeat expecting denormal
                mant = mant0;
                exp = exp0;
                trunc = trunc0;
                while (exp < bias32)
                {
                    trunc |= mant & 1L;
                    mant >>= 1L;
                    exp++;
                }

                if (mant & 1L != 0L && (trunc != 0L || mant & 2L != 0L))
                {
                    mant++;
                }

                mant >>= 1L;
                exp++;
                if (mant < 1L << (int)(mantbits32))
                {
                    return sign | mant;
                }

            }

            return sign | uint32(exp - bias32) << (int)(mantbits32) | mant & (1L << (int)(mantbits32) - 1L);

        }

        private static ulong fadd64(ulong f, ulong g)
        {
            var (fs, fm, fe, fi, fn) = funpack64(f);
            var (gs, gm, ge, gi, gn) = funpack64(g); 

            // Special cases.

            if (fn || gn) // NaN + x or x + NaN = NaN
                return nan64;
            else if (fi && gi && fs != gs) // +Inf + -Inf or -Inf + +Inf = NaN
                return nan64;
            else if (fi) // ±Inf + g = ±Inf
                return f;
            else if (gi) // f + ±Inf = ±Inf
                return g;
            else if (fm == 0L && gm == 0L && fs != 0L && gs != 0L) // -0 + -0 = -0
                return f;
            else if (fm == 0L) // 0 + g = g but 0 + -0 = +0
                if (gm == 0L)
                {
                    g ^= gs;
                }

                return g;
            else if (gm == 0L) // f + 0 = f
                return f;
                        if (fe < ge || fe == ge && fm < gm)
            {
                f = g;
                g = f;
                fs = gs;
                fm = gm;
                fe = ge;
                gs = fs;
                gm = fm;
                ge = fe;

            }

            var shift = uint(fe - ge);
            fm <<= 2L;
            gm <<= 2L;
            var trunc = gm & (1L << (int)(shift) - 1L);
            gm >>= shift;
            if (fs == gs)
            {
                fm += gm;
            }
            else
            {
                fm -= gm;
                if (trunc != 0L)
                {
                    fm--;
                }

            }

            if (fm == 0L)
            {
                fs = 0L;
            }

            return fpack64(fs, fm, fe - 2L, trunc);

        }

        private static ulong fsub64(ulong f, ulong g)
        {
            return fadd64(f, fneg64(g));
        }

        private static ulong fneg64(ulong f)
        {
            return f ^ (1L << (int)((mantbits64 + expbits64)));
        }

        private static ulong fmul64(ulong f, ulong g)
        {
            var (fs, fm, fe, fi, fn) = funpack64(f);
            var (gs, gm, ge, gi, gn) = funpack64(g); 

            // Special cases.

            if (fn || gn) // NaN * g or f * NaN = NaN
                return nan64;
            else if (fi && gi) // Inf * Inf = Inf (with sign adjusted)
                return f ^ gs;
            else if (fi && gm == 0L || fm == 0L && gi) // 0 * Inf = Inf * 0 = NaN
                return nan64;
            else if (fm == 0L) // 0 * x = 0 (with sign adjusted)
                return f ^ gs;
            else if (gm == 0L) // x * 0 = 0 (with sign adjusted)
                return g ^ fs;
            // 53-bit * 53-bit = 107- or 108-bit
            var (lo, hi) = mullu(fm, gm);
            var shift = mantbits64 - 1L;
            var trunc = lo & (1L << (int)(shift) - 1L);
            var mant = hi << (int)((64L - shift)) | lo >> (int)(shift);
            return fpack64(fs ^ gs, mant, fe + ge - 1L, trunc);

        }

        private static ulong fdiv64(ulong f, ulong g)
        {
            var (fs, fm, fe, fi, fn) = funpack64(f);
            var (gs, gm, ge, gi, gn) = funpack64(g); 

            // Special cases.

            if (fn || gn) // NaN / g = f / NaN = NaN
                return nan64;
            else if (fi && gi) // ±Inf / ±Inf = NaN
                return nan64;
            else if (!fi && !gi && fm == 0L && gm == 0L) // 0 / 0 = NaN
                return nan64;
            else if (fi || !gi && gm == 0L) // Inf / g = f / 0 = Inf
                return fs ^ gs ^ inf64;
            else if (gi || fm == 0L) // f / Inf = 0 / g = Inf
                return fs ^ gs ^ 0L;
                        _ = fi;
            _ = fn;
            _ = gi;
            _ = gn; 

            // 53-bit<<54 / 53-bit = 53- or 54-bit.
            var shift = mantbits64 + 2L;
            var (q, r) = divlu(fm >> (int)((64L - shift)), fm << (int)(shift), gm);
            return fpack64(fs ^ gs, q, fe - ge - 2L, r);

        }

        private static uint f64to32(ulong f)
        {
            var (fs, fm, fe, fi, fn) = funpack64(f);
            if (fn)
            {
                return nan32;
            }

            var fs32 = uint32(fs >> (int)(32L));
            if (fi)
            {
                return fs32 ^ inf32;
            }

            const var d = (var)mantbits64 - mantbits32 - 1L;

            return fpack32(fs32, uint32(fm >> (int)(d)), fe - 1L, uint32(fm & (1L << (int)(d) - 1L)));

        }

        private static ulong f32to64(uint f)
        {
            const var d = (var)mantbits64 - mantbits32;

            var (fs, fm, fe, fi, fn) = funpack32(f);
            if (fn)
            {
                return nan64;
            }

            var fs64 = uint64(fs) << (int)(32L);
            if (fi)
            {
                return fs64 ^ inf64;
            }

            return fpack64(fs64, uint64(fm) << (int)(d), fe, 0L);

        }

        private static (int, bool) fcmp64(ulong f, ulong g)
        {
            int cmp = default;
            bool isnan = default;

            var (fs, fm, _, fi, fn) = funpack64(f);
            var (gs, gm, _, gi, gn) = funpack64(g);


            if (fn || gn) // flag NaN
                return (0L, true);
            else if (!fi && !gi && fm == 0L && gm == 0L) // ±0 == ±0
                return (0L, false);
            else if (fs > gs) // f < 0, g > 0
                return (-1L, false);
            else if (fs < gs) // f > 0, g < 0
                return (+1L, false); 

                // Same sign, not NaN.
                // Can compare encodings directly now.
                // Reverse for sign.
            else if (fs == 0L && f < g || fs != 0L && f > g) 
                return (-1L, false);
            else if (fs == 0L && f > g || fs != 0L && f < g) 
                return (+1L, false);
            // f == g
            return (0L, false);

        }

        private static (long, bool) f64toint(ulong f)
        {
            long val = default;
            bool ok = default;

            var (fs, fm, fe, fi, fn) = funpack64(f);


            if (fi || fn) // NaN
                return (0L, false);
            else if (fe < -1L) // f < 0.5
                return (0L, false);
            else if (fe > 63L) // f >= 2^63
                if (fs != 0L && fm == 0L)
                { // f == -2^63
                    return (-1L << (int)(63L), true);

                }

                if (fs != 0L)
                {
                    return (0L, false);
                }

                return (0L, false);
                        while (fe > int(mantbits64))
            {
                fe--;
                fm <<= 1L;
            }

            while (fe < int(mantbits64))
            {
                fe++;
                fm >>= 1L;
            }

            val = int64(fm);
            if (fs != 0L)
            {
                val = -val;
            }

            return (val, true);

        }

        private static ulong fintto64(long val)
        {
            ulong f = default;

            var fs = uint64(val) & (1L << (int)(63L));
            var mant = uint64(val);
            if (fs != 0L)
            {
                mant = -mant;
            }

            return fpack64(fs, mant, int(mantbits64), 0L);

        }

        // 64x64 -> 128 multiply.
        // adapted from hacker's delight.
        private static (ulong, ulong) mullu(ulong u, ulong v)
        {
            ulong lo = default;
            ulong hi = default;

            const long s = (long)32L;
            const long mask = (long)1L << (int)(s) - 1L;
            var u0 = u & mask;
            var u1 = u >> (int)(s);
            var v0 = v & mask;
            var v1 = v >> (int)(s);
            var w0 = u0 * v0;
            var t = u1 * v0 + w0 >> (int)(s);
            var w1 = t & mask;
            var w2 = t >> (int)(s);
            w1 += u0 * v1;
            return (u * v, u1 * v1 + w2 + w1 >> (int)(s));
        }

        // 128/64 -> 64 quotient, 64 remainder.
        // adapted from hacker's delight
        private static (ulong, ulong) divlu(ulong u1, ulong u0, ulong v)
        {
            ulong q = default;
            ulong r = default;

            const long b = (long)1L << (int)(32L);



            if (u1 >= v)
            {
                return (1L << (int)(64L) - 1L, 1L << (int)(64L) - 1L);
            } 

            // s = nlz(v); v <<= s
            var s = uint(0L);
            while (v & (1L << (int)(63L)) == 0L)
            {
                s++;
                v <<= 1L;
            }


            var vn1 = v >> (int)(32L);
            var vn0 = v & (1L << (int)(32L) - 1L);
            var un32 = u1 << (int)(s) | u0 >> (int)((64L - s));
            var un10 = u0 << (int)(s);
            var un1 = un10 >> (int)(32L);
            var un0 = un10 & (1L << (int)(32L) - 1L);
            var q1 = un32 / vn1;
            var rhat = un32 - q1 * vn1;

again1:

            if (q1 >= b || q1 * vn0 > b * rhat + un1)
            {
                q1--;
                rhat += vn1;
                if (rhat < b)
                {
                    goto again1;
                }

            }

            var un21 = un32 * b + un1 - q1 * v;
            var q0 = un21 / vn1;
            rhat = un21 - q0 * vn1;

again2:

            if (q0 >= b || q0 * vn0 > b * rhat + un0)
            {
                q0--;
                rhat += vn1;
                if (rhat < b)
                {
                    goto again2;
                }

            }

            return (q1 * b + q0, (un21 * b + un0 - q0 * v) >> (int)(s));

        }

        private static uint fadd32(uint x, uint y)
        {
            return f64to32(fadd64(f32to64(x), f32to64(y)));
        }

        private static uint fmul32(uint x, uint y)
        {
            return f64to32(fmul64(f32to64(x), f32to64(y)));
        }

        private static uint fdiv32(uint x, uint y)
        {
            return f64to32(fdiv64(f32to64(x), f32to64(y)));
        }

        private static bool feq32(uint x, uint y)
        {
            var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
            return cmp == 0L && !nan;
        }

        private static bool fgt32(uint x, uint y)
        {
            var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
            return cmp >= 1L && !nan;
        }

        private static bool fge32(uint x, uint y)
        {
            var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
            return cmp >= 0L && !nan;
        }

        private static bool feq64(ulong x, ulong y)
        {
            var (cmp, nan) = fcmp64(x, y);
            return cmp == 0L && !nan;
        }

        private static bool fgt64(ulong x, ulong y)
        {
            var (cmp, nan) = fcmp64(x, y);
            return cmp >= 1L && !nan;
        }

        private static bool fge64(ulong x, ulong y)
        {
            var (cmp, nan) = fcmp64(x, y);
            return cmp >= 0L && !nan;
        }

        private static uint fint32to32(int x)
        {
            return f64to32(fintto64(int64(x)));
        }

        private static ulong fint32to64(int x)
        {
            return fintto64(int64(x));
        }

        private static uint fint64to32(long x)
        {
            return f64to32(fintto64(x));
        }

        private static ulong fint64to64(long x)
        {
            return fintto64(x);
        }

        private static int f32toint32(uint x)
        {
            var (val, _) = f64toint(f32to64(x));
            return int32(val);
        }

        private static long f32toint64(uint x)
        {
            var (val, _) = f64toint(f32to64(x));
            return val;
        }

        private static int f64toint32(ulong x)
        {
            var (val, _) = f64toint(x);
            return int32(val);
        }

        private static long f64toint64(ulong x)
        {
            var (val, _) = f64toint(x);
            return val;
        }

        private static ulong f64touint64(double x)
        {
            if (x < float64(1L << (int)(63L)))
            {
                return uint64(int64(x));
            }

            var y = x - float64(1L << (int)(63L));
            var z = uint64(int64(y));
            return z | (1L << (int)(63L));

        }

        private static ulong f32touint64(float x)
        {
            if (x < float32(1L << (int)(63L)))
            {
                return uint64(int64(x));
            }

            var y = x - float32(1L << (int)(63L));
            var z = uint64(int64(y));
            return z | (1L << (int)(63L));

        }

        private static double fuint64to64(ulong x)
        {
            if (int64(x) >= 0L)
            {
                return float64(int64(x));
            } 
            // See ../cmd/compile/internal/gc/ssa.go:uint64Tofloat
            var y = x & 1L;
            var z = x >> (int)(1L);
            z = z | y;
            var r = float64(int64(z));
            return r + r;

        }

        private static float fuint64to32(ulong x)
        {
            return float32(fuint64to64(x));
        }
    }
}
