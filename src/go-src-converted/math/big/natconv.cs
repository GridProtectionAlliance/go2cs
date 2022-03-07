// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements nat-to-string conversion functions.

// package big -- go2cs converted at 2022 March 06 22:18:01 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\natconv.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using bits = go.math.bits_package;
using sync = go.sync_package;

namespace go.math;

public static partial class big_package {

private static readonly @string digits = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

// Note: MaxBase = len(digits), but it must remain an untyped rune constant
//       for API compatibility.

// MaxBase is the largest number base accepted for string conversions.


// Note: MaxBase = len(digits), but it must remain an untyped rune constant
//       for API compatibility.

// MaxBase is the largest number base accepted for string conversions.
public static readonly nint MaxBase = 10 + ('z' - 'a' + 1) + ('Z' - 'A' + 1);

private static readonly nint maxBaseSmall = 10 + ('z' - 'a' + 1);

// maxPow returns (b**n, n) such that b**n is the largest power b**n <= _M.
// For instance maxPow(10) == (1e19, 19) for 19 decimal digits in a 64bit Word.
// In other words, at most n digits in base b fit into a Word.
// TODO(gri) replace this with a table, generated at build time.


// maxPow returns (b**n, n) such that b**n is the largest power b**n <= _M.
// For instance maxPow(10) == (1e19, 19) for 19 decimal digits in a 64bit Word.
// In other words, at most n digits in base b fit into a Word.
// TODO(gri) replace this with a table, generated at build time.
private static (Word, nint) maxPow(Word b) {
    Word p = default;
    nint n = default;

    (p, n) = (b, 1);    {
        var max = _M / b;

        while (p <= max) { 
            // p == b**n && p <= max
            p *= b;
            n++;

        }
    } 
    // p == b**n && p <= _M
    return ;

}

// pow returns x**n for n > 0, and 1 otherwise.
private static Word pow(Word x, nint n) {
    Word p = default;
 
    // n == sum of bi * 2**i, for 0 <= i < imax, and bi is 0 or 1
    // thus x**n == product of x**(2**i) for all i where bi == 1
    // (Russian Peasant Method for exponentiation)
    p = 1;
    while (n > 0) {
        if (n & 1 != 0) {
            p *= x;
        }
        x *= x;
        n>>=1;

    }
    return ;

}

// scan errors
private static var errNoDigits = errors.New("number has no digits");private static var errInvalSep = errors.New("'_' must separate successive digits");

// scan scans the number corresponding to the longest possible prefix
// from r representing an unsigned number in a given conversion base.
// scan returns the corresponding natural number res, the actual base b,
// a digit count, and a read or syntax error err, if any.
//
// For base 0, an underscore character ``_'' may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number, or the returned
// digit count. Incorrect placement of underscores is reported as an
// error if there are no other errors. If base != 0, underscores are
// not recognized and thus terminate scanning like any other character
// that is not a valid radix point or digit.
//
//     number    = mantissa | prefix pmantissa .
//     prefix    = "0" [ "b" | "B" | "o" | "O" | "x" | "X" ] .
//     mantissa  = digits "." [ digits ] | digits | "." digits .
//     pmantissa = [ "_" ] digits "." [ digits ] | [ "_" ] digits | "." digits .
//     digits    = digit { [ "_" ] digit } .
//     digit     = "0" ... "9" | "a" ... "z" | "A" ... "Z" .
//
// Unless fracOk is set, the base argument must be 0 or a value between
// 2 and MaxBase. If fracOk is set, the base argument must be one of
// 0, 2, 8, 10, or 16. Providing an invalid base argument leads to a run-
// time panic.
//
// For base 0, the number prefix determines the actual base: A prefix of
// ``0b'' or ``0B'' selects base 2, ``0o'' or ``0O'' selects base 8, and
// ``0x'' or ``0X'' selects base 16. If fracOk is false, a ``0'' prefix
// (immediately followed by digits) selects base 8 as well. Otherwise,
// the selected base is 10 and no prefix is accepted.
//
// If fracOk is set, a period followed by a fractional part is permitted.
// The result value is computed as if there were no period present; and
// the count value is used to determine the fractional part.
//
// For bases <= 36, lower and upper case letters are considered the same:
// The letters 'a' to 'z' and 'A' to 'Z' represent digit values 10 to 35.
// For bases > 36, the upper case letters 'A' to 'Z' represent the digit
// values 36 to 61.
//
// A result digit count > 0 corresponds to the number of (non-prefix) digits
// parsed. A digit count <= 0 indicates the presence of a period (if fracOk
// is set, only), and -count is the number of fractional digits found.
// In this case, the actual value of the scanned number is res * b**count.
//
private static (nat, nint, nint, error) scan(this nat z, io.ByteScanner r, nint @base, bool fracOk) => func((_, panic, _) => {
    nat res = default;
    nint b = default;
    nint count = default;
    error err = default!;
 
    // reject invalid bases
    var baseOk = base == 0 || !fracOk && 2 <= base && base <= MaxBase || fracOk && (base == 2 || base == 8 || base == 10 || base == 16);
    if (!baseOk) {
        panic(fmt.Sprintf("invalid number base %d", base));
    }
    char prev = '.';
    var invalSep = false; 

    // one char look-ahead
    var (ch, err) = r.ReadByte(); 

    // determine actual base
    var b = base;
    nint prefix = 0;
    if (base == 0) { 
        // actual base is 10 unless there's a base prefix
        b = 10;
        if (err == null && ch == '0') {
            prev = '0';
            count = 1;
            ch, err = r.ReadByte();
            if (err == null) { 
                // possibly one of 0b, 0B, 0o, 0O, 0x, 0X
                switch (ch) {
                    case 'b': 

                    case 'B': 
                        (b, prefix) = (2, 'b');
                        break;
                    case 'o': 

                    case 'O': 
                        (b, prefix) = (8, 'o');
                        break;
                    case 'x': 

                    case 'X': 
                        (b, prefix) = (16, 'x');
                        break;
                    default: 
                        if (!fracOk) {
                            (b, prefix) = (8, '0');
                        }

                        break;
                }
                if (prefix != 0) {
                    count = 0; // prefix is not counted
                    if (prefix != '0') {
                        ch, err = r.ReadByte();
                    }

                }

            }

        }
    }
    z = z[..(int)0];
    var b1 = Word(b);
    var (bn, n) = maxPow(b1); // at most n digits in base b1 fit into Word
    var di = Word(0); // 0 <= di < b1**i < bn
    nint i = 0; // 0 <= i < n
    nint dp = -1; // position of decimal point
    while (err == null) {
        if (ch == '.' && fracOk) {
            fracOk = false;
            if (prev == '_') {
                invalSep = true;
            }
            prev = '.';
            dp = count;
        }
        else if (ch == '_' && base == 0) {
            if (prev != '0') {
                invalSep = true;
            }
            prev = '_';
        }
        else
 { 
            // convert rune into digit value d1
            Word d1 = default;

            if ('0' <= ch && ch <= '9') 
                d1 = Word(ch - '0');
            else if ('a' <= ch && ch <= 'z') 
                d1 = Word(ch - 'a' + 10);
            else if ('A' <= ch && ch <= 'Z') 
                if (b <= maxBaseSmall) {
                    d1 = Word(ch - 'A' + 10);
                }
                else
 {
                    d1 = Word(ch - 'A' + maxBaseSmall);
                }

            else 
                d1 = MaxBase + 1;
                        if (d1 >= b1) {
                r.UnreadByte(); // ch does not belong to number anymore
                break;

            }

            prev = '0';
            count++; 

            // collect d1 in di
            di = di * b1 + d1;
            i++; 

            // if di is "full", add it to the result
            if (i == n) {
                z = z.mulAddWW(z, bn, di);
                di = 0;
                i = 0;
            }

        }
        ch, err = r.ReadByte();

    }

    if (err == io.EOF) {
        err = null;
    }
    if (err == null && (invalSep || prev == '_')) {
        err = errInvalSep;
    }
    if (count == 0) { 
        // no digits found
        if (prefix == '0') { 
            // there was only the octal prefix 0 (possibly followed by separators and digits > 7);
            // interpret as decimal 0
            return (z[..(int)0], 10, 1, error.As(err)!);

        }
        err = errNoDigits; // fall through; result will be 0
    }
    if (i > 0) {
        z = z.mulAddWW(z, pow(b1, i), di);
    }
    res = z.norm(); 

    // adjust count for fraction, if any
    if (dp >= 0) { 
        // 0 <= dp <= count
        count = dp - count;

    }
    return ;

});

// utoa converts x to an ASCII representation in the given base;
// base must be between 2 and MaxBase, inclusive.
private static slice<byte> utoa(this nat x, nint @base) {
    return x.itoa(false, base);
}

// itoa is like utoa but it prepends a '-' if neg && x != 0.
private static slice<byte> itoa(this nat x, bool neg, nint @base) => func((_, panic, _) => {
    if (base < 2 || base > MaxBase) {
        panic("invalid base");
    }
    if (len(x) == 0) {
        return (slice<byte>)"0";
    }
    var i = int(float64(x.bitLen()) / math.Log2(float64(base))) + 1; // off by 1 at most
    if (neg) {
        i++;
    }
    var s = make_slice<byte>(i); 

    // convert power of two and non power of two bases separately
    {
        var b = Word(base);

        if (b == b & -b) { 
            // shift is base b digit size in bits
            var shift = uint(bits.TrailingZeros(uint(b))); // shift > 0 because b >= 2
            var mask = Word(1 << (int)(shift) - 1);
            var w = x[0]; // current word
            var nbits = uint(_W); // number of unprocessed bits in w

            // convert less-significant words (include leading zeros)
            for (nint k = 1; k < len(x); k++) { 
                // convert full digits
                while (nbits >= shift) {
                    i--;
                    s[i] = digits[w & mask];
                    w>>=shift;
                    nbits -= shift;
                }
        else
 

                // convert any partial leading digit and advance to next word
 

                // convert any partial leading digit and advance to next word
                if (nbits == 0) { 
                    // no partial digit remaining, just advance
                    w = x[k];
                    nbits = _W;

                }
                else
 { 
                    // partial digit in current word w (== x[k-1]) and next word x[k]
                    w |= x[k] << (int)(nbits);
                    i--;
                    s[i] = digits[w & mask]; 

                    // advance
                    w = x[k] >> (int)((shift - nbits));
                    nbits = _W - (shift - nbits);

                }

            } 

            // convert digits of most-significant word w (omit leading zeros)
 

            // convert digits of most-significant word w (omit leading zeros)
            while (w != 0) {
                i--;
                s[i] = digits[w & mask];
                w>>=shift;
            }



        } {
            var (bb, ndigits) = maxPow(b); 

            // construct table of successive squares of bb*leafSize to use in subdivisions
            // result (table != nil) <=> (len(x) > leafSize > 0)
            var table = divisors(len(x), b, ndigits, bb); 

            // preserve x, create local copy for use by convertWords
            var q = nat(null).set(x); 

            // convert q to string s in base b
            q.convertWords(s, b, ndigits, bb, table); 

            // strip leading zeros
            // (x != 0; thus s must contain at least one non-zero digit
            // and the loop will terminate)
            i = 0;
            while (s[i] == '0') {
                i++;
            }


        }
    }


    if (neg) {
        i--;
        s[i] = '-';
    }
    return s[(int)i..];

});

// Convert words of q to base b digits in s. If q is large, it is recursively "split in half"
// by nat/nat division using tabulated divisors. Otherwise, it is converted iteratively using
// repeated nat/Word division.
//
// The iterative method processes n Words by n divW() calls, each of which visits every Word in the
// incrementally shortened q for a total of n + (n-1) + (n-2) ... + 2 + 1, or n(n+1)/2 divW()'s.
// Recursive conversion divides q by its approximate square root, yielding two parts, each half
// the size of q. Using the iterative method on both halves means 2 * (n/2)(n/2 + 1)/2 divW()'s
// plus the expensive long div(). Asymptotically, the ratio is favorable at 1/2 the divW()'s, and
// is made better by splitting the subblocks recursively. Best is to split blocks until one more
// split would take longer (because of the nat/nat div()) than the twice as many divW()'s of the
// iterative approach. This threshold is represented by leafSize. Benchmarking of leafSize in the
// range 2..64 shows that values of 8 and 16 work well, with a 4x speedup at medium lengths and
// ~30x for 20000 digits. Use nat_test.go's BenchmarkLeafSize tests to optimize leafSize for
// specific hardware.
//
private static void convertWords(this nat q, slice<byte> s, Word b, nint ndigits, Word bb, slice<divisor> table) => func((_, panic, _) => { 
    // split larger blocks recursively
    if (table != null) { 
        // len(q) > leafSize > 0
        nat r = default;
        var index = len(table) - 1;
        while (len(q) > leafSize) { 
            // find divisor close to sqrt(q) if possible, but in any case < q
            var maxLength = q.bitLen(); // ~= log2 q, or at of least largest possible q of this bit length
            var minLength = maxLength >> 1; // ~= log2 sqrt(q)
            while (index > 0 && table[index - 1].nbits > minLength) {
                index--; // desired
            }

            if (table[index].nbits >= maxLength && table[index].bbb.cmp(q) >= 0) {
                index--;
                if (index < 0) {
                    panic("internal inconsistency");
                }
            } 

            // split q into the two digit number (q'*bbb + r) to form independent subblocks
            q, r = q.div(r, q, table[index].bbb); 

            // convert subblocks and collect results in s[:h] and s[h:]
            var h = len(s) - table[index].ndigits;
            r.convertWords(s[(int)h..], b, ndigits, bb, table[(int)0..(int)index]);
            s = s[..(int)h]; // == q.convertWords(s, b, ndigits, bb, table[0:index+1])
        }

    }
    var i = len(s);
    r = default;
    if (b == 10) { 
        // hard-coding for 10 here speeds this up by 1.25x (allows for / and % by constants)
        while (len(q) > 0) { 
            // extract least significant, base bb "digit"
            q, r = q.divW(q, bb);
            {
                nint j__prev2 = j;

                for (nint j = 0; j < ndigits && i > 0; j++) {
                    i--; 
                    // avoid % computation since r%10 == r - int(r/10)*10;
                    // this appears to be faster for BenchmarkString10000Base10
                    // and smaller strings (but a bit slower for larger ones)
                    var t = r / 10;
                    s[i] = '0' + byte(r - t * 10);
                    r = t;

                }
    else


                j = j__prev2;
            }

        }

    } {
        while (len(q) > 0) { 
            // extract least significant, base bb "digit"
            q, r = q.divW(q, bb);
            {
                nint j__prev2 = j;

                for (j = 0; j < ndigits && i > 0; j++) {
                    i--;
                    s[i] = digits[r % b];
                    r /= b;
                }


                j = j__prev2;
            }

        }

    }
    while (i > 0) { // while need more leading zeros
        i--;
        s[i] = '0';

    }

});

// Split blocks greater than leafSize Words (or set to 0 to disable recursive conversion)
// Benchmark and configure leafSize using: go test -bench="Leaf"
//   8 and 16 effective on 3.0 GHz Xeon "Clovertown" CPU (128 byte cache lines)
//   8 and 16 effective on 2.66 GHz Core 2 Duo "Penryn" CPU
private static nint leafSize = 8; // number of Word-size binary values treat as a monolithic block

private partial struct divisor {
    public nat bbb; // divisor
    public nint nbits; // bit length of divisor (discounting leading zeros) ~= log2(bbb)
    public nint ndigits; // digit length of divisor in terms of output base digits
}

private static var cacheBase10 = default;

// expWW computes x**y
private static nat expWW(this nat z, Word x, Word y) {
    return z.expNN(nat(null).setWord(x), nat(null).setWord(y), null);
}

// construct table of powers of bb*leafSize to use in subdivisions
private static slice<divisor> divisors(nint m, Word b, nint ndigits, Word bb) { 
    // only compute table when recursive conversion is enabled and x is large
    if (leafSize == 0 || m <= leafSize) {
        return null;
    }
    nint k = 1;
    {
        var words = leafSize;

        while (words < m >> 1 && k < len(cacheBase10.table)) {
            k++;
            words<<=1;
        }
    } 

    // reuse and extend existing table of divisors or create new table as appropriate
    slice<divisor> table = default; // for b == 10, table overlaps with cacheBase10.table
    if (b == 10) {
        cacheBase10.Lock();
        table = cacheBase10.table[(int)0..(int)k]; // reuse old table for this conversion
    }
    else
 {
        table = make_slice<divisor>(k); // create new table for this conversion
    }
    if (table[k - 1].ndigits == 0) { 
        // add new entries as needed
        nat larger = default;
        for (nint i = 0; i < k; i++) {
            if (table[i].ndigits == 0) {
                if (i == 0) {
                    table[0].bbb = nat(null).expWW(bb, Word(leafSize));
                    table[0].ndigits = ndigits * leafSize;
                }
                else
 {
                    table[i].bbb = nat(null).sqr(table[i - 1].bbb);
                    table[i].ndigits = 2 * table[i - 1].ndigits;
                } 

                // optimization: exploit aggregated extra bits in macro blocks
                larger = nat(null).set(table[i].bbb);
                while (mulAddVWW(larger, larger, b, 0) == 0) {
                    table[i].bbb = table[i].bbb.set(larger);
                    table[i].ndigits++;
                }


                table[i].nbits = table[i].bbb.bitLen();

            }

        }

    }
    if (b == 10) {
        cacheBase10.Unlock();
    }
    return table;

}

} // end big_package
