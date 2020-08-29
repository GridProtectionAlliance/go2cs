// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package quotedprintable implements quoted-printable encoding as specified by
// RFC 2045.
// package quotedprintable -- go2cs converted at 2020 August 29 08:32:34 UTC
// import "mime/quotedprintable" ==> using quotedprintable = go.mime.quotedprintable_package
// Original source: C:\Go\src\mime\quotedprintable\reader.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace mime
{
    public static partial class quotedprintable_package
    {
        // Reader is a quoted-printable decoder.
        public partial struct Reader
        {
            public ptr<bufio.Reader> br;
            public error rerr; // last read error
            public slice<byte> line; // to be consumed before more of br
        }

        // NewReader returns a quoted-printable reader, decoding from r.
        public static ref Reader NewReader(io.Reader r)
        {
            return ref new Reader(br:bufio.NewReader(r),);
        }

        private static (byte, error) fromHex(byte b)
        {

            if (b >= '0' && b <= '9') 
                return (b - '0', null);
            else if (b >= 'A' && b <= 'F') 
                return (b - 'A' + 10L, null); 
                // Accept badly encoded bytes.
            else if (b >= 'a' && b <= 'f') 
                return (b - 'a' + 10L, null);
                        return (0L, fmt.Errorf("quotedprintable: invalid hex byte 0x%02x", b));
        }

        private static (byte, error) readHexByte(slice<byte> v)
        {
            if (len(v) < 2L)
            {
                return (0L, io.ErrUnexpectedEOF);
            }
            byte hb = default;            byte lb = default;

            hb, err = fromHex(v[0L]);

            if (err != null)
            {
                return (0L, err);
            }
            lb, err = fromHex(v[1L]);

            if (err != null)
            {
                return (0L, err);
            }
            return (hb << (int)(4L) | lb, null);
        }

        private static bool isQPDiscardWhitespace(int r)
        {
            switch (r)
            {
                case '\n': 

                case '\r': 

                case ' ': 

                case '\t': 
                    return true;
                    break;
            }
            return false;
        }

        private static slice<byte> crlf = (slice<byte>)"\r\n";        private static slice<byte> lf = (slice<byte>)"\n";        private static slice<byte> softSuffix = (slice<byte>)"=";

        // Read reads and decodes quoted-printable data from the underlying reader.
        private static (long, error) Read(this ref Reader r, slice<byte> p)
        { 
            // Deviations from RFC 2045:
            // 1. in addition to "=\r\n", "=\n" is also treated as soft line break.
            // 2. it will pass through a '\r' or '\n' not preceded by '=', consistent
            //    with other broken QP encoders & decoders.
            // 3. it accepts soft line-break (=) at end of message (issue 15486); i.e.
            //    the final byte read from the underlying reader is allowed to be '=',
            //    and it will be silently ignored.
            // 4. it takes = as literal = if not followed by two hex digits
            //    but not at end of line (issue 13219).
            while (len(p) > 0L)
            {
                if (len(r.line) == 0L)
                {
                    if (r.rerr != null)
                    {
                        return (n, r.rerr);
                    }
                    r.line, r.rerr = r.br.ReadSlice('\n'); 

                    // Does the line end in CRLF instead of just LF?
                    var hasLF = bytes.HasSuffix(r.line, lf);
                    var hasCR = bytes.HasSuffix(r.line, crlf);
                    var wholeLine = r.line;
                    r.line = bytes.TrimRightFunc(wholeLine, isQPDiscardWhitespace);
                    if (bytes.HasSuffix(r.line, softSuffix))
                    {
                        var rightStripped = wholeLine[len(r.line)..];
                        r.line = r.line[..len(r.line) - 1L];
                        if (!bytes.HasPrefix(rightStripped, lf) && !bytes.HasPrefix(rightStripped, crlf) && !(len(rightStripped) == 0L && len(r.line) > 0L && r.rerr == io.EOF))
                        {
                            r.rerr = fmt.Errorf("quotedprintable: invalid bytes after =: %q", rightStripped);
                        }
                    }
                    else if (hasLF)
                    {
                        if (hasCR)
                        {
                            r.line = append(r.line, '\r', '\n');
                        }
                        else
                        {
                            r.line = append(r.line, '\n');
                        }
                    }
                    continue;
                }
                var b = r.line[0L];


                if (b == '=') 
                    b, err = readHexByte(r.line[1L..]);
                    if (err != null)
                    {
                        if (len(r.line) >= 2L && r.line[1L] != '\r' && r.line[1L] != '\n')
                        { 
                            // Take the = as a literal =.
                            b = '=';
                            break;
                        }
                        return (n, err);
                    }
                    r.line = r.line[2L..]; // 2 of the 3; other 1 is done below
                else if (b == '\t' || b == '\r' || b == '\n') 
                    break;
                else if (b < ' ' || b > '~') 
                    return (n, fmt.Errorf("quotedprintable: invalid unescaped byte 0x%02x in body", b));
                                p[0L] = b;
                p = p[1L..];
                r.line = r.line[1L..];
                n++;
            }

            return (n, null);
        }
    }
}}
