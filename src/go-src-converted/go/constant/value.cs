// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package constant implements Values representing untyped
// Go constants and their corresponding operations.
//
// A special Unknown value may be used when a value
// is unknown due to an error. Operations on unknown
// values produce unknown values unless specified
// otherwise.
//
// package constant -- go2cs converted at 2020 August 29 08:47:02 UTC
// import "go/constant" ==> using constant = go.go.constant_package
// Original source: C:\Go\src\go\constant\value.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using math = go.math_package;
using big = go.math.big_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class constant_package
    {
        // Kind specifies the kind of value represented by a Value.
        public partial struct Kind // : long
        {
        }

 
        // unknown values
        public static readonly Kind Unknown = iota; 

        // non-numeric values
        public static readonly var Bool = 0;
        public static readonly var String = 1; 

        // numeric values
        public static readonly var Int = 2;
        public static readonly var Float = 3;
        public static readonly var Complex = 4;

        // A Value represents the value of a Go constant.
        public partial interface Value
        {
            @string Kind(); // String returns a short, quoted (human-readable) form of the value.
// For numeric values, the result may be an approximation;
// for String values the result may be a shortened string.
// Use ExactString for a string representing a value exactly.
            @string String(); // ExactString returns an exact, quoted (human-readable) form of the value.
// If the Value is of Kind String, use StringVal to obtain the unquoted string.
            @string ExactString(); // Prevent external implementations.
            @string implementsValue();
        }

        // ----------------------------------------------------------------------------
        // Implementations

        // Maximum supported mantissa precision.
        // The spec requires at least 256 bits; typical implementations use 512 bits.
        private static readonly long prec = 512L;



        private partial struct unknownVal
        {
        }
        private partial struct boolVal // : bool
        {
        }
        private partial struct stringVal
        {
            public sync.Mutex mu;
            public @string s;
            public ptr<stringVal> l;
            public ptr<stringVal> r;
        }
        private partial struct int64Val // : long
        {
        } // Int values representable as an int64
        private partial struct intVal
        {
            public ptr<big.Int> val;
        } // Int values not representable as an int64
        private partial struct ratVal
        {
            public ptr<big.Rat> val;
        } // Float values representable as a fraction
        private partial struct floatVal
        {
            public ptr<big.Float> val;
        } // Float values not representable as a fraction
        private partial struct complexVal
        {
            public Value re;
            public Value im;
        }        private static Kind Kind(this unknownVal _p0)
        {
            return Unknown;
        }
        private static Kind Kind(this boolVal _p0)
        {
            return Bool;
        }
        private static Kind Kind(this ref stringVal _p0)
        {
            return String;
        }
        private static Kind Kind(this int64Val _p0)
        {
            return Int;
        }
        private static Kind Kind(this intVal _p0)
        {
            return Int;
        }
        private static Kind Kind(this ratVal _p0)
        {
            return Float;
        }
        private static Kind Kind(this floatVal _p0)
        {
            return Float;
        }
        private static Kind Kind(this complexVal _p0)
        {
            return Complex;
        }

        private static @string String(this unknownVal _p0)
        {
            return "unknown";
        }
        private static @string String(this boolVal x)
        {
            return strconv.FormatBool(bool(x));
        }

        // String returns a possibly shortened quoted form of the String value.
        private static @string String(this ref stringVal x)
        {
            const long maxLen = 72L; // a reasonable length
 // a reasonable length
            var s = strconv.Quote(x.@string());
            if (utf8.RuneCountInString(s) > maxLen)
            { 
                // The string without the enclosing quotes is greater than maxLen-2 runes
                // long. Remove the last 3 runes (including the closing '"') by keeping
                // only the first maxLen-3 runes; then add "...".
                long i = 0L;
                for (long n = 0L; n < maxLen - 3L; n++)
                {
                    var (_, size) = utf8.DecodeRuneInString(s[i..]);
                    i += size;
                }

                s = s[..i] + "...";
            }
            return s;
        }

        // string constructs and returns the actual string literal value.
        // If x represents an addition, then it rewrites x to be a single
        // string, to speed future calls. This lazy construction avoids
        // building different string values for all subpieces of a large
        // concatenation. See golang.org/issue/23348.
        private static @string @string(this ref stringVal x)
        {
            x.mu.Lock();
            if (x.l != null)
            {
                x.s = strings.Join(reverse(x.appendReverse(null)), "");
                x.l = null;
                x.r = null;
            }
            var s = x.s;
            x.mu.Unlock();

            return s;
        }

        // reverse reverses x in place and returns it.
        private static slice<@string> reverse(slice<@string> x)
        {
            var n = len(x);
            for (long i = 0L; i + i < n; i++)
            {
                x[i] = x[n - 1L - i];
                x[n - 1L - i] = x[i];
            }

            return x;
        }

        // appendReverse appends to list all of x's subpieces, but in reverse,
        // and returns the result. Appending the reversal allows processing
        // the right side in a recursive call and the left side in a loop.
        // Because a chain like a + b + c + d + e is actually represented
        // as ((((a + b) + c) + d) + e), the left-side loop avoids deep recursion.
        // x must be locked.
        private static slice<@string> appendReverse(this ref stringVal x, slice<@string> list)
        {
            var y = x;
            while (y.r != null)
            {
                y.r.mu.Lock();
                list = y.r.appendReverse(list);
                y.r.mu.Unlock();

                var l = y.l;
                if (y != x)
                {
                    y.mu.Unlock();
                }
                l.mu.Lock();
                y = l;
            }

            var s = y.s;
            if (y != x)
            {
                y.mu.Unlock();
            }
            return append(list, s);
        }

        private static @string String(this int64Val x)
        {
            return strconv.FormatInt(int64(x), 10L);
        }
        private static @string String(this intVal x)
        {
            return x.val.String();
        }
        private static @string String(this ratVal x)
        {
            return rtof(x).String();
        }

        // String returns returns a decimal approximation of the Float value.
        private static @string String(this floatVal x)
        {
            var f = x.val; 

            // Don't try to convert infinities (will not terminate).
            if (f.IsInf())
            {
                return f.String();
            } 

            // Use exact fmt formatting if in float64 range (common case):
            // proceed if f doesn't underflow to 0 or overflow to inf.
            {
                var (x, _) = f.Float64();

                if (f.Sign() == 0L == (x == 0L) && !math.IsInf(x, 0L))
                {
                    return fmt.Sprintf("%.6g", x);
                } 

                // Out of float64 range. Do approximate manual to decimal
                // conversion to avoid precise but possibly slow Float
                // formatting.
                // f = mant * 2**exp

            } 

            // Out of float64 range. Do approximate manual to decimal
            // conversion to avoid precise but possibly slow Float
            // formatting.
            // f = mant * 2**exp
            big.Float mant = default;
            var exp = f.MantExp(ref mant); // 0.5 <= |mant| < 1.0

            // approximate float64 mantissa m and decimal exponent d
            // f ~ m * 10**d
            var (m, _) = mant.Float64(); // 0.5 <= |m| < 1.0
            var d = float64(exp) * (math.Ln2 / math.Ln10); // log_10(2)

            // adjust m for truncated (integer) decimal exponent e
            var e = int64(d);
            m *= math.Pow(10L, d - float64(e)); 

            // ensure 1 <= |m| < 10
            {
                var am = math.Abs(m);


                if (am < 1L - 0.5e-6F) 
                    // The %.6g format below rounds m to 5 digits after the
                    // decimal point. Make sure that m*10 < 10 even after
                    // rounding up: m*10 + 0.5e-5 < 10 => m < 1 - 0.5e6.
                    m *= 10L;
                    e--;
                else if (am >= 10L) 
                    m /= 10L;
                    e++;

            }

            return fmt.Sprintf("%.6ge%+d", m, e);
        }

        private static @string String(this complexVal x)
        {
            return fmt.Sprintf("(%s + %si)", x.re, x.im);
        }

        private static @string ExactString(this unknownVal x)
        {
            return x.String();
        }
        private static @string ExactString(this boolVal x)
        {
            return x.String();
        }
        private static @string ExactString(this ref stringVal x)
        {
            return strconv.Quote(x.@string());
        }
        private static @string ExactString(this int64Val x)
        {
            return x.String();
        }
        private static @string ExactString(this intVal x)
        {
            return x.String();
        }

        private static @string ExactString(this ratVal x)
        {
            var r = x.val;
            if (r.IsInt())
            {
                return r.Num().String();
            }
            return r.String();
        }

        private static @string ExactString(this floatVal x)
        {
            return x.val.Text('p', 0L);
        }

        private static @string ExactString(this complexVal x)
        {
            return fmt.Sprintf("(%s + %si)", x.re.ExactString(), x.im.ExactString());
        }

        private static void implementsValue(this unknownVal _p0)
        {
        }
        private static void implementsValue(this boolVal _p0)
        {
        }
        private static void implementsValue(this ref stringVal _p0)
        {
        }
        private static void implementsValue(this int64Val _p0)
        {
        }
        private static void implementsValue(this ratVal _p0)
        {
        }
        private static void implementsValue(this intVal _p0)
        {
        }
        private static void implementsValue(this floatVal _p0)
        {
        }
        private static void implementsValue(this complexVal _p0)
        {
        }

        private static ref big.Int newInt()
        {
            return @new<big.Int>();
        }
        private static ref big.Rat newRat()
        {
            return @new<big.Rat>();
        }
        private static ref big.Float newFloat()
        {
            return @new<big.Float>().SetPrec(prec);
        }

        private static intVal i64toi(int64Val x)
        {
            return new intVal(newInt().SetInt64(int64(x)));
        }
        private static ratVal i64tor(int64Val x)
        {
            return new ratVal(newRat().SetInt64(int64(x)));
        }
        private static floatVal i64tof(int64Val x)
        {
            return new floatVal(newFloat().SetInt64(int64(x)));
        }
        private static ratVal itor(intVal x)
        {
            return new ratVal(newRat().SetInt(x.val));
        }
        private static floatVal itof(intVal x)
        {
            return new floatVal(newFloat().SetInt(x.val));
        }

        private static floatVal rtof(ratVal x)
        {
            var a = newFloat().SetInt(x.val.Num());
            var b = newFloat().SetInt(x.val.Denom());
            return new floatVal(a.Quo(a,b));
        }

        private static complexVal vtoc(Value x)
        {
            return new complexVal(x,int64Val(0));
        }

        private static Value makeInt(ref big.Int x)
        {
            if (x.IsInt64())
            {
                return int64Val(x.Int64());
            }
            return new intVal(x);
        }

        // Permit fractions with component sizes up to maxExp
        // before switching to using floating-point numbers.
        private static readonly long maxExp = 4L << (int)(10L);



        private static Value makeRat(ref big.Rat x)
        {
            var a = x.Num();
            var b = x.Denom();
            if (a.BitLen() < maxExp && b.BitLen() < maxExp)
            { 
                // ok to remain fraction
                return new ratVal(x);
            } 
            // components too large => switch to float
            var fa = newFloat().SetInt(a);
            var fb = newFloat().SetInt(b);
            return new floatVal(fa.Quo(fa,fb));
        }

        private static floatVal floatVal0 = new floatVal(newFloat());

        private static Value makeFloat(ref big.Float x)
        { 
            // convert -0
            if (x.Sign() == 0L)
            {
                return floatVal0;
            }
            return new floatVal(x);
        }

        private static Value makeComplex(Value re, Value im)
        {
            return new complexVal(re,im);
        }

        private static Value makeFloatFromLiteral(@string lit)
        {
            {
                var (f, ok) = newFloat().SetString(lit);

                if (ok)
                {
                    if (smallRat(f))
                    { 
                        // ok to use rationals
                        if (f.Sign() == 0L)
                        { 
                            // Issue 20228: If the float underflowed to zero, parse just "0".
                            // Otherwise, lit might contain a value with a large negative exponent,
                            // such as -6e-1886451601. As a float, that will underflow to 0,
                            // but it'll take forever to parse as a Rat.
                            lit = "0";
                        }
                        var (r, _) = newRat().SetString(lit);
                        return new ratVal(r);
                    } 
                    // otherwise use floats
                    return makeFloat(f);
                }

            }
            return null;
        }

        // smallRat reports whether x would lead to "reasonably"-sized fraction
        // if converted to a *big.Rat.
        private static bool smallRat(ref big.Float x)
        {
            if (!x.IsInf())
            {
                var e = x.MantExp(null);
                return -maxExp < e && e < maxExp;
            }
            return false;
        }

        // ----------------------------------------------------------------------------
        // Factories

        // MakeUnknown returns the Unknown value.
        public static Value MakeUnknown()
        {
            return new unknownVal();
        }

        // MakeBool returns the Bool value for b.
        public static Value MakeBool(bool b)
        {
            return boolVal(b);
        }

        // MakeString returns the String value for s.
        public static Value MakeString(@string s)
        {
            return ref new stringVal(s:s);
        }

        // MakeInt64 returns the Int value for x.
        public static Value MakeInt64(long x)
        {
            return int64Val(x);
        }

        // MakeUint64 returns the Int value for x.
        public static Value MakeUint64(ulong x)
        {
            if (x < 1L << (int)(63L))
            {
                return int64Val(int64(x));
            }
            return new intVal(newInt().SetUint64(x));
        }

        // MakeFloat64 returns the Float value for x.
        // If x is not finite, the result is an Unknown.
        public static Value MakeFloat64(double x)
        {
            if (math.IsInf(x, 0L) || math.IsNaN(x))
            {
                return new unknownVal();
            } 
            // convert -0 to 0
            if (x == 0L)
            {
                return int64Val(0L);
            }
            return new ratVal(newRat().SetFloat64(x));
        }

        // MakeFromLiteral returns the corresponding integer, floating-point,
        // imaginary, character, or string value for a Go literal string. The
        // tok value must be one of token.INT, token.FLOAT, token.IMAG,
        // token.CHAR, or token.STRING. The final argument must be zero.
        // If the literal string syntax is invalid, the result is an Unknown.
        public static Value MakeFromLiteral(@string lit, token.Token tok, ulong zero) => func((_, panic, __) =>
        {
            if (zero != 0L)
            {
                panic("MakeFromLiteral called with non-zero last argument");
            }

            if (tok == token.INT) 
                {
                    var x__prev1 = x;

                    var (x, err) = strconv.ParseInt(lit, 0L, 64L);

                    if (err == null)
                    {
                        return int64Val(x);
                    }

                    x = x__prev1;

                }
                {
                    var x__prev1 = x;

                    var (x, ok) = newInt().SetString(lit, 0L);

                    if (ok)
                    {
                        return new intVal(x);
                    }

                    x = x__prev1;

                }
            else if (tok == token.FLOAT) 
                {
                    var x__prev1 = x;

                    var x = makeFloatFromLiteral(lit);

                    if (x != null)
                    {
                        return x;
                    }

                    x = x__prev1;

                }
            else if (tok == token.IMAG) 
                {
                    var n__prev1 = n;

                    var n = len(lit);

                    if (n > 0L && lit[n - 1L] == 'i')
                    {
                        {
                            var im = makeFloatFromLiteral(lit[..n - 1L]);

                            if (im != null)
                            {
                                return makeComplex(int64Val(0L), im);
                            }

                        }
                    }

                    n = n__prev1;

                }
            else if (tok == token.CHAR) 
                {
                    var n__prev1 = n;

                    n = len(lit);

                    if (n >= 2L)
                    {
                        {
                            var (code, _, _, err) = strconv.UnquoteChar(lit[1L..n - 1L], '\'');

                            if (err == null)
                            {
                                return MakeInt64(int64(code));
                            }

                        }
                    }

                    n = n__prev1;

                }
            else if (tok == token.STRING) 
                {
                    var (s, err) = strconv.Unquote(lit);

                    if (err == null)
                    {
                        return MakeString(s);
                    }

                }
            else 
                panic(fmt.Sprintf("%v is not a valid token", tok));
                        return new unknownVal();
        });

        // ----------------------------------------------------------------------------
        // Accessors
        //
        // For unknown arguments the result is the zero value for the respective
        // accessor type, except for Sign, where the result is 1.

        // BoolVal returns the Go boolean value of x, which must be a Bool or an Unknown.
        // If x is Unknown, the result is false.
        public static bool BoolVal(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case boolVal x:
                    return bool(x);
                    break;
                case unknownVal x:
                    return false;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not a Bool", x));
                    break;
                }
            }
        });

        // StringVal returns the Go string value of x, which must be a String or an Unknown.
        // If x is Unknown, the result is "".
        public static @string StringVal(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case ref stringVal x:
                    return x.@string();
                    break;
                case unknownVal x:
                    return "";
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not a String", x));
                    break;
                }
            }
        });

        // Int64Val returns the Go int64 value of x and whether the result is exact;
        // x must be an Int or an Unknown. If the result is not exact, its value is undefined.
        // If x is Unknown, the result is (0, false).
        public static (long, bool) Int64Val(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    return (int64(x), true);
                    break;
                case intVal x:
                    return (x.val.Int64(), false); // not an int64Val and thus not exact
                    break;
                case unknownVal x:
                    return (0L, false);
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not an Int", x));
                    break;
                }
            }
        });

        // Uint64Val returns the Go uint64 value of x and whether the result is exact;
        // x must be an Int or an Unknown. If the result is not exact, its value is undefined.
        // If x is Unknown, the result is (0, false).
        public static (ulong, bool) Uint64Val(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    return (uint64(x), x >= 0L);
                    break;
                case intVal x:
                    return (x.val.Uint64(), x.val.IsUint64());
                    break;
                case unknownVal x:
                    return (0L, false);
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not an Int", x));
                    break;
                }
            }
        });

        // Float32Val is like Float64Val but for float32 instead of float64.
        public static (float, bool) Float32Val(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    var f = float32(x);
                    return (f, int64Val(f) == x);
                    break;
                case intVal x:
                    var (f, acc) = newFloat().SetInt(x.val).Float32();
                    return (f, acc == big.Exact);
                    break;
                case ratVal x:
                    return x.val.Float32();
                    break;
                case floatVal x:
                    (f, acc) = x.val.Float32();
                    return (f, acc == big.Exact);
                    break;
                case unknownVal x:
                    return (0L, false);
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not a Float", x));
                    break;
                }
            }
        });

        // Float64Val returns the nearest Go float64 value of x and whether the result is exact;
        // x must be numeric or an Unknown, but not Complex. For values too small (too close to 0)
        // to represent as float64, Float64Val silently underflows to 0. The result sign always
        // matches the sign of x, even for 0.
        // If x is Unknown, the result is (0, false).
        public static (double, bool) Float64Val(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    var f = float64(int64(x));
                    return (f, int64Val(f) == x);
                    break;
                case intVal x:
                    var (f, acc) = newFloat().SetInt(x.val).Float64();
                    return (f, acc == big.Exact);
                    break;
                case ratVal x:
                    return x.val.Float64();
                    break;
                case floatVal x:
                    (f, acc) = x.val.Float64();
                    return (f, acc == big.Exact);
                    break;
                case unknownVal x:
                    return (0L, false);
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not a Float", x));
                    break;
                }
            }
        });

        // BitLen returns the number of bits required to represent
        // the absolute value x in binary representation; x must be an Int or an Unknown.
        // If x is Unknown, the result is 0.
        public static long BitLen(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    return i64toi(x).val.BitLen();
                    break;
                case intVal x:
                    return x.val.BitLen();
                    break;
                case unknownVal x:
                    return 0L;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not an Int", x));
                    break;
                }
            }
        });

        // Sign returns -1, 0, or 1 depending on whether x < 0, x == 0, or x > 0;
        // x must be numeric or Unknown. For complex values x, the sign is 0 if x == 0,
        // otherwise it is != 0. If x is Unknown, the result is 1.
        public static long Sign(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:

                    if (x < 0L) 
                        return -1L;
                    else if (x > 0L) 
                        return 1L;
                                        return 0L;
                    break;
                case intVal x:
                    return x.val.Sign();
                    break;
                case ratVal x:
                    return x.val.Sign();
                    break;
                case floatVal x:
                    return x.val.Sign();
                    break;
                case complexVal x:
                    return Sign(x.re) | Sign(x.im);
                    break;
                case unknownVal x:
                    return 1L; // avoid spurious division by zero errors
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not numeric", x));
                    break;
                }
            }
        });

        // ----------------------------------------------------------------------------
        // Support for assembling/disassembling numeric values

 
        // Compute the size of a Word in bytes.
        private static readonly var _m = ~big.Word(0L);
        private static readonly var _log = _m >> (int)(8L) & 1L + _m >> (int)(16L) & 1L + _m >> (int)(32L) & 1L;
        private static readonly long wordSize = 1L << (int)(_log);

        // Bytes returns the bytes for the absolute value of x in little-
        // endian binary representation; x must be an Int.
        public static slice<byte> Bytes(Value x) => func((_, panic, __) =>
        {
            intVal t = default;
            switch (x.type())
            {
                case int64Val x:
                    t = i64toi(x);
                    break;
                case intVal x:
                    t = x;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not an Int", x));
                    break;
                }

            }

            var words = t.val.Bits();
            var bytes = make_slice<byte>(len(words) * wordSize);

            long i = 0L;
            foreach (var (_, w) in words)
            {
                for (long j = 0L; j < wordSize; j++)
                {
                    bytes[i] = byte(w);
                    w >>= 8L;
                    i++;
                }

            } 
            // remove leading 0's
            while (i > 0L && bytes[i - 1L] == 0L)
            {
                i--;
            }


            return bytes[..i];
        });

        // MakeFromBytes returns the Int value given the bytes of its little-endian
        // binary representation. An empty byte slice argument represents 0.
        public static Value MakeFromBytes(slice<byte> bytes)
        {
            var words = make_slice<big.Word>((len(bytes) + (wordSize - 1L)) / wordSize);

            long i = 0L;
            big.Word w = default;
            ulong s = default;
            foreach (var (_, b) in bytes)
            {
                w |= big.Word(b) << (int)(s);
                s += 8L;

                if (s == wordSize * 8L)
                {
                    words[i] = w;
                    i++;
                    w = 0L;
                    s = 0L;
                }
            } 
            // store last word
            if (i < len(words))
            {
                words[i] = w;
                i++;
            } 
            // remove leading 0's
            while (i > 0L && words[i - 1L] == 0L)
            {
                i--;
            }


            return makeInt(newInt().SetBits(words[..i]));
        }

        // Num returns the numerator of x; x must be Int, Float, or Unknown.
        // If x is Unknown, or if it is too large or small to represent as a
        // fraction, the result is Unknown. Otherwise the result is an Int
        // with the same sign as x.
        public static Value Num(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    return x;
                    break;
                case intVal x:
                    return x;
                    break;
                case ratVal x:
                    return makeInt(x.val.Num());
                    break;
                case floatVal x:
                    if (smallRat(x.val))
                    {
                        var (r, _) = x.val.Rat(null);
                        return makeInt(r.Num());
                    }
                    break;
                case unknownVal x:
                    break;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not Int or Float", x));
                    break;
                }
            }
            return new unknownVal();
        });

        // Denom returns the denominator of x; x must be Int, Float, or Unknown.
        // If x is Unknown, or if it is too large or small to represent as a
        // fraction, the result is Unknown. Otherwise the result is an Int >= 1.
        public static Value Denom(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case int64Val x:
                    return int64Val(1L);
                    break;
                case intVal x:
                    return int64Val(1L);
                    break;
                case ratVal x:
                    return makeInt(x.val.Denom());
                    break;
                case floatVal x:
                    if (smallRat(x.val))
                    {
                        var (r, _) = x.val.Rat(null);
                        return makeInt(r.Denom());
                    }
                    break;
                case unknownVal x:
                    break;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not Int or Float", x));
                    break;
                }
            }
            return new unknownVal();
        });

        // MakeImag returns the Complex value x*i;
        // x must be Int, Float, or Unknown.
        // If x is Unknown, the result is Unknown.
        public static Value MakeImag(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case unknownVal _:
                    return x;
                    break;
                case int64Val _:
                    return makeComplex(int64Val(0L), x);
                    break;
                case intVal _:
                    return makeComplex(int64Val(0L), x);
                    break;
                case ratVal _:
                    return makeComplex(int64Val(0L), x);
                    break;
                case floatVal _:
                    return makeComplex(int64Val(0L), x);
                    break;
                default:
                {
                    panic(fmt.Sprintf("%v not Int or Float", x));
                    break;
                }
            }
        });

        // Real returns the real part of x, which must be a numeric or unknown value.
        // If x is Unknown, the result is Unknown.
        public static Value Real(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case unknownVal x:
                    return x;
                    break;
                case int64Val x:
                    return x;
                    break;
                case intVal x:
                    return x;
                    break;
                case ratVal x:
                    return x;
                    break;
                case floatVal x:
                    return x;
                    break;
                case complexVal x:
                    return x.re;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not numeric", x));
                    break;
                }
            }
        });

        // Imag returns the imaginary part of x, which must be a numeric or unknown value.
        // If x is Unknown, the result is Unknown.
        public static Value Imag(Value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case unknownVal x:
                    return x;
                    break;
                case int64Val x:
                    return int64Val(0L);
                    break;
                case intVal x:
                    return int64Val(0L);
                    break;
                case ratVal x:
                    return int64Val(0L);
                    break;
                case floatVal x:
                    return int64Val(0L);
                    break;
                case complexVal x:
                    return x.im;
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("%v not numeric", x));
                    break;
                }
            }
        });

        // ----------------------------------------------------------------------------
        // Numeric conversions

        // ToInt converts x to an Int value if x is representable as an Int.
        // Otherwise it returns an Unknown.
        public static Value ToInt(Value x)
        {
            switch (x.type())
            {
                case int64Val x:
                    return x;
                    break;
                case intVal x:
                    return x;
                    break;
                case ratVal x:
                    if (x.val.IsInt())
                    {
                        return makeInt(x.val.Num());
                    }
                    break;
                case floatVal x:
                    if (smallRat(x.val))
                    {
                        var i = newInt();
                        {
                            var (_, acc) = x.val.Int(i);

                            if (acc == big.Exact)
                            {
                                return makeInt(i);
                            } 

                            // If we can get an integer by rounding up or down,
                            // assume x is not an integer because of rounding
                            // errors in prior computations.

                        } 

                        // If we can get an integer by rounding up or down,
                        // assume x is not an integer because of rounding
                        // errors in prior computations.

                        const long delta = 4L; // a small number of bits > 0
 // a small number of bits > 0
                        big.Float t = default;
                        t.SetPrec(prec - delta); 

                        // try rounding down a little
                        t.SetMode(big.ToZero);
                        t.Set(x.val);
                        {
                            (_, acc) = t.Int(i);

                            if (acc == big.Exact)
                            {
                                return makeInt(i);
                            } 

                            // try rounding up a little

                        } 

                        // try rounding up a little
                        t.SetMode(big.AwayFromZero);
                        t.Set(x.val);
                        {
                            (_, acc) = t.Int(i);

                            if (acc == big.Exact)
                            {
                                return makeInt(i);
                            }

                        }
                    }
                    break;
                case complexVal x:
                    {
                        var re = ToFloat(x);

                        if (re.Kind() == Float)
                        {
                            return ToInt(re);
                        }

                    }
                    break;

            }

            return new unknownVal();
        }

        // ToFloat converts x to a Float value if x is representable as a Float.
        // Otherwise it returns an Unknown.
        public static Value ToFloat(Value x)
        {
            switch (x.type())
            {
                case int64Val x:
                    return i64tof(x);
                    break;
                case intVal x:
                    return itof(x);
                    break;
                case ratVal x:
                    return x;
                    break;
                case floatVal x:
                    return x;
                    break;
                case complexVal x:
                    {
                        var im = ToInt(x.im);

                        if (im.Kind() == Int && Sign(im) == 0L)
                        { 
                            // imaginary component is 0
                            return ToFloat(x.re);
                        }

                    }
                    break;
            }
            return new unknownVal();
        }

        // ToComplex converts x to a Complex value if x is representable as a Complex.
        // Otherwise it returns an Unknown.
        public static Value ToComplex(Value x)
        {
            switch (x.type())
            {
                case int64Val x:
                    return vtoc(i64tof(x));
                    break;
                case intVal x:
                    return vtoc(itof(x));
                    break;
                case ratVal x:
                    return vtoc(x);
                    break;
                case floatVal x:
                    return vtoc(x);
                    break;
                case complexVal x:
                    return x;
                    break;
            }
            return new unknownVal();
        }

        // ----------------------------------------------------------------------------
        // Operations

        // is32bit reports whether x can be represented using 32 bits.
        private static bool is32bit(long x)
        {
            const long s = 32L;

            return -1L << (int)((s - 1L)) <= x && x <= 1L << (int)((s - 1L)) - 1L;
        }

        // is63bit reports whether x can be represented using 63 bits.
        private static bool is63bit(long x)
        {
            const long s = 63L;

            return -1L << (int)((s - 1L)) <= x && x <= 1L << (int)((s - 1L)) - 1L;
        }

        // UnaryOp returns the result of the unary expression op y.
        // The operation must be defined for the operand.
        // If prec > 0 it specifies the ^ (xor) result size in bits.
        // If y is Unknown, the result is Unknown.
        //
        public static Value UnaryOp(token.Token op, Value y, ulong prec) => func((_, panic, __) =>
        {

            if (op == token.ADD) 
                switch (y.type())
                {
                    case unknownVal _:
                        return y;
                        break;
                    case int64Val _:
                        return y;
                        break;
                    case intVal _:
                        return y;
                        break;
                    case ratVal _:
                        return y;
                        break;
                    case floatVal _:
                        return y;
                        break;
                    case complexVal _:
                        return y;
                        break;

                }
            else if (op == token.SUB) 
                switch (y.type())
                {
                    case unknownVal y:
                        return y;
                        break;
                    case int64Val y:
                        {
                            var z__prev1 = z;

                            var z = -y;

                            if (z != y)
                            {
                                return z; // no overflow
                            }

                            z = z__prev1;

                        }
                        return makeInt(newInt().Neg(big.NewInt(int64(y))));
                        break;
                    case intVal y:
                        return makeInt(newInt().Neg(y.val));
                        break;
                    case ratVal y:
                        return makeRat(newRat().Neg(y.val));
                        break;
                    case floatVal y:
                        return makeFloat(newFloat().Neg(y.val));
                        break;
                    case complexVal y:
                        var re = UnaryOp(token.SUB, y.re, 0L);
                        var im = UnaryOp(token.SUB, y.im, 0L);
                        return makeComplex(re, im);
                        break;

                }
            else if (op == token.XOR) 
                z = newInt();
                switch (y.type())
                {
                    case unknownVal y:
                        return y;
                        break;
                    case int64Val y:
                        z.Not(big.NewInt(int64(y)));
                        break;
                    case intVal y:
                        z.Not(y.val);
                        break;
                    default:
                    {
                        var y = y.type();
                        goto Error;
                        break;
                    } 
                    // For unsigned types, the result will be negative and
                    // thus "too large": We must limit the result precision
                    // to the type's precision.
                } 
                // For unsigned types, the result will be negative and
                // thus "too large": We must limit the result precision
                // to the type's precision.
                if (prec > 0L)
                {
                    z.AndNot(z, newInt().Lsh(big.NewInt(-1L), prec)); // z &^= (-1)<<prec
                }
                return makeInt(z);
            else if (op == token.NOT) 
                switch (y.type())
                {
                    case unknownVal y:
                        return y;
                        break;
                    case boolVal y:
                        return !y;
                        break;
                }
            Error:
            panic(fmt.Sprintf("invalid unary operation %s%v", op, y));
        });

        private static long ord(Value x)
        {
            switch (x.type())
            {
                case unknownVal _:
                    return 0L;
                    break;
                case boolVal _:
                    return 1L;
                    break;
                case ref stringVal _:
                    return 1L;
                    break;
                case int64Val _:
                    return 2L;
                    break;
                case intVal _:
                    return 3L;
                    break;
                case ratVal _:
                    return 4L;
                    break;
                case floatVal _:
                    return 5L;
                    break;
                case complexVal _:
                    return 6L;
                    break;
                default:
                {
                    return -1L;
                    break;
                }
            }
        }

        // match returns the matching representation (same type) with the
        // smallest complexity for two values x and y. If one of them is
        // numeric, both of them must be numeric. If one of them is Unknown
        // or invalid (say, nil) both results are that value.
        //
        private static (Value, Value) match(Value x, Value y)
        {
            if (ord(x) > ord(y))
            {
                y, x = match(y, x);
                return (x, y);
            } 
            // ord(x) <= ord(y)
            switch (x.type())
            {
                case boolVal x:
                    return (x, y);
                    break;
                case ref stringVal x:
                    return (x, y);
                    break;
                case complexVal x:
                    return (x, y);
                    break;
                case int64Val x:
                    switch (y.type())
                    {
                        case int64Val y:
                            return (x, y);
                            break;
                        case intVal y:
                            return (i64toi(x), y);
                            break;
                        case ratVal y:
                            return (i64tor(x), y);
                            break;
                        case floatVal y:
                            return (i64tof(x), y);
                            break;
                        case complexVal y:
                            return (vtoc(x), y);
                            break;

                    }
                    break;
                case intVal x:
                    switch (y.type())
                    {
                        case intVal y:
                            return (x, y);
                            break;
                        case ratVal y:
                            return (itor(x), y);
                            break;
                        case floatVal y:
                            return (itof(x), y);
                            break;
                        case complexVal y:
                            return (vtoc(x), y);
                            break;

                    }
                    break;
                case ratVal x:
                    switch (y.type())
                    {
                        case ratVal y:
                            return (x, y);
                            break;
                        case floatVal y:
                            return (rtof(x), y);
                            break;
                        case complexVal y:
                            return (vtoc(x), y);
                            break;

                    }
                    break;
                case floatVal x:
                    switch (y.type())
                    {
                        case floatVal y:
                            return (x, y);
                            break;
                        case complexVal y:
                            return (vtoc(x), y);
                            break;
                    }
                    break; 

                // force unknown and invalid values into "x position" in callers of match
                // (don't panic here so that callers can provide a better error message)
            } 

            // force unknown and invalid values into "x position" in callers of match
            // (don't panic here so that callers can provide a better error message)
            return (x, x);
        }

        // BinaryOp returns the result of the binary expression x op y.
        // The operation must be defined for the operands. If one of the
        // operands is Unknown, the result is Unknown.
        // BinaryOp doesn't handle comparisons or shifts; use Compare
        // or Shift instead.
        //
        // To force integer division of Int operands, use op == token.QUO_ASSIGN
        // instead of token.QUO; the result is guaranteed to be Int in this case.
        // Division by zero leads to a run-time panic.
        //
        public static Value BinaryOp(Value x_, token.Token op, Value y_) => func((_, panic, __) =>
        {
            var (x, y) = match(x_, y_);

            switch (x.type())
            {
                case unknownVal x:
                    return x;
                    break;
                case boolVal x:
                    boolVal y = y._<boolVal>();

                    if (op == token.LAND) 
                        return x && y;
                    else if (op == token.LOR) 
                        return x || y;
                                        break;
                case int64Val x:
                    var a = int64(x);
                    var b = int64(y._<int64Val>());
                    long c = default;

                    if (op == token.ADD) 
                        if (!is63bit(a) || !is63bit(b))
                        {
                            return makeInt(newInt().Add(big.NewInt(a), big.NewInt(b)));
                        }
                        c = a + b;
                    else if (op == token.SUB) 
                        if (!is63bit(a) || !is63bit(b))
                        {
                            return makeInt(newInt().Sub(big.NewInt(a), big.NewInt(b)));
                        }
                        c = a - b;
                    else if (op == token.MUL) 
                        if (!is32bit(a) || !is32bit(b))
                        {
                            return makeInt(newInt().Mul(big.NewInt(a), big.NewInt(b)));
                        }
                        c = a * b;
                    else if (op == token.QUO) 
                        return makeRat(big.NewRat(a, b));
                    else if (op == token.QUO_ASSIGN) // force integer division
                        c = a / b;
                    else if (op == token.REM) 
                        c = a % b;
                    else if (op == token.AND) 
                        c = a & b;
                    else if (op == token.OR) 
                        c = a | b;
                    else if (op == token.XOR) 
                        c = a ^ b;
                    else if (op == token.AND_NOT) 
                        c = a & ~b;
                    else 
                        goto Error;
                                        return int64Val(c);
                    break;
                case intVal x:
                    a = x.val;
                    b = y._<intVal>().val;
                    c = newInt();

                    if (op == token.ADD) 
                        c.Add(a, b);
                    else if (op == token.SUB) 
                        c.Sub(a, b);
                    else if (op == token.MUL) 
                        c.Mul(a, b);
                    else if (op == token.QUO) 
                        return makeRat(newRat().SetFrac(a, b));
                    else if (op == token.QUO_ASSIGN) // force integer division
                        c.Quo(a, b);
                    else if (op == token.REM) 
                        c.Rem(a, b);
                    else if (op == token.AND) 
                        c.And(a, b);
                    else if (op == token.OR) 
                        c.Or(a, b);
                    else if (op == token.XOR) 
                        c.Xor(a, b);
                    else if (op == token.AND_NOT) 
                        c.AndNot(a, b);
                    else 
                        goto Error;
                                        return makeInt(c);
                    break;
                case ratVal x:
                    a = x.val;
                    b = y._<ratVal>().val;
                    c = newRat();

                    if (op == token.ADD) 
                        c.Add(a, b);
                    else if (op == token.SUB) 
                        c.Sub(a, b);
                    else if (op == token.MUL) 
                        c.Mul(a, b);
                    else if (op == token.QUO) 
                        c.Quo(a, b);
                    else 
                        goto Error;
                                        return makeRat(c);
                    break;
                case floatVal x:
                    a = x.val;
                    b = y._<floatVal>().val;
                    c = newFloat();

                    if (op == token.ADD) 
                        c.Add(a, b);
                    else if (op == token.SUB) 
                        c.Sub(a, b);
                    else if (op == token.MUL) 
                        c.Mul(a, b);
                    else if (op == token.QUO) 
                        c.Quo(a, b);
                    else 
                        goto Error;
                                        return makeFloat(c);
                    break;
                case complexVal x:
                    y = y._<complexVal>();
                    a = x.re;
                    b = x.im;
                    c = y.re;
                    var d = y.im;
                    Value re = default;                    Value im = default;


                    if (op == token.ADD) 
                        // (a+c) + i(b+d)
                        re = Value.As(add(a, c));
                        im = Value.As(add(b, d));
                    else if (op == token.SUB) 
                        // (a-c) + i(b-d)
                        re = Value.As(sub(a, c));
                        im = Value.As(sub(b, d));
                    else if (op == token.MUL) 
                        // (ac-bd) + i(bc+ad)
                        var ac = mul(a, c);
                        var bd = mul(b, d);
                        var bc = mul(b, c);
                        var ad = mul(a, d);
                        re = Value.As(sub(ac, bd));
                        im = Value.As(add(bc, ad));
                    else if (op == token.QUO) 
                        // (ac+bd)/s + i(bc-ad)/s, with s = cc + dd
                        ac = mul(a, c);
                        bd = mul(b, d);
                        bc = mul(b, c);
                        ad = mul(a, d);
                        var cc = mul(c, c);
                        var dd = mul(d, d);
                        var s = add(cc, dd);
                        re = Value.As(add(ac, bd));
                        re = Value.As(quo(re, s));
                        im = Value.As(sub(bc, ad));
                        im = Value.As(quo(im, s));
                    else 
                        goto Error;
                                        return makeComplex(re, im);
                    break;
                case ref stringVal x:
                    if (op == token.ADD)
                    {
                        return ref new stringVal(l:x,r:y.(*stringVal));
                    }
                    break;

            }

Error:
            panic(fmt.Sprintf("invalid binary operation %v %s %v", x_, op, y_));
        });

        private static Value add(Value x, Value y)
        {
            return BinaryOp(x, token.ADD, y);
        }
        private static Value sub(Value x, Value y)
        {
            return BinaryOp(x, token.SUB, y);
        }
        private static Value mul(Value x, Value y)
        {
            return BinaryOp(x, token.MUL, y);
        }
        private static Value quo(Value x, Value y)
        {
            return BinaryOp(x, token.QUO, y);
        }

        // Shift returns the result of the shift expression x op s
        // with op == token.SHL or token.SHR (<< or >>). x must be
        // an Int or an Unknown. If x is Unknown, the result is x.
        //
        public static Value Shift(Value x, token.Token op, ulong s) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case unknownVal x:
                    return x;
                    break;
                case int64Val x:
                    if (s == 0L)
                    {
                        return x;
                    }

                    if (op == token.SHL) 
                        var z = i64toi(x).val;
                        return makeInt(z.Lsh(z, s));
                    else if (op == token.SHR) 
                        return x >> (int)(s);
                                        break;
                case intVal x:
                    if (s == 0L)
                    {
                        return x;
                    }
                    z = newInt();

                    if (op == token.SHL) 
                        return makeInt(z.Lsh(x.val, s));
                    else if (op == token.SHR) 
                        return makeInt(z.Rsh(x.val, s));
                                        break;

            }

            panic(fmt.Sprintf("invalid shift %v %s %d", x, op, s));
        });

        private static bool cmpZero(long x, token.Token op) => func((_, panic, __) =>
        {

            if (op == token.EQL) 
                return x == 0L;
            else if (op == token.NEQ) 
                return x != 0L;
            else if (op == token.LSS) 
                return x < 0L;
            else if (op == token.LEQ) 
                return x <= 0L;
            else if (op == token.GTR) 
                return x > 0L;
            else if (op == token.GEQ) 
                return x >= 0L;
                        panic(fmt.Sprintf("invalid comparison %v %s 0", x, op));
        });

        // Compare returns the result of the comparison x op y.
        // The comparison must be defined for the operands.
        // If one of the operands is Unknown, the result is
        // false.
        //
        public static bool Compare(Value x_, token.Token op, Value y_) => func((_, panic, __) =>
        {
            var (x, y) = match(x_, y_);

            switch (x.type())
            {
                case unknownVal x:
                    return false;
                    break;
                case boolVal x:
                    boolVal y = y._<boolVal>();

                    if (op == token.EQL) 
                        return x == y;
                    else if (op == token.NEQ) 
                        return x != y;
                                        break;
                case int64Val x:
                    y = y._<int64Val>();

                    if (op == token.EQL) 
                        return x == y;
                    else if (op == token.NEQ) 
                        return x != y;
                    else if (op == token.LSS) 
                        return x < y;
                    else if (op == token.LEQ) 
                        return x <= y;
                    else if (op == token.GTR) 
                        return x > y;
                    else if (op == token.GEQ) 
                        return x >= y;
                                        break;
                case intVal x:
                    return cmpZero(x.val.Cmp(y._<intVal>().val), op);
                    break;
                case ratVal x:
                    return cmpZero(x.val.Cmp(y._<ratVal>().val), op);
                    break;
                case floatVal x:
                    return cmpZero(x.val.Cmp(y._<floatVal>().val), op);
                    break;
                case complexVal x:
                    y = y._<complexVal>();
                    var re = Compare(x.re, token.EQL, y.re);
                    var im = Compare(x.im, token.EQL, y.im);

                    if (op == token.EQL) 
                        return re && im;
                    else if (op == token.NEQ) 
                        return !re || !im;
                                        break;
                case ref stringVal x:
                    var xs = x.@string();
                    ref stringVal ys = y._<ref stringVal>().@string();

                    if (op == token.EQL) 
                        return xs == ys;
                    else if (op == token.NEQ) 
                        return xs != ys;
                    else if (op == token.LSS) 
                        return xs < ys;
                    else if (op == token.LEQ) 
                        return xs <= ys;
                    else if (op == token.GTR) 
                        return xs > ys;
                    else if (op == token.GEQ) 
                        return xs >= ys;
                                        break;

            }

            panic(fmt.Sprintf("invalid comparison %v %s %v", x_, op, y_));
        });
    }
}}
