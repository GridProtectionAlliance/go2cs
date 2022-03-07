// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Software IEEE754 64-bit floating point.
// Only referred to (and thus linked in) by arm port
// and by tests in this directory.

// package runtime -- go2cs converted at 2022 March 06 22:11:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\softfloat64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint mantbits64 = 52;
private static readonly nuint expbits64 = 11;
private static readonly nint bias64 = -1 << (int)((expbits64 - 1)) + 1;

private static readonly ulong nan64 = (1 << (int)(expbits64) - 1) << (int)(mantbits64) + 1 << (int)((mantbits64 - 1)); // quiet NaN, 0 payload
private static readonly ulong inf64 = (1 << (int)(expbits64) - 1) << (int)(mantbits64);
private static readonly ulong neg64 = 1 << (int)((expbits64 + mantbits64));

private static readonly nuint mantbits32 = 23;
private static readonly nuint expbits32 = 8;
private static readonly nint bias32 = -1 << (int)((expbits32 - 1)) + 1;

private static readonly uint nan32 = (1 << (int)(expbits32) - 1) << (int)(mantbits32) + 1 << (int)((mantbits32 - 1)); // quiet NaN, 0 payload
private static readonly uint inf32 = (1 << (int)(expbits32) - 1) << (int)(mantbits32);
private static readonly uint neg32 = 1 << (int)((expbits32 + mantbits32));


private static (ulong, ulong, nint, bool, bool) funpack64(ulong f) {
    ulong sign = default;
    ulong mant = default;
    nint exp = default;
    bool inf = default;
    bool nan = default;

    sign = f & (1 << (int)((mantbits64 + expbits64)));
    mant = f & (1 << (int)(mantbits64) - 1);
    exp = int(f >> (int)(mantbits64)) & (1 << (int)(expbits64) - 1);

    switch (exp) {
        case 1 << (int)(expbits64) - 1: 
            if (mant != 0) {
                nan = true;
                return ;
            }
            inf = true;
            return ;

            break;
        case 0: 
            // denormalized
            if (mant != 0) {
                exp += bias64 + 1;
                while (mant < 1 << (int)(mantbits64)) {
                    mant<<=1;
                    exp--;
                }
            }
            break;
        default: 
            // add implicit top bit
            mant |= 1 << (int)(mantbits64);
            exp += bias64;
            break;
    }
    return ;

}

private static (uint, uint, nint, bool, bool) funpack32(uint f) {
    uint sign = default;
    uint mant = default;
    nint exp = default;
    bool inf = default;
    bool nan = default;

    sign = f & (1 << (int)((mantbits32 + expbits32)));
    mant = f & (1 << (int)(mantbits32) - 1);
    exp = int(f >> (int)(mantbits32)) & (1 << (int)(expbits32) - 1);

    switch (exp) {
        case 1 << (int)(expbits32) - 1: 
            if (mant != 0) {
                nan = true;
                return ;
            }
            inf = true;
            return ;

            break;
        case 0: 
            // denormalized
            if (mant != 0) {
                exp += bias32 + 1;
                while (mant < 1 << (int)(mantbits32)) {
                    mant<<=1;
                    exp--;
                }
            }
            break;
        default: 
            // add implicit top bit
            mant |= 1 << (int)(mantbits32);
            exp += bias32;
            break;
    }
    return ;

}

private static ulong fpack64(ulong sign, ulong mant, nint exp, ulong trunc) {
    var mant0 = mant;
    var exp0 = exp;
    var trunc0 = trunc;
    if (mant == 0) {
        return sign;
    }
    while (mant < 1 << (int)(mantbits64)) {
        mant<<=1;
        exp--;
    }
    while (mant >= 4 << (int)(mantbits64)) {
        trunc |= mant & 1;
        mant>>=1;
        exp++;
    }
    if (mant >= 2 << (int)(mantbits64)) {
        if (mant & 1 != 0 && (trunc != 0 || mant & 2 != 0)) {
            mant++;
            if (mant >= 4 << (int)(mantbits64)) {
                mant>>=1;
                exp++;
            }
        }
        mant>>=1;
        exp++;

    }
    if (exp >= 1 << (int)(expbits64) - 1 + bias64) {
        return sign ^ inf64;
    }
    if (exp < bias64 + 1) {
        if (exp < bias64 - int(mantbits64)) {
            return sign | 0;
        }
        (mant, exp, trunc) = (mant0, exp0, trunc0);        while (exp < bias64) {
            trunc |= mant & 1;
            mant>>=1;
            exp++;
        }
        if (mant & 1 != 0 && (trunc != 0 || mant & 2 != 0)) {
            mant++;
        }
        mant>>=1;
        exp++;
        if (mant < 1 << (int)(mantbits64)) {
            return sign | mant;
        }
    }
    return sign | uint64(exp - bias64) << (int)(mantbits64) | mant & (1 << (int)(mantbits64) - 1);

}

private static uint fpack32(uint sign, uint mant, nint exp, uint trunc) {
    var mant0 = mant;
    var exp0 = exp;
    var trunc0 = trunc;
    if (mant == 0) {
        return sign;
    }
    while (mant < 1 << (int)(mantbits32)) {
        mant<<=1;
        exp--;
    }
    while (mant >= 4 << (int)(mantbits32)) {
        trunc |= mant & 1;
        mant>>=1;
        exp++;
    }
    if (mant >= 2 << (int)(mantbits32)) {
        if (mant & 1 != 0 && (trunc != 0 || mant & 2 != 0)) {
            mant++;
            if (mant >= 4 << (int)(mantbits32)) {
                mant>>=1;
                exp++;
            }
        }
        mant>>=1;
        exp++;

    }
    if (exp >= 1 << (int)(expbits32) - 1 + bias32) {
        return sign ^ inf32;
    }
    if (exp < bias32 + 1) {
        if (exp < bias32 - int(mantbits32)) {
            return sign | 0;
        }
        (mant, exp, trunc) = (mant0, exp0, trunc0);        while (exp < bias32) {
            trunc |= mant & 1;
            mant>>=1;
            exp++;
        }
        if (mant & 1 != 0 && (trunc != 0 || mant & 2 != 0)) {
            mant++;
        }
        mant>>=1;
        exp++;
        if (mant < 1 << (int)(mantbits32)) {
            return sign | mant;
        }
    }
    return sign | uint32(exp - bias32) << (int)(mantbits32) | mant & (1 << (int)(mantbits32) - 1);

}

private static ulong fadd64(ulong f, ulong g) {
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
    else if (fm == 0 && gm == 0 && fs != 0 && gs != 0) // -0 + -0 = -0
        return f;
    else if (fm == 0) // 0 + g = g but 0 + -0 = +0
        if (gm == 0) {
            g ^= gs;
        }
        return g;
    else if (gm == 0) // f + 0 = f
        return f;
        if (fe < ge || fe == ge && fm < gm) {
        (f, g, fs, fm, fe, gs, gm, ge) = (g, f, gs, gm, ge, fs, fm, fe);
    }
    var shift = uint(fe - ge);
    fm<<=2;
    gm<<=2;
    var trunc = gm & (1 << (int)(shift) - 1);
    gm>>=shift;
    if (fs == gs) {
        fm += gm;
    }
    else
 {
        fm -= gm;
        if (trunc != 0) {
            fm--;
        }
    }
    if (fm == 0) {
        fs = 0;
    }
    return fpack64(fs, fm, fe - 2, trunc);

}

private static ulong fsub64(ulong f, ulong g) {
    return fadd64(f, fneg64(g));
}

private static ulong fneg64(ulong f) {
    return f ^ (1 << (int)((mantbits64 + expbits64)));
}

private static ulong fmul64(ulong f, ulong g) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    var (gs, gm, ge, gi, gn) = funpack64(g); 

    // Special cases.

    if (fn || gn) // NaN * g or f * NaN = NaN
        return nan64;
    else if (fi && gi) // Inf * Inf = Inf (with sign adjusted)
        return f ^ gs;
    else if (fi && gm == 0 || fm == 0 && gi) // 0 * Inf = Inf * 0 = NaN
        return nan64;
    else if (fm == 0) // 0 * x = 0 (with sign adjusted)
        return f ^ gs;
    else if (gm == 0) // x * 0 = 0 (with sign adjusted)
        return g ^ fs;
    // 53-bit * 53-bit = 107- or 108-bit
    var (lo, hi) = mullu(fm, gm);
    var shift = mantbits64 - 1;
    var trunc = lo & (1 << (int)(shift) - 1);
    var mant = hi << (int)((64 - shift)) | lo >> (int)(shift);
    return fpack64(fs ^ gs, mant, fe + ge - 1, trunc);

}

private static ulong fdiv64(ulong f, ulong g) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    var (gs, gm, ge, gi, gn) = funpack64(g); 

    // Special cases.

    if (fn || gn) // NaN / g = f / NaN = NaN
        return nan64;
    else if (fi && gi) // ±Inf / ±Inf = NaN
        return nan64;
    else if (!fi && !gi && fm == 0 && gm == 0) // 0 / 0 = NaN
        return nan64;
    else if (fi || !gi && gm == 0) // Inf / g = f / 0 = Inf
        return fs ^ gs ^ inf64;
    else if (gi || fm == 0) // f / Inf = 0 / g = Inf
        return fs ^ gs ^ 0;
        (_, _, _, _) = (fi, fn, gi, gn);    var shift = mantbits64 + 2;
    var (q, r) = divlu(fm >> (int)((64 - shift)), fm << (int)(shift), gm);
    return fpack64(fs ^ gs, q, fe - ge - 2, r);

}

private static uint f64to32(ulong f) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    if (fn) {
        return nan32;
    }
    var fs32 = uint32(fs >> 32);
    if (fi) {
        return fs32 ^ inf32;
    }
    const var d = mantbits64 - mantbits32 - 1;

    return fpack32(fs32, uint32(fm >> (int)(d)), fe - 1, uint32(fm & (1 << (int)(d) - 1)));

}

private static ulong f32to64(uint f) {
    const var d = mantbits64 - mantbits32;

    var (fs, fm, fe, fi, fn) = funpack32(f);
    if (fn) {
        return nan64;
    }
    var fs64 = uint64(fs) << 32;
    if (fi) {
        return fs64 ^ inf64;
    }
    return fpack64(fs64, uint64(fm) << (int)(d), fe, 0);

}

private static (int, bool) fcmp64(ulong f, ulong g) {
    int cmp = default;
    bool isnan = default;

    var (fs, fm, _, fi, fn) = funpack64(f);
    var (gs, gm, _, gi, gn) = funpack64(g);


    if (fn || gn) // flag NaN
        return (0, true);
    else if (!fi && !gi && fm == 0 && gm == 0) // ±0 == ±0
        return (0, false);
    else if (fs > gs) // f < 0, g > 0
        return (-1, false);
    else if (fs < gs) // f > 0, g < 0
        return (+1, false); 

        // Same sign, not NaN.
        // Can compare encodings directly now.
        // Reverse for sign.
    else if (fs == 0 && f < g || fs != 0 && f > g) 
        return (-1, false);
    else if (fs == 0 && f > g || fs != 0 && f < g) 
        return (+1, false);
    // f == g
    return (0, false);

}

private static (long, bool) f64toint(ulong f) {
    long val = default;
    bool ok = default;

    var (fs, fm, fe, fi, fn) = funpack64(f);


    if (fi || fn) // NaN
        return (0, false);
    else if (fe < -1) // f < 0.5
        return (0, false);
    else if (fe > 63) // f >= 2^63
        if (fs != 0 && fm == 0) { // f == -2^63
            return (-1 << 63, true);

        }
        if (fs != 0) {
            return (0, false);
        }
        return (0, false);
        while (fe > int(mantbits64)) {
        fe--;
        fm<<=1;
    }
    while (fe < int(mantbits64)) {
        fe++;
        fm>>=1;
    }
    val = int64(fm);
    if (fs != 0) {
        val = -val;
    }
    return (val, true);

}

private static ulong fintto64(long val) {
    ulong f = default;

    var fs = uint64(val) & (1 << 63);
    var mant = uint64(val);
    if (fs != 0) {
        mant = -mant;
    }
    return fpack64(fs, mant, int(mantbits64), 0);

}

// 64x64 -> 128 multiply.
// adapted from hacker's delight.
private static (ulong, ulong) mullu(ulong u, ulong v) {
    ulong lo = default;
    ulong hi = default;

    const nint s = 32;
    const nint mask = 1 << (int)(s) - 1;
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
private static (ulong, ulong) divlu(ulong u1, ulong u0, ulong v) {
    ulong q = default;
    ulong r = default;

    const nint b = 1 << 32;



    if (u1 >= v) {
        return (1 << 64 - 1, 1 << 64 - 1);
    }
    var s = uint(0);
    while (v & (1 << 63) == 0) {
        s++;
        v<<=1;
    }

    var vn1 = v >> 32;
    var vn0 = v & (1 << 32 - 1);
    var un32 = u1 << (int)(s) | u0 >> (int)((64 - s));
    var un10 = u0 << (int)(s);
    var un1 = un10 >> 32;
    var un0 = un10 & (1 << 32 - 1);
    var q1 = un32 / vn1;
    var rhat = un32 - q1 * vn1;

again1:

    if (q1 >= b || q1 * vn0 > b * rhat + un1) {
        q1--;
        rhat += vn1;
        if (rhat < b) {
            goto again1;
        }
    }
    var un21 = un32 * b + un1 - q1 * v;
    var q0 = un21 / vn1;
    rhat = un21 - q0 * vn1;

again2:

    if (q0 >= b || q0 * vn0 > b * rhat + un0) {
        q0--;
        rhat += vn1;
        if (rhat < b) {
            goto again2;
        }
    }
    return (q1 * b + q0, (un21 * b + un0 - q0 * v) >> (int)(s));

}

private static uint fadd32(uint x, uint y) {
    return f64to32(fadd64(f32to64(x), f32to64(y)));
}

private static uint fmul32(uint x, uint y) {
    return f64to32(fmul64(f32to64(x), f32to64(y)));
}

private static uint fdiv32(uint x, uint y) {
    return f64to32(fdiv64(f32to64(x), f32to64(y)));
}

private static bool feq32(uint x, uint y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp == 0 && !nan;
}

private static bool fgt32(uint x, uint y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp >= 1 && !nan;
}

private static bool fge32(uint x, uint y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp >= 0 && !nan;
}

private static bool feq64(ulong x, ulong y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp == 0 && !nan;
}

private static bool fgt64(ulong x, ulong y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp >= 1 && !nan;
}

private static bool fge64(ulong x, ulong y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp >= 0 && !nan;
}

private static uint fint32to32(int x) {
    return f64to32(fintto64(int64(x)));
}

private static ulong fint32to64(int x) {
    return fintto64(int64(x));
}

private static uint fint64to32(long x) {
    return f64to32(fintto64(x));
}

private static ulong fint64to64(long x) {
    return fintto64(x);
}

private static int f32toint32(uint x) {
    var (val, _) = f64toint(f32to64(x));
    return int32(val);
}

private static long f32toint64(uint x) {
    var (val, _) = f64toint(f32to64(x));
    return val;
}

private static int f64toint32(ulong x) {
    var (val, _) = f64toint(x);
    return int32(val);
}

private static long f64toint64(ulong x) {
    var (val, _) = f64toint(x);
    return val;
}

private static ulong f64touint64(double x) {
    if (x < float64(1 << 63)) {
        return uint64(int64(x));
    }
    var y = x - float64(1 << 63);
    var z = uint64(int64(y));
    return z | (1 << 63);

}

private static ulong f32touint64(float x) {
    if (x < float32(1 << 63)) {
        return uint64(int64(x));
    }
    var y = x - float32(1 << 63);
    var z = uint64(int64(y));
    return z | (1 << 63);

}

private static double fuint64to64(ulong x) {
    if (int64(x) >= 0) {
        return float64(int64(x));
    }
    var y = x & 1;
    var z = x >> 1;
    z = z | y;
    var r = float64(int64(z));
    return r + r;

}

private static float fuint64to32(ulong x) {
    return float32(fuint64to64(x));
}

} // end runtime_package
