// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using io = io_package;

partial class norm_package {

[GoType] partial struct normWriter {
    internal reorderBuffer rb;
    internal io_package.Writer w;
    internal slice<byte> buf;
}

// Write implements the standard write interface.  If the last characters are
// not at a normalization boundary, the bytes will be buffered for the next
// write. The remaining bytes will be written on close.
[GoRecv] internal static (nint n, error err) Write(this ref normWriter w, slice<byte> data) {
    nint n = default!;
    error err = default!;

    // Process data in pieces to keep w.buf size bounded.
    static readonly UntypedInt chunk = 4000;
    while (len(data) > 0) {
        // Normalize into w.buf.
        nint m = len(data);
        if (m > chunk) {
            m = chunk;
        }
        w.rb.src = inputBytes(data[..(int)(m)]);
        w.rb.nsrc = m;
        w.buf = doAppend(Ꮡ(w.rb), w.buf, 0);
        data = data[(int)(m)..];
        n += m;
        // Write out complete prefix, save remainder.
        // Note that lastBoundary looks back at most 31 runes.
        nint i = lastBoundary(Ꮡw.rb.of(reorderBuffer.Ꮡf), w.buf);
        if (i == -1) {
            i = 0;
        }
        if (i > 0) {
            {
                (_, err) = w.w.Write(w.buf[..(int)(i)]); if (err != default!) {
                    break;
                }
            }
            nint bn = copy(w.buf, w.buf[(int)(i)..]);
            w.buf = w.buf[..(int)(bn)];
        }
    }
    return (n, err);
}

// Close forces data that remains in the buffer to be written.
[GoRecv] internal static error Close(this ref normWriter w) {
    if (len(w.buf) > 0) {
        var (_, err) = w.w.Write(w.buf);
        if (err != default!) {
            return err;
        }
    }
    return default!;
}

// Writer returns a new writer that implements Write(b)
// by writing f(b) to w. The returned writer may use an
// internal buffer to maintain state across Write calls.
// Calling its Close method writes any buffered data to w.
public static io.WriteCloser Writer(this Form f, io.Writer w) {
    var wr = Ꮡ(new normWriter(rb: new reorderBuffer(nil), w: w));
    (~wr).rb.init(f, default!);
    return ~wr;
}

[GoType] partial struct normReader {
    internal reorderBuffer rb;
    internal io_package.Reader r;
    internal slice<byte> inbuf;
    internal slice<byte> outbuf;
    internal nint bufStart;
    internal nint lastBoundary;
    internal error err;
}

// Read implements the standard read interface.
[GoRecv] internal static (nint, error) Read(this ref normReader r, slice<byte> p) {
    while (ᐧ) {
        if (r.lastBoundary - r.bufStart > 0) {
            nint n = copy(p, r.outbuf[(int)(r.bufStart)..(int)(r.lastBoundary)]);
            r.bufStart += n;
            if (r.lastBoundary - r.bufStart > 0) {
                return (n, default!);
            }
            return (n, r.err);
        }
        if (r.err != default!) {
            return (0, r.err);
        }
        nint outn = copy(r.outbuf, r.outbuf[(int)(r.lastBoundary)..]);
        r.outbuf = r.outbuf[0..(int)(outn)];
        r.bufStart = 0;
        var (n, err) = r.r.Read(r.inbuf);
        r.rb.src = inputBytes(r.inbuf[0..(int)(n)]);
        (r.rb.nsrc, r.err) = (n, err);
        if (n > 0) {
            r.outbuf = doAppend(Ꮡ(r.rb), r.outbuf, 0);
        }
        if (AreEqual(err, io.EOF)){
            r.lastBoundary = len(r.outbuf);
        } else {
            r.lastBoundary = lastBoundary(Ꮡr.rb.of(reorderBuffer.Ꮡf), r.outbuf);
            if (r.lastBoundary == -1) {
                r.lastBoundary = 0;
            }
        }
    }
}

// Reader returns a new reader that implements Read
// by reading data from r and returning f(data).
public static io.Reader Reader(this Form f, io.Reader r) {
    static readonly UntypedInt chunk = 4000;
    var buf = new slice<byte>(chunk);
    var rr = Ꮡ(new normReader(rb: new reorderBuffer(nil), r: r, inbuf: buf));
    (~rr).rb.init(f, buf);
    return ~rr;
}

} // end norm_package
