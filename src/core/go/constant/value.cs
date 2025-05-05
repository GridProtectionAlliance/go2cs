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
namespace go.go;

using fmt = fmt_package;
using token = go.token_package;
using math = math_package;
using big = math.big_package;
using bits = math.bits_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using utf8 = unicode.utf8_package;
using math;
using unicode;

partial class constant_package {

[GoType("num:nint")] partial struct ΔKind;

//go:generate stringer -type Kind
public static readonly ΔKind Unknown = /* iota */ 0;
public static readonly ΔKind Bool = 1;
public static readonly ΔKind ΔString = 2;
public static readonly ΔKind Int = 3;
public static readonly ΔKind Float = 4;
public static readonly ΔKind Complex = 5;

// A Value represents the value of a Go constant.
[GoType] partial interface Value {
    // Kind returns the value kind.
    ΔKind Kind();
    // String returns a short, quoted (human-readable) form of the value.
    // For numeric values, the result may be an approximation;
    // for String values the result may be a shortened string.
    // Use ExactString for a string representing a value exactly.
    @string String();
    // ExactString returns an exact, quoted (human-readable) form of the value.
    // If the Value is of Kind String, use StringVal to obtain the unquoted string.
    @string ExactString();
    // Prevent external implementations.
    void implementsValue();
}

// ----------------------------------------------------------------------------
// Implementations

// Maximum supported mantissa precision.
// The spec requires at least 256 bits; typical implementations use 512 bits.
internal static readonly UntypedInt prec = 512;

// TODO(gri) Consider storing "error" information in an unknownVal so clients
// can provide better error messages. For instance, if a number is
// too large (incl. infinity), that could be recorded in unknownVal.
// See also #20583 and #42695 for use cases.
// Representation of values:
//
// Values of Int and Float Kind have two different representations each: int64Val
// and intVal, and ratVal and floatVal. When possible, the "smaller", respectively
// more precise (for Floats) representation is chosen. However, once a Float value
// is represented as a floatVal, any subsequent results remain floatVals (unless
// explicitly converted); i.e., no attempt is made to convert a floatVal back into
// a ratVal. The reasoning is that all representations but floatVal are mathematically
// exact, but once that precision is lost (by moving to floatVal), moving back to
// a different representation implies a precision that's not actually there.
[GoType] partial struct unknownVal {
}

[GoType("bool")] partial struct boolVal;

[GoType] partial struct stringVal {
    // Lazy value: either a string (l,r==nil) or an addition (l,r!=nil).
    internal sync_package.Mutex mu;
    internal @string s;
    internal ж<stringVal> l;
    internal ж<stringVal> r;
}

[GoType("num:int64")] partial struct int64Val;

[GoType] partial struct intVal {
    internal ж<math.big_package.ΔInt> val;
}

[GoType] partial struct ratVal {
    internal ж<math.big_package.ΔRat> val;
}

[GoType] partial struct floatVal {
    internal ж<math.big_package.Float> val;
}

[GoType] partial struct complexVal {
    internal Value re;
    internal Value im;
}

internal static ΔKind Kind(this unknownVal _) {
    return Unknown;
}

internal static ΔKind Kind(this boolVal _) {
    return Bool;
}

[GoRecv] internal static ΔKind Kind(this ref stringVal _) {
    return ΔString;
}

internal static ΔKind Kind(this int64Val _) {
    return Int;
}

internal static ΔKind Kind(this intVal _) {
    return Int;
}

internal static ΔKind Kind(this ratVal _) {
    return Float;
}

internal static ΔKind Kind(this floatVal _) {
    return Float;
}

internal static ΔKind Kind(this complexVal _) {
    return Complex;
}

internal static @string String(this unknownVal _) {
    return "unknown"u8;
}

internal static @string String(this boolVal x) {
    return strconv.FormatBool(((bool)x));
}

// String returns a possibly shortened quoted form of the String value.
[GoRecv] internal static @string String(this ref stringVal x) {
    static readonly UntypedInt maxLen = 72; // a reasonable length
    @string s = strconv.Quote(x.@string());
    if (utf8.RuneCountInString(s) > maxLen) {
        // The string without the enclosing quotes is greater than maxLen-2 runes
        // long. Remove the last 3 runes (including the closing '"') by keeping
        // only the first maxLen-3 runes; then add "...".
        nint i = 0;
        for (nint n = 0; n < maxLen - 3; n++) {
            var (_, size) = utf8.DecodeRuneInString(s[(int)(i)..]);
            i += size;
        }
        s = s[..(int)(i)] + "...";
    }
    return s;
}

// string constructs and returns the actual string literal value.
// If x represents an addition, then it rewrites x to be a single
// string, to speed future calls. This lazy construction avoids
// building different string values for all subpieces of a large
// concatenation. See golang.org/issue/23348.
[GoRecv] internal static @string @string(this ref stringVal x) {
    x.mu.Lock();
    if (x.l != nil) {
        x.s = strings.Join(reverse(x.appendReverse(default!)), ""u8);
        x.l = default!;
        x.r = default!;
    }
    @string s = x.s;
    x.mu.Unlock();
    return s;
}

// reverse reverses x in place and returns it.
internal static slice<@string> reverse(slice<@string> x) {
    nint n = len(x);
    for (nint i = 0; i + i < n; i++) {
        (x[i], x[n - 1 - i]) = (x[n - 1 - i], x[i]);
    }
    return x;
}

// appendReverse appends to list all of x's subpieces, but in reverse,
// and returns the result. Appending the reversal allows processing
// the right side in a recursive call and the left side in a loop.
// Because a chain like a + b + c + d + e is actually represented
// as ((((a + b) + c) + d) + e), the left-side loop avoids deep recursion.
// x must be locked.
[GoRecv] internal static slice<@string> appendReverse(this ref stringVal x, slice<@string> list) {
    var y = x;
    while ((~y).r != nil) {
        (~(~y).r).mu.Lock();
        list = (~y).r.appendReverse(list);
        (~(~y).r).mu.Unlock();
        var l = y.val.l;
        if (y != x) {
            (~y).mu.Unlock();
        }
        (~l).mu.Lock();
        y = l;
    }
    @string s = y.val.s;
    if (y != x) {
        (~y).mu.Unlock();
    }
    return append(list, s);
}

internal static @string String(this int64Val x) {
    return strconv.FormatInt(((int64)x), 10);
}

internal static @string String(this intVal x) {
    return x.val.String();
}

internal static @string String(this ratVal x) {
    return rtof(x).String();
}

// String returns a decimal approximation of the Float value.
internal static @string String(this floatVal x) {
    var f = x.val;
    // Don't try to convert infinities (will not terminate).
    if (f.IsInf()) {
        return f.String();
    }
    // Use exact fmt formatting if in float64 range (common case):
    // proceed if f doesn't underflow to 0 or overflow to inf.
    {
        var (xΔ1, _) = f.Float64(); if (f.Sign() == 0 == (xΔ1 == 0) && !math.IsInf(xΔ1, 0)) {
            @string s = fmt.Sprintf("%.6g"u8, xΔ1);
            if (!f.IsInt() && strings.IndexByte(s, (rune)'.') < 0) {
                // f is not an integer, but its string representation
                // doesn't reflect that. Use more digits. See issue 56220.
                s = fmt.Sprintf("%g"u8, xΔ1);
            }
            return s;
        }
    }
    // Out of float64 range. Do approximate manual to decimal
    // conversion to avoid precise but possibly slow Float
    // formatting.
    // f = mant * 2**exp
    ref var mant = ref heap(new math.big_package.Float(), out var Ꮡmant);
    nint exp = f.MantExp(Ꮡmant);
    // 0.5 <= |mant| < 1.0
    // approximate float64 mantissa m and decimal exponent d
    // f ~ m * 10**d
    var (m, _) = mant.Float64();
    // 0.5 <= |m| < 1.0
    var d = ((float64)exp) * (math.Ln2 / math.Ln10);
    // log_10(2)
    // adjust m for truncated (integer) decimal exponent e
    var e = ((int64)d);
    m *= math.Pow(10, d - ((float64)e));
    // ensure 1 <= |m| < 10
    {
        var am = math.Abs(m);
        switch (ᐧ) {
        case {} when am is < 1 - 0.5e-6F: {
            m *= 10;
            e--;
            break;
        }
        case {} when am is >= 10: {
            m /= 10;
            e++;
            break;
        }}
    }

    // The %.6g format below rounds m to 5 digits after the
    // decimal point. Make sure that m*10 < 10 even after
    // rounding up: m*10 + 0.5e-5 < 10 => m < 1 - 0.5e6.
    return fmt.Sprintf("%.6ge%+d"u8, m, e);
}

internal static @string String(this complexVal x) {
    return fmt.Sprintf("(%s + %si)"u8, x.re, x.im);
}

internal static @string ExactString(this unknownVal x) {
    return x.String();
}

internal static @string ExactString(this boolVal x) {
    return x.String();
}

[GoRecv] internal static @string ExactString(this ref stringVal x) {
    return strconv.Quote(x.@string());
}

internal static @string ExactString(this int64Val x) {
    return x.String();
}

internal static @string ExactString(this intVal x) {
    return x.String();
}

internal static @string ExactString(this ratVal x) {
    var r = x.val;
    if (r.IsInt()) {
        return r.Num().String();
    }
    return r.String();
}

internal static @string ExactString(this floatVal x) {
    return x.val.Text((rune)'p', 0);
}

internal static @string ExactString(this complexVal x) {
    return fmt.Sprintf("(%s + %si)"u8, x.re.ExactString(), x.im.ExactString());
}

internal static void implementsValue(this unknownVal _) {
}

internal static void implementsValue(this boolVal _) {
}

[GoRecv] internal static void implementsValue(this ref stringVal _) {
}

internal static void implementsValue(this int64Val _) {
}

internal static void implementsValue(this ratVal _) {
}

internal static void implementsValue(this intVal _) {
}

internal static void implementsValue(this floatVal _) {
}

internal static void implementsValue(this complexVal _) {
}

internal static ж<bigꓸInt> newInt() {
    return @new<bigꓸInt>();
}

internal static ж<bigꓸRat> newRat() {
    return @new<bigꓸRat>();
}

internal static ж<big.Float> newFloat() {
    return @new<big.Float>().SetPrec(prec);
}

internal static intVal i64toi(int64Val x) {
    return new intVal(newInt().SetInt64(((int64)x)));
}

internal static ratVal i64tor(int64Val x) {
    return new ratVal(newRat().SetInt64(((int64)x)));
}

internal static floatVal i64tof(int64Val x) {
    return new floatVal(newFloat().SetInt64(((int64)x)));
}

internal static ratVal itor(intVal x) {
    return new ratVal(newRat().SetInt(x.val));
}

internal static floatVal itof(intVal x) {
    return new floatVal(newFloat().SetInt(x.val));
}

internal static floatVal rtof(ratVal x) {
    return new floatVal(newFloat().SetRat(x.val));
}

internal static complexVal vtoc(Value x) {
    return new complexVal(x, ((int64Val)0));
}

internal static Value makeInt(ж<bigꓸInt> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (x.IsInt64()) {
        return ((int64Val)x.Int64());
    }
    return new intVal(Ꮡx);
}

internal static Value makeRat(ж<bigꓸRat> Ꮡx) {
    ref var x = ref Ꮡx.val;

    var a = x.Num();
    var b = x.Denom();
    if (smallInt(a) && smallInt(b)) {
        // ok to remain fraction
        return new ratVal(Ꮡx);
    }
    // components too large => switch to float
    return new floatVal(newFloat().SetRat(Ꮡx));
}

internal static floatVal floatVal0 = new floatVal(newFloat());

internal static Value makeFloat(ж<big.Float> Ꮡx) {
    ref var x = ref Ꮡx.val;

    // convert -0
    if (x.Sign() == 0) {
        return floatVal0;
    }
    if (x.IsInf()) {
        return new unknownVal(nil);
    }
    // No attempt is made to "go back" to ratVal, even if possible,
    // to avoid providing the illusion of a mathematically exact
    // representation.
    return new floatVal(Ꮡx);
}

internal static Value makeComplex(Value re, Value im) {
    if (re.Kind() == Unknown || im.Kind() == Unknown) {
        return new unknownVal(nil);
    }
    return new complexVal(re, im);
}

internal static Value makeFloatFromLiteral(@string lit) {
    {
        var (f, ok) = newFloat().SetString(lit); if (ok) {
            if (smallFloat(f)) {
                // ok to use rationals
                if (f.Sign() == 0) {
                    // Issue 20228: If the float underflowed to zero, parse just "0".
                    // Otherwise, lit might contain a value with a large negative exponent,
                    // such as -6e-1886451601. As a float, that will underflow to 0,
                    // but it'll take forever to parse as a Rat.
                    lit = "0"u8;
                }
                {
                    var (r, okΔ1) = newRat().SetString(lit); if (okΔ1) {
                        return new ratVal(r);
                    }
                }
            }
            // otherwise use floats
            return makeFloat(f);
        }
    }
    return default!;
}

// Permit fractions with component sizes up to maxExp
// before switching to using floating-point numbers.
internal static readonly UntypedInt maxExp = /* 4 << 10 */ 4096;

// smallInt reports whether x would lead to "reasonably"-sized fraction
// if converted to a *big.Rat.
internal static bool smallInt(ж<bigꓸInt> Ꮡx) {
    ref var x = ref Ꮡx.val;

    return x.BitLen() < maxExp;
}

// smallFloat64 reports whether x would lead to "reasonably"-sized fraction
// if converted to a *big.Rat.
internal static bool smallFloat64(float64 x) {
    if (math.IsInf(x, 0)) {
        return false;
    }
    var (_, e) = math.Frexp(x);
    return -maxExp < e && e < maxExp;
}

// smallFloat reports whether x would lead to "reasonably"-sized fraction
// if converted to a *big.Rat.
internal static bool smallFloat(ж<big.Float> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (x.IsInf()) {
        return false;
    }
    nint e = x.MantExp(nil);
    return -maxExp < e && e < maxExp;
}

// ----------------------------------------------------------------------------
// Factories

// MakeUnknown returns the [Unknown] value.
public static Value MakeUnknown() {
    return new unknownVal(nil);
}

// MakeBool returns the [Bool] value for b.
public static Value MakeBool(bool b) {
    return ((boolVal)b);
}

// MakeString returns the [String] value for s.
public static Value MakeString(@string s) {
    if (s == ""u8) {
        return emptyString;
    }
    // common case
    return new stringVal(s: s);
}

internal static stringVal emptyString;

// MakeInt64 returns the [Int] value for x.
public static Value MakeInt64(int64 x) {
    return ((int64Val)x);
}

// MakeUint64 returns the [Int] value for x.
public static Value MakeUint64(uint64 x) {
    if (x < 1 << (int)(63)) {
        return ((int64Val)((int64)x));
    }
    return new intVal(newInt().SetUint64(x));
}

// MakeFloat64 returns the [Float] value for x.
// If x is -0.0, the result is 0.0.
// If x is not finite, the result is an [Unknown].
public static Value MakeFloat64(float64 x) {
    if (math.IsInf(x, 0) || math.IsNaN(x)) {
        return new unknownVal(nil);
    }
    if (smallFloat64(x)) {
        return new ratVal(newRat().SetFloat64(x + 0));
    }
    // convert -0 to 0
    return new floatVal(newFloat().SetFloat64(x + 0));
}

// MakeFromLiteral returns the corresponding integer, floating-point,
// imaginary, character, or string value for a Go literal string. The
// tok value must be one of [token.INT], [token.FLOAT], [token.IMAG],
// [token.CHAR], or [token.STRING]. The final argument must be zero.
// If the literal string syntax is invalid, the result is an [Unknown].
public static Value MakeFromLiteral(@string lit, token.Token tok, nuint zero) {
    if (zero != 0) {
        throw panic("MakeFromLiteral called with non-zero last argument");
    }
    var exprᴛ1 = tok;
    if (exprᴛ1 == token.INT) {
        {
            var (x, err) = strconv.ParseInt(lit, 0, 64); if (err == default!) {
                return ((int64Val)x);
            }
        }
        {
            var (x, ok) = newInt().SetString(lit, 0); if (ok) {
                return new intVal(x);
            }
        }
    }
    if (exprᴛ1 == token.FLOAT) {
        {
            var x = makeFloatFromLiteral(lit); if (x != default!) {
                return x;
            }
        }
    }
    if (exprᴛ1 == token.IMAG) {
        {
            nint n = len(lit); if (n > 0 && lit[n - 1] == (rune)'i') {
                {
                    var im = makeFloatFromLiteral(lit[..(int)(n - 1)]); if (im != default!) {
                        return makeComplex(((int64Val)0), im);
                    }
                }
            }
        }
    }
    if (exprᴛ1 == token.CHAR) {
        {
            nint n = len(lit); if (n >= 2) {
                {
                    var (code, _, _, err) = strconv.UnquoteChar(lit[1..(int)(n - 1)], (rune)'\''); if (err == default!) {
                        return MakeInt64(((int64)code));
                    }
                }
            }
        }
    }
    if (exprᴛ1 == token.STRING) {
        {
            var (s, err) = strconv.Unquote(lit); if (err == default!) {
                return MakeString(s);
            }
        }
    }
    { /* default: */
        throw panic(fmt.Sprintf("%v is not a valid token"u8, tok));
    }

    return new unknownVal(nil);
}

// ----------------------------------------------------------------------------
// Accessors
//
// For unknown arguments the result is the zero value for the respective
// accessor type, except for Sign, where the result is 1.

// BoolVal returns the Go boolean value of x, which must be a [Bool] or an [Unknown].
// If x is [Unknown], the result is false.
public static bool BoolVal(Value x) {
    switch (x.type()) {
    case boolVal x: {
        return ((bool)x);
    }
    case unknownVal x: {
        return false;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not a Bool"u8, x));
        break;
    }}
}

// StringVal returns the Go string value of x, which must be a [String] or an [Unknown].
// If x is [Unknown], the result is "".
public static @string StringVal(Value x) {
    switch (x.type()) {
    case stringVal.val x: {
        return x.@string();
    }
    case unknownVal x: {
        return ""u8;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not a String"u8, x));
        break;
    }}
}

// Int64Val returns the Go int64 value of x and whether the result is exact;
// x must be an [Int] or an [Unknown]. If the result is not exact, its value is undefined.
// If x is [Unknown], the result is (0, false).
public static (int64, bool) Int64Val(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return (((int64)x), true);
    }
    case intVal x: {
        return (x.val.Int64(), false);
    }
    case unknownVal x: {
        return (0, false);
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not an Int"u8, // not an int64Val and thus not exact
 x));
        break;
    }}
}

// Uint64Val returns the Go uint64 value of x and whether the result is exact;
// x must be an [Int] or an [Unknown]. If the result is not exact, its value is undefined.
// If x is [Unknown], the result is (0, false).
public static (uint64, bool) Uint64Val(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return (((uint64)x), x >= 0);
    }
    case intVal x: {
        return (x.val.Uint64(), x.val.IsUint64());
    }
    case unknownVal x: {
        return (0, false);
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not an Int"u8, x));
        break;
    }}
}

// Float32Val is like [Float64Val] but for float32 instead of float64.
public static (float32, bool) Float32Val(Value x) {
    switch (x.type()) {
    case int64Val x: {
        var f = ((float32)x);
        return (f, ((int64Val)f) == x);
    }
    case intVal x: {
        var (f, acc) = newFloat().SetInt(x.val).Float32();
        return (f, acc == big.Exact);
    }
    case ratVal x: {
        return x.val.Float32();
    }
    case floatVal x: {
        (f, acc) = x.val.Float32();
        return (f, acc == big.Exact);
    }
    case unknownVal x: {
        return (0, false);
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not a Float"u8, x));
        break;
    }}
}

// Float64Val returns the nearest Go float64 value of x and whether the result is exact;
// x must be numeric or an [Unknown], but not [Complex]. For values too small (too close to 0)
// to represent as float64, [Float64Val] silently underflows to 0. The result sign always
// matches the sign of x, even for 0.
// If x is [Unknown], the result is (0, false).
public static (float64, bool) Float64Val(Value x) {
    switch (x.type()) {
    case int64Val x: {
        var f = ((float64)((int64)x));
        return (f, ((int64Val)f) == x);
    }
    case intVal x: {
        var (f, acc) = newFloat().SetInt(x.val).Float64();
        return (f, acc == big.Exact);
    }
    case ratVal x: {
        return x.val.Float64();
    }
    case floatVal x: {
        (f, acc) = x.val.Float64();
        return (f, acc == big.Exact);
    }
    case unknownVal x: {
        return (0, false);
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not a Float"u8, x));
        break;
    }}
}

// Val returns the underlying value for a given constant. Since it returns an
// interface, it is up to the caller to type assert the result to the expected
// type. The possible dynamic return types are:
//
//	x Kind             type of result
//	-----------------------------------------
//	Bool               bool
//	String             string
//	Int                int64 or *big.Int
//	Float              *big.Float or *big.Rat
//	everything else    nil
public static any Val(Value x) {
    switch (x.type()) {
    case boolVal x: {
        return ((bool)x);
    }
    case stringVal.val x: {
        return x.@string();
    }
    case int64Val x: {
        return ((int64)x);
    }
    case intVal x: {
        return x.val;
    }
    case ratVal x: {
        return x.val;
    }
    case floatVal x: {
        return x.val;
    }
    default: {
        var x = x.type();
        return default!;
    }}
}

// Make returns the [Value] for x.
//
//	type of x        result Kind
//	----------------------------
//	bool             Bool
//	string           String
//	int64            Int
//	*big.Int         Int
//	*big.Float       Float
//	*big.Rat         Float
//	anything else    Unknown
public static Value Make(any x) {
    switch (x.type()) {
    case bool x: {
        return ((boolVal)x);
    }
    case @string x: {
        return new stringVal(s: x);
    }
    case int64 x: {
        return ((int64Val)x);
    }
    case ж<bigꓸInt> x: {
        return makeInt(Ꮡx);
    }
    case ж<bigꓸRat> x: {
        return makeRat(Ꮡx);
    }
    case ж<big.Float> x: {
        return makeFloat(Ꮡx);
    }
    default: {
        var x = x.type();
        return new unknownVal(nil);
    }}
}

// BitLen returns the number of bits required to represent
// the absolute value x in binary representation; x must be an [Int] or an [Unknown].
// If x is [Unknown], the result is 0.
public static nint BitLen(Value x) {
    switch (x.type()) {
    case int64Val x: {
        var u = ((uint64)x);
        if (x < 0) {
            u = ((uint64)(-x));
        }
        return 64 - bits.LeadingZeros64(u);
    }
    case intVal x: {
        return x.val.BitLen();
    }
    case unknownVal x: {
        return 0;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not an Int"u8, x));
        break;
    }}
}

// Sign returns -1, 0, or 1 depending on whether x < 0, x == 0, or x > 0;
// x must be numeric or [Unknown]. For complex values x, the sign is 0 if x == 0,
// otherwise it is != 0. If x is [Unknown], the result is 1.
public static nint Sign(Value x) {
    switch (x.type()) {
    case int64Val x: {
        switch (ᐧ) {
        case {} when x is < 0: {
            return -1;
        }
        case {} when x is > 0: {
            return 1;
        }}

        return 0;
    }
    case intVal x: {
        return x.val.Sign();
    }
    case ratVal x: {
        return x.val.Sign();
    }
    case floatVal x: {
        return x.val.Sign();
    }
    case complexVal x: {
        return (nint)(Sign(x.re) | Sign(x.im));
    }
    case unknownVal x: {
        return 1;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not numeric"u8, // avoid spurious division by zero errors
 x));
        break;
    }}
}

// ----------------------------------------------------------------------------
// Support for assembling/disassembling numeric values
internal static readonly GoUntyped _m = /* ^big.Word(0) */
    GoUntyped.Parse("18446744073709551615");
internal static readonly big.Word _log = /* _m>>8&1 + _m>>16&1 + _m>>32&1 */ 3;
internal static readonly UntypedInt wordSize = /* 1 << _log */ 8;

// Bytes returns the bytes for the absolute value of x in little-
// endian binary representation; x must be an [Int].
public static slice<byte> Bytes(Value x) {
    intVal t = default!;
    switch (x.type()) {
    case int64Val x: {
        t = i64toi(x);
        break;
    }
    case intVal x: {
        t = x;
        break;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not an Int"u8, x));
        break;
    }}
    var words = t.val.Bits();
    var bytes = new slice<byte>(len(words) * wordSize);
    nint i = 0;
    foreach (var (_, w) in words) {
        for (nint j = 0; j < wordSize; j++) {
            bytes[i] = ((byte)w);
            w >>= (UntypedInt)(8);
            i++;
        }
    }
    // remove leading 0's
    while (i > 0 && bytes[i - 1] == 0) {
        i--;
    }
    return bytes[..(int)(i)];
}

// MakeFromBytes returns the [Int] value given the bytes of its little-endian
// binary representation. An empty byte slice argument represents 0.
public static Value MakeFromBytes(slice<byte> bytes) {
    var words = new slice<big.Word>((len(bytes) + (wordSize - 1)) / wordSize);
    nint i = 0;
    big.Word w = default!;
    nuint s = default!;
    foreach (var (_, b) in bytes) {
        w |= (big.Word)(((big.Word)b) << (int)(s));
        {
            s += 8; if (s == wordSize * 8) {
                words[i] = w;
                i++;
                w = 0;
                s = 0;
            }
        }
    }
    // store last word
    if (i < len(words)) {
        words[i] = w;
        i++;
    }
    // remove leading 0's
    while (i > 0 && words[i - 1] == 0) {
        i--;
    }
    return makeInt(newInt().SetBits(words[..(int)(i)]));
}

// Num returns the numerator of x; x must be [Int], [Float], or [Unknown].
// If x is [Unknown], or if it is too large or small to represent as a
// fraction, the result is [Unknown]. Otherwise the result is an [Int]
// with the same sign as x.
public static Value Num(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return x;
    }
    case intVal x: {
        return x;
    }
    case ratVal x: {
        return makeInt(x.val.Num());
    }
    case floatVal x: {
        if (smallFloat(x.val)) {
            var (r, _) = x.val.Rat(nil);
            return makeInt(r.Num());
        }
        break;
    }
    case unknownVal x: {
        break;
        break;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not Int or Float"u8, x));
        break;
    }}
    return new unknownVal(nil);
}

// Denom returns the denominator of x; x must be [Int], [Float], or [Unknown].
// If x is [Unknown], or if it is too large or small to represent as a
// fraction, the result is [Unknown]. Otherwise the result is an [Int] >= 1.
public static Value Denom(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return ((int64Val)1);
    }
    case intVal x: {
        return ((int64Val)1);
    }
    case ratVal x: {
        return makeInt(x.val.Denom());
    }
    case floatVal x: {
        if (smallFloat(x.val)) {
            var (r, _) = x.val.Rat(nil);
            return makeInt(r.Denom());
        }
        break;
    }
    case unknownVal x: {
        break;
        break;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not Int or Float"u8, x));
        break;
    }}
    return new unknownVal(nil);
}

// MakeImag returns the [Complex] value x*i;
// x must be [Int], [Float], or [Unknown].
// If x is [Unknown], the result is [Unknown].
public static Value MakeImag(Value x) {
    switch (x.type()) {
    case unknownVal : {
        return x;
    }
    case int64Val : {
        return makeComplex(((int64Val)0), x);
    }
    case intVal : {
        return makeComplex(((int64Val)0), x);
    }
    case ratVal : {
        return makeComplex(((int64Val)0), x);
    }
    case floatVal : {
        return makeComplex(((int64Val)0), x);
    }
    default: {

        throw panic(fmt.Sprintf("%v not Int or Float"u8, x));
        break;
    }}

}

// Real returns the real part of x, which must be a numeric or unknown value.
// If x is [Unknown], the result is [Unknown].
public static Value Real(Value x) {
    switch (x.type()) {
    case unknownVal x: {
        return x;
    }
    case int64Val x: {
        return x;
    }
    case intVal x: {
        return x;
    }
    case ratVal x: {
        return x;
    }
    case floatVal x: {
        return x;
    }
    case complexVal x: {
        return x.re;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not numeric"u8, x));
        break;
    }}
}

// Imag returns the imaginary part of x, which must be a numeric or unknown value.
// If x is [Unknown], the result is [Unknown].
public static Value Imag(Value x) {
    switch (x.type()) {
    case unknownVal x: {
        return x;
    }
    case int64Val x: {
        return ((int64Val)0);
    }
    case intVal x: {
        return ((int64Val)0);
    }
    case ratVal x: {
        return ((int64Val)0);
    }
    case floatVal x: {
        return ((int64Val)0);
    }
    case complexVal x: {
        return x.im;
    }
    default: {
        var x = x.type();
        throw panic(fmt.Sprintf("%v not numeric"u8, x));
        break;
    }}
}

// ----------------------------------------------------------------------------
// Numeric conversions

// ToInt converts x to an [Int] value if x is representable as an [Int].
// Otherwise it returns an [Unknown].
public static Value ToInt(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return x;
    }
    case intVal x: {
        return x;
    }
    case ratVal x: {
        if (x.val.IsInt()) {
            return makeInt(x.val.Num());
        }
        break;
    }
    case floatVal x: {
        if (smallFloat(x.val)) {
            // avoid creation of huge integers
            // (Existing tests require permitting exponents of at least 1024;
            // allow any value that would also be permissible as a fraction.)
            var i = newInt();
            {
                var (_, acc) = x.val.Int(i); if (acc == big.Exact) {
                    return makeInt(i);
                }
            }
            // If we can get an integer by rounding up or down,
            // assume x is not an integer because of rounding
            // errors in prior computations.
            static readonly UntypedInt delta = 4; // a small number of bits > 0
            big.Float t = default!;
            t.SetPrec(prec - delta);
            // try rounding down a little
            t.SetMode(big.ToZero);
            t.Set(x.val);
            {
                var (_, acc) = t.Int(i); if (acc == big.Exact) {
                    return makeInt(i);
                }
            }
            // try rounding up a little
            t.SetMode(big.AwayFromZero);
            t.Set(x.val);
            {
                var (_, acc) = t.Int(i); if (acc == big.Exact) {
                    return makeInt(i);
                }
            }
        }
        break;
    }
    case complexVal x: {
        {
            var re = ToFloat(x); if (re.Kind() == Float) {
                return ToInt(re);
            }
        }
        break;
    }}
    return new unknownVal(nil);
}

// ToFloat converts x to a [Float] value if x is representable as a [Float].
// Otherwise it returns an [Unknown].
public static Value ToFloat(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return i64tor(x);
    }
    case intVal x: {
        if (smallInt(x.val)) {
            // x is always a small int
            return itor(x);
        }
        return itof(x);
    }
    case ratVal x: {
        return x;
    }
    case floatVal x: {
        return x;
    }
    case complexVal x: {
        if (Sign(x.im) == 0) {
            return ToFloat(x.re);
        }
        break;
    }}
    return new unknownVal(nil);
}

// ToComplex converts x to a [Complex] value if x is representable as a [Complex].
// Otherwise it returns an [Unknown].
public static Value ToComplex(Value x) {
    switch (x.type()) {
    case int64Val x: {
        return vtoc(x);
    }
    case intVal x: {
        return vtoc(x);
    }
    case ratVal x: {
        return vtoc(x);
    }
    case floatVal x: {
        return vtoc(x);
    }
    case complexVal x: {
        return x;
    }}
    return new unknownVal(nil);
}

// ----------------------------------------------------------------------------
// Operations

// is32bit reports whether x can be represented using 32 bits.
internal static bool is32bit(int64 x) {
    static readonly UntypedInt s = 32;
    return -1 << (int)((s - 1)) <= x && x <= 1 << (int)((s - 1)) - 1;
}

// is63bit reports whether x can be represented using 63 bits.
internal static bool is63bit(int64 x) {
    static readonly UntypedInt s = 63;
    return -1 << (int)((s - 1)) <= x && x <= 1 << (int)((s - 1)) - 1;
}

// UnaryOp returns the result of the unary expression op y.
// The operation must be defined for the operand.
// If prec > 0 it specifies the ^ (xor) result size in bits.
// If y is [Unknown], the result is [Unknown].
public static Value UnaryOp(token.Token op, Value y, nuint prec) {
    var exprᴛ1 = op;
    if (exprᴛ1 == token.ADD) {
        switch (y.type()) {
        case unknownVal : {
            return y;
        }
        case int64Val : {
            return y;
        }
        case intVal : {
            return y;
        }
        case ratVal : {
            return y;
        }
        case floatVal : {
            return y;
        }
        case complexVal : {
            return y;
        }}

    }
    if (exprᴛ1 == token.SUB) {
        switch (y.type()) {
        case unknownVal y: {
            return y;
        }
        case int64Val y: {
            {
                var z = -y; if (z != y) {
                    return z;
                }
            }
            return makeInt(newInt().Neg(big.NewInt(((int64)y))));
        }
        case intVal y: {
            return makeInt(newInt().Neg(y.val));
        }
        case ratVal y: {
            return makeRat(newRat().Neg(y.val));
        }
        case floatVal y: {
            return makeFloat(newFloat().Neg(y.val));
        }
        case complexVal y: {
            var re = UnaryOp(token.SUB, // no overflow
 y.re, 0);
            var im = UnaryOp(token.SUB, y.im, 0);
            return makeComplex(re, im);
        }}
    }
    if (exprᴛ1 == token.XOR) {
        var z = newInt();
        switch (y.type()) {
        case unknownVal y: {
            return y;
        }
        case int64Val y: {
            z.Not(big.NewInt(((int64)y)));
            break;
        }
        case intVal y: {
            z.Not(y.val);
            break;
        }
        default: {
            var y = y.type();
            goto Error;
            break;
        }}
        if (prec > 0) {
            // For unsigned types, the result will be negative and
            // thus "too large": We must limit the result precision
            // to the type's precision.
            z.AndNot(z, newInt().Lsh(big.NewInt(-1), prec));
        }
        return makeInt(z);
    }
    if (exprᴛ1 == token.NOT) {
        switch (y.type()) {
        case unknownVal y: {
            return y;
        }
        case boolVal y: {
            return !y;
        }}
    }

    // z &^= (-1)<<prec
Error:
    throw panic(fmt.Sprintf("invalid unary operation %s%v"u8, op, y));
}

internal static nint ord(Value x) {
    switch (x.type()) {
    default: {

        return -1;
    }
    case unknownVal : {
        return 0;
    }
    case boolVal : {
        return 1;
    }
    case stringVal.val : {
        return 1;
    }
    case int64Val : {
        return 2;
    }
    case intVal : {
        return 3;
    }
    case ratVal : {
        return 4;
    }
    case floatVal : {
        return 5;
    }
    case complexVal : {
        return 6;
    }}

}

// force invalid value into "x position" in match
// (don't panic here so that callers can provide a better error message)

// match returns the matching representation (same type) with the
// smallest complexity for two values x and y. If one of them is
// numeric, both of them must be numeric. If one of them is Unknown
// or invalid (say, nil) both results are that value.
internal static (Value _, Value _) match(Value x, Value y) {
    {
        nint ox = ord(x);
        nint oy = ord(y);
        switch (ᐧ) {
        case {} when ox is < oy: {
            (x, y) = match0(x, y);
            break;
        }
        case {} when ox is > oy: {
            (y, x) = match0(y, x);
            break;
        }}
    }

    return (x, y);
}

// match0 must only be called by match.
// Invariant: ord(x) < ord(y)
internal static (Value _, Value _) match0(Value x, Value y) {
    // Prefer to return the original x and y arguments when possible,
    // to avoid unnecessary heap allocations.
    switch (y.type()) {
    case intVal : {
        switch (x.type()) {
        case int64Val x1: {
            return (i64toi(x1), y);
        }}
        break;
    }
    case ratVal : {
        switch (x.type()) {
        case int64Val x1: {
            return (i64tor(x1), y);
        }
        case intVal x1: {
            return (itor(x1), y);
        }}
        break;
    }
    case floatVal : {
        switch (x.type()) {
        case int64Val x1: {
            return (i64tof(x1), y);
        }
        case intVal x1: {
            return (itof(x1), y);
        }
        case ratVal x1: {
            return (rtof(x1), y);
        }}
        break;
    }
    case complexVal : {
        return (vtoc(x), y);
    }}

    // force unknown and invalid values into "x position" in callers of match
    // (don't panic here so that callers can provide a better error message)
    return (x, x);
}

// BinaryOp returns the result of the binary expression x op y.
// The operation must be defined for the operands. If one of the
// operands is [Unknown], the result is [Unknown].
// BinaryOp doesn't handle comparisons or shifts; use [Compare]
// or [Shift] instead.
//
// To force integer division of [Int] operands, use op == [token.QUO_ASSIGN]
// instead of [token.QUO]; the result is guaranteed to be [Int] in this case.
// Division by zero leads to a run-time panic.
public static Value BinaryOp(Value x_, token.Token op, Value y_) {
    (x, y) = match(x_, y_);
    switch (x.type()) {
    case unknownVal x: {
        return x;
    }
    case boolVal x: {
        var yΔ1 = y._<boolVal>();
        var exprᴛ1 = op;
        if (exprᴛ1 == token.LAND) {
            return x && yΔ1;
        }
        if (exprᴛ1 == token.LOR) {
            return x || yΔ1;
        }

        break;
    }
    case int64Val x: {
        var a = ((int64)x);
        var b = ((int64)(y._<int64Val>()));
        int64 c = default!;
        var exprᴛ2 = op;
        if (exprᴛ2 == token.ADD) {
            if (!is63bit(a) || !is63bit(b)) {
                return makeInt(newInt().Add(big.NewInt(a), big.NewInt(b)));
            }
            c = a + b;
        }
        else if (exprᴛ2 == token.SUB) {
            if (!is63bit(a) || !is63bit(b)) {
                return makeInt(newInt().Sub(big.NewInt(a), big.NewInt(b)));
            }
            c = a - b;
        }
        else if (exprᴛ2 == token.MUL) {
            if (!is32bit(a) || !is32bit(b)) {
                return makeInt(newInt().Mul(big.NewInt(a), big.NewInt(b)));
            }
            c = a * b;
        }
        else if (exprᴛ2 == token.QUO) {
            return makeRat(big.NewRat(a, b));
        }
        if (exprᴛ2 == token.QUO_ASSIGN) {
            c = a / b;
        }
        else if (exprᴛ2 == token.REM) {
            c = a % b;
        }
        else if (exprᴛ2 == token.AND) {
            c = (int64)(a & b);
        }
        else if (exprᴛ2 == token.OR) {
            c = (int64)(a | b);
        }
        else if (exprᴛ2 == token.XOR) {
            c = (int64)(a ^ b);
        }
        else if (exprᴛ2 == token.AND_NOT) {
            c = (int64)(a & ~b);
        }
        else { /* default: */
            goto Error;
        }

        return ((int64Val)c);
    }
    case intVal x: {
        a = x.val;
        b = y._<intVal>().val;
        c = newInt();
        var exprᴛ3 = op;
        if (exprᴛ3 == token.ADD) {
            c.Add(a, // force integer division
 b);
        }
        else if (exprᴛ3 == token.SUB) {
            c.Sub(a, b);
        }
        else if (exprᴛ3 == token.MUL) {
            c.Mul(a, b);
        }
        else if (exprᴛ3 == token.QUO) {
            return makeRat(newRat().SetFrac(a, b));
        }
        if (exprᴛ3 == token.QUO_ASSIGN) {
            c.Quo(a, // force integer division
 b);
        }
        else if (exprᴛ3 == token.REM) {
            c.Rem(a, b);
        }
        else if (exprᴛ3 == token.AND) {
            c.And(a, b);
        }
        else if (exprᴛ3 == token.OR) {
            c.Or(a, b);
        }
        else if (exprᴛ3 == token.XOR) {
            c.Xor(a, b);
        }
        else if (exprᴛ3 == token.AND_NOT) {
            c.AndNot(a, b);
        }
        else { /* default: */
            goto Error;
        }

        return makeInt(c);
    }
    case ratVal x: {
        a = x.val;
        b = y._<ratVal>().val;
        c = newRat();
        var exprᴛ4 = op;
        if (exprᴛ4 == token.ADD) {
            c.Add(a, b);
        }
        else if (exprᴛ4 == token.SUB) {
            c.Sub(a, b);
        }
        else if (exprᴛ4 == token.MUL) {
            c.Mul(a, b);
        }
        else if (exprᴛ4 == token.QUO) {
            c.Quo(a, b);
        }
        else { /* default: */
            goto Error;
        }

        return makeRat(c);
    }
    case floatVal x: {
        a = x.val;
        b = y._<floatVal>().val;
        c = newFloat();
        var exprᴛ5 = op;
        if (exprᴛ5 == token.ADD) {
            c.Add(a, b);
        }
        else if (exprᴛ5 == token.SUB) {
            c.Sub(a, b);
        }
        else if (exprᴛ5 == token.MUL) {
            c.Mul(a, b);
        }
        else if (exprᴛ5 == token.QUO) {
            c.Quo(a, b);
        }
        else { /* default: */
            goto Error;
        }

        return makeFloat(c);
    }
    case complexVal x: {
        y = y._<complexVal>();
        (a, b) = (x.re, x.im);
        c = y.re;
        var d = y.im;
        Value re = default!;
        Value im = default!;
        var exprᴛ6 = op;
        if (exprᴛ6 == token.ADD) {
            re = add(a, // (a+c) + i(b+d)
 c);
            im = add(b, d);
        }
        else if (exprᴛ6 == token.SUB) {
            re = sub(a, // (a-c) + i(b-d)
 c);
            im = sub(b, d);
        }
        else if (exprᴛ6 == token.MUL) {
            var ac = mul(a, // (ac-bd) + i(bc+ad)
 c);
            var bd = mul(b, d);
            var bc = mul(b, c);
            var ad = mul(a, d);
            re = sub(ac, bd);
            im = add(bc, ad);
        }
        else if (exprᴛ6 == token.QUO) {
            var ac = mul(a, // (ac+bd)/s + i(bc-ad)/s, with s = cc + dd
 c);
            var bd = mul(b, d);
            var bc = mul(b, c);
            var ad = mul(a, d);
            var cc = mul(c, c);
            var dd = mul(d, d);
            var s = add(cc, dd);
            re = add(ac, bd);
            re = quo(re, s);
            im = sub(bc, ad);
            im = quo(im, s);
        }
        else { /* default: */
            goto Error;
        }

        return makeComplex(re, im);
    }
    case stringVal.val x: {
        if (op == token.ADD) {
            return new stringVal(l: x, r: y._<stringVal.val>());
        }
        break;
    }}
Error:
    throw panic(fmt.Sprintf("invalid binary operation %v %s %v"u8, x_, op, y_));
}

internal static Value add(Value x, Value y) {
    return BinaryOp(x, token.ADD, y);
}

internal static Value sub(Value x, Value y) {
    return BinaryOp(x, token.SUB, y);
}

internal static Value mul(Value x, Value y) {
    return BinaryOp(x, token.MUL, y);
}

internal static Value quo(Value x, Value y) {
    return BinaryOp(x, token.QUO, y);
}

// Shift returns the result of the shift expression x op s
// with op == [token.SHL] or [token.SHR] (<< or >>). x must be
// an [Int] or an [Unknown]. If x is [Unknown], the result is x.
public static Value Shift(Value x, token.Token op, nuint s) {
    switch (x.type()) {
    case unknownVal x: {
        return x;
    }
    case int64Val x: {
        if (s == 0) {
            return x;
        }
        var exprᴛ1 = op;
        if (exprᴛ1 == token.SHL) {
            var z = i64toi(x).val;
            return makeInt(z.Lsh(z, s));
        }
        if (exprᴛ1 == token.SHR) {
            return x >> (int)(s);
        }

        break;
    }
    case intVal x: {
        if (s == 0) {
            return x;
        }
        var z = newInt();
        var exprᴛ2 = op;
        if (exprᴛ2 == token.SHL) {
            return makeInt(z.Lsh(x.val, s));
        }
        if (exprᴛ2 == token.SHR) {
            return makeInt(z.Rsh(x.val, s));
        }

        break;
    }}
    throw panic(fmt.Sprintf("invalid shift %v %s %d"u8, x, op, s));
}

internal static bool cmpZero(nint x, token.Token op) {
    var exprᴛ1 = op;
    if (exprᴛ1 == token.EQL) {
        return x == 0;
    }
    if (exprᴛ1 == token.NEQ) {
        return x != 0;
    }
    if (exprᴛ1 == token.LSS) {
        return x < 0;
    }
    if (exprᴛ1 == token.LEQ) {
        return x <= 0;
    }
    if (exprᴛ1 == token.GTR) {
        return x > 0;
    }
    if (exprᴛ1 == token.GEQ) {
        return x >= 0;
    }

    throw panic(fmt.Sprintf("invalid comparison %v %s 0"u8, x, op));
}

// Compare returns the result of the comparison x op y.
// The comparison must be defined for the operands.
// If one of the operands is [Unknown], the result is
// false.
public static bool Compare(Value x_, token.Token op, Value y_) {
    (x, y) = match(x_, y_);
    switch (x.type()) {
    case unknownVal x: {
        return false;
    }
    case boolVal x: {
        var yΔ1 = y._<boolVal>();
        var exprᴛ1 = op;
        if (exprᴛ1 == token.EQL) {
            return x == yΔ1;
        }
        if (exprᴛ1 == token.NEQ) {
            return x != yΔ1;
        }

        break;
    }
    case int64Val x: {
        y = y._<int64Val>();
        var exprᴛ2 = op;
        if (exprᴛ2 == token.EQL) {
            return x == y;
        }
        if (exprᴛ2 == token.NEQ) {
            return x != y;
        }
        if (exprᴛ2 == token.LSS) {
            return x < y;
        }
        if (exprᴛ2 == token.LEQ) {
            return x <= y;
        }
        if (exprᴛ2 == token.GTR) {
            return x > y;
        }
        if (exprᴛ2 == token.GEQ) {
            return x >= y;
        }

        break;
    }
    case intVal x: {
        return cmpZero(x.val.Cmp(y._<intVal>().val), op);
    }
    case ratVal x: {
        return cmpZero(x.val.Cmp(y._<ratVal>().val), op);
    }
    case floatVal x: {
        return cmpZero(x.val.Cmp(y._<floatVal>().val), op);
    }
    case complexVal x: {
        y = y._<complexVal>();
        var re = Compare(x.re, token.EQL, y.re);
        var im = Compare(x.im, token.EQL, y.im);
        var exprᴛ3 = op;
        if (exprᴛ3 == token.EQL) {
            return re && im;
        }
        if (exprᴛ3 == token.NEQ) {
            return !re || !im;
        }

        break;
    }
    case stringVal.val x: {
        @string xs = x.@string();
        @string ys = y._<stringVal.val>().@string();
        var exprᴛ4 = op;
        if (exprᴛ4 == token.EQL) {
            return xs == ys;
        }
        if (exprᴛ4 == token.NEQ) {
            return xs != ys;
        }
        if (exprᴛ4 == token.LSS) {
            return xs < ys;
        }
        if (exprᴛ4 == token.LEQ) {
            return xs <= ys;
        }
        if (exprᴛ4 == token.GTR) {
            return xs > ys;
        }
        if (exprᴛ4 == token.GEQ) {
            return xs >= ys;
        }

        break;
    }}
    throw panic(fmt.Sprintf("invalid comparison %v %s %v"u8, x_, op, y_));
}

} // end constant_package
