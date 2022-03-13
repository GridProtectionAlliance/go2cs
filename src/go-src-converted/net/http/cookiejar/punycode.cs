// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cookiejar -- go2cs converted at 2022 March 13 05:38:15 UTC
// import "net/http/cookiejar" ==> using cookiejar = go.net.http.cookiejar_package
// Original source: C:\Program Files\Go\src\net\http\cookiejar\punycode.go
namespace go.net.http;
// This file implements the Punycode algorithm from RFC 3492.


using fmt = fmt_package;
using ascii = net.http.@internal.ascii_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;


// These parameter values are specified in section 5.
//
// All computation is done with int32s, so that overflow behavior is identical
// regardless of whether int is 32-bit or 64-bit.

public static partial class cookiejar_package {

private static readonly int base = 36;
private static readonly int damp = 700;
private static readonly int initialBias = 72;
private static readonly int initialN = 128;
private static readonly int skew = 38;
private static readonly int tmax = 26;
private static readonly int tmin = 1;

// encode encodes a string as specified in section 6.3 and prepends prefix to
// the result.
//
// The "while h < length(input)" line in the specification becomes "for
// remaining != 0" in the Go code, because len(s) in Go is in bytes, not runes.
private static (@string, error) encode(@string prefix, @string s) {
    @string _p0 = default;
    error _p0 = default!;

    var output = make_slice<byte>(len(prefix), len(prefix) + 1 + 2 * len(s));
    copy(output, prefix);
    var delta = int32(0);
    var n = initialN;
    var bias = initialBias;
    var b = int32(0);
    var remaining = int32(0);
    {
        var r__prev1 = r;

        foreach (var (_, __r) in s) {
            r = __r;
            if (r < utf8.RuneSelf) {
                b++;
                output = append(output, byte(r));
            }
            else
 {
                remaining++;
            }
        }
        r = r__prev1;
    }

    var h = b;
    if (b > 0) {
        output = append(output, '-');
    }
    while (remaining != 0) {
        var m = int32(0x7fffffff);
        {
            var r__prev2 = r;

            foreach (var (_, __r) in s) {
                r = __r;
                if (m > r && r >= n) {
                    m = r;
                }
            }

            r = r__prev2;
        }

        delta += (m - n) * (h + 1);
        if (delta < 0) {
            return ("", error.As(fmt.Errorf("cookiejar: invalid label %q", s))!);
        }
        n = m;
        {
            var r__prev2 = r;

            foreach (var (_, __r) in s) {
                r = __r;
                if (r < n) {
                    delta++;
                    if (delta < 0) {
                        return ("", error.As(fmt.Errorf("cookiejar: invalid label %q", s))!);
                    }
                    continue;
                }
                if (r > n) {
                    continue;
                }
                var q = delta;
                {
                    var k = base;

                    while () {
                        var t = k - bias;
                        if (t < tmin) {
                            t = tmin;
                        k += base;
                        }
                        else if (t > tmax) {
                            t = tmax;
                        }
                        if (q < t) {
                            break;
                        }
                        output = append(output, encodeDigit(t + (q - t) % (base - t)));
                        q = (q - t) / (base - t);
                    }

                }
                output = append(output, encodeDigit(q));
                bias = adapt(delta, h + 1, h == b);
                delta = 0;
                h++;
                remaining--;
            }

            r = r__prev2;
        }

        delta++;
        n++;
    }
    return (string(output), error.As(null!)!);
}

private static byte encodeDigit(int digit) => func((_, panic, _) => {

    if (0 <= digit && digit < 26) 
        return byte(digit + 'a');
    else if (26 <= digit && digit < 36) 
        return byte(digit + ('0' - 26));
        panic("cookiejar: internal error in punycode encoding");
});

// adapt is the bias adaptation function specified in section 6.1.
private static int adapt(int delta, int numPoints, bool firstTime) {
    if (firstTime) {
        delta /= damp;
    }
    else
 {
        delta /= 2;
    }
    delta += delta / numPoints;
    var k = int32(0);
    while (delta > ((base - tmin) * tmax) / 2) {
        delta /= base - tmin;
        k += base;
    }
    return k + (base - tmin + 1) * delta / (delta + skew);
}

// Strictly speaking, the remaining code below deals with IDNA (RFC 5890 and
// friends) and not Punycode (RFC 3492) per se.

// acePrefix is the ASCII Compatible Encoding prefix.
private static readonly @string acePrefix = "xn--";

// toASCII converts a domain or domain label to its ASCII form. For example,
// toASCII("bücher.example.com") is "xn--bcher-kva.example.com", and
// toASCII("golang") is "golang".


// toASCII converts a domain or domain label to its ASCII form. For example,
// toASCII("bücher.example.com") is "xn--bcher-kva.example.com", and
// toASCII("golang") is "golang".
private static (@string, error) toASCII(@string s) {
    @string _p0 = default;
    error _p0 = default!;

    if (ascii.Is(s)) {
        return (s, error.As(null!)!);
    }
    var labels = strings.Split(s, ".");
    foreach (var (i, label) in labels) {
        if (!ascii.Is(label)) {
            var (a, err) = encode(acePrefix, label);
            if (err != null) {
                return ("", error.As(err)!);
            }
            labels[i] = a;
        }
    }    return (strings.Join(labels, "."), error.As(null!)!);
}

} // end cookiejar_package
