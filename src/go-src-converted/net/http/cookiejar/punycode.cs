// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cookiejar -- go2cs converted at 2020 August 29 08:34:07 UTC
// import "net/http/cookiejar" ==> using cookiejar = go.net.http.cookiejar_package
// Original source: C:\Go\src\net\http\cookiejar\punycode.go
// This file implements the Punycode algorithm from RFC 3492.

using fmt = go.fmt_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace net {
namespace http
{
    public static partial class cookiejar_package
    {
        // These parameter values are specified in section 5.
        //
        // All computation is done with int32s, so that overflow behavior is identical
        // regardless of whether int is 32-bit or 64-bit.
        private static readonly int base = 36L;
        private static readonly int damp = 700L;
        private static readonly int initialBias = 72L;
        private static readonly int initialN = 128L;
        private static readonly int skew = 38L;
        private static readonly int tmax = 26L;
        private static readonly int tmin = 1L;

        // encode encodes a string as specified in section 6.3 and prepends prefix to
        // the result.
        //
        // The "while h < length(input)" line in the specification becomes "for
        // remaining != 0" in the Go code, because len(s) in Go is in bytes, not runes.
        private static (@string, error) encode(@string prefix, @string s)
        {
            var output = make_slice<byte>(len(prefix), len(prefix) + 1L + 2L * len(s));
            copy(output, prefix);
            var delta = int32(0L);
            var n = initialN;
            var bias = initialBias;
            var b = int32(0L);
            var remaining = int32(0L);
            {
                var r__prev1 = r;

                foreach (var (_, __r) in s)
                {
                    r = __r;
                    if (r < utf8.RuneSelf)
                    {
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
            if (b > 0L)
            {
                output = append(output, '-');
            }
            while (remaining != 0L)
            {
                var m = int32(0x7fffffffUL);
                {
                    var r__prev2 = r;

                    foreach (var (_, __r) in s)
                    {
                        r = __r;
                        if (m > r && r >= n)
                        {
                            m = r;
                        }
                    }

                    r = r__prev2;
                }

                delta += (m - n) * (h + 1L);
                if (delta < 0L)
                {
                    return ("", fmt.Errorf("cookiejar: invalid label %q", s));
                }
                n = m;
                {
                    var r__prev2 = r;

                    foreach (var (_, __r) in s)
                    {
                        r = __r;
                        if (r < n)
                        {
                            delta++;
                            if (delta < 0L)
                            {
                                return ("", fmt.Errorf("cookiejar: invalid label %q", s));
                            }
                            continue;
                        }
                        if (r > n)
                        {
                            continue;
                        }
                        var q = delta;
                        {
                            var k = base;

                            while (>>MARKER:FOREXPRESSION_LEVEL_3<<)
                            {
                                var t = k - bias;
                                if (t < tmin)
                                {
                                    t = tmin;
                                k += base;
                                }
                                else if (t > tmax)
                                {
                                    t = tmax;
                                }
                                if (q < t)
                                {
                                    break;
                                }
                                output = append(output, encodeDigit(t + (q - t) % (base - t)));
                                q = (q - t) / (base - t);
                            }

                        }
                        output = append(output, encodeDigit(q));
                        bias = adapt(delta, h + 1L, h == b);
                        delta = 0L;
                        h++;
                        remaining--;
                    }

                    r = r__prev2;
                }

                delta++;
                n++;
            }

            return (string(output), null);
        }

        private static byte encodeDigit(int digit) => func((_, panic, __) =>
        {

            if (0L <= digit && digit < 26L) 
                return byte(digit + 'a');
            else if (26L <= digit && digit < 36L) 
                return byte(digit + ('0' - 26L));
                        panic("cookiejar: internal error in punycode encoding");
        });

        // adapt is the bias adaptation function specified in section 6.1.
        private static int adapt(int delta, int numPoints, bool firstTime)
        {
            if (firstTime)
            {
                delta /= damp;
            }
            else
            {
                delta /= 2L;
            }
            delta += delta / numPoints;
            var k = int32(0L);
            while (delta > ((base - tmin) * tmax) / 2L)
            {
                delta /= base - tmin;
                k += base;
            }

            return k + (base - tmin + 1L) * delta / (delta + skew);
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
        private static (@string, error) toASCII(@string s)
        {
            if (ascii(s))
            {
                return (s, null);
            }
            var labels = strings.Split(s, ".");
            foreach (var (i, label) in labels)
            {
                if (!ascii(label))
                {
                    var (a, err) = encode(acePrefix, label);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    labels[i] = a;
                }
            }
            return (strings.Join(labels, "."), null);
        }

        private static bool ascii(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] >= utf8.RuneSelf)
                {
                    return false;
                }
            }

            return true;
        }
    }
}}}
