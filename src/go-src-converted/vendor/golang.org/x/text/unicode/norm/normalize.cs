// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Note: the file data_test.go that is generated should not be checked in.
//go:generate go run maketables.go triegen.go
//go:generate go test -tags test

// Package norm contains types and functions for normalizing Unicode strings.
// package norm -- go2cs converted at 2020 October 08 05:02:23 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\normalize.go
// import "golang.org/x/text/unicode/norm"

using utf8 = go.unicode.utf8_package;

using transform = go.golang.org.x.text.transform_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class norm_package
    {
        // A Form denotes a canonical representation of Unicode code points.
        // The Unicode-defined normalization and equivalence forms are:
        //
        //   NFC   Unicode Normalization Form C
        //   NFD   Unicode Normalization Form D
        //   NFKC  Unicode Normalization Form KC
        //   NFKD  Unicode Normalization Form KD
        //
        // For a Form f, this documentation uses the notation f(x) to mean
        // the bytes or string x converted to the given form.
        // A position n in x is called a boundary if conversion to the form can
        // proceed independently on both sides:
        //   f(x) == append(f(x[0:n]), f(x[n:])...)
        //
        // References: https://unicode.org/reports/tr15/ and
        // https://unicode.org/notes/tn5/.
        public partial struct Form // : long
        {
        }

        public static readonly Form NFC = (Form)iota;
        public static readonly var NFD = (var)0;
        public static readonly var NFKC = (var)1;
        public static readonly var NFKD = (var)2;


        // Bytes returns f(b). May return b if f(b) = b.
        public static slice<byte> Bytes(this Form f, slice<byte> b)
        {
            var src = inputBytes(b);
            var ft = formTable[f];
            var (n, ok) = ft.quickSpan(src, 0L, len(b), true);
            if (ok)
            {
                return b;
            }

            var @out = make_slice<byte>(n, len(b));
            copy(out, b[0L..n]);
            ref reorderBuffer rb = ref heap(new reorderBuffer(f:*ft,src:src,nsrc:len(b),out:out,flushF:appendFlush), out ptr<reorderBuffer> _addr_rb);
            return doAppendInner(_addr_rb, n);

        }

        // String returns f(s).
        public static @string String(this Form f, @string s)
        {
            var src = inputString(s);
            var ft = formTable[f];
            var (n, ok) = ft.quickSpan(src, 0L, len(s), true);
            if (ok)
            {
                return s;
            }

            var @out = make_slice<byte>(n, len(s));
            copy(out, s[0L..n]);
            ref reorderBuffer rb = ref heap(new reorderBuffer(f:*ft,src:src,nsrc:len(s),out:out,flushF:appendFlush), out ptr<reorderBuffer> _addr_rb);
            return string(doAppendInner(_addr_rb, n));

        }

        // IsNormal returns true if b == f(b).
        public static bool IsNormal(this Form f, slice<byte> b)
        {
            var src = inputBytes(b);
            var ft = formTable[f];
            var (bp, ok) = ft.quickSpan(src, 0L, len(b), true);
            if (ok)
            {
                return true;
            }

            ref reorderBuffer rb = ref heap(new reorderBuffer(f:*ft,src:src,nsrc:len(b)), out ptr<reorderBuffer> _addr_rb);
            rb.setFlusher(null, cmpNormalBytes);
            while (bp < len(b))
            {
                rb.@out = b[bp..];
                bp = decomposeSegment(_addr_rb, bp, true);

                if (bp < 0L)
                {
                    return false;
                }

                bp, _ = rb.f.quickSpan(rb.src, bp, len(b), true);

            }

            return true;

        }

        private static bool cmpNormalBytes(ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var b = rb.@out;
            for (long i = 0L; i < rb.nrune; i++)
            {
                var info = rb.rune[i];
                if (int(info.size) > len(b))
                {
                    return false;
                }

                var p = info.pos;
                var pe = p + info.size;
                while (p < pe)
                {
                    if (b[0L] != rb.@byte[p])
                    {
                        return false;
                    p++;
                    }

                    b = b[1L..];

                }


            }

            return true;

        }

        // IsNormalString returns true if s == f(s).
        public static bool IsNormalString(this Form f, @string s)
        {
            var src = inputString(s);
            var ft = formTable[f];
            var (bp, ok) = ft.quickSpan(src, 0L, len(s), true);
            if (ok)
            {
                return true;
            }

            ref reorderBuffer rb = ref heap(new reorderBuffer(f:*ft,src:src,nsrc:len(s)), out ptr<reorderBuffer> _addr_rb);
            rb.setFlusher(null, rb =>
            {
                for (long i = 0L; i < rb.nrune; i++)
                {
                    var info = rb.rune[i];
                    if (bp + int(info.size) > len(s))
                    {
                        return false;
                    }

                    var p = info.pos;
                    var pe = p + info.size;
                    while (p < pe)
                    {
                        if (s[bp] != rb.@byte[p])
                        {
                            return false;
                        p++;
                        }

                        bp++;

                    }


                }

                return true;

            });
            while (bp < len(s))
            {
                bp = decomposeSegment(_addr_rb, bp, true);

                if (bp < 0L)
                {
                    return false;
                }

                bp, _ = rb.f.quickSpan(rb.src, bp, len(s), true);

            }

            return true;

        }

        // patchTail fixes a case where a rune may be incorrectly normalized
        // if it is followed by illegal continuation bytes. It returns the
        // patched buffer and whether the decomposition is still in progress.
        private static bool patchTail(ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var (info, p) = lastRuneStart(_addr_rb.f, rb.@out);
            if (p == -1L || info.size == 0L)
            {
                return true;
            }

            var end = p + int(info.size);
            var extra = len(rb.@out) - end;
            if (extra > 0L)
            { 
                // Potentially allocating memory. However, this only
                // happens with ill-formed UTF-8.
                var x = make_slice<byte>(0L);
                x = append(x, rb.@out[len(rb.@out) - extra..]);
                rb.@out = rb.@out[..end];
                decomposeToLastBoundary(_addr_rb);
                rb.doFlush();
                rb.@out = append(rb.@out, x);
                return false;

            }

            var buf = rb.@out[p..];
            rb.@out = rb.@out[..p];
            decomposeToLastBoundary(_addr_rb);
            {
                var s = rb.ss.next(info);

                if (s == ssStarter)
                {
                    rb.doFlush();
                    rb.ss.first(info);
                }
                else if (s == ssOverflow)
                {
                    rb.doFlush();
                    rb.insertCGJ();
                    rb.ss = 0L;
                }


            }

            rb.insertUnsafe(inputBytes(buf), 0L, info);
            return true;

        }

        private static long appendQuick(ptr<reorderBuffer> _addr_rb, long i)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            if (rb.nsrc == i)
            {
                return i;
            }

            var (end, _) = rb.f.quickSpan(rb.src, i, rb.nsrc, true);
            rb.@out = rb.src.appendSlice(rb.@out, i, end);
            return end;

        }

        // Append returns f(append(out, b...)).
        // The buffer out must be nil, empty, or equal to f(out).
        public static slice<byte> Append(this Form f, slice<byte> @out, params byte[] src)
        {
            src = src.Clone();

            return f.doAppend(out, inputBytes(src), len(src));
        }

        public static slice<byte> doAppend(this Form f, slice<byte> @out, input src, long n)
        {
            if (n == 0L)
            {
                return out;
            }

            var ft = formTable[f]; 
            // Attempt to do a quickSpan first so we can avoid initializing the reorderBuffer.
            if (len(out) == 0L)
            {
                var (p, _) = ft.quickSpan(src, 0L, n, true);
                out = src.appendSlice(out, 0L, p);
                if (p == n)
                {
                    return out;
                }

                ref reorderBuffer rb = ref heap(new reorderBuffer(f:*ft,src:src,nsrc:n,out:out,flushF:appendFlush), out ptr<reorderBuffer> _addr_rb);
                return doAppendInner(_addr_rb, p);

            }

            rb = new reorderBuffer(f:*ft,src:src,nsrc:n);
            return doAppend(_addr_rb, out, 0L);

        }

        private static slice<byte> doAppend(ptr<reorderBuffer> _addr_rb, slice<byte> @out, long p)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.setFlusher(out, appendFlush);
            var src = rb.src;
            var n = rb.nsrc;
            var doMerge = len(out) > 0L;
            {
                var q = src.skipContinuationBytes(p);

                if (q > p)
                { 
                    // Move leading non-starters to destination.
                    rb.@out = src.appendSlice(rb.@out, p, q);
                    p = q;
                    doMerge = patchTail(_addr_rb);

                }

            }

            var fd = _addr_rb.f;
            if (doMerge)
            {
                Properties info = default;
                if (p < n)
                {
                    info = fd.info(src, p);
                    if (!info.BoundaryBefore() || info.nLeadingNonStarters() > 0L)
                    {
                        if (p == 0L)
                        {
                            decomposeToLastBoundary(_addr_rb);
                        }

                        p = decomposeSegment(_addr_rb, p, true);

                    }

                }

                if (info.size == 0L)
                {
                    rb.doFlush(); 
                    // Append incomplete UTF-8 encoding.
                    return src.appendSlice(rb.@out, p, n);

                }

                if (rb.nrune > 0L)
                {
                    return doAppendInner(_addr_rb, p);
                }

            }

            p = appendQuick(_addr_rb, p);
            return doAppendInner(_addr_rb, p);

        }

        private static slice<byte> doAppendInner(ptr<reorderBuffer> _addr_rb, long p)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            {
                var n = rb.nsrc;

                while (p < n)
                {
                    p = decomposeSegment(_addr_rb, p, true);
                    p = appendQuick(_addr_rb, p);
                }

            }
            return rb.@out;

        }

        // AppendString returns f(append(out, []byte(s))).
        // The buffer out must be nil, empty, or equal to f(out).
        public static slice<byte> AppendString(this Form f, slice<byte> @out, @string src)
        {
            return f.doAppend(out, inputString(src), len(src));
        }

        // QuickSpan returns a boundary n such that b[0:n] == f(b[0:n]).
        // It is not guaranteed to return the largest such n.
        public static long QuickSpan(this Form f, slice<byte> b)
        {
            var (n, _) = formTable[f].quickSpan(inputBytes(b), 0L, len(b), true);
            return n;
        }

        // Span implements transform.SpanningTransformer. It returns a boundary n such
        // that b[0:n] == f(b[0:n]). It is not guaranteed to return the largest such n.
        public static (long, error) Span(this Form f, slice<byte> b, bool atEOF)
        {
            long n = default;
            error err = default!;

            var (n, ok) = formTable[f].quickSpan(inputBytes(b), 0L, len(b), atEOF);
            if (n < len(b))
            {
                if (!ok)
                {
                    err = transform.ErrEndOfSpan;
                }
                else
                {
                    err = transform.ErrShortSrc;
                }

            }

            return (n, error.As(err)!);

        }

        // SpanString returns a boundary n such that s[0:n] == f(s[0:n]).
        // It is not guaranteed to return the largest such n.
        public static (long, error) SpanString(this Form f, @string s, bool atEOF)
        {
            long n = default;
            error err = default!;

            var (n, ok) = formTable[f].quickSpan(inputString(s), 0L, len(s), atEOF);
            if (n < len(s))
            {
                if (!ok)
                {
                    err = transform.ErrEndOfSpan;
                }
                else
                {
                    err = transform.ErrShortSrc;
                }

            }

            return (n, error.As(err)!);

        }

        // quickSpan returns a boundary n such that src[0:n] == f(src[0:n]) and
        // whether any non-normalized parts were found. If atEOF is false, n will
        // not point past the last segment if this segment might be become
        // non-normalized by appending other runes.
        private static (long, bool) quickSpan(this ptr<formInfo> _addr_f, input src, long i, long end, bool atEOF)
        {
            long n = default;
            bool ok = default;
            ref formInfo f = ref _addr_f.val;

            byte lastCC = default;
            var ss = streamSafe(0L);
            var lastSegStart = i;
            n = end;

            while (i < n)
            {
                {
                    var j = src.skipASCII(i, n);

                    if (i != j)
                    {
                        i = j;
                        lastSegStart = i - 1L;
                        lastCC = 0L;
                        ss = 0L;
                        continue;
                    }

                }

                var info = f.info(src, i);
                if (info.size == 0L)
                {
                    if (atEOF)
                    { 
                        // include incomplete runes
                        return (n, true);

                    }

                    return (lastSegStart, true);

                } 
                // This block needs to be before the next, because it is possible to
                // have an overflow for runes that are starters (e.g. with U+FF9E).

                if (ss.next(info) == ssStarter) 
                    lastSegStart = i;
                else if (ss.next(info) == ssOverflow) 
                    return (lastSegStart, false);
                else if (ss.next(info) == ssSuccess) 
                    if (lastCC > info.ccc)
                    {
                        return (lastSegStart, false);
                    }

                                if (f.composing)
                {
                    if (!info.isYesC())
                    {
                        break;
                    }

                }
                else
                {
                    if (!info.isYesD())
                    {
                        break;
                    }

                }

                lastCC = info.ccc;
                i += int(info.size);

            }

            if (i == n)
            {
                if (!atEOF)
                {
                    n = lastSegStart;
                }

                return (n, true);

            }

            return (lastSegStart, false);

        }

        // QuickSpanString returns a boundary n such that s[0:n] == f(s[0:n]).
        // It is not guaranteed to return the largest such n.
        public static long QuickSpanString(this Form f, @string s)
        {
            var (n, _) = formTable[f].quickSpan(inputString(s), 0L, len(s), true);
            return n;
        }

        // FirstBoundary returns the position i of the first boundary in b
        // or -1 if b contains no boundary.
        public static long FirstBoundary(this Form f, slice<byte> b)
        {
            return f.firstBoundary(inputBytes(b), len(b));
        }

        public static long firstBoundary(this Form f, input src, long nsrc)
        {
            var i = src.skipContinuationBytes(0L);
            if (i >= nsrc)
            {
                return -1L;
            }

            var fd = formTable[f];
            var ss = streamSafe(0L); 
            // We should call ss.first here, but we can't as the first rune is
            // skipped already. This means FirstBoundary can't really determine
            // CGJ insertion points correctly. Luckily it doesn't have to.
            while (true)
            {
                var info = fd.info(src, i);
                if (info.size == 0L)
                {
                    return -1L;
                }

                {
                    var s = ss.next(info);

                    if (s != ssSuccess)
                    {
                        return i;
                    }

                }

                i += int(info.size);
                if (i >= nsrc)
                {
                    if (!info.BoundaryAfter() && !ss.isMax())
                    {
                        return -1L;
                    }

                    return nsrc;

                }

            }


        }

        // FirstBoundaryInString returns the position i of the first boundary in s
        // or -1 if s contains no boundary.
        public static long FirstBoundaryInString(this Form f, @string s)
        {
            return f.firstBoundary(inputString(s), len(s));
        }

        // NextBoundary reports the index of the boundary between the first and next
        // segment in b or -1 if atEOF is false and there are not enough bytes to
        // determine this boundary.
        public static long NextBoundary(this Form f, slice<byte> b, bool atEOF)
        {
            return f.nextBoundary(inputBytes(b), len(b), atEOF);
        }

        // NextBoundaryInString reports the index of the boundary between the first and
        // next segment in b or -1 if atEOF is false and there are not enough bytes to
        // determine this boundary.
        public static long NextBoundaryInString(this Form f, @string s, bool atEOF)
        {
            return f.nextBoundary(inputString(s), len(s), atEOF);
        }

        public static long nextBoundary(this Form f, input src, long nsrc, bool atEOF)
        {
            if (nsrc == 0L)
            {
                if (atEOF)
                {
                    return 0L;
                }

                return -1L;

            }

            var fd = formTable[f];
            var info = fd.info(src, 0L);
            if (info.size == 0L)
            {
                if (atEOF)
                {
                    return 1L;
                }

                return -1L;

            }

            var ss = streamSafe(0L);
            ss.first(info);

            {
                var i = int(info.size);

                while (i < nsrc)
                {
                    info = fd.info(src, i);
                    if (info.size == 0L)
                    {
                        if (atEOF)
                        {
                            return i;
                    i += int(info.size);
                        }

                        return -1L;

                    } 
                    // TODO: Using streamSafe to determine the boundary isn't the same as
                    // using BoundaryBefore. Determine which should be used.
                    {
                        var s = ss.next(info);

                        if (s != ssSuccess)
                        {
                            return i;
                        }

                    }

                }

            }
            if (!atEOF && !info.BoundaryAfter() && !ss.isMax())
            {
                return -1L;
            }

            return nsrc;

        }

        // LastBoundary returns the position i of the last boundary in b
        // or -1 if b contains no boundary.
        public static long LastBoundary(this Form f, slice<byte> b)
        {
            return lastBoundary(_addr_formTable[f], b);
        }

        private static long lastBoundary(ptr<formInfo> _addr_fd, slice<byte> b)
        {
            ref formInfo fd = ref _addr_fd.val;

            var i = len(b);
            var (info, p) = lastRuneStart(_addr_fd, b);
            if (p == -1L)
            {
                return -1L;
            }

            if (info.size == 0L)
            { // ends with incomplete rune
                if (p == 0L)
                { // starts with incomplete rune
                    return -1L;

                }

                i = p;
                info, p = lastRuneStart(_addr_fd, b[..i]);
                if (p == -1L)
                { // incomplete UTF-8 encoding or non-starter bytes without a starter
                    return i;

                }

            }

            if (p + int(info.size) != i)
            { // trailing non-starter bytes: illegal UTF-8
                return i;

            }

            if (info.BoundaryAfter())
            {
                return i;
            }

            var ss = streamSafe(0L);
            var v = ss.backwards(info);
            i = p;

            while (i >= 0L && v != ssStarter)
            {
                info, p = lastRuneStart(_addr_fd, b[..i]);
                v = ss.backwards(info);

                if (v == ssOverflow)
                {
                    break;
                i = p;
                }

                if (p + int(info.size) != i)
                {
                    if (p == -1L)
                    { // no boundary found
                        return -1L;

                    }

                    return i; // boundary after an illegal UTF-8 encoding
                }

            }

            return i;

        }

        // decomposeSegment scans the first segment in src into rb. It inserts 0x034f
        // (Grapheme Joiner) when it encounters a sequence of more than 30 non-starters
        // and returns the number of bytes consumed from src or iShortDst or iShortSrc.
        private static long decomposeSegment(ptr<reorderBuffer> _addr_rb, long sp, bool atEOF)
        {
            ref reorderBuffer rb = ref _addr_rb.val;
 
            // Force one character to be consumed.
            var info = rb.f.info(rb.src, sp);
            if (info.size == 0L)
            {
                return 0L;
            }

            {
                var s__prev1 = s;

                var s = rb.ss.next(info);

                if (s == ssStarter)
                { 
                    // TODO: this could be removed if we don't support merging.
                    if (rb.nrune > 0L)
                    {
                        goto end;
                    }

                }
                else if (s == ssOverflow)
                {
                    rb.insertCGJ();
                    goto end;
                }


                s = s__prev1;

            }

            {
                var err__prev1 = err;

                var err = rb.insertFlush(rb.src, sp, info);

                if (err != iSuccess)
                {
                    return int(err);
                }

                err = err__prev1;

            }

            while (true)
            {
                sp += int(info.size);
                if (sp >= rb.nsrc)
                {
                    if (!atEOF && !info.BoundaryAfter())
                    {
                        return int(iShortSrc);
                    }

                    break;

                }

                info = rb.f.info(rb.src, sp);
                if (info.size == 0L)
                {
                    if (!atEOF)
                    {
                        return int(iShortSrc);
                    }

                    break;

                }

                {
                    var s__prev1 = s;

                    s = rb.ss.next(info);

                    if (s == ssStarter)
                    {
                        break;
                    }
                    else if (s == ssOverflow)
                    {
                        rb.insertCGJ();
                        break;
                    }


                    s = s__prev1;

                }

                {
                    var err__prev1 = err;

                    err = rb.insertFlush(rb.src, sp, info);

                    if (err != iSuccess)
                    {
                        return int(err);
                    }

                    err = err__prev1;

                }

            }

end:
            if (!rb.doFlush())
            {
                return int(iShortDst);
            }

            return sp;

        }

        // lastRuneStart returns the runeInfo and position of the last
        // rune in buf or the zero runeInfo and -1 if no rune was found.
        private static (Properties, long) lastRuneStart(ptr<formInfo> _addr_fd, slice<byte> buf)
        {
            Properties _p0 = default;
            long _p0 = default;
            ref formInfo fd = ref _addr_fd.val;

            var p = len(buf) - 1L;
            while (p >= 0L && !utf8.RuneStart(buf[p]))
            {
                p--;
            }

            if (p < 0L)
            {
                return (new Properties(), -1L);
            }

            return (fd.info(inputBytes(buf), p), p);

        }

        // decomposeToLastBoundary finds an open segment at the end of the buffer
        // and scans it into rb. Returns the buffer minus the last segment.
        private static void decomposeToLastBoundary(ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var fd = _addr_rb.f;
            var (info, i) = lastRuneStart(_addr_fd, rb.@out);
            if (int(info.size) != len(rb.@out) - i)
            { 
                // illegal trailing continuation bytes
                return ;

            }

            if (info.BoundaryAfter())
            {
                return ;
            }

            array<Properties> add = new array<Properties>(maxNonStarters + 1L); // stores runeInfo in reverse order
            long padd = 0L;
            var ss = streamSafe(0L);
            var p = len(rb.@out);
            while (true)
            {
                add[padd] = info;
                var v = ss.backwards(info);
                if (v == ssOverflow)
                { 
                    // Note that if we have an overflow, it the string we are appending to
                    // is not correctly normalized. In this case the behavior is undefined.
                    break;

                }

                padd++;
                p -= int(info.size);
                if (v == ssStarter || p < 0L)
                {
                    break;
                }

                info, i = lastRuneStart(_addr_fd, rb.@out[..p]);
                if (int(info.size) != p - i)
                {
                    break;
                }

            }

            rb.ss = ss; 
            // Copy bytes for insertion as we may need to overwrite rb.out.
            array<byte> buf = new array<byte>(maxBufferSize * utf8.UTFMax);
            var cp = buf[..copy(buf[..], rb.@out[p..])];
            rb.@out = rb.@out[..p];
            padd--;

            while (padd >= 0L)
            {
                info = add[padd];
                rb.insertUnsafe(inputBytes(cp), 0L, info);
                cp = cp[info.size..];
                padd--;
            }


        }
    }
}}}}}}
