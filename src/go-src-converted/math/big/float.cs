// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements multi-precision floating-point numbers.
// Like in the GNU MPFR library (https://www.mpfr.org/), operands
// can be of mixed precision. Unlike MPFR, the rounding mode is
// not specified with each operation, but with each operand. The
// rounding mode of the result operand determines the rounding
// mode of an operation. This is a from-scratch implementation.

// package big -- go2cs converted at 2020 October 09 04:53:22 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\float.go
using fmt = go.fmt_package;
using math = go.math_package;
using bits = go.math.bits_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static readonly var debugFloat = false; // enable for debugging

        // A nonzero finite Float represents a multi-precision floating point number
        //
        //   sign × mantissa × 2**exponent
        //
        // with 0.5 <= mantissa < 1.0, and MinExp <= exponent <= MaxExp.
        // A Float may also be zero (+0, -0) or infinite (+Inf, -Inf).
        // All Floats are ordered, and the ordering of two Floats x and y
        // is defined by x.Cmp(y).
        //
        // Each Float value also has a precision, rounding mode, and accuracy.
        // The precision is the maximum number of mantissa bits available to
        // represent the value. The rounding mode specifies how a result should
        // be rounded to fit into the mantissa bits, and accuracy describes the
        // rounding error with respect to the exact result.
        //
        // Unless specified otherwise, all operations (including setters) that
        // specify a *Float variable for the result (usually via the receiver
        // with the exception of MantExp), round the numeric result according
        // to the precision and rounding mode of the result variable.
        //
        // If the provided result precision is 0 (see below), it is set to the
        // precision of the argument with the largest precision value before any
        // rounding takes place, and the rounding mode remains unchanged. Thus,
        // uninitialized Floats provided as result arguments will have their
        // precision set to a reasonable value determined by the operands, and
        // their mode is the zero value for RoundingMode (ToNearestEven).
        //
        // By setting the desired precision to 24 or 53 and using matching rounding
        // mode (typically ToNearestEven), Float operations produce the same results
        // as the corresponding float32 or float64 IEEE-754 arithmetic for operands
        // that correspond to normal (i.e., not denormal) float32 or float64 numbers.
        // Exponent underflow and overflow lead to a 0 or an Infinity for different
        // values than IEEE-754 because Float exponents have a much larger range.
        //
        // The zero (uninitialized) value for a Float is ready to use and represents
        // the number +0.0 exactly, with precision 0 and rounding mode ToNearestEven.
        //
        // Operations always take pointer arguments (*Float) rather
        // than Float values, and each unique Float value requires
        // its own unique *Float pointer. To "copy" a Float value,
        // an existing (or newly allocated) Float must be set to
        // a new value using the Float.Set method; shallow copies
        // of Floats are not supported and may lead to errors.
 // enable for debugging

        // A nonzero finite Float represents a multi-precision floating point number
        //
        //   sign × mantissa × 2**exponent
        //
        // with 0.5 <= mantissa < 1.0, and MinExp <= exponent <= MaxExp.
        // A Float may also be zero (+0, -0) or infinite (+Inf, -Inf).
        // All Floats are ordered, and the ordering of two Floats x and y
        // is defined by x.Cmp(y).
        //
        // Each Float value also has a precision, rounding mode, and accuracy.
        // The precision is the maximum number of mantissa bits available to
        // represent the value. The rounding mode specifies how a result should
        // be rounded to fit into the mantissa bits, and accuracy describes the
        // rounding error with respect to the exact result.
        //
        // Unless specified otherwise, all operations (including setters) that
        // specify a *Float variable for the result (usually via the receiver
        // with the exception of MantExp), round the numeric result according
        // to the precision and rounding mode of the result variable.
        //
        // If the provided result precision is 0 (see below), it is set to the
        // precision of the argument with the largest precision value before any
        // rounding takes place, and the rounding mode remains unchanged. Thus,
        // uninitialized Floats provided as result arguments will have their
        // precision set to a reasonable value determined by the operands, and
        // their mode is the zero value for RoundingMode (ToNearestEven).
        //
        // By setting the desired precision to 24 or 53 and using matching rounding
        // mode (typically ToNearestEven), Float operations produce the same results
        // as the corresponding float32 or float64 IEEE-754 arithmetic for operands
        // that correspond to normal (i.e., not denormal) float32 or float64 numbers.
        // Exponent underflow and overflow lead to a 0 or an Infinity for different
        // values than IEEE-754 because Float exponents have a much larger range.
        //
        // The zero (uninitialized) value for a Float is ready to use and represents
        // the number +0.0 exactly, with precision 0 and rounding mode ToNearestEven.
        //
        // Operations always take pointer arguments (*Float) rather
        // than Float values, and each unique Float value requires
        // its own unique *Float pointer. To "copy" a Float value,
        // an existing (or newly allocated) Float must be set to
        // a new value using the Float.Set method; shallow copies
        // of Floats are not supported and may lead to errors.
        public partial struct Float
        {
            public uint prec;
            public RoundingMode mode;
            public Accuracy acc;
            public form form;
            public bool neg;
            public nat mant;
            public int exp;
        }

        // An ErrNaN panic is raised by a Float operation that would lead to
        // a NaN under IEEE-754 rules. An ErrNaN implements the error interface.
        public partial struct ErrNaN
        {
            public @string msg;
        }

        public static @string Error(this ErrNaN err)
        {
            return err.msg;
        }

        // NewFloat allocates and returns a new Float set to x,
        // with precision 53 and rounding mode ToNearestEven.
        // NewFloat panics with ErrNaN if x is a NaN.
        public static ptr<Float> NewFloat(double x) => func((_, panic, __) =>
        {
            if (math.IsNaN(x))
            {
                panic(new ErrNaN("NewFloat(NaN)"));
            }

            return @new<Float>().SetFloat64(x);

        });

        // Exponent and precision limits.
        public static readonly var MaxExp = math.MaxInt32; // largest supported exponent
        public static readonly var MinExp = math.MinInt32; // smallest supported exponent
        public static readonly var MaxPrec = math.MaxUint32; // largest (theoretically) supported precision; likely memory-limited

        // Internal representation: The mantissa bits x.mant of a nonzero finite
        // Float x are stored in a nat slice long enough to hold up to x.prec bits;
        // the slice may (but doesn't have to) be shorter if the mantissa contains
        // trailing 0 bits. x.mant is normalized if the msb of x.mant == 1 (i.e.,
        // the msb is shifted all the way "to the left"). Thus, if the mantissa has
        // trailing 0 bits or x.prec is not a multiple of the Word size _W,
        // x.mant[0] has trailing zero bits. The msb of the mantissa corresponds
        // to the value 0.5; the exponent x.exp shifts the binary point as needed.
        //
        // A zero or non-finite Float x ignores x.mant and x.exp.
        //
        // x                 form      neg      mant         exp
        // ----------------------------------------------------------
        // ±0                zero      sign     -            -
        // 0 < |x| < +Inf    finite    sign     mantissa     exponent
        // ±Inf              inf       sign     -            -

        // A form value describes the internal representation.
        private partial struct form // : byte
        {
        }

        // The form value order is relevant - do not change!
        private static readonly form zero = (form)iota;
        private static readonly var finite = 0;
        private static readonly var inf = 1;


        // RoundingMode determines how a Float value is rounded to the
        // desired precision. Rounding may change the Float value; the
        // rounding error is described by the Float's Accuracy.
        public partial struct RoundingMode // : byte
        {
        }

        // These constants define supported rounding modes.
        public static readonly RoundingMode ToNearestEven = (RoundingMode)iota; // == IEEE 754-2008 roundTiesToEven
        public static readonly var ToNearestAway = 0; // == IEEE 754-2008 roundTiesToAway
        public static readonly var ToZero = 1; // == IEEE 754-2008 roundTowardZero
        public static readonly var AwayFromZero = 2; // no IEEE 754-2008 equivalent
        public static readonly var ToNegativeInf = 3; // == IEEE 754-2008 roundTowardNegative
        public static readonly var ToPositiveInf = 4; // == IEEE 754-2008 roundTowardPositive

        //go:generate stringer -type=RoundingMode

        // Accuracy describes the rounding error produced by the most recent
        // operation that generated a Float value, relative to the exact value.
        public partial struct Accuracy // : sbyte
        {
        }

        // Constants describing the Accuracy of a Float.
        public static readonly Accuracy Below = (Accuracy)-1L;
        public static readonly Accuracy Exact = (Accuracy)0L;
        public static readonly Accuracy Above = (Accuracy)+1L;


        //go:generate stringer -type=Accuracy

        // SetPrec sets z's precision to prec and returns the (possibly) rounded
        // value of z. Rounding occurs according to z's rounding mode if the mantissa
        // cannot be represented in prec bits without loss of precision.
        // SetPrec(0) maps all finite values to ±0; infinite values remain unchanged.
        // If prec > MaxPrec, it is set to MaxPrec.
        private static ptr<Float> SetPrec(this ptr<Float> _addr_z, ulong prec)
        {
            ref Float z = ref _addr_z.val;

            z.acc = Exact; // optimistically assume no rounding is needed

            // special case
            if (prec == 0L)
            {
                z.prec = 0L;
                if (z.form == finite)
                { 
                    // truncate z to 0
                    z.acc = makeAcc(z.neg);
                    z.form = zero;

                }

                return _addr_z!;

            } 

            // general case
            if (prec > MaxPrec)
            {
                prec = MaxPrec;
            }

            var old = z.prec;
            z.prec = uint32(prec);
            if (z.prec < old)
            {
                z.round(0L);
            }

            return _addr_z!;

        }

        private static Accuracy makeAcc(bool above)
        {
            if (above)
            {
                return Above;
            }

            return Below;

        }

        // SetMode sets z's rounding mode to mode and returns an exact z.
        // z remains unchanged otherwise.
        // z.SetMode(z.Mode()) is a cheap way to set z's accuracy to Exact.
        private static ptr<Float> SetMode(this ptr<Float> _addr_z, RoundingMode mode)
        {
            ref Float z = ref _addr_z.val;

            z.mode = mode;
            z.acc = Exact;
            return _addr_z!;
        }

        // Prec returns the mantissa precision of x in bits.
        // The result may be 0 for |x| == 0 and |x| == Inf.
        private static ulong Prec(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            return uint(x.prec);
        }

        // MinPrec returns the minimum precision required to represent x exactly
        // (i.e., the smallest prec before x.SetPrec(prec) would start rounding x).
        // The result is 0 for |x| == 0 and |x| == Inf.
        private static ulong MinPrec(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            if (x.form != finite)
            {
                return 0L;
            }

            return uint(len(x.mant)) * _W - x.mant.trailingZeroBits();

        }

        // Mode returns the rounding mode of x.
        private static RoundingMode Mode(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            return x.mode;
        }

        // Acc returns the accuracy of x produced by the most recent
        // operation, unless explicitly documented otherwise by that
        // operation.
        private static Accuracy Acc(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            return x.acc;
        }

        // Sign returns:
        //
        //    -1 if x <   0
        //     0 if x is ±0
        //    +1 if x >   0
        //
        private static long Sign(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }

            if (x.form == zero)
            {
                return 0L;
            }

            if (x.neg)
            {
                return -1L;
            }

            return 1L;

        }

        // MantExp breaks x into its mantissa and exponent components
        // and returns the exponent. If a non-nil mant argument is
        // provided its value is set to the mantissa of x, with the
        // same precision and rounding mode as x. The components
        // satisfy x == mant × 2**exp, with 0.5 <= |mant| < 1.0.
        // Calling MantExp with a nil argument is an efficient way to
        // get the exponent of the receiver.
        //
        // Special cases are:
        //
        //    (  ±0).MantExp(mant) = 0, with mant set to   ±0
        //    (±Inf).MantExp(mant) = 0, with mant set to ±Inf
        //
        // x and mant may be the same in which case x is set to its
        // mantissa value.
        private static long MantExp(this ptr<Float> _addr_x, ptr<Float> _addr_mant)
        {
            long exp = default;
            ref Float x = ref _addr_x.val;
            ref Float mant = ref _addr_mant.val;

            if (debugFloat)
            {
                x.validate();
            }

            if (x.form == finite)
            {
                exp = int(x.exp);
            }

            if (mant != null)
            {
                mant.Copy(x);
                if (mant.form == finite)
                {
                    mant.exp = 0L;
                }

            }

            return ;

        }

        private static void setExpAndRound(this ptr<Float> _addr_z, long exp, ulong sbit)
        {
            ref Float z = ref _addr_z.val;

            if (exp < MinExp)
            { 
                // underflow
                z.acc = makeAcc(z.neg);
                z.form = zero;
                return ;

            }

            if (exp > MaxExp)
            { 
                // overflow
                z.acc = makeAcc(!z.neg);
                z.form = inf;
                return ;

            }

            z.form = finite;
            z.exp = int32(exp);
            z.round(sbit);

        }

        // SetMantExp sets z to mant × 2**exp and returns z.
        // The result z has the same precision and rounding mode
        // as mant. SetMantExp is an inverse of MantExp but does
        // not require 0.5 <= |mant| < 1.0. Specifically:
        //
        //    mant := new(Float)
        //    new(Float).SetMantExp(mant, x.MantExp(mant)).Cmp(x) == 0
        //
        // Special cases are:
        //
        //    z.SetMantExp(  ±0, exp) =   ±0
        //    z.SetMantExp(±Inf, exp) = ±Inf
        //
        // z and mant may be the same in which case z's exponent
        // is set to exp.
        private static ptr<Float> SetMantExp(this ptr<Float> _addr_z, ptr<Float> _addr_mant, long exp)
        {
            ref Float z = ref _addr_z.val;
            ref Float mant = ref _addr_mant.val;

            if (debugFloat)
            {
                z.validate();
                mant.validate();
            }

            z.Copy(mant);
            if (z.form != finite)
            {
                return _addr_z!;
            }

            z.setExpAndRound(int64(z.exp) + int64(exp), 0L);
            return _addr_z!;

        }

        // Signbit reports whether x is negative or negative zero.
        private static bool Signbit(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            return x.neg;
        }

        // IsInf reports whether x is +Inf or -Inf.
        private static bool IsInf(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            return x.form == inf;
        }

        // IsInt reports whether x is an integer.
        // ±Inf values are not integers.
        private static bool IsInt(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            } 
            // special cases
            if (x.form != finite)
            {
                return x.form == zero;
            } 
            // x.form == finite
            if (x.exp <= 0L)
            {
                return false;
            } 
            // x.exp > 0
            return x.prec <= uint32(x.exp) || x.MinPrec() <= uint(x.exp); // not enough bits for fractional mantissa
        }

        // debugging support
        private static void validate(this ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            ref Float x = ref _addr_x.val;

            if (!debugFloat)
            { 
                // avoid performance bugs
                panic("validate called but debugFloat is not set");

            }

            if (x.form != finite)
            {
                return ;
            }

            var m = len(x.mant);
            if (m == 0L)
            {
                panic("nonzero finite number with empty mantissa");
            }

            const long msb = (long)1L << (int)((_W - 1L));

            if (x.mant[m - 1L] & msb == 0L)
            {
                panic(fmt.Sprintf("msb not set in last word %#x of %s", x.mant[m - 1L], x.Text('p', 0L)));
            }

            if (x.prec == 0L)
            {
                panic("zero precision finite number");
            }

        });

        // round rounds z according to z.mode to z.prec bits and sets z.acc accordingly.
        // sbit must be 0 or 1 and summarizes any "sticky bit" information one might
        // have before calling round. z's mantissa must be normalized (with the msb set)
        // or empty.
        //
        // CAUTION: The rounding modes ToNegativeInf, ToPositiveInf are affected by the
        // sign of z. For correct rounding, the sign of z must be set correctly before
        // calling round.
        private static void round(this ptr<Float> _addr_z, ulong sbit) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;

            if (debugFloat)
            {
                z.validate();
            }

            z.acc = Exact;
            if (z.form != finite)
            { 
                // ±0 or ±Inf => nothing left to do
                return ;

            } 
            // z.form == finite && len(z.mant) > 0
            // m > 0 implies z.prec > 0 (checked by validate)
            var m = uint32(len(z.mant)); // present mantissa length in words
            var bits = m * _W; // present mantissa bits; bits > 0
            if (bits <= z.prec)
            { 
                // mantissa fits => nothing to do
                return ;

            } 
            // bits > z.prec

            // Rounding is based on two bits: the rounding bit (rbit) and the
            // sticky bit (sbit). The rbit is the bit immediately before the
            // z.prec leading mantissa bits (the "0.5"). The sbit is set if any
            // of the bits before the rbit are set (the "0.25", "0.125", etc.):
            //
            //   rbit  sbit  => "fractional part"
            //
            //   0     0        == 0
            //   0     1        >  0  , < 0.5
            //   1     0        == 0.5
            //   1     1        >  0.5, < 1.0

            // bits > z.prec: mantissa too large => round
            var r = uint(bits - z.prec - 1L); // rounding bit position; r >= 0
            var rbit = z.mant.bit(r) & 1L; // rounding bit; be safe and ensure it's a single bit
            // The sticky bit is only needed for rounding ToNearestEven
            // or when the rounding bit is zero. Avoid computation otherwise.
            if (sbit == 0L && (rbit == 0L || z.mode == ToNearestEven))
            {
                sbit = z.mant.sticky(r);
            }

            sbit &= 1L; // be safe and ensure it's a single bit

            // cut off extra words
            var n = (z.prec + (_W - 1L)) / _W; // mantissa length in words for desired precision
            if (m > n)
            {
                copy(z.mant, z.mant[m - n..]); // move n last words to front
                z.mant = z.mant[..n];

            } 

            // determine number of trailing zero bits (ntz) and compute lsb mask of mantissa's least-significant word
            var ntz = n * _W - z.prec; // 0 <= ntz < _W
            var lsb = Word(1L) << (int)(ntz); 

            // round if result is inexact
            if (rbit | sbit != 0L)
            { 
                // Make rounding decision: The result mantissa is truncated ("rounded down")
                // by default. Decide if we need to increment, or "round up", the (unsigned)
                // mantissa.
                var inc = false;

                if (z.mode == ToNegativeInf) 
                    inc = z.neg;
                else if (z.mode == ToZero)                 else if (z.mode == ToNearestEven) 
                    inc = rbit != 0L && (sbit != 0L || z.mant[0L] & lsb != 0L);
                else if (z.mode == ToNearestAway) 
                    inc = rbit != 0L;
                else if (z.mode == AwayFromZero) 
                    inc = true;
                else if (z.mode == ToPositiveInf) 
                    inc = !z.neg;
                else 
                    panic("unreachable");
                // A positive result (!z.neg) is Above the exact result if we increment,
                // and it's Below if we truncate (Exact results require no rounding).
                // For a negative result (z.neg) it is exactly the opposite.
                z.acc = makeAcc(inc != z.neg);

                if (inc)
                { 
                    // add 1 to mantissa
                    if (addVW(z.mant, z.mant, lsb) != 0L)
                    { 
                        // mantissa overflow => adjust exponent
                        if (z.exp >= MaxExp)
                        { 
                            // exponent overflow
                            z.form = inf;
                            return ;

                        }

                        z.exp++; 
                        // adjust mantissa: divide by 2 to compensate for exponent adjustment
                        shrVU(z.mant, z.mant, 1L); 
                        // set msb == carry == 1 from the mantissa overflow above
                        const long msb = (long)1L << (int)((_W - 1L));

                        z.mant[n - 1L] |= msb;

                    }

                }

            } 

            // zero out trailing bits in least-significant word
            z.mant[0L] &= lsb - 1L;

            if (debugFloat)
            {
                z.validate();
            }

        });

        private static ptr<Float> setBits64(this ptr<Float> _addr_z, bool neg, ulong x)
        {
            ref Float z = ref _addr_z.val;

            if (z.prec == 0L)
            {
                z.prec = 64L;
            }

            z.acc = Exact;
            z.neg = neg;
            if (x == 0L)
            {
                z.form = zero;
                return _addr_z!;
            } 
            // x != 0
            z.form = finite;
            var s = bits.LeadingZeros64(x);
            z.mant = z.mant.setUint64(x << (int)(uint(s)));
            z.exp = int32(64L - s); // always fits
            if (z.prec < 64L)
            {
                z.round(0L);
            }

            return _addr_z!;

        }

        // SetUint64 sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to 64 (and rounding will have
        // no effect).
        private static ptr<Float> SetUint64(this ptr<Float> _addr_z, ulong x)
        {
            ref Float z = ref _addr_z.val;

            return _addr_z.setBits64(false, x)!;
        }

        // SetInt64 sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to 64 (and rounding will have
        // no effect).
        private static ptr<Float> SetInt64(this ptr<Float> _addr_z, long x)
        {
            ref Float z = ref _addr_z.val;

            var u = x;
            if (u < 0L)
            {
                u = -u;
            } 
            // We cannot simply call z.SetUint64(uint64(u)) and change
            // the sign afterwards because the sign affects rounding.
            return _addr_z.setBits64(x < 0L, uint64(u))!;

        }

        // SetFloat64 sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to 53 (and rounding will have
        // no effect). SetFloat64 panics with ErrNaN if x is a NaN.
        private static ptr<Float> SetFloat64(this ptr<Float> _addr_z, double x) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;

            if (z.prec == 0L)
            {
                z.prec = 53L;
            }

            if (math.IsNaN(x))
            {
                panic(new ErrNaN("Float.SetFloat64(NaN)"));
            }

            z.acc = Exact;
            z.neg = math.Signbit(x); // handle -0, -Inf correctly
            if (x == 0L)
            {
                z.form = zero;
                return _addr_z!;
            }

            if (math.IsInf(x, 0L))
            {
                z.form = inf;
                return _addr_z!;
            } 
            // normalized x != 0
            z.form = finite;
            var (fmant, exp) = math.Frexp(x); // get normalized mantissa
            z.mant = z.mant.setUint64(1L << (int)(63L) | math.Float64bits(fmant) << (int)(11L));
            z.exp = int32(exp); // always fits
            if (z.prec < 53L)
            {
                z.round(0L);
            }

            return _addr_z!;

        });

        // fnorm normalizes mantissa m by shifting it to the left
        // such that the msb of the most-significant word (msw) is 1.
        // It returns the shift amount. It assumes that len(m) != 0.
        private static long fnorm(nat m) => func((_, panic, __) =>
        {
            if (debugFloat && (len(m) == 0L || m[len(m) - 1L] == 0L))
            {
                panic("msw of mantissa is 0");
            }

            var s = nlz(m[len(m) - 1L]);
            if (s > 0L)
            {
                var c = shlVU(m, m, s);
                if (debugFloat && c != 0L)
                {
                    panic("nlz or shlVU incorrect");
                }

            }

            return int64(s);

        });

        // SetInt sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to the larger of x.BitLen()
        // or 64 (and rounding will have no effect).
        private static ptr<Float> SetInt(this ptr<Float> _addr_z, ptr<Int> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Int x = ref _addr_x.val;
 
            // TODO(gri) can be more efficient if z.prec > 0
            // but small compared to the size of x, or if there
            // are many trailing 0's.
            var bits = uint32(x.BitLen());
            if (z.prec == 0L)
            {
                z.prec = umax32(bits, 64L);
            }

            z.acc = Exact;
            z.neg = x.neg;
            if (len(x.abs) == 0L)
            {
                z.form = zero;
                return _addr_z!;
            } 
            // x != 0
            z.mant = z.mant.set(x.abs);
            fnorm(z.mant);
            z.setExpAndRound(int64(bits), 0L);
            return _addr_z!;

        }

        // SetRat sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to the largest of a.BitLen(),
        // b.BitLen(), or 64; with x = a/b.
        private static ptr<Float> SetRat(this ptr<Float> _addr_z, ptr<Rat> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Rat x = ref _addr_x.val;

            if (x.IsInt())
            {
                return _addr_z.SetInt(x.Num())!;
            }

            ref Float a = ref heap(out ptr<Float> _addr_a);            ref Float b = ref heap(out ptr<Float> _addr_b);

            a.SetInt(x.Num());
            b.SetInt(x.Denom());
            if (z.prec == 0L)
            {
                z.prec = umax32(a.prec, b.prec);
            }

            return _addr_z.Quo(_addr_a, _addr_b)!;

        }

        // SetInf sets z to the infinite Float -Inf if signbit is
        // set, or +Inf if signbit is not set, and returns z. The
        // precision of z is unchanged and the result is always
        // Exact.
        private static ptr<Float> SetInf(this ptr<Float> _addr_z, bool signbit)
        {
            ref Float z = ref _addr_z.val;

            z.acc = Exact;
            z.form = inf;
            z.neg = signbit;
            return _addr_z!;
        }

        // Set sets z to the (possibly rounded) value of x and returns z.
        // If z's precision is 0, it is changed to the precision of x
        // before setting z (and rounding will have no effect).
        // Rounding is performed according to z's precision and rounding
        // mode; and z's accuracy reports the result error relative to the
        // exact (not rounded) result.
        private static ptr<Float> Set(this ptr<Float> _addr_z, ptr<Float> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }

            z.acc = Exact;
            if (z != x)
            {
                z.form = x.form;
                z.neg = x.neg;
                if (x.form == finite)
                {
                    z.exp = x.exp;
                    z.mant = z.mant.set(x.mant);
                }

                if (z.prec == 0L)
                {
                    z.prec = x.prec;
                }
                else if (z.prec < x.prec)
                {
                    z.round(0L);
                }

            }

            return _addr_z!;

        }

        // Copy sets z to x, with the same precision, rounding mode, and
        // accuracy as x, and returns z. x is not changed even if z and
        // x are the same.
        private static ptr<Float> Copy(this ptr<Float> _addr_z, ptr<Float> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }

            if (z != x)
            {
                z.prec = x.prec;
                z.mode = x.mode;
                z.acc = x.acc;
                z.form = x.form;
                z.neg = x.neg;
                if (z.form == finite)
                {
                    z.mant = z.mant.set(x.mant);
                    z.exp = x.exp;
                }

            }

            return _addr_z!;

        }

        // msb32 returns the 32 most significant bits of x.
        private static uint msb32(nat x) => func((_, panic, __) =>
        {
            var i = len(x) - 1L;
            if (i < 0L)
            {
                return 0L;
            }

            if (debugFloat && x[i] & (1L << (int)((_W - 1L))) == 0L)
            {
                panic("x not normalized");
            }

            switch (_W)
            {
                case 32L: 
                    return uint32(x[i]);
                    break;
                case 64L: 
                    return uint32(x[i] >> (int)(32L));
                    break;
            }
            panic("unreachable");

        });

        // msb64 returns the 64 most significant bits of x.
        private static ulong msb64(nat x) => func((_, panic, __) =>
        {
            var i = len(x) - 1L;
            if (i < 0L)
            {
                return 0L;
            }

            if (debugFloat && x[i] & (1L << (int)((_W - 1L))) == 0L)
            {
                panic("x not normalized");
            }

            switch (_W)
            {
                case 32L: 
                    var v = uint64(x[i]) << (int)(32L);
                    if (i > 0L)
                    {
                        v |= uint64(x[i - 1L]);
                    }

                    return v;
                    break;
                case 64L: 
                    return uint64(x[i]);
                    break;
            }
            panic("unreachable");

        });

        // Uint64 returns the unsigned integer resulting from truncating x
        // towards zero. If 0 <= x <= math.MaxUint64, the result is Exact
        // if x is an integer and Below otherwise.
        // The result is (0, Above) for x < 0, and (math.MaxUint64, Below)
        // for x > math.MaxUint64.
        private static (ulong, Accuracy) Uint64(this ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            ulong _p0 = default;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }


            if (x.form == finite) 
                if (x.neg)
                {
                    return (0L, Above);
                } 
                // 0 < x < +Inf
                if (x.exp <= 0L)
                { 
                    // 0 < x < 1
                    return (0L, Below);

                } 
                // 1 <= x < Inf
                if (x.exp <= 64L)
                { 
                    // u = trunc(x) fits into a uint64
                    var u = msb64(x.mant) >> (int)((64L - uint32(x.exp)));
                    if (x.MinPrec() <= 64L)
                    {
                        return (u, Exact);
                    }

                    return (u, Below); // x truncated
                } 
                // x too large
                return (math.MaxUint64, Below);
            else if (x.form == zero) 
                return (0L, Exact);
            else if (x.form == inf) 
                if (x.neg)
                {
                    return (0L, Above);
                }

                return (math.MaxUint64, Below);
                        panic("unreachable");

        });

        // Int64 returns the integer resulting from truncating x towards zero.
        // If math.MinInt64 <= x <= math.MaxInt64, the result is Exact if x is
        // an integer, and Above (x < 0) or Below (x > 0) otherwise.
        // The result is (math.MinInt64, Above) for x < math.MinInt64,
        // and (math.MaxInt64, Below) for x > math.MaxInt64.
        private static (long, Accuracy) Int64(this ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            long _p0 = default;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }


            if (x.form == finite) 
                // 0 < |x| < +Inf
                var acc = makeAcc(x.neg);
                if (x.exp <= 0L)
                { 
                    // 0 < |x| < 1
                    return (0L, acc);

                } 
                // x.exp > 0

                // 1 <= |x| < +Inf
                if (x.exp <= 63L)
                { 
                    // i = trunc(x) fits into an int64 (excluding math.MinInt64)
                    var i = int64(msb64(x.mant) >> (int)((64L - uint32(x.exp))));
                    if (x.neg)
                    {
                        i = -i;
                    }

                    if (x.MinPrec() <= uint(x.exp))
                    {
                        return (i, Exact);
                    }

                    return (i, acc); // x truncated
                }

                if (x.neg)
                { 
                    // check for special case x == math.MinInt64 (i.e., x == -(0.5 << 64))
                    if (x.exp == 64L && x.MinPrec() == 1L)
                    {
                        acc = Exact;
                    }

                    return (math.MinInt64, acc);

                } 
                // x too large
                return (math.MaxInt64, Below);
            else if (x.form == zero) 
                return (0L, Exact);
            else if (x.form == inf) 
                if (x.neg)
                {
                    return (math.MinInt64, Above);
                }

                return (math.MaxInt64, Below);
                        panic("unreachable");

        });

        // Float32 returns the float32 value nearest to x. If x is too small to be
        // represented by a float32 (|x| < math.SmallestNonzeroFloat32), the result
        // is (0, Below) or (-0, Above), respectively, depending on the sign of x.
        // If x is too large to be represented by a float32 (|x| > math.MaxFloat32),
        // the result is (+Inf, Above) or (-Inf, Below), depending on the sign of x.
        private static (float, Accuracy) Float32(this ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            float _p0 = default;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }


            if (x.form == finite) 
                // 0 < |x| < +Inf

                const long fbits = (long)32L; //        float size
                const long mbits = (long)23L; //        mantissa size (excluding implicit msb)
                const var ebits = fbits - mbits - 1L; //     8  exponent size
                const long bias = (long)1L << (int)((ebits - 1L)) - 1L; //   127  exponent bias
                const long dmin = (long)1L - bias - mbits; //  -149  smallest unbiased exponent (denormal)
                const long emin = (long)1L - bias; //  -126  smallest unbiased exponent (normal)
                const var emax = bias; //   127  largest unbiased exponent (normal) 

                // Float mantissa m is 0.5 <= m < 1.0; compute exponent e for float32 mantissa.
                var e = x.exp - 1L; // exponent for normal mantissa m with 1.0 <= m < 2.0

                // Compute precision p for float32 mantissa.
                // If the exponent is too small, we have a denormal number before
                // rounding and fewer than p mantissa bits of precision available
                // (the exponent remains fixed but the mantissa gets shifted right).
                var p = mbits + 1L; // precision of normal float
                if (e < emin)
                { 
                    // recompute precision
                    p = mbits + 1L - emin + int(e); 
                    // If p == 0, the mantissa of x is shifted so much to the right
                    // that its msb falls immediately to the right of the float32
                    // mantissa space. In other words, if the smallest denormal is
                    // considered "1.0", for p == 0, the mantissa value m is >= 0.5.
                    // If m > 0.5, it is rounded up to 1.0; i.e., the smallest denormal.
                    // If m == 0.5, it is rounded down to even, i.e., 0.0.
                    // If p < 0, the mantissa value m is <= "0.25" which is never rounded up.
                    if (p < 0L || p == 0L && x.mant.sticky(uint(len(x.mant)) * _W - 1L) == 0L)
                    { 
                        // underflow to ±0
                        if (x.neg)
                        {
                            float z = default;
                            return (-z, Above);
                        }

                        return (0.0F, Below);

                    } 
                    // otherwise, round up
                    // We handle p == 0 explicitly because it's easy and because
                    // Float.round doesn't support rounding to 0 bits of precision.
                    if (p == 0L)
                    {
                        if (x.neg)
                        {
                            return (-math.SmallestNonzeroFloat32, Below);
                        }

                        return (math.SmallestNonzeroFloat32, Above);

                    }

                } 
                // p > 0

                // round
                Float r = default;
                r.prec = uint32(p);
                r.Set(x);
                e = r.exp - 1L; 

                // Rounding may have caused r to overflow to ±Inf
                // (rounding never causes underflows to 0).
                // If the exponent is too large, also overflow to ±Inf.
                if (r.form == inf || e > emax)
                { 
                    // overflow
                    if (x.neg)
                    {
                        return (float32(math.Inf(-1L)), Below);
                    }

                    return (float32(math.Inf(+1L)), Above);

                } 
                // e <= emax

                // Determine sign, biased exponent, and mantissa.
                uint sign = default;                uint bexp = default;                uint mant = default;

                if (x.neg)
                {
                    sign = 1L << (int)((fbits - 1L));
                } 

                // Rounding may have caused a denormal number to
                // become normal. Check again.
                if (e < emin)
                { 
                    // denormal number: recompute precision
                    // Since rounding may have at best increased precision
                    // and we have eliminated p <= 0 early, we know p > 0.
                    // bexp == 0 for denormals
                    p = mbits + 1L - emin + int(e);
                    mant = msb32(r.mant) >> (int)(uint(fbits - p));

                }
                else
                { 
                    // normal number: emin <= e <= emax
                    bexp = uint32(e + bias) << (int)(mbits);
                    mant = msb32(r.mant) >> (int)(ebits) & (1L << (int)(mbits) - 1L); // cut off msb (implicit 1 bit)
                }

                return (math.Float32frombits(sign | bexp | mant), r.acc);
            else if (x.form == zero) 
                if (x.neg)
                {
                    z = default;
                    return (-z, Exact);
                }

                return (0.0F, Exact);
            else if (x.form == inf) 
                if (x.neg)
                {
                    return (float32(math.Inf(-1L)), Exact);
                }

                return (float32(math.Inf(+1L)), Exact);
                        panic("unreachable");

        });

        // Float64 returns the float64 value nearest to x. If x is too small to be
        // represented by a float64 (|x| < math.SmallestNonzeroFloat64), the result
        // is (0, Below) or (-0, Above), respectively, depending on the sign of x.
        // If x is too large to be represented by a float64 (|x| > math.MaxFloat64),
        // the result is (+Inf, Above) or (-Inf, Below), depending on the sign of x.
        private static (double, Accuracy) Float64(this ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            double _p0 = default;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;

            if (debugFloat)
            {
                x.validate();
            }


            if (x.form == finite) 
                // 0 < |x| < +Inf

                const long fbits = (long)64L; //        float size
                const long mbits = (long)52L; //        mantissa size (excluding implicit msb)
                const var ebits = fbits - mbits - 1L; //    11  exponent size
                const long bias = (long)1L << (int)((ebits - 1L)) - 1L; //  1023  exponent bias
                const long dmin = (long)1L - bias - mbits; // -1074  smallest unbiased exponent (denormal)
                const long emin = (long)1L - bias; // -1022  smallest unbiased exponent (normal)
                const var emax = bias; //  1023  largest unbiased exponent (normal) 

                // Float mantissa m is 0.5 <= m < 1.0; compute exponent e for float64 mantissa.
                var e = x.exp - 1L; // exponent for normal mantissa m with 1.0 <= m < 2.0

                // Compute precision p for float64 mantissa.
                // If the exponent is too small, we have a denormal number before
                // rounding and fewer than p mantissa bits of precision available
                // (the exponent remains fixed but the mantissa gets shifted right).
                var p = mbits + 1L; // precision of normal float
                if (e < emin)
                { 
                    // recompute precision
                    p = mbits + 1L - emin + int(e); 
                    // If p == 0, the mantissa of x is shifted so much to the right
                    // that its msb falls immediately to the right of the float64
                    // mantissa space. In other words, if the smallest denormal is
                    // considered "1.0", for p == 0, the mantissa value m is >= 0.5.
                    // If m > 0.5, it is rounded up to 1.0; i.e., the smallest denormal.
                    // If m == 0.5, it is rounded down to even, i.e., 0.0.
                    // If p < 0, the mantissa value m is <= "0.25" which is never rounded up.
                    if (p < 0L || p == 0L && x.mant.sticky(uint(len(x.mant)) * _W - 1L) == 0L)
                    { 
                        // underflow to ±0
                        if (x.neg)
                        {
                            double z = default;
                            return (-z, Above);
                        }

                        return (0.0F, Below);

                    } 
                    // otherwise, round up
                    // We handle p == 0 explicitly because it's easy and because
                    // Float.round doesn't support rounding to 0 bits of precision.
                    if (p == 0L)
                    {
                        if (x.neg)
                        {
                            return (-math.SmallestNonzeroFloat64, Below);
                        }

                        return (math.SmallestNonzeroFloat64, Above);

                    }

                } 
                // p > 0

                // round
                Float r = default;
                r.prec = uint32(p);
                r.Set(x);
                e = r.exp - 1L; 

                // Rounding may have caused r to overflow to ±Inf
                // (rounding never causes underflows to 0).
                // If the exponent is too large, also overflow to ±Inf.
                if (r.form == inf || e > emax)
                { 
                    // overflow
                    if (x.neg)
                    {
                        return (math.Inf(-1L), Below);
                    }

                    return (math.Inf(+1L), Above);

                } 
                // e <= emax

                // Determine sign, biased exponent, and mantissa.
                ulong sign = default;                ulong bexp = default;                ulong mant = default;

                if (x.neg)
                {
                    sign = 1L << (int)((fbits - 1L));
                } 

                // Rounding may have caused a denormal number to
                // become normal. Check again.
                if (e < emin)
                { 
                    // denormal number: recompute precision
                    // Since rounding may have at best increased precision
                    // and we have eliminated p <= 0 early, we know p > 0.
                    // bexp == 0 for denormals
                    p = mbits + 1L - emin + int(e);
                    mant = msb64(r.mant) >> (int)(uint(fbits - p));

                }
                else
                { 
                    // normal number: emin <= e <= emax
                    bexp = uint64(e + bias) << (int)(mbits);
                    mant = msb64(r.mant) >> (int)(ebits) & (1L << (int)(mbits) - 1L); // cut off msb (implicit 1 bit)
                }

                return (math.Float64frombits(sign | bexp | mant), r.acc);
            else if (x.form == zero) 
                if (x.neg)
                {
                    z = default;
                    return (-z, Exact);
                }

                return (0.0F, Exact);
            else if (x.form == inf) 
                if (x.neg)
                {
                    return (math.Inf(-1L), Exact);
                }

                return (math.Inf(+1L), Exact);
                        panic("unreachable");

        });

        // Int returns the result of truncating x towards zero;
        // or nil if x is an infinity.
        // The result is Exact if x.IsInt(); otherwise it is Below
        // for x > 0, and Above for x < 0.
        // If a non-nil *Int argument z is provided, Int stores
        // the result in z instead of allocating a new Int.
        private static (ptr<Int>, Accuracy) Int(this ptr<Float> _addr_x, ptr<Int> _addr_z) => func((_, panic, __) =>
        {
            ptr<Int> _p0 = default!;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;
            ref Int z = ref _addr_z.val;

            if (debugFloat)
            {
                x.validate();
            }

            if (z == null && x.form <= finite)
            {
                z = @new<Int>();
            }


            if (x.form == finite) 
                // 0 < |x| < +Inf
                var acc = makeAcc(x.neg);
                if (x.exp <= 0L)
                { 
                    // 0 < |x| < 1
                    return (_addr_z.SetInt64(0L)!, acc);

                } 
                // x.exp > 0

                // 1 <= |x| < +Inf
                // determine minimum required precision for x
                var allBits = uint(len(x.mant)) * _W;
                var exp = uint(x.exp);
                if (x.MinPrec() <= exp)
                {
                    acc = Exact;
                } 
                // shift mantissa as needed
                if (z == null)
                {
                    z = @new<Int>();
                }

                z.neg = x.neg;

                if (exp > allBits) 
                    z.abs = z.abs.shl(x.mant, exp - allBits);
                else if (exp < allBits) 
                    z.abs = z.abs.shr(x.mant, allBits - exp);
                else 
                    z.abs = z.abs.set(x.mant);
                                return (_addr_z!, acc);
            else if (x.form == zero) 
                return (_addr_z.SetInt64(0L)!, Exact);
            else if (x.form == inf) 
                return (_addr_null!, makeAcc(x.neg));
                        panic("unreachable");

        });

        // Rat returns the rational number corresponding to x;
        // or nil if x is an infinity.
        // The result is Exact if x is not an Inf.
        // If a non-nil *Rat argument z is provided, Rat stores
        // the result in z instead of allocating a new Rat.
        private static (ptr<Rat>, Accuracy) Rat(this ptr<Float> _addr_x, ptr<Rat> _addr_z) => func((_, panic, __) =>
        {
            ptr<Rat> _p0 = default!;
            Accuracy _p0 = default;
            ref Float x = ref _addr_x.val;
            ref Rat z = ref _addr_z.val;

            if (debugFloat)
            {
                x.validate();
            }

            if (z == null && x.form <= finite)
            {
                z = @new<Rat>();
            }


            if (x.form == finite) 
                // 0 < |x| < +Inf
                var allBits = int32(len(x.mant)) * _W; 
                // build up numerator and denominator
                z.a.neg = x.neg;

                if (x.exp > allBits) 
                    z.a.abs = z.a.abs.shl(x.mant, uint(x.exp - allBits));
                    z.b.abs = z.b.abs[..0L]; // == 1 (see Rat)
                    // z already in normal form
                else if (x.exp < allBits) 
                    z.a.abs = z.a.abs.set(x.mant);
                    var t = z.b.abs.setUint64(1L);
                    z.b.abs = t.shl(t, uint(allBits - x.exp));
                    z.norm();
                else 
                    z.a.abs = z.a.abs.set(x.mant);
                    z.b.abs = z.b.abs[..0L]; // == 1 (see Rat)
                    // z already in normal form
                                return (_addr_z!, Exact);
            else if (x.form == zero) 
                return (_addr_z.SetInt64(0L)!, Exact);
            else if (x.form == inf) 
                return (_addr_null!, makeAcc(x.neg));
                        panic("unreachable");

        });

        // Abs sets z to the (possibly rounded) value |x| (the absolute value of x)
        // and returns z.
        private static ptr<Float> Abs(this ptr<Float> _addr_z, ptr<Float> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;

            z.Set(x);
            z.neg = false;
            return _addr_z!;
        }

        // Neg sets z to the (possibly rounded) value of x with its sign negated,
        // and returns z.
        private static ptr<Float> Neg(this ptr<Float> _addr_z, ptr<Float> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;

            z.Set(x);
            z.neg = !z.neg;
            return _addr_z!;
        }

        private static void validateBinaryOperands(ptr<Float> _addr_x, ptr<Float> _addr_y) => func((_, panic, __) =>
        {
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (!debugFloat)
            { 
                // avoid performance bugs
                panic("validateBinaryOperands called but debugFloat is not set");

            }

            if (len(x.mant) == 0L)
            {
                panic("empty mantissa for x");
            }

            if (len(y.mant) == 0L)
            {
                panic("empty mantissa for y");
            }

        });

        // z = x + y, ignoring signs of x and y for the addition
        // but using the sign of z for rounding the result.
        // x and y must have a non-empty mantissa and valid exponent.
        private static void uadd(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;
 
            // Note: This implementation requires 2 shifts most of the
            // time. It is also inefficient if exponents or precisions
            // differ by wide margins. The following article describes
            // an efficient (but much more complicated) implementation
            // compatible with the internal representation used here:
            //
            // Vincent Lefèvre: "The Generic Multiple-Precision Floating-
            // Point Addition With Exact Rounding (as in the MPFR Library)"
            // http://www.vinc17.net/research/papers/rnc6.pdf

            if (debugFloat)
            {
                validateBinaryOperands(_addr_x, _addr_y);
            } 

            // compute exponents ex, ey for mantissa with "binary point"
            // on the right (mantissa.0) - use int64 to avoid overflow
            var ex = int64(x.exp) - int64(len(x.mant)) * _W;
            var ey = int64(y.exp) - int64(len(y.mant)) * _W;

            var al = alias(z.mant, x.mant) || alias(z.mant, y.mant); 

            // TODO(gri) having a combined add-and-shift primitive
            //           could make this code significantly faster

            if (ex < ey) 
                if (al)
                {
                    var t = nat(null).shl(y.mant, uint(ey - ex));
                    z.mant = z.mant.add(x.mant, t);
                }
                else
                {
                    z.mant = z.mant.shl(y.mant, uint(ey - ex));
                    z.mant = z.mant.add(x.mant, z.mant);
                }

            else if (ex > ey) 
                if (al)
                {
                    t = nat(null).shl(x.mant, uint(ex - ey));
                    z.mant = z.mant.add(t, y.mant);
                }
                else
                {
                    z.mant = z.mant.shl(x.mant, uint(ex - ey));
                    z.mant = z.mant.add(z.mant, y.mant);
                }

                ex = ey;
            else 
                // ex == ey, no shift needed
                z.mant = z.mant.add(x.mant, y.mant);
            // len(z.mant) > 0

            z.setExpAndRound(ex + int64(len(z.mant)) * _W - fnorm(z.mant), 0L);

        }

        // z = x - y for |x| > |y|, ignoring signs of x and y for the subtraction
        // but using the sign of z for rounding the result.
        // x and y must have a non-empty mantissa and valid exponent.
        private static void usub(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;
 
            // This code is symmetric to uadd.
            // We have not factored the common code out because
            // eventually uadd (and usub) should be optimized
            // by special-casing, and the code will diverge.

            if (debugFloat)
            {
                validateBinaryOperands(_addr_x, _addr_y);
            }

            var ex = int64(x.exp) - int64(len(x.mant)) * _W;
            var ey = int64(y.exp) - int64(len(y.mant)) * _W;

            var al = alias(z.mant, x.mant) || alias(z.mant, y.mant);


            if (ex < ey) 
                if (al)
                {
                    var t = nat(null).shl(y.mant, uint(ey - ex));
                    z.mant = t.sub(x.mant, t);
                }
                else
                {
                    z.mant = z.mant.shl(y.mant, uint(ey - ex));
                    z.mant = z.mant.sub(x.mant, z.mant);
                }

            else if (ex > ey) 
                if (al)
                {
                    t = nat(null).shl(x.mant, uint(ex - ey));
                    z.mant = t.sub(t, y.mant);
                }
                else
                {
                    z.mant = z.mant.shl(x.mant, uint(ex - ey));
                    z.mant = z.mant.sub(z.mant, y.mant);
                }

                ex = ey;
            else 
                // ex == ey, no shift needed
                z.mant = z.mant.sub(x.mant, y.mant);
            // operands may have canceled each other out
            if (len(z.mant) == 0L)
            {
                z.acc = Exact;
                z.form = zero;
                z.neg = false;
                return ;
            } 
            // len(z.mant) > 0
            z.setExpAndRound(ex + int64(len(z.mant)) * _W - fnorm(z.mant), 0L);

        }

        // z = x * y, ignoring signs of x and y for the multiplication
        // but using the sign of z for rounding the result.
        // x and y must have a non-empty mantissa and valid exponent.
        private static void umul(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                validateBinaryOperands(_addr_x, _addr_y);
            } 

            // Note: This is doing too much work if the precision
            // of z is less than the sum of the precisions of x
            // and y which is often the case (e.g., if all floats
            // have the same precision).
            // TODO(gri) Optimize this for the common case.
            var e = int64(x.exp) + int64(y.exp);
            if (x == y)
            {
                z.mant = z.mant.sqr(x.mant);
            }
            else
            {
                z.mant = z.mant.mul(x.mant, y.mant);
            }

            z.setExpAndRound(e - fnorm(z.mant), 0L);

        }

        // z = x / y, ignoring signs of x and y for the division
        // but using the sign of z for rounding the result.
        // x and y must have a non-empty mantissa and valid exponent.
        private static void uquo(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                validateBinaryOperands(_addr_x, _addr_y);
            } 

            // mantissa length in words for desired result precision + 1
            // (at least one extra bit so we get the rounding bit after
            // the division)
            var n = int(z.prec / _W) + 1L; 

            // compute adjusted x.mant such that we get enough result precision
            var xadj = x.mant;
            {
                var d__prev1 = d;

                var d = n - len(x.mant) + len(y.mant);

                if (d > 0L)
                { 
                    // d extra words needed => add d "0 digits" to x
                    xadj = make(nat, len(x.mant) + d);
                    copy(xadj[d..], x.mant);

                } 
                // TODO(gri): If we have too many digits (d < 0), we should be able
                // to shorten x for faster division. But we must be extra careful
                // with rounding in that case.

                // Compute d before division since there may be aliasing of x.mant
                // (via xadj) or y.mant with z.mant.

                d = d__prev1;

            } 
            // TODO(gri): If we have too many digits (d < 0), we should be able
            // to shorten x for faster division. But we must be extra careful
            // with rounding in that case.

            // Compute d before division since there may be aliasing of x.mant
            // (via xadj) or y.mant with z.mant.
            d = len(xadj) - len(y.mant); 

            // divide
            nat r = default;
            z.mant, r = z.mant.div(null, xadj, y.mant);
            var e = int64(x.exp) - int64(y.exp) - int64(d - len(z.mant)) * _W; 

            // The result is long enough to include (at least) the rounding bit.
            // If there's a non-zero remainder, the corresponding fractional part
            // (if it were computed), would have a non-zero sticky bit (if it were
            // zero, it couldn't have a non-zero remainder).
            ulong sbit = default;
            if (len(r) > 0L)
            {
                sbit = 1L;
            }

            z.setExpAndRound(e - fnorm(z.mant), sbit);

        }

        // ucmp returns -1, 0, or +1, depending on whether
        // |x| < |y|, |x| == |y|, or |x| > |y|.
        // x and y must have a non-empty mantissa and valid exponent.
        private static long ucmp(this ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                validateBinaryOperands(_addr_x, _addr_y);
            }


            if (x.exp < y.exp) 
                return -1L;
            else if (x.exp > y.exp) 
                return +1L;
            // x.exp == y.exp

            // compare mantissas
            var i = len(x.mant);
            var j = len(y.mant);
            while (i > 0L || j > 0L)
            {
                Word xm = default;                Word ym = default;

                if (i > 0L)
                {
                    i--;
                    xm = x.mant[i];
                }

                if (j > 0L)
                {
                    j--;
                    ym = y.mant[j];
                }


                if (xm < ym) 
                    return -1L;
                else if (xm > ym) 
                    return +1L;
                
            }


            return 0L;

        }

        // Handling of sign bit as defined by IEEE 754-2008, section 6.3:
        //
        // When neither the inputs nor result are NaN, the sign of a product or
        // quotient is the exclusive OR of the operands’ signs; the sign of a sum,
        // or of a difference x−y regarded as a sum x+(−y), differs from at most
        // one of the addends’ signs; and the sign of the result of conversions,
        // the quantize operation, the roundToIntegral operations, and the
        // roundToIntegralExact (see 5.3.1) is the sign of the first or only operand.
        // These rules shall apply even when operands or results are zero or infinite.
        //
        // When the sum of two operands with opposite signs (or the difference of
        // two operands with like signs) is exactly zero, the sign of that sum (or
        // difference) shall be +0 in all rounding-direction attributes except
        // roundTowardNegative; under that attribute, the sign of an exact zero
        // sum (or difference) shall be −0. However, x+x = x−(−x) retains the same
        // sign as x even when x is zero.
        //
        // See also: https://play.golang.org/p/RtH3UCt5IH

        // Add sets z to the rounded sum x+y and returns z. If z's precision is 0,
        // it is changed to the larger of x's or y's precision before the operation.
        // Rounding is performed according to z's precision and rounding mode; and
        // z's accuracy reports the result error relative to the exact (not rounded)
        // result. Add panics with ErrNaN if x and y are infinities with opposite
        // signs. The value of z is undefined in that case.
        private static ptr<Float> Add(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                x.validate();
                y.validate();
            }

            if (z.prec == 0L)
            {
                z.prec = umax32(x.prec, y.prec);
            }

            if (x.form == finite && y.form == finite)
            { 
                // x + y (common case)

                // Below we set z.neg = x.neg, and when z aliases y this will
                // change the y operand's sign. This is fine, because if an
                // operand aliases the receiver it'll be overwritten, but we still
                // want the original x.neg and y.neg values when we evaluate
                // x.neg != y.neg, so we need to save y.neg before setting z.neg.
                var yneg = y.neg;

                z.neg = x.neg;
                if (x.neg == yneg)
                { 
                    // x + y == x + y
                    // (-x) + (-y) == -(x + y)
                    z.uadd(x, y);

                }
                else
                { 
                    // x + (-y) == x - y == -(y - x)
                    // (-x) + y == y - x == -(x - y)
                    if (x.ucmp(y) > 0L)
                    {
                        z.usub(x, y);
                    }
                    else
                    {
                        z.neg = !z.neg;
                        z.usub(y, x);
                    }

                }

                if (z.form == zero && z.mode == ToNegativeInf && z.acc == Exact)
                {
                    z.neg = true;
                }

                return _addr_z!;

            }

            if (x.form == inf && y.form == inf && x.neg != y.neg)
            { 
                // +Inf + -Inf
                // -Inf + +Inf
                // value of z is undefined but make sure it's valid
                z.acc = Exact;
                z.form = zero;
                z.neg = false;
                panic(new ErrNaN("addition of infinities with opposite signs"));

            }

            if (x.form == zero && y.form == zero)
            { 
                // ±0 + ±0
                z.acc = Exact;
                z.form = zero;
                z.neg = x.neg && y.neg; // -0 + -0 == -0
                return _addr_z!;

            }

            if (x.form == inf || y.form == zero)
            { 
                // ±Inf + y
                // x + ±0
                return _addr_z.Set(x)!;

            } 

            // ±0 + y
            // x + ±Inf
            return _addr_z.Set(y)!;

        });

        // Sub sets z to the rounded difference x-y and returns z.
        // Precision, rounding, and accuracy reporting are as for Add.
        // Sub panics with ErrNaN if x and y are infinities with equal
        // signs. The value of z is undefined in that case.
        private static ptr<Float> Sub(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                x.validate();
                y.validate();
            }

            if (z.prec == 0L)
            {
                z.prec = umax32(x.prec, y.prec);
            }

            if (x.form == finite && y.form == finite)
            { 
                // x - y (common case)
                var yneg = y.neg;
                z.neg = x.neg;
                if (x.neg != yneg)
                { 
                    // x - (-y) == x + y
                    // (-x) - y == -(x + y)
                    z.uadd(x, y);

                }
                else
                { 
                    // x - y == x - y == -(y - x)
                    // (-x) - (-y) == y - x == -(x - y)
                    if (x.ucmp(y) > 0L)
                    {
                        z.usub(x, y);
                    }
                    else
                    {
                        z.neg = !z.neg;
                        z.usub(y, x);
                    }

                }

                if (z.form == zero && z.mode == ToNegativeInf && z.acc == Exact)
                {
                    z.neg = true;
                }

                return _addr_z!;

            }

            if (x.form == inf && y.form == inf && x.neg == y.neg)
            { 
                // +Inf - +Inf
                // -Inf - -Inf
                // value of z is undefined but make sure it's valid
                z.acc = Exact;
                z.form = zero;
                z.neg = false;
                panic(new ErrNaN("subtraction of infinities with equal signs"));

            }

            if (x.form == zero && y.form == zero)
            { 
                // ±0 - ±0
                z.acc = Exact;
                z.form = zero;
                z.neg = x.neg && !y.neg; // -0 - +0 == -0
                return _addr_z!;

            }

            if (x.form == inf || y.form == zero)
            { 
                // ±Inf - y
                // x - ±0
                return _addr_z.Set(x)!;

            } 

            // ±0 - y
            // x - ±Inf
            return _addr_z.Neg(y)!;

        });

        // Mul sets z to the rounded product x*y and returns z.
        // Precision, rounding, and accuracy reporting are as for Add.
        // Mul panics with ErrNaN if one operand is zero and the other
        // operand an infinity. The value of z is undefined in that case.
        private static ptr<Float> Mul(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                x.validate();
                y.validate();
            }

            if (z.prec == 0L)
            {
                z.prec = umax32(x.prec, y.prec);
            }

            z.neg = x.neg != y.neg;

            if (x.form == finite && y.form == finite)
            { 
                // x * y (common case)
                z.umul(x, y);
                return _addr_z!;

            }

            z.acc = Exact;
            if (x.form == zero && y.form == inf || x.form == inf && y.form == zero)
            { 
                // ±0 * ±Inf
                // ±Inf * ±0
                // value of z is undefined but make sure it's valid
                z.form = zero;
                z.neg = false;
                panic(new ErrNaN("multiplication of zero with infinity"));

            }

            if (x.form == inf || y.form == inf)
            { 
                // ±Inf * y
                // x * ±Inf
                z.form = inf;
                return _addr_z!;

            } 

            // ±0 * y
            // x * ±0
            z.form = zero;
            return _addr_z!;

        });

        // Quo sets z to the rounded quotient x/y and returns z.
        // Precision, rounding, and accuracy reporting are as for Add.
        // Quo panics with ErrNaN if both operands are zero or infinities.
        // The value of z is undefined in that case.
        private static ptr<Float> Quo(this ptr<Float> _addr_z, ptr<Float> _addr_x, ptr<Float> _addr_y) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                x.validate();
                y.validate();
            }

            if (z.prec == 0L)
            {
                z.prec = umax32(x.prec, y.prec);
            }

            z.neg = x.neg != y.neg;

            if (x.form == finite && y.form == finite)
            { 
                // x / y (common case)
                z.uquo(x, y);
                return _addr_z!;

            }

            z.acc = Exact;
            if (x.form == zero && y.form == zero || x.form == inf && y.form == inf)
            { 
                // ±0 / ±0
                // ±Inf / ±Inf
                // value of z is undefined but make sure it's valid
                z.form = zero;
                z.neg = false;
                panic(new ErrNaN("division of zero by zero or infinity by infinity"));

            }

            if (x.form == zero || y.form == inf)
            { 
                // ±0 / y
                // x / ±Inf
                z.form = zero;
                return _addr_z!;

            } 

            // x / ±0
            // ±Inf / y
            z.form = inf;
            return _addr_z!;

        });

        // Cmp compares x and y and returns:
        //
        //   -1 if x <  y
        //    0 if x == y (incl. -0 == 0, -Inf == -Inf, and +Inf == +Inf)
        //   +1 if x >  y
        //
        private static long Cmp(this ptr<Float> _addr_x, ptr<Float> _addr_y)
        {
            ref Float x = ref _addr_x.val;
            ref Float y = ref _addr_y.val;

            if (debugFloat)
            {
                x.validate();
                y.validate();
            }

            var mx = x.ord();
            var my = y.ord();

            if (mx < my) 
                return -1L;
            else if (mx > my) 
                return +1L;
            // mx == my

            // only if |mx| == 1 we have to compare the mantissae
            switch (mx)
            {
                case -1L: 
                    return y.ucmp(x);
                    break;
                case +1L: 
                    return x.ucmp(y);
                    break;
            }

            return 0L;

        }

        // ord classifies x and returns:
        //
        //    -2 if -Inf == x
        //    -1 if -Inf < x < 0
        //     0 if x == 0 (signed or unsigned)
        //    +1 if 0 < x < +Inf
        //    +2 if x == +Inf
        //
        private static long ord(this ptr<Float> _addr_x)
        {
            ref Float x = ref _addr_x.val;

            long m = default;

            if (x.form == finite) 
                m = 1L;
            else if (x.form == zero) 
                return 0L;
            else if (x.form == inf) 
                m = 2L;
                        if (x.neg)
            {
                m = -m;
            }

            return m;

        }

        private static uint umax32(uint x, uint y)
        {
            if (x > y)
            {
                return x;
            }

            return y;

        }
    }
}}
