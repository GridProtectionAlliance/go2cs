// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package transform provides reader and writer wrappers that transform the
// bytes passing through as well as various transformations. Example
// transformations provided by other packages include normalization and
// conversion between character sets.

// package transform -- go2cs converted at 2022 March 13 06:46:37 UTC
// import "vendor/golang.org/x/text/transform" ==> using transform = go.vendor.golang.org.x.text.transform_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\transform\transform.go
namespace go.vendor.golang.org.x.text;
// import "golang.org/x/text/transform"


using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class transform_package {

 
// ErrShortDst means that the destination buffer was too short to
// receive all of the transformed bytes.
public static var ErrShortDst = errors.New("transform: short destination buffer");public static var ErrShortSrc = errors.New("transform: short source buffer");public static var ErrEndOfSpan = errors.New("transform: input and output are not identical");private static var errInconsistentByteCount = errors.New("transform: inconsistent byte count returned");private static var errShortInternal = errors.New("transform: short internal buffer");

// Transformer transforms bytes.
public partial interface Transformer {
    (nint, nint, error) Transform(slice<byte> dst, slice<byte> src, bool atEOF); // Reset resets the state and allows a Transformer to be reused.
    (nint, nint, error) Reset();
}

// SpanningTransformer extends the Transformer interface with a Span method
// that determines how much of the input already conforms to the Transformer.
public partial interface SpanningTransformer {
    (nint, error) Span(slice<byte> src, bool atEOF);
}

// NopResetter can be embedded by implementations of Transformer to add a nop
// Reset method.
public partial struct NopResetter {
}

// Reset implements the Reset method of the Transformer interface.
public static void Reset(this NopResetter _p0) {
}

// Reader wraps another io.Reader by transforming the bytes read.
public partial struct Reader {
    public io.Reader r;
    public Transformer t;
    public error err; // dst[dst0:dst1] contains bytes that have been transformed by t but
// not yet copied out via Read.
    public slice<byte> dst;
    public nint dst0; // src[src0:src1] contains bytes that have been read from r but not
// yet transformed through t.
    public nint dst1; // src[src0:src1] contains bytes that have been read from r but not
// yet transformed through t.
    public slice<byte> src;
    public nint src0; // transformComplete is whether the transformation is complete,
// regardless of whether or not it was successful.
    public nint src1; // transformComplete is whether the transformation is complete,
// regardless of whether or not it was successful.
    public bool transformComplete;
}

private static readonly nint defaultBufSize = 4096;

// NewReader returns a new Reader that wraps r by transforming the bytes read
// via t. It calls Reset on t.


// NewReader returns a new Reader that wraps r by transforming the bytes read
// via t. It calls Reset on t.
public static ptr<Reader> NewReader(io.Reader r, Transformer t) {
    t.Reset();
    return addr(new Reader(r:r,t:t,dst:make([]byte,defaultBufSize),src:make([]byte,defaultBufSize),));
}

// Read implements the io.Reader interface.
private static (nint, error) Read(this ptr<Reader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    nint n = 0;
    var err = error(null);
    while (true) { 
        // Copy out any transformed bytes and return the final error if we are done.
        if (r.dst0 != r.dst1) {
            n = copy(p, r.dst[(int)r.dst0..(int)r.dst1]);
            r.dst0 += n;
            if (r.dst0 == r.dst1 && r.transformComplete) {
                return (n, error.As(r.err)!);
            }
            return (n, error.As(null!)!);
        }
        else if (r.transformComplete) {
            return (0, error.As(r.err)!);
        }
        if (r.src0 != r.src1 || r.err != null) {
            r.dst0 = 0;
            r.dst1, n, err = r.t.Transform(r.dst, r.src[(int)r.src0..(int)r.src1], r.err == io.EOF);
            r.src0 += n;


            if (err == null) 
                if (r.src0 != r.src1) {
                    r.err = errInconsistentByteCount;
                } 
                // The Transform call was successful; we are complete if we
                // cannot read more bytes into src.
                r.transformComplete = r.err != null;
                continue;
            else if (err == ErrShortDst && (r.dst1 != 0 || n != 0)) 
                // Make room in dst by copying out, and try again.
                continue;
            else if (err == ErrShortSrc && r.src1 - r.src0 != len(r.src) && r.err == null)             else 
                r.transformComplete = true; 
                // The reader error (r.err) takes precedence over the
                // transformer error (err) unless r.err is nil or io.EOF.
                if (r.err == null || r.err == io.EOF) {
                    r.err = err;
                }
                continue;
                    }
        if (r.src0 != 0) {
            (r.src0, r.src1) = (0, copy(r.src, r.src[(int)r.src0..(int)r.src1]));
        }
        n, r.err = r.r.Read(r.src[(int)r.src1..]);
        r.src1 += n;
    }
}

// TODO: implement ReadByte (and ReadRune??).

// Writer wraps another io.Writer by transforming the bytes read.
// The user needs to call Close to flush unwritten bytes that may
// be buffered.
public partial struct Writer {
    public io.Writer w;
    public Transformer t;
    public slice<byte> dst; // src[:n] contains bytes that have not yet passed through t.
    public slice<byte> src;
    public nint n;
}

// NewWriter returns a new Writer that wraps w by transforming the bytes written
// via t. It calls Reset on t.
public static ptr<Writer> NewWriter(io.Writer w, Transformer t) {
    t.Reset();
    return addr(new Writer(w:w,t:t,dst:make([]byte,defaultBufSize),src:make([]byte,defaultBufSize),));
}

// Write implements the io.Writer interface. If there are not enough
// bytes available to complete a Transform, the bytes will be buffered
// for the next write. Call Close to convert the remaining bytes.
private static (nint, error) Write(this ptr<Writer> _addr_w, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref Writer w = ref _addr_w.val;

    var src = data;
    if (w.n > 0) { 
        // Append bytes from data to the last remainder.
        // TODO: limit the amount copied on first try.
        n = copy(w.src[(int)w.n..], data);
        w.n += n;
        src = w.src[..(int)w.n];
    }
    while (true) {
        var (nDst, nSrc, err) = w.t.Transform(w.dst, src, false);
        {
            var (_, werr) = w.w.Write(w.dst[..(int)nDst]);

            if (werr != null) {
                return (n, error.As(werr)!);
            }

        }
        src = src[(int)nSrc..];
        if (w.n == 0) {
            n += nSrc;
        }
        else if (len(src) <= n) { 
            // Enough bytes from w.src have been consumed. We make src point
            // to data instead to reduce the copying.
            w.n = 0;
            n -= len(src);
            src = data[(int)n..];
            if (n < len(data) && (err == null || err == ErrShortSrc)) {
                continue;
            }
        }

        if (err == ErrShortDst) 
            // This error is okay as long as we are making progress.
            if (nDst > 0 || nSrc > 0) {
                continue;
            }
        else if (err == ErrShortSrc) 
            if (len(src) < len(w.src)) {
                var m = copy(w.src, src); 
                // If w.n > 0, bytes from data were already copied to w.src and n
                // was already set to the number of bytes consumed.
                if (w.n == 0) {
                    n += m;
                }
                w.n = m;
                err = null;
            }
            else if (nDst > 0 || nSrc > 0) { 
                // Not enough buffer to store the remainder. Keep processing as
                // long as there is progress. Without this case, transforms that
                // require a lookahead larger than the buffer may result in an
                // error. This is not something one may expect to be common in
                // practice, but it may occur when buffers are set to small
                // sizes during testing.
                continue;
            }
        else if (err == null) 
            if (w.n > 0) {
                err = errInconsistentByteCount;
            }
                return (n, error.As(err)!);
    }
}

// Close implements the io.Closer interface.
private static error Close(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    var src = w.src[..(int)w.n];
    while (true) {
        var (nDst, nSrc, err) = w.t.Transform(w.dst, src, true);
        {
            var (_, werr) = w.w.Write(w.dst[..(int)nDst]);

            if (werr != null) {
                return error.As(werr)!;
            }

        }
        if (err != ErrShortDst) {
            return error.As(err)!;
        }
        src = src[(int)nSrc..];
    }
}

private partial struct nop {
    public ref NopResetter NopResetter => ref NopResetter_val;
}

private static (nint, nint, error) Transform(this nop _p0, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;

    var n = copy(dst, src);
    if (n < len(src)) {
        err = ErrShortDst;
    }
    return (n, n, error.As(err)!);
}

private static (nint, error) Span(this nop _p0, slice<byte> src, bool atEOF) {
    nint n = default;
    error err = default!;

    return (len(src), error.As(null!)!);
}

private partial struct discard {
    public ref NopResetter NopResetter => ref NopResetter_val;
}

private static (nint, nint, error) Transform(this discard _p0, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;

    return (0, len(src), error.As(null!)!);
}

 
// Discard is a Transformer for which all Transform calls succeed
// by consuming all bytes and writing nothing.
public static Transformer Discard = Transformer.As(new discard())!;public static SpanningTransformer Nop = SpanningTransformer.As(new nop())!;

// chain is a sequence of links. A chain with N Transformers has N+1 links and
// N+1 buffers. Of those N+1 buffers, the first and last are the src and dst
// buffers given to chain.Transform and the middle N-1 buffers are intermediate
// buffers owned by the chain. The i'th link transforms bytes from the i'th
// buffer chain.link[i].b at read offset chain.link[i].p to the i+1'th buffer
// chain.link[i+1].b at write offset chain.link[i+1].n, for i in [0, N).
private partial struct chain {
    public slice<link> link;
    public error err; // errStart is the index at which the error occurred plus 1. Processing
// errStart at this level at the next call to Transform. As long as
// errStart > 0, chain will not consume any more source bytes.
    public nint errStart;
}

private static void fatalError(this ptr<chain> _addr_c, nint errIndex, error err) {
    ref chain c = ref _addr_c.val;

    {
        var i = errIndex + 1;

        if (i > c.errStart) {
            c.errStart = i;
            c.err = err;
        }
    }
}

private partial struct link {
    public Transformer t; // b[p:n] holds the bytes to be transformed by t.
    public slice<byte> b;
    public nint p;
    public nint n;
}

private static slice<byte> src(this ptr<link> _addr_l) {
    ref link l = ref _addr_l.val;

    return l.b[(int)l.p..(int)l.n];
}

private static slice<byte> dst(this ptr<link> _addr_l) {
    ref link l = ref _addr_l.val;

    return l.b[(int)l.n..];
}

// Chain returns a Transformer that applies t in sequence.
public static Transformer Chain(params Transformer[] t) {
    t = t.Clone();

    if (len(t) == 0) {
        return new nop();
    }
    ptr<chain> c = addr(new chain(link:make([]link,len(t)+1)));
    {
        var i__prev1 = i;

        foreach (var (__i, __tt) in t) {
            i = __i;
            tt = __tt;
            c.link[i].t = tt;
        }
        i = i__prev1;
    }

    var b = make_slice<array<byte>>(len(t) - 1);
    {
        var i__prev1 = i;

        foreach (var (__i) in b) {
            i = __i;
            c.link[i + 1].b = b[i][..];
        }
        i = i__prev1;
    }

    return c;
}

// Reset resets the state of Chain. It calls Reset on all the Transformers.
private static void Reset(this ptr<chain> _addr_c) {
    ref chain c = ref _addr_c.val;

    foreach (var (i, l) in c.link) {
        if (l.t != null) {
            l.t.Reset();
        }
        (c.link[i].p, c.link[i].n) = (0, 0);
    }
}

// TODO: make chain use Span (is going to be fun to implement!)

// Transform applies the transformers of c in sequence.
private static (nint, nint, error) Transform(this ptr<chain> _addr_c, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;
    ref chain c = ref _addr_c.val;
 
    // Set up src and dst in the chain.
    var srcL = _addr_c.link[0];
    var dstL = _addr_c.link[len(c.link) - 1];
    (srcL.b, srcL.p, srcL.n) = (src, 0, len(src));    (dstL.b, dstL.n) = (dst, 0);    bool lastFull = default;    bool needProgress = default; // for detecting progress

    // i is the index of the next Transformer to apply, for i in [low, high].
    // low is the lowest index for which c.link[low] may still produce bytes.
    // high is the highest index for which c.link[high] has a Transformer.
    // The error returned by Transform determines whether to increase or
    // decrease i. We try to completely fill a buffer before converting it.
 // for detecting progress

    // i is the index of the next Transformer to apply, for i in [low, high].
    // low is the lowest index for which c.link[low] may still produce bytes.
    // high is the highest index for which c.link[high] has a Transformer.
    // The error returned by Transform determines whether to increase or
    // decrease i. We try to completely fill a buffer before converting it.
    {
        var i__prev1 = i;

        var low = c.errStart;
        var i = c.errStart;
        var high = len(c.link) - 2;

        while (low <= i && i <= high) {
            var @in = _addr_c.link[i];
            var @out = _addr_c.link[i + 1];
            var (nDst, nSrc, err0) = @in.t.Transform(@out.dst(), @in.src(), atEOF && low == i);
            @out.n += nDst;
            @in.p += nSrc;
            if (i > 0 && @in.p == @in.n) {
                (@in.p, @in.n) = (0, 0);
            }
            (needProgress, lastFull) = (lastFull, false);
            if (err0 == ErrShortDst) 
            {
                // Process the destination buffer next. Return if we are already
                // at the high index.
                if (i == high) {
                    return (dstL.n, srcL.p, error.As(ErrShortDst)!);
                }
                if (@out.n != 0) {
                    i++; 
                    // If the Transformer at the next index is not able to process any
                    // source bytes there is nothing that can be done to make progress
                    // and the bytes will remain unprocessed. lastFull is used to
                    // detect this and break out of the loop with a fatal error.
                    lastFull = true;
                    continue;
                } 
                // The destination buffer was too small, but is completely empty.
                // Return a fatal error as this transformation can never complete.
                c.fatalError(i, errShortInternal);
                goto __switch_break0;
            }
            if (err0 == ErrShortSrc)
            {
                if (i == 0) { 
                    // Save ErrShortSrc in err. All other errors take precedence.
                    err = ErrShortSrc;
                    break;
                } 
                // Source bytes were depleted before filling up the destination buffer.
                // Verify we made some progress, move the remaining bytes to the errStart
                // and try to get more source bytes.
                if (needProgress && nSrc == 0 || @in.n - @in.p == len(@in.b)) { 
                    // There were not enough source bytes to proceed while the source
                    // buffer cannot hold any more bytes. Return a fatal error as this
                    // transformation can never complete.
                    c.fatalError(i, errShortInternal);
                    break;
                } 
                // in.b is an internal buffer and we can make progress.
                (@in.p, @in.n) = (0, copy(@in.b, @in.src()));                fallthrough = true;
            }
            if (fallthrough || err0 == null) 
            {
                // if i == low, we have depleted the bytes at index i or any lower levels.
                // In that case we increase low and i. In all other cases we decrease i to
                // fetch more bytes before proceeding to the next index.
                if (i > low) {
                    i--;
                    continue;
                }
                goto __switch_break0;
            }
            // default: 
                c.fatalError(i, err0);

            __switch_break0:; 
            // Exhausted level low or fatal error: increase low and continue
            // to process the bytes accepted so far.
            i++;
            low = i;
        }

        i = i__prev1;
    } 

    // If c.errStart > 0, this means we found a fatal error.  We will clear
    // all upstream buffers. At this point, no more progress can be made
    // downstream, as Transform would have bailed while handling ErrShortDst.
    if (c.errStart > 0) {
        {
            var i__prev1 = i;

            for (i = 1; i < c.errStart; i++) {
                (c.link[i].p, c.link[i].n) = (0, 0);
            }


            i = i__prev1;
        }
        (err, c.errStart, c.err) = (c.err, 0, null);
    }
    return (dstL.n, srcL.p, error.As(err)!);
}

// Deprecated: Use runes.Remove instead.
public static Transformer RemoveFunc(Func<int, bool> f) {
    return removeF(f);
}

public delegate  bool removeF(int);

private static void Reset(this removeF _p0) {
}

// Transform implements the Transformer interface.
private static (nint, nint, error) Transform(this removeF t, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;

    {
        var r = rune(0);
        nint sz = 0;

        while (len(src) > 0) {
            r = rune(src[0]);

            if (r < utf8.RuneSelf) {
                sz = 1;
            src = src[(int)sz..];
            }
            else
 {
                r, sz = utf8.DecodeRune(src);

                if (sz == 1) { 
                    // Invalid rune.
                    if (!atEOF && !utf8.FullRune(src)) {
                        err = ErrShortSrc;
                        break;
                    } 
                    // We replace illegal bytes with RuneError. Not doing so might
                    // otherwise turn a sequence of invalid UTF-8 into valid UTF-8.
                    // The resulting byte sequence may subsequently contain runes
                    // for which t(r) is true that were passed unnoticed.
                    if (!t(r)) {
                        if (nDst + 3 > len(dst)) {
                            err = ErrShortDst;
                            break;
                        }
                        nDst += copy(dst[(int)nDst..], "\uFFFD");
                    }
                    nSrc++;
                    continue;
                }
            }
            if (!t(r)) {
                if (nDst + sz > len(dst)) {
                    err = ErrShortDst;
                    break;
                }
                nDst += copy(dst[(int)nDst..], src[..(int)sz]);
            }
            nSrc += sz;
        }
    }
    return ;
}

// grow returns a new []byte that is longer than b, and copies the first n bytes
// of b to the start of the new slice.
private static slice<byte> grow(slice<byte> b, nint n) {
    var m = len(b);
    if (m <= 32) {
        m = 64;
    }
    else if (m <= 256) {
        m *= 2;
    }
    else
 {
        m += m >> 1;
    }
    var buf = make_slice<byte>(m);
    copy(buf, b[..(int)n]);
    return buf;
}

private static readonly nint initialBufSize = 128;

// String returns a string with the result of converting s[:n] using t, where
// n <= len(s). If err == nil, n will be len(s). It calls Reset on t.


// String returns a string with the result of converting s[:n] using t, where
// n <= len(s). If err == nil, n will be len(s). It calls Reset on t.
public static (@string, nint, error) String(Transformer t, @string s) {
    @string result = default;
    nint n = default;
    error err = default!;

    t.Reset();
    if (s == "") { 
        // Fast path for the common case for empty input. Results in about a
        // 86% reduction of running time for BenchmarkStringLowerEmpty.
        {
            var (_, _, err) = t.Transform(null, null, true);

            if (err == null) {
                return ("", 0, error.As(null!)!);
            }

        }
    }
    array<byte> buf = new array<byte>(new byte[] {  });
    var dst = buf.slice(-1, initialBufSize, initialBufSize);
    var src = buf[(int)initialBufSize..(int)2 * initialBufSize]; 

    // The input string s is transformed in multiple chunks (starting with a
    // chunk size of initialBufSize). nDst and nSrc are per-chunk (or
    // per-Transform-call) indexes, pDst and pSrc are overall indexes.
    nint nDst = 0;
    nint nSrc = 0;
    nint pDst = 0;
    nint pSrc = 0; 

    // pPrefix is the length of a common prefix: the first pPrefix bytes of the
    // result will equal the first pPrefix bytes of s. It is not guaranteed to
    // be the largest such value, but if pPrefix, len(result) and len(s) are
    // all equal after the final transform (i.e. calling Transform with atEOF
    // being true returned nil error) then we don't need to allocate a new
    // result string.
    nint pPrefix = 0;
    while (true) { 
        // Invariant: pDst == pPrefix && pSrc == pPrefix.

        var n = copy(src, s[(int)pSrc..]);
        nDst, nSrc, err = t.Transform(dst, src[..(int)n], pSrc + n == len(s));
        pDst += nDst;
        pSrc += nSrc; 

        // TODO:  let transformers implement an optional Spanner interface, akin
        // to norm's QuickSpan. This would even allow us to avoid any allocation.
        if (!bytes.Equal(dst[..(int)nDst], src[..(int)nSrc])) {
            break;
        }
        pPrefix = pSrc;
        if (err == ErrShortDst) { 
            // A buffer can only be short if a transformer modifies its input.
            break;
        }
        else if (err == ErrShortSrc) {
            if (nSrc == 0) { 
                // No progress was made.
                break;
            } 
            // Equal so far and !atEOF, so continue checking.
        }
        else if (err != null || pPrefix == len(s)) {
            return (string(s[..(int)pPrefix]), pPrefix, error.As(err)!);
        }
    } 
    // Post-condition: pDst == pPrefix + nDst && pSrc == pPrefix + nSrc.

    // We have transformed the first pSrc bytes of the input s to become pDst
    // transformed bytes. Those transformed bytes are discontiguous: the first
    // pPrefix of them equal s[:pPrefix] and the last nDst of them equal
    // dst[:nDst]. We copy them around, into a new dst buffer if necessary, so
    // that they become one contiguous slice: dst[:pDst].
    if (pPrefix != 0) {
        var newDst = dst;
        if (pDst > len(newDst)) {
            newDst = make_slice<byte>(len(s) + nDst - nSrc);
        }
        copy(newDst[(int)pPrefix..(int)pDst], dst[..(int)nDst]);
        copy(newDst[..(int)pPrefix], s[..(int)pPrefix]);
        dst = newDst;
    }
    if ((err == null && pSrc == len(s)) || (err != null && err != ErrShortDst && err != ErrShortSrc)) {
        return (string(dst[..(int)pDst]), pSrc, error.As(err)!);
    }
    while (true) {
        n = copy(src, s[(int)pSrc..]);
        var atEOF = pSrc + n == len(s);
        var (nDst, nSrc, err) = t.Transform(dst[(int)pDst..], src[..(int)n], atEOF);
        pDst += nDst;
        pSrc += nSrc; 

        // If we got ErrShortDst or ErrShortSrc, do not grow as long as we can
        // make progress. This may avoid excessive allocations.
        if (err == ErrShortDst) {
            if (nDst == 0) {
                dst = grow(dst, pDst);
            }
        }
        else if (err == ErrShortSrc) {
            if (atEOF) {
                return (string(dst[..(int)pDst]), pSrc, error.As(err)!);
            }
            if (nSrc == 0) {
                src = grow(src, 0);
            }
        }
        else if (err != null || pSrc == len(s)) {
            return (string(dst[..(int)pDst]), pSrc, error.As(err)!);
        }
    }
}

// Bytes returns a new byte slice with the result of converting b[:n] using t,
// where n <= len(b). If err == nil, n will be len(b). It calls Reset on t.
public static (slice<byte>, nint, error) Bytes(Transformer t, slice<byte> b) {
    slice<byte> result = default;
    nint n = default;
    error err = default!;

    return doAppend(t, 0, make_slice<byte>(len(b)), b);
}

// Append appends the result of converting src[:n] using t to dst, where
// n <= len(src), If err == nil, n will be len(src). It calls Reset on t.
public static (slice<byte>, nint, error) Append(Transformer t, slice<byte> dst, slice<byte> src) {
    slice<byte> result = default;
    nint n = default;
    error err = default!;

    if (len(dst) == cap(dst)) {
        var n = len(src) + len(dst); // It is okay for this to be 0.
        var b = make_slice<byte>(n);
        dst = b[..(int)copy(b, dst)];
    }
    return doAppend(t, len(dst), dst[..(int)cap(dst)], src);
}

private static (slice<byte>, nint, error) doAppend(Transformer t, nint pDst, slice<byte> dst, slice<byte> src) {
    slice<byte> result = default;
    nint n = default;
    error err = default!;

    t.Reset();
    nint pSrc = 0;
    while (true) {
        var (nDst, nSrc, err) = t.Transform(dst[(int)pDst..], src[(int)pSrc..], true);
        pDst += nDst;
        pSrc += nSrc;
        if (err != ErrShortDst) {
            return (dst[..(int)pDst], pSrc, error.As(err)!);
        }
        if (nDst == 0) {
            dst = grow(dst, pDst);
        }
    }
}

} // end transform_package
