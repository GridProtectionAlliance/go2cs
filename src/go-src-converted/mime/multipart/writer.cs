// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package multipart -- go2cs converted at 2020 October 08 03:38:36 UTC
// import "mime/multipart" ==> using multipart = go.mime.multipart_package
// Original source: C:\Go\src\mime\multipart\writer.go
using bytes = go.bytes_package;
using rand = go.crypto.rand_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using textproto = go.net.textproto_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace mime
{
    public static partial class multipart_package
    {
        // A Writer generates multipart messages.
        public partial struct Writer
        {
            public io.Writer w;
            public @string boundary;
            public ptr<part> lastpart;
        }

        // NewWriter returns a new multipart Writer with a random boundary,
        // writing to w.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            return addr(new Writer(w:w,boundary:randomBoundary(),));
        }

        // Boundary returns the Writer's boundary.
        private static @string Boundary(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            return w.boundary;
        }

        // SetBoundary overrides the Writer's default randomly-generated
        // boundary separator with an explicit value.
        //
        // SetBoundary must be called before any parts are created, may only
        // contain certain ASCII characters, and must be non-empty and
        // at most 70 bytes long.
        private static error SetBoundary(this ptr<Writer> _addr_w, @string boundary)
        {
            ref Writer w = ref _addr_w.val;

            if (w.lastpart != null)
            {
                return error.As(errors.New("mime: SetBoundary called after write"))!;
            } 
            // rfc2046#section-5.1.1
            if (len(boundary) < 1L || len(boundary) > 70L)
            {
                return error.As(errors.New("mime: invalid boundary length"))!;
            }

            var end = len(boundary) - 1L;
            foreach (var (i, b) in boundary)
            {
                if ('A' <= b && b <= 'Z' || 'a' <= b && b <= 'z' || '0' <= b && b <= '9')
                {
                    continue;
                }

                switch (b)
                {
                    case '\'': 

                    case '(': 

                    case ')': 

                    case '+': 

                    case '_': 

                    case ',': 

                    case '-': 

                    case '.': 

                    case '/': 

                    case ':': 

                    case '=': 

                    case '?': 
                        continue;
                        break;
                    case ' ': 
                        if (i != end)
                        {
                            continue;
                        }

                        break;
                }
                return error.As(errors.New("mime: invalid boundary character"))!;

            }
            w.boundary = boundary;
            return error.As(null!)!;

        }

        // FormDataContentType returns the Content-Type for an HTTP
        // multipart/form-data with this Writer's Boundary.
        private static @string FormDataContentType(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            var b = w.boundary; 
            // We must quote the boundary if it contains any of the
            // tspecials characters defined by RFC 2045, or space.
            if (strings.ContainsAny(b, "()<>@,;:\\\"/[]?= "))
            {
                b = "\"" + b + "\"";
            }

            return "multipart/form-data; boundary=" + b;

        }

        private static @string randomBoundary() => func((_, panic, __) =>
        {
            array<byte> buf = new array<byte>(30L);
            var (_, err) = io.ReadFull(rand.Reader, buf[..]);
            if (err != null)
            {
                panic(err);
            }

            return fmt.Sprintf("%x", buf[..]);

        });

        // CreatePart creates a new multipart section with the provided
        // header. The body of the part should be written to the returned
        // Writer. After calling CreatePart, any previous part may no longer
        // be written to.
        private static (io.Writer, error) CreatePart(this ptr<Writer> _addr_w, textproto.MIMEHeader header)
        {
            io.Writer _p0 = default;
            error _p0 = default!;
            ref Writer w = ref _addr_w.val;

            if (w.lastpart != null)
            {
                {
                    var err = w.lastpart.close();

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

            }

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            if (w.lastpart != null)
            {
                fmt.Fprintf(_addr_b, "\r\n--%s\r\n", w.boundary);
            }
            else
            {
                fmt.Fprintf(_addr_b, "--%s\r\n", w.boundary);
            }

            var keys = make_slice<@string>(0L, len(header));
            {
                var k__prev1 = k;

                foreach (var (__k) in header)
                {
                    k = __k;
                    keys = append(keys, k);
                }

                k = k__prev1;
            }

            sort.Strings(keys);
            {
                var k__prev1 = k;

                foreach (var (_, __k) in keys)
                {
                    k = __k;
                    foreach (var (_, v) in header[k])
                    {
                        fmt.Fprintf(_addr_b, "%s: %s\r\n", k, v);
                    }

                }

                k = k__prev1;
            }

            fmt.Fprintf(_addr_b, "\r\n");
            var (_, err) = io.Copy(w.w, _addr_b);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            ptr<part> p = addr(new part(mw:w,));
            w.lastpart = p;
            return (p, error.As(null!)!);

        }

        private static var quoteEscaper = strings.NewReplacer("\\", "\\\\", "\"", "\\\"");

        private static @string escapeQuotes(@string s)
        {
            return quoteEscaper.Replace(s);
        }

        // CreateFormFile is a convenience wrapper around CreatePart. It creates
        // a new form-data header with the provided field name and file name.
        private static (io.Writer, error) CreateFormFile(this ptr<Writer> _addr_w, @string fieldname, @string filename)
        {
            io.Writer _p0 = default;
            error _p0 = default!;
            ref Writer w = ref _addr_w.val;

            var h = make(textproto.MIMEHeader);
            h.Set("Content-Disposition", fmt.Sprintf("form-data; name=\"%s\"; filename=\"%s\"", escapeQuotes(fieldname), escapeQuotes(filename)));
            h.Set("Content-Type", "application/octet-stream");
            return w.CreatePart(h);
        }

        // CreateFormField calls CreatePart with a header using the
        // given field name.
        private static (io.Writer, error) CreateFormField(this ptr<Writer> _addr_w, @string fieldname)
        {
            io.Writer _p0 = default;
            error _p0 = default!;
            ref Writer w = ref _addr_w.val;

            var h = make(textproto.MIMEHeader);
            h.Set("Content-Disposition", fmt.Sprintf("form-data; name=\"%s\"", escapeQuotes(fieldname)));
            return w.CreatePart(h);
        }

        // WriteField calls CreateFormField and then writes the given value.
        private static error WriteField(this ptr<Writer> _addr_w, @string fieldname, @string value)
        {
            ref Writer w = ref _addr_w.val;

            var (p, err) = w.CreateFormField(fieldname);
            if (err != null)
            {
                return error.As(err)!;
            }

            _, err = p.Write((slice<byte>)value);
            return error.As(err)!;

        }

        // Close finishes the multipart message and writes the trailing
        // boundary end line to the output.
        private static error Close(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            if (w.lastpart != null)
            {
                {
                    var err = w.lastpart.close();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                w.lastpart = null;

            }

            var (_, err) = fmt.Fprintf(w.w, "\r\n--%s--\r\n", w.boundary);
            return error.As(err)!;

        }

        private partial struct part
        {
            public ptr<Writer> mw;
            public bool closed;
            public error we; // last error that occurred writing
        }

        private static error close(this ptr<part> _addr_p)
        {
            ref part p = ref _addr_p.val;

            p.closed = true;
            return error.As(p.we)!;
        }

        private static (long, error) Write(this ptr<part> _addr_p, slice<byte> d)
        {
            long n = default;
            error err = default!;
            ref part p = ref _addr_p.val;

            if (p.closed)
            {
                return (0L, error.As(errors.New("multipart: can't write to finished part"))!);
            }

            n, err = p.mw.w.Write(d);
            if (err != null)
            {
                p.we = err;
            }

            return ;

        }
    }
}}
