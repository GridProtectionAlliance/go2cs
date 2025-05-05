// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = unicode.utf8_package;
using transform = golang.org.x.text.transform_package;
using golang.org.x.text;
using unicode;

partial class norm_package {

// Reset implements the Reset method of the transform.Transformer interface.
public static void Reset(this Form _) {
}

// Transform implements the Transform method of the transform.Transformer
// interface. It may need to write segments of up to MaxSegmentSize at once.
// Users should either catch ErrShortDst and allow dst to grow or have dst be at
// least of size MaxTransformChunkSize to be guaranteed of progress.
public static (nint nDst, nint nSrc, error err) Transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default!;
    nint nSrc = default!;
    error err = default!;

    // Cap the maximum number of src bytes to check.
    var b = src;
    var eof = atEOF;
    {
        nint ns = len(dst); if (ns < len(b)) {
            err = transform.ErrShortDst;
            eof = false;
            b = b[..(int)(ns)];
        }
    }
    var (i, ok) = formTable[f].quickSpan(inputBytes(b), 0, len(b), eof);
    nint n = copy(dst, b[..(int)(i)]);
    if (!ok) {
        (nDst, nSrc, err) = f.transform(dst[(int)(n)..], src[(int)(n)..], atEOF);
        return (nDst + n, nSrc + n, err);
    }
    if (err == default! && n < len(src) && !atEOF) {
        err = transform.ErrShortSrc;
    }
    return (n, n, err);
}

internal static bool flushTransform(ж<reorderBuffer> Ꮡrb) {
    ref var rb = ref Ꮡrb.val;

    // Write out (must fully fit in dst, or else it is an ErrShortDst).
    if (len(rb.@out) < rb.nrune * utf8.UTFMax) {
        return false;
    }
    rb.@out = rb.@out[(int)(rb.flushCopy(rb.@out))..];
    return true;
}

internal static slice<error> errs = new error[]{default!, transform.ErrShortDst, transform.ErrShortSrc}.slice();

// transform implements the transform.Transformer interface. It is only called
// when quickSpan does not pass for a given string.
internal static (nint nDst, nint nSrc, error err) transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default!;
    nint nSrc = default!;
    error err = default!;

    // TODO: get rid of reorderBuffer. See CL 23460044.
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(nil);
    rb.init(f, src);
    while (ᐧ) {
        // Load segment into reorder buffer.
        rb.setFlusher(dst[(int)(nDst)..], flushTransform);
        nint end = decomposeSegment(Ꮡrb, nSrc, atEOF);
        if (end < 0) {
            return (nDst, nSrc, errs[-end]);
        }
        nDst = len(dst) - len(rb.@out);
        nSrc = end;
        // Next quickSpan.
        end = rb.nsrc;
        var eof = atEOF;
        {
            nint n = nSrc + len(dst) - nDst; if (n < end) {
                err = transform.ErrShortDst;
                end = n;
                eof = false;
            }
        }
        var (end, ok) = rb.f.quickSpan(rb.src, nSrc, end, eof);
        nint n = copy(dst[(int)(nDst)..], rb.src.bytes[(int)(nSrc)..(int)(end)]);
        nSrc += n;
        nDst += n;
        if (ok) {
            if (err == default! && n < rb.nsrc && !atEOF) {
                err = transform.ErrShortSrc;
            }
            return (nDst, nSrc, err);
        }
    }
}

} // end norm_package
