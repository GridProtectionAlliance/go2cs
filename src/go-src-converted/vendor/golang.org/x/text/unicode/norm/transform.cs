// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 06 23:40:04 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\transform.go
using utf8 = go.unicode.utf8_package;

using transform = go.golang.org.x.text.transform_package;

namespace go.vendor.golang.org.x.text.unicode;

public static partial class norm_package {

    // Reset implements the Reset method of the transform.Transformer interface.
public static void Reset(this Form _p0) {
}

// Transform implements the Transform method of the transform.Transformer
// interface. It may need to write segments of up to MaxSegmentSize at once.
// Users should either catch ErrShortDst and allow dst to grow or have dst be at
// least of size MaxTransformChunkSize to be guaranteed of progress.
public static (nint, nint, error) Transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;
 
    // Cap the maximum number of src bytes to check.
    var b = src;
    var eof = atEOF;
    {
        var ns = len(dst);

        if (ns < len(b)) {
            err = transform.ErrShortDst;
            eof = false;
            b = b[..(int)ns];
        }
    }

    var (i, ok) = formTable[f].quickSpan(inputBytes(b), 0, len(b), eof);
    var n = copy(dst, b[..(int)i]);
    if (!ok) {
        nDst, nSrc, err = f.transform(dst[(int)n..], src[(int)n..], atEOF);
        return (nDst + n, nSrc + n, error.As(err)!);
    }
    if (err == null && n < len(src) && !atEOF) {
        err = transform.ErrShortSrc;
    }
    return (n, n, error.As(err)!);

}

private static bool flushTransform(ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;
 
    // Write out (must fully fit in dst, or else it is an ErrShortDst).
    if (len(rb.@out) < rb.nrune * utf8.UTFMax) {
        return false;
    }
    rb.@out = rb.@out[(int)rb.flushCopy(rb.@out)..];
    return true;

}

private static error errs = new slice<error>(new error[] { error.As(nil)!, error.As(transform.ErrShortDst)!, error.As(transform.ErrShortSrc)! });

// transform implements the transform.Transformer interface. It is only called
// when quickSpan does not pass for a given string.
public static (nint, nint, error) transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;
 
    // TODO: get rid of reorderBuffer. See CL 23460044.
    ref reorderBuffer rb = ref heap(new reorderBuffer(), out ptr<reorderBuffer> _addr_rb);
    rb.init(f, src);
    while (true) { 
        // Load segment into reorder buffer.
        rb.setFlusher(dst[(int)nDst..], flushTransform);
        var end = decomposeSegment(_addr_rb, nSrc, atEOF);
        if (end < 0) {
            return (nDst, nSrc, error.As(errs[-end])!);
        }
        nDst = len(dst) - len(rb.@out);
        nSrc = end; 

        // Next quickSpan.
        end = rb.nsrc;
        var eof = atEOF;
        {
            var n__prev1 = n;

            var n = nSrc + len(dst) - nDst;

            if (n < end) {
                err = transform.ErrShortDst;
                end = n;
                eof = false;
            }

            n = n__prev1;

        }

        var (end, ok) = rb.f.quickSpan(rb.src, nSrc, end, eof);
        n = copy(dst[(int)nDst..], rb.src.bytes[(int)nSrc..(int)end]);
        nSrc += n;
        nDst += n;
        if (ok) {
            if (err == null && n < rb.nsrc && !atEOF) {
                err = transform.ErrShortSrc;
            }
            return (nDst, nSrc, error.As(err)!);
        }
    }

}

} // end norm_package
