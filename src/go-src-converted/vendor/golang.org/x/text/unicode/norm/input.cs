// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2020 October 08 05:02:19 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\input.go
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class norm_package
    {
        private partial struct input
        {
            public @string str;
            public slice<byte> bytes;
        }

        private static input inputBytes(slice<byte> str)
        {
            return new input(bytes:str);
        }

        private static input inputString(@string str)
        {
            return new input(str:str);
        }

        private static void setBytes(this ptr<input> _addr_@in, slice<byte> str)
        {
            ref input @in = ref _addr_@in.val;

            @in.str = "";
            @in.bytes = str;
        }

        private static void setString(this ptr<input> _addr_@in, @string str)
        {
            ref input @in = ref _addr_@in.val;

            @in.str = str;
            @in.bytes = null;
        }

        private static byte _byte(this ptr<input> _addr_@in, long p)
        {
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                return @in.str[p];
            }

            return @in.bytes[p];

        }

        private static long skipASCII(this ptr<input> _addr_@in, long p, long max)
        {
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                while (p < max && @in.str[p] < utf8.RuneSelf)
                {
                    p++;
                }
            else


            }            {
                while (p < max && @in.bytes[p] < utf8.RuneSelf)
                {
                    p++;
                }


            }

            return p;

        }

        private static long skipContinuationBytes(this ptr<input> _addr_@in, long p)
        {
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                while (p < len(@in.str) && !utf8.RuneStart(@in.str[p]))
                {
                    p++;
                }
            else


            }            {
                while (p < len(@in.bytes) && !utf8.RuneStart(@in.bytes[p]))
                {
                    p++;
                }


            }

            return p;

        }

        private static slice<byte> appendSlice(this ptr<input> _addr_@in, slice<byte> buf, long b, long e)
        {
            ref input @in = ref _addr_@in.val;

            if (@in.bytes != null)
            {
                return append(buf, @in.bytes[b..e]);
            }

            for (var i = b; i < e; i++)
            {
                buf = append(buf, @in.str[i]);
            }

            return buf;

        }

        private static long copySlice(this ptr<input> _addr_@in, slice<byte> buf, long b, long e)
        {
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                return copy(buf, @in.str[b..e]);
            }

            return copy(buf, @in.bytes[b..e]);

        }

        private static (ushort, long) charinfoNFC(this ptr<input> _addr_@in, long p)
        {
            ushort _p0 = default;
            long _p0 = default;
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                return nfcData.lookupString(@in.str[p..]);
            }

            return nfcData.lookup(@in.bytes[p..]);

        }

        private static (ushort, long) charinfoNFKC(this ptr<input> _addr_@in, long p)
        {
            ushort _p0 = default;
            long _p0 = default;
            ref input @in = ref _addr_@in.val;

            if (@in.bytes == null)
            {
                return nfkcData.lookupString(@in.str[p..]);
            }

            return nfkcData.lookup(@in.bytes[p..]);

        }

        private static int hangul(this ptr<input> _addr_@in, long p)
        {
            int r = default;
            ref input @in = ref _addr_@in.val;

            long size = default;
            if (@in.bytes == null)
            {
                if (!isHangulString(@in.str[p..]))
                {
                    return 0L;
                }

                r, size = utf8.DecodeRuneInString(@in.str[p..]);

            }
            else
            {
                if (!isHangul(@in.bytes[p..]))
                {
                    return 0L;
                }

                r, size = utf8.DecodeRune(@in.bytes[p..]);

            }

            if (size != hangulUTF8Size)
            {
                return 0L;
            }

            return r;

        }
    }
}}}}}}
