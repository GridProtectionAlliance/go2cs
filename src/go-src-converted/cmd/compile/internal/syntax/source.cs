// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements source, a buffered rune reader
// specialized for scanning Go code: Reading
// ASCII characters, maintaining current (line, col)
// position information, and recording of the most
// recently read source segment are highly optimized.
// This file is self-contained (go tool compile source.go
// compiles) and thus could be made into its own package.

// package syntax -- go2cs converted at 2020 October 08 04:28:31 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\source.go
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        // The source buffer is accessed using three indices b (begin),
        // r (read), and e (end):
        //
        // - If b >= 0, it points to the beginning of a segment of most
        //   recently read characters (typically a Go literal).
        //
        // - r points to the byte immediately following the most recently
        //   read character ch, which starts at r-chw.
        //
        // - e points to the byte immediately following the last byte that
        //   was read into the buffer.
        //
        // The buffer content is terminated at buf[e] with the sentinel
        // character utf8.RuneSelf. This makes it possible to test for
        // the common case of ASCII characters with a single 'if' (see
        // nextch method).
        //
        //                +------ content in use -------+
        //                v                             v
        // buf [...read...|...segment...|ch|...unread...|s|...free...]
        //                ^             ^  ^            ^
        //                |             |  |            |
        //                b         r-chw  r            e
        //
        // Invariant: -1 <= b < r <= e < len(buf) && buf[e] == sentinel
        private partial struct source
        {
            public io.Reader @in;
            public Action<ulong, ulong, @string> errh;
            public slice<byte> buf; // source buffer
            public error ioerr; // pending I/O error, or nil
            public long b; // buffer indices (see comment above)
            public long r; // buffer indices (see comment above)
            public long e; // buffer indices (see comment above)
            public ulong line; // source position of ch (0-based)
            public ulong col; // source position of ch (0-based)
            public int ch; // most recently read character
            public long chw; // width of ch
        }

        private static readonly var sentinel = (var)utf8.RuneSelf;



        private static void init(this ptr<source> _addr_s, io.Reader @in, Action<ulong, ulong, @string> errh)
        {
            ref source s = ref _addr_s.val;

            s.@in = in;
            s.errh = errh;

            if (s.buf == null)
            {
                s.buf = make_slice<byte>(nextSize(0L));
            }

            s.buf[0L] = sentinel;
            s.ioerr = null;
            s.b = -1L;
            s.r = 0L;
            s.e = 0L;
            s.line = 0L;
            s.col = 0L;
            s.ch = ' ';
            s.chw = 0L;

        }

        // starting points for line and column numbers
        private static readonly long linebase = (long)1L;

        private static readonly long colbase = (long)1L;

        // pos returns the (line, col) source position of s.ch.


        // pos returns the (line, col) source position of s.ch.
        private static (ulong, ulong) pos(this ptr<source> _addr_s)
        {
            ulong line = default;
            ulong col = default;
            ref source s = ref _addr_s.val;

            return (linebase + s.line, colbase + s.col);
        }

        // error reports the error msg at source position s.pos().
        private static void error(this ptr<source> _addr_s, @string msg)
        {
            ref source s = ref _addr_s.val;

            var (line, col) = s.pos();
            s.errh(line, col, msg);
        }

        // start starts a new active source segment (including s.ch).
        // As long as stop has not been called, the active segment's
        // bytes (excluding s.ch) may be retrieved by calling segment.
        private static void start(this ptr<source> _addr_s)
        {
            ref source s = ref _addr_s.val;

            s.b = s.r - s.chw;
        }
        private static void stop(this ptr<source> _addr_s)
        {
            ref source s = ref _addr_s.val;

            s.b = -1L;
        }
        private static slice<byte> segment(this ptr<source> _addr_s)
        {
            ref source s = ref _addr_s.val;

            return s.buf[s.b..s.r - s.chw];
        }

        // rewind rewinds the scanner's read position and character s.ch
        // to the start of the currently active segment, which must not
        // contain any newlines (otherwise position information will be
        // incorrect). Currently, rewind is only needed for handling the
        // source sequence ".."; it must not be called outside an active
        // segment.
        private static void rewind(this ptr<source> _addr_s) => func((_, panic, __) =>
        {
            ref source s = ref _addr_s.val;
 
            // ok to verify precondition - rewind is rarely called
            if (s.b < 0L)
            {
                panic("no active segment");
            }

            s.col -= uint(s.r - s.b);
            s.r = s.b;
            s.nextch();

        });

        private static void nextch(this ptr<source> _addr_s)
        {
            ref source s = ref _addr_s.val;

redo:
            s.col += uint(s.chw);
            if (s.ch == '\n')
            {
                s.line++;
                s.col = 0L;
            } 

            // fast common case: at least one ASCII character
            s.ch = rune(s.buf[s.r]);

            if (s.ch < sentinel)
            {
                s.r++;
                s.chw = 1L;
                if (s.ch == 0L)
                {
                    s.error("invalid NUL character");
                    goto redo;
                }

                return ;

            } 

            // slower general case: add more bytes to buffer if we don't have a full rune
            while (s.e - s.r < utf8.UTFMax && !utf8.FullRune(s.buf[s.r..s.e]) && s.ioerr == null)
            {
                s.fill();
            } 

            // EOF
 

            // EOF
            if (s.r == s.e)
            {
                if (s.ioerr != io.EOF)
                { 
                    // ensure we never start with a '/' (e.g., rooted path) in the error message
                    s.error("I/O error: " + s.ioerr.Error());
                    s.ioerr = null;

                }

                s.ch = -1L;
                s.chw = 0L;
                return ;

            }

            s.ch, s.chw = utf8.DecodeRune(s.buf[s.r..s.e]);
            s.r += s.chw;

            if (s.ch == utf8.RuneError && s.chw == 1L)
            {
                s.error("invalid UTF-8 encoding");
                goto redo;
            } 

            // BOM's are only allowed as the first character in a file
            const ulong BOM = (ulong)0xfeffUL;

            if (s.ch == BOM)
            {
                if (s.line > 0L || s.col > 0L)
                {
                    s.error("invalid BOM in the middle of the file");
                }

                goto redo;

            }

        }

        // fill reads more source bytes into s.buf.
        // It returns with at least one more byte in the buffer, or with s.ioerr != nil.
        private static void fill(this ptr<source> _addr_s) => func((_, panic, __) =>
        {
            ref source s = ref _addr_s.val;
 
            // determine content to preserve
            var b = s.r;
            if (s.b >= 0L)
            {
                b = s.b;
                s.b = 0L; // after buffer has grown or content has been moved down
            }

            var content = s.buf[b..s.e]; 

            // grow buffer or move content down
            if (len(content) * 2L > len(s.buf))
            {
                s.buf = make_slice<byte>(nextSize(len(s.buf)));
                copy(s.buf, content);
            }
            else if (b > 0L)
            {
                copy(s.buf, content);
            }

            s.r -= b;
            s.e -= b; 

            // read more data: try a limited number of times
            for (long i = 0L; i < 10L; i++)
            {
                long n = default;
                n, s.ioerr = s.@in.Read(s.buf[s.e..len(s.buf) - 1L]); // -1 to leave space for sentinel
                if (n < 0L)
                {
                    panic("negative read"); // incorrect underlying io.Reader implementation
                }

                if (n > 0L || s.ioerr != null)
                {
                    s.e += n;
                    s.buf[s.e] = sentinel;
                    return ;
                } 
                // n == 0
            }


            s.buf[s.e] = sentinel;
            s.ioerr = io.ErrNoProgress;

        });

        // nextSize returns the next bigger size for a buffer of a given size.
        private static long nextSize(long size)
        {
            const long min = (long)4L << (int)(10L); // 4K: minimum buffer size
 // 4K: minimum buffer size
            const long max = (long)1L << (int)(20L); // 1M: maximum buffer size which is still doubled
 // 1M: maximum buffer size which is still doubled
            if (size < min)
            {
                return min;
            }

            if (size <= max)
            {
                return size << (int)(1L);
            }

            return size + max;

        }
    }
}}}}
