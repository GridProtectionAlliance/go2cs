// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2020 August 29 08:16:05 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\builder.go
using utf8 = go.unicode.utf8_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class strings_package
    {
        // A Builder is used to efficiently build a string using Write methods.
        // It minimizes memory copying. The zero value is ready to use.
        // Do not copy a non-zero Builder.
        public partial struct Builder
        {
            public ptr<Builder> addr; // of receiver, to detect copies by value
            public slice<byte> buf;
        }

        // noescape hides a pointer from escape analysis.  noescape is
        // the identity function but escape analysis doesn't think the
        // output depends on the input. noescape is inlined and currently
        // compiles down to zero instructions.
        // USE CAREFULLY!
        // This was copied from the runtime; see issues 23382 and 7921.
        //go:nosplit
        private static unsafe.Pointer noescape(unsafe.Pointer p)
        {
            var x = uintptr(p);
            return @unsafe.Pointer(x ^ 0L);
        }

        private static void copyCheck(this ref Builder _b) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        {
            if (b.addr == null)
            { 
                // This hack works around a failing of Go's escape analysis
                // that was causing b to escape and be heap allocated.
                // See issue 23382.
                // TODO: once issue 7921 is fixed, this should be reverted to
                // just "b.addr = b".
                b.addr = (Builder.Value)(noescape(@unsafe.Pointer(b)));
            }
            else if (b.addr != b)
            {
                panic("strings: illegal use of non-zero Builder copied by value");
            }
        });

        // String returns the accumulated string.
        private static @string String(this ref Builder b)
        {
            return @unsafe.Pointer(ref b.buf).Value;
        }

        // Len returns the number of accumulated bytes; b.Len() == len(b.String()).
        private static long Len(this ref Builder b)
        {
            return len(b.buf);
        }

        // Reset resets the Builder to be empty.
        private static void Reset(this ref Builder b)
        {
            b.addr = null;
            b.buf = null;
        }

        // grow copies the buffer to a new, larger buffer so that there are at least n
        // bytes of capacity beyond len(b.buf).
        private static void grow(this ref Builder b, long n)
        {
            var buf = make_slice<byte>(len(b.buf), 2L * cap(b.buf) + n);
            copy(buf, b.buf);
            b.buf = buf;
        }

        // Grow grows b's capacity, if necessary, to guarantee space for
        // another n bytes. After Grow(n), at least n bytes can be written to b
        // without another allocation. If n is negative, Grow panics.
        private static void Grow(this ref Builder _b, long n) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        {
            b.copyCheck();
            if (n < 0L)
            {
                panic("strings.Builder.Grow: negative count");
            }
            if (cap(b.buf) - len(b.buf) < n)
            {
                b.grow(n);
            }
        });

        // Write appends the contents of p to b's buffer.
        // Write always returns len(p), nil.
        private static (long, error) Write(this ref Builder b, slice<byte> p)
        {
            b.copyCheck();
            b.buf = append(b.buf, p);
            return (len(p), null);
        }

        // WriteByte appends the byte c to b's buffer.
        // The returned error is always nil.
        private static error WriteByte(this ref Builder b, byte c)
        {
            b.copyCheck();
            b.buf = append(b.buf, c);
            return error.As(null);
        }

        // WriteRune appends the UTF-8 encoding of Unicode code point r to b's buffer.
        // It returns the length of r and a nil error.
        private static (long, error) WriteRune(this ref Builder b, int r)
        {
            b.copyCheck();
            if (r < utf8.RuneSelf)
            {
                b.buf = append(b.buf, byte(r));
                return (1L, null);
            }
            var l = len(b.buf);
            if (cap(b.buf) - l < utf8.UTFMax)
            {
                b.grow(utf8.UTFMax);
            }
            var n = utf8.EncodeRune(b.buf[l..l + utf8.UTFMax], r);
            b.buf = b.buf[..l + n];
            return (n, null);
        }

        // WriteString appends the contents of s to b's buffer.
        // It returns the length of s and a nil error.
        private static (long, error) WriteString(this ref Builder b, @string s)
        {
            b.copyCheck();
            b.buf = append(b.buf, s);
            return (len(s), null);
        }
    }
}
