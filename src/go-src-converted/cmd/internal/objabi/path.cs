// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 08 03:50:14 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\path.go
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // PathToPrefix converts raw string to the prefix that will be used in the
        // symbol table. All control characters, space, '%' and '"', as well as
        // non-7-bit clean bytes turn into %xx. The period needs escaping only in the
        // last segment of the path, and it makes for happier users if we escape that as
        // little as possible.
        public static @string PathToPrefix(@string s)
        {
            var slash = strings.LastIndex(s, "/"); 
            // check for chars that need escaping
            long n = 0L;
            {
                long r__prev1 = r;

                for (long r = 0L; r < len(s); r++)
                {
                    {
                        var c__prev1 = c;

                        var c = s[r];

                        if (c <= ' ' || (c == '.' && r > slash) || c == '%' || c == '"' || c >= 0x7FUL)
                        {
                            n++;
                        }
                        c = c__prev1;

                    }

                }

                r = r__prev1;
            } 

            // quick exit
            if (n == 0L)
            {
                return s;
            }
            const @string hex = (@string)"0123456789abcdef";

            var p = make_slice<byte>(0L, len(s) + 2L * n);
            {
                long r__prev1 = r;

                for (r = 0L; r < len(s); r++)
                {
                    {
                        var c__prev1 = c;

                        c = s[r];

                        if (c <= ' ' || (c == '.' && r > slash) || c == '%' || c == '"' || c >= 0x7FUL)
                        {
                            p = append(p, '%', hex[c >> (int)(4L)], hex[c & 0xFUL]);
                        }
                        else
                        {
                            p = append(p, c);
                        }
                        c = c__prev1;

                    }

                }

                r = r__prev1;
            }

            return string(p);

        }
    }
}}}
