// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements source, a buffered rune reader
// which is specialized for the needs of the Go scanner:
// Contiguous sequences of runes (literals) are extracted
// directly as []byte without the need to re-encode the
// runes in UTF-8 (as would be necessary with bufio.Reader).
//
// This file is self-contained (go tool compile source.go
// compiles) and thus could be made into its own package.

// package syntax -- go2cs converted at 2020 August 29 09:26:27 UTC
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
        // starting points for line and column numbers
        private static readonly long linebase = 1L;

        private static readonly long colbase = 1L;

        // buf [...read...|...|...unread...|s|...free...]
        //         ^      ^   ^            ^
        //         |      |   |            |
        //        suf     r0  r            w



        // buf [...read...|...|...unread...|s|...free...]
        //         ^      ^   ^            ^
        //         |      |   |            |
        //        suf     r0  r            w

        private partial struct source
        {
            public io.Reader src;
            public Action<ulong, ulong, @string> errh; // source buffer
            public array<byte> buf;
            public long offs; // source offset of buf
            public long r0; // previous/current read and write buf positions, excluding sentinel
            public long r; // previous/current read and write buf positions, excluding sentinel
            public long w; // previous/current read and write buf positions, excluding sentinel
            public ulong line0; // previous/current line
            public ulong line; // previous/current line
            public ulong col0; // previous/current column (byte offsets from line start)
            public ulong col; // previous/current column (byte offsets from line start)
            public error ioerr; // pending io error

// literal buffer
            public slice<byte> lit; // literal prefix
            public long suf; // literal suffix; suf >= 0 means we are scanning a literal
        }

        // init initializes source to read from src and to report errors via errh.
        // errh must not be nil.
        private static void init(this ref source s, io.Reader src, Action<ulong, ulong, @string> errh)
        {
            s.src = src;
            s.errh = errh;

            s.buf[0L] = utf8.RuneSelf; // terminate with sentinel
            s.offs = 0L;
            s.r0 = 0L;
            s.r = 0L;
            s.w = 0L;
            s.line0 = 0L;
            s.line = linebase;
            s.col0 = 0L;
            s.col = colbase;
            s.ioerr = null;

            s.lit = s.lit[..0L];
            s.suf = -1L;
        }

        // ungetr ungets the most recently read rune.
        private static void ungetr(this ref source s)
        {
            s.r = s.r0;
            s.line = s.line0;
            s.col = s.col0;
        }

        // ungetr2 is like ungetr but enables a 2nd ungetr.
        // It must not be called if one of the runes seen
        // was a newline.
        private static void ungetr2(this ref source s)
        {
            s.ungetr(); 
            // line must not have changed
            s.r0--;
            s.col0--;
        }

        private static void error(this ref source s, @string msg)
        {
            s.errh(s.line0, s.col0, msg);
        }

        // getr reads and returns the next rune.
        //
        // If a read or source encoding error occurs, getr
        // calls the error handler installed with init.
        // The handler must exist.
        //
        // The (line, col) position passed to the error handler
        // is always at the current source reading position.
        private static int getr(this ref source s)
        {
redo: 

            // We could avoid at least one test that is always taken in the
            // for loop below by duplicating the common case code (ASCII)
            // here since we always have at least the sentinel (utf8.RuneSelf)
            // in the buffer. Measure and optimize if necessary.

            // make sure we have at least one rune in buffer, or we are at EOF
            s.r0 = s.r;
            s.line0 = s.line;
            s.col0 = s.col; 

            // We could avoid at least one test that is always taken in the
            // for loop below by duplicating the common case code (ASCII)
            // here since we always have at least the sentinel (utf8.RuneSelf)
            // in the buffer. Measure and optimize if necessary.

            // make sure we have at least one rune in buffer, or we are at EOF
            while (s.r + utf8.UTFMax > s.w && !utf8.FullRune(s.buf[s.r..s.w]) && s.ioerr == null && s.w - s.r < len(s.buf))
            {
                s.fill(); // s.w-s.r < len(s.buf) => buffer is not full
            } 

            // common case: ASCII and enough bytes
            // (invariant: s.buf[s.w] == utf8.RuneSelf)
 

            // common case: ASCII and enough bytes
            // (invariant: s.buf[s.w] == utf8.RuneSelf)
            {
                var b = s.buf[s.r];

                if (b < utf8.RuneSelf)
                {
                    s.r++; 
                    // TODO(gri) Optimization: Instead of adjusting s.col for each character,
                    // remember the line offset instead and then compute the offset as needed
                    // (which is less often).
                    s.col++;
                    if (b == 0L)
                    {
                        s.error("invalid NUL character");
                        goto redo;
                    }
                    if (b == '\n')
                    {
                        s.line++;
                        s.col = colbase;
                    }
                    return rune(b);
                } 

                // EOF

            } 

            // EOF
            if (s.r == s.w)
            {
                if (s.ioerr != io.EOF)
                {
                    s.error(s.ioerr.Error());
                }
                return -1L;
            } 

            // uncommon case: not ASCII
            var (r, w) = utf8.DecodeRune(s.buf[s.r..s.w]);
            s.r += w;
            s.col += uint(w);

            if (r == utf8.RuneError && w == 1L)
            {
                s.error("invalid UTF-8 encoding");
                goto redo;
            } 

            // BOM's are only allowed as the first character in a file
            const ulong BOM = 0xfeffUL;

            if (r == BOM)
            {
                if (s.r0 > 0L)
                { // s.r0 is always > 0 after 1st character (fill will set it to 1)
                    s.error("invalid BOM in the middle of the file");
                }
                goto redo;
            }
            return r;
        }

        private static void fill(this ref source _s) => func(_s, (ref source s, Defer _, Panic panic, Recover __) =>
        { 
            // Slide unread bytes to beginning but preserve last read char
            // (for one ungetr call) plus one extra byte (for a 2nd ungetr
            // call, only for ".." character sequence and float literals
            // starting with ".").
            if (s.r0 > 1L)
            { 
                // save literal prefix, if any
                // (We see at most one ungetr call while reading
                // a literal, so make sure s.r0 remains in buf.)
                if (s.suf >= 0L)
                {
                    s.lit = append(s.lit, s.buf[s.suf..s.r0]);
                    s.suf = 1L; // == s.r0 after slide below
                }
                var n = s.r0 - 1L;
                copy(s.buf[..], s.buf[n..s.w]);
                s.offs += n;
                s.r0 = 1L; // eqv: s.r0 -= n
                s.r -= n;
                s.w -= n;
            } 

            // read more data: try a limited number of times
            for (long i = 100L; i > 0L; i--)
            {
                var (n, err) = s.src.Read(s.buf[s.w..len(s.buf) - 1L]); // -1 to leave space for sentinel
                if (n < 0L)
                {
                    panic("negative read"); // incorrect underlying io.Reader implementation
                }
                s.w += n;
                if (n > 0L || err != null)
                {
                    s.buf[s.w] = utf8.RuneSelf; // sentinel
                    if (err != null)
                    {
                        s.ioerr = err;
                    }
                    return;
                }
            }


            s.ioerr = io.ErrNoProgress;
        });

        private static void startLit(this ref source s)
        {
            s.suf = s.r0;
            s.lit = s.lit[..0L]; // reuse lit
        }

        private static slice<byte> stopLit(this ref source s)
        {
            var lit = s.buf[s.suf..s.r];
            if (len(s.lit) > 0L)
            {
                lit = append(s.lit, lit);
            }
            s.suf = -1L; // no pending literal
            return lit;
        }
    }
}}}}
