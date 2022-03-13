// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 13 06:47:09 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\readwriter.go
namespace go.vendor.golang.org.x.text.unicode;

using io = io_package;

public static partial class norm_package {

private partial struct normWriter {
    public reorderBuffer rb;
    public io.Writer w;
    public slice<byte> buf;
}

// Write implements the standard write interface.  If the last characters are
// not at a normalization boundary, the bytes will be buffered for the next
// write. The remaining bytes will be written on close.
private static (nint, error) Write(this ptr<normWriter> _addr_w, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref normWriter w = ref _addr_w.val;
 
    // Process data in pieces to keep w.buf size bounded.
    const nint chunk = 4000;



    while (len(data) > 0) { 
        // Normalize into w.buf.
        var m = len(data);
        if (m > chunk) {
            m = chunk;
        }
        w.rb.src = inputBytes(data[..(int)m]);
        w.rb.nsrc = m;
        w.buf = doAppend(_addr_w.rb, w.buf, 0);
        data = data[(int)m..];
        n += m; 

        // Write out complete prefix, save remainder.
        // Note that lastBoundary looks back at most 31 runes.
        var i = lastBoundary(_addr_w.rb.f, w.buf);
        if (i == -1) {
            i = 0;
        }
        if (i > 0) {
            _, err = w.w.Write(w.buf[..(int)i]);

            if (err != null) {
                break;
            }
            var bn = copy(w.buf, w.buf[(int)i..]);
            w.buf = w.buf[..(int)bn];
        }
    }
    return (n, error.As(err)!);
}

// Close forces data that remains in the buffer to be written.
private static error Close(this ptr<normWriter> _addr_w) {
    ref normWriter w = ref _addr_w.val;

    if (len(w.buf) > 0) {
        var (_, err) = w.w.Write(w.buf);
        if (err != null) {
            return error.As(err)!;
        }
    }
    return error.As(null!)!;
}

// Writer returns a new writer that implements Write(b)
// by writing f(b) to w. The returned writer may use an
// internal buffer to maintain state across Write calls.
// Calling its Close method writes any buffered data to w.
public static io.WriteCloser Writer(this Form f, io.Writer w) {
    ptr<normWriter> wr = addr(new normWriter(rb:reorderBuffer{},w:w));
    wr.rb.init(f, null);
    return wr;
}

private partial struct normReader {
    public reorderBuffer rb;
    public io.Reader r;
    public slice<byte> inbuf;
    public slice<byte> outbuf;
    public nint bufStart;
    public nint lastBoundary;
    public error err;
}

// Read implements the standard read interface.
private static (nint, error) Read(this ptr<normReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref normReader r = ref _addr_r.val;

    while (true) {
        if (r.lastBoundary - r.bufStart > 0) {
            var n = copy(p, r.outbuf[(int)r.bufStart..(int)r.lastBoundary]);
            r.bufStart += n;
            if (r.lastBoundary - r.bufStart > 0) {
                return (n, error.As(null!)!);
            }
            return (n, error.As(r.err)!);
        }
        if (r.err != null) {
            return (0, error.As(r.err)!);
        }
        var outn = copy(r.outbuf, r.outbuf[(int)r.lastBoundary..]);
        r.outbuf = r.outbuf[(int)0..(int)outn];
        r.bufStart = 0;

        var (n, err) = r.r.Read(r.inbuf);
        r.rb.src = inputBytes(r.inbuf[(int)0..(int)n]);
        (r.rb.nsrc, r.err) = (n, err);        if (n > 0) {
            r.outbuf = doAppend(_addr_r.rb, r.outbuf, 0);
        }
        if (err == io.EOF) {
            r.lastBoundary = len(r.outbuf);
        }
        else
 {
            r.lastBoundary = lastBoundary(_addr_r.rb.f, r.outbuf);
            if (r.lastBoundary == -1) {
                r.lastBoundary = 0;
            }
        }
    }
}

// Reader returns a new reader that implements Read
// by reading data from r and returning f(data).
public static io.Reader Reader(this Form f, io.Reader r) {
    const nint chunk = 4000;

    var buf = make_slice<byte>(chunk);
    ptr<normReader> rr = addr(new normReader(rb:reorderBuffer{},r:r,inbuf:buf));
    rr.rb.init(f, buf);
    return rr;
}

} // end norm_package
