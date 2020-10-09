// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2020 October 09 06:08:25 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\readwriter.go
using io = go.io_package;
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
        private partial struct normWriter
        {
            public reorderBuffer rb;
            public io.Writer w;
            public slice<byte> buf;
        }

        // Write implements the standard write interface.  If the last characters are
        // not at a normalization boundary, the bytes will be buffered for the next
        // write. The remaining bytes will be written on close.
        private static (long, error) Write(this ptr<normWriter> _addr_w, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref normWriter w = ref _addr_w.val;
 
            // Process data in pieces to keep w.buf size bounded.
            const long chunk = (long)4000L;



            while (len(data) > 0L)
            { 
                // Normalize into w.buf.
                var m = len(data);
                if (m > chunk)
                {
                    m = chunk;
                }

                w.rb.src = inputBytes(data[..m]);
                w.rb.nsrc = m;
                w.buf = doAppend(_addr_w.rb, w.buf, 0L);
                data = data[m..];
                n += m; 

                // Write out complete prefix, save remainder.
                // Note that lastBoundary looks back at most 31 runes.
                var i = lastBoundary(_addr_w.rb.f, w.buf);
                if (i == -1L)
                {
                    i = 0L;
                }

                if (i > 0L)
                {
                    _, err = w.w.Write(w.buf[..i]);

                    if (err != null)
                    {
                        break;
                    }

                    var bn = copy(w.buf, w.buf[i..]);
                    w.buf = w.buf[..bn];

                }

            }

            return (n, error.As(err)!);

        }

        // Close forces data that remains in the buffer to be written.
        private static error Close(this ptr<normWriter> _addr_w)
        {
            ref normWriter w = ref _addr_w.val;

            if (len(w.buf) > 0L)
            {
                var (_, err) = w.w.Write(w.buf);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            return error.As(null!)!;

        }

        // Writer returns a new writer that implements Write(b)
        // by writing f(b) to w. The returned writer may use an
        // internal buffer to maintain state across Write calls.
        // Calling its Close method writes any buffered data to w.
        public static io.WriteCloser Writer(this Form f, io.Writer w)
        {
            ptr<normWriter> wr = addr(new normWriter(rb:reorderBuffer{},w:w));
            wr.rb.init(f, null);
            return wr;
        }

        private partial struct normReader
        {
            public reorderBuffer rb;
            public io.Reader r;
            public slice<byte> inbuf;
            public slice<byte> outbuf;
            public long bufStart;
            public long lastBoundary;
            public error err;
        }

        // Read implements the standard read interface.
        private static (long, error) Read(this ptr<normReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref normReader r = ref _addr_r.val;

            while (true)
            {
                if (r.lastBoundary - r.bufStart > 0L)
                {
                    var n = copy(p, r.outbuf[r.bufStart..r.lastBoundary]);
                    r.bufStart += n;
                    if (r.lastBoundary - r.bufStart > 0L)
                    {
                        return (n, error.As(null!)!);
                    }

                    return (n, error.As(r.err)!);

                }

                if (r.err != null)
                {
                    return (0L, error.As(r.err)!);
                }

                var outn = copy(r.outbuf, r.outbuf[r.lastBoundary..]);
                r.outbuf = r.outbuf[0L..outn];
                r.bufStart = 0L;

                var (n, err) = r.r.Read(r.inbuf);
                r.rb.src = inputBytes(r.inbuf[0L..n]);
                r.rb.nsrc = n;
                r.err = err;
                if (n > 0L)
                {
                    r.outbuf = doAppend(_addr_r.rb, r.outbuf, 0L);
                }

                if (err == io.EOF)
                {
                    r.lastBoundary = len(r.outbuf);
                }
                else
                {
                    r.lastBoundary = lastBoundary(_addr_r.rb.f, r.outbuf);
                    if (r.lastBoundary == -1L)
                    {
                        r.lastBoundary = 0L;
                    }

                }

            }


        }

        // Reader returns a new reader that implements Read
        // by reading data from r and returning f(data).
        public static io.Reader Reader(this Form f, io.Reader r)
        {
            const long chunk = (long)4000L;

            var buf = make_slice<byte>(chunk);
            ptr<normReader> rr = addr(new normReader(rb:reorderBuffer{},r:r,inbuf:buf));
            rr.rb.init(f, buf);
            return rr;
        }
    }
}}}}}}
