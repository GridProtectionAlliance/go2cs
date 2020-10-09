// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package str provides string manipulation utilities.
// package str -- go2cs converted at 2020 October 09 05:46:04 UTC
// import "cmd/go/internal/str" ==> using str = go.cmd.go.@internal.str_package
// Original source: C:\Go\src\cmd\go\internal\str\str.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class str_package
    {
        // StringList flattens its arguments into a single []string.
        // Each argument in args must have type string or []string.
        public static slice<@string> StringList(params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            slice<@string> x = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in args)
                {
                    arg = __arg;
                    switch (arg.type())
                    {
                        case slice<@string> arg:
                            x = append(x, arg);
                            break;
                        case @string arg:
                            x = append(x, arg);
                            break;
                        default:
                        {
                            var arg = arg.type();
                            panic("stringList: invalid argument of type " + fmt.Sprintf("%T", arg));
                            break;
                        }
                    }

                }
                arg = arg__prev1;
            }

            return x;

        });

        // ToFold returns a string with the property that
        //    strings.EqualFold(s, t) iff ToFold(s) == ToFold(t)
        // This lets us test a large set of strings for fold-equivalent
        // duplicates without making a quadratic number of calls
        // to EqualFold. Note that strings.ToUpper and strings.ToLower
        // do not have the desired property in some corner cases.
        public static @string ToFold(@string s)
        { 
            // Fast path: all ASCII, no upper case.
            // Most paths look like this already.
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];
                if (c >= utf8.RuneSelf || 'A' <= c && c <= 'Z')
                {
                    goto Slow;
                }

            }

            return s;

Slow:
            bytes.Buffer buf = default;
            foreach (var (_, r) in s)
            { 
                // SimpleFold(x) cycles to the next equivalent rune > x
                // or wraps around to smaller values. Iterate until it wraps,
                // and we've found the minimum value.
                while (true)
                {
                    var r0 = r;
                    r = unicode.SimpleFold(r0);
                    if (r <= r0)
                    {
                        break;
                    }

                } 
                // Exception to allow fast path above: A-Z => a-z
 
                // Exception to allow fast path above: A-Z => a-z
                if ('A' <= r && r <= 'Z')
                {
                    r += 'a' - 'A';
                }

                buf.WriteRune(r);

            }
            return buf.String();

        }

        // FoldDup reports a pair of strings from the list that are
        // equal according to strings.EqualFold.
        // It returns "", "" if there are no such strings.
        public static (@string, @string) FoldDup(slice<@string> list)
        {
            @string _p0 = default;
            @string _p0 = default;

            map clash = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
            foreach (var (_, s) in list)
            {
                var fold = ToFold(s);
                {
                    var t = clash[fold];

                    if (t != "")
                    {
                        if (s > t)
                        {
                            s = t;
                            t = s;

                        }

                        return (s, t);

                    }

                }

                clash[fold] = s;

            }
            return ("", "");

        }

        // Contains reports whether x contains s.
        public static bool Contains(slice<@string> x, @string s)
        {
            foreach (var (_, t) in x)
            {
                if (t == s)
                {
                    return true;
                }

            }
            return false;

        }

        private static bool isSpaceByte(byte c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        // SplitQuotedFields splits s into a list of fields,
        // allowing single or double quotes around elements.
        // There is no unescaping or other processing within
        // quoted fields.
        public static (slice<@string>, error) SplitQuotedFields(@string s)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
 
            // Split fields allowing '' or "" around elements.
            // Quotes further inside the string do not count.
            slice<@string> f = default;
            while (len(s) > 0L)
            {
                while (len(s) > 0L && isSpaceByte(s[0L]))
                {
                    s = s[1L..];
                }

                if (len(s) == 0L)
                {
                    break;
                } 
                // Accepted quoted string. No unescaping inside.
                if (s[0L] == '"' || s[0L] == '\'')
                {
                    var quote = s[0L];
                    s = s[1L..];
                    long i = 0L;
                    while (i < len(s) && s[i] != quote)
                    {
                        i++;
                    }

                    if (i >= len(s))
                    {
                        return (null, error.As(fmt.Errorf("unterminated %c string", quote))!);
                    }

                    f = append(f, s[..i]);
                    s = s[i + 1L..];
                    continue;

                }

                i = 0L;
                while (i < len(s) && !isSpaceByte(s[i]))
                {
                    i++;
                }

                f = append(f, s[..i]);
                s = s[i..];

            }

            return (f, error.As(null!)!);

        }
    }
}}}}
