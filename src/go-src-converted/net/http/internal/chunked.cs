// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The wire protocol for HTTP's "chunked" Transfer-Encoding.

// Package internal contains HTTP internals shared by net/http and
// net/http/httputil.
// package @internal -- go2cs converted at 2020 August 29 08:33:50 UTC
// import "net/http/internal" ==> using @internal = go.net.http.@internal_package
// Original source: C:\Go\src\net\http\internal\chunked.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace net {
namespace http
{
    public static partial class @internal_package
    {
        private static readonly long maxLineLength = 4096L; // assumed <= bufio.defaultBufSize

 // assumed <= bufio.defaultBufSize

        public static var ErrLineTooLong = errors.New("header line too long");

        // NewChunkedReader returns a new chunkedReader that translates the data read from r
        // out of HTTP "chunked" format before returning it.
        // The chunkedReader returns io.EOF when the final 0-length chunk is read.
        //
        // NewChunkedReader is not needed by normal applications. The http package
        // automatically decodes chunking when reading response bodies.
        public static io.Reader NewChunkedReader(io.Reader r)
        {
            ref bufio.Reader (br, ok) = r._<ref bufio.Reader>();
            if (!ok)
            {
                br = bufio.NewReader(r);
            }
            return ref new chunkedReader(r:br);
        }

        private partial struct chunkedReader
        {
            public ptr<bufio.Reader> r;
            public ulong n; // unread bytes in chunk
            public error err;
            public array<byte> buf;
            public bool checkEnd; // whether need to check for \r\n chunk footer
        }

        private static void beginChunk(this ref chunkedReader cr)
        { 
            // chunk-size CRLF
            slice<byte> line = default;
            line, cr.err = readChunkLine(cr.r);
            if (cr.err != null)
            {
                return;
            }
            cr.n, cr.err = parseHexUint(line);
            if (cr.err != null)
            {
                return;
            }
            if (cr.n == 0L)
            {
                cr.err = io.EOF;
            }
        }

        private static bool chunkHeaderAvailable(this ref chunkedReader cr)
        {
            var n = cr.r.Buffered();
            if (n > 0L)
            {
                var (peek, _) = cr.r.Peek(n);
                return bytes.IndexByte(peek, '\n') >= 0L;
            }
            return false;
        }

        private static (long, error) Read(this ref chunkedReader cr, slice<byte> b)
        {
            while (cr.err == null)
            {
                if (cr.checkEnd)
                {
                    if (n > 0L && cr.r.Buffered() < 2L)
                    { 
                        // We have some data. Return early (per the io.Reader
                        // contract) instead of potentially blocking while
                        // reading more.
                        break;
                    }
                    _, cr.err = io.ReadFull(cr.r, cr.buf[..2L]);

                    if (cr.err == null)
                    {
                        if (string(cr.buf[..]) != "\r\n")
                        {
                            cr.err = errors.New("malformed chunked encoding");
                            break;
                        }
                    }
                    cr.checkEnd = false;
                }
                if (cr.n == 0L)
                {
                    if (n > 0L && !cr.chunkHeaderAvailable())
                    { 
                        // We've read enough. Don't potentially block
                        // reading a new chunk header.
                        break;
                    }
                    cr.beginChunk();
                    continue;
                }
                if (len(b) == 0L)
                {
                    break;
                }
                var rbuf = b;
                if (uint64(len(rbuf)) > cr.n)
                {
                    rbuf = rbuf[..cr.n];
                }
                long n0 = default;
                n0, cr.err = cr.r.Read(rbuf);
                n += n0;
                b = b[n0..];
                cr.n -= uint64(n0); 
                // If we're at the end of a chunk, read the next two
                // bytes to verify they are "\r\n".
                if (cr.n == 0L && cr.err == null)
                {
                    cr.checkEnd = true;
                }
            }

            return (n, cr.err);
        }

        // Read a line of bytes (up to \n) from b.
        // Give up if the line exceeds maxLineLength.
        // The returned bytes are owned by the bufio.Reader
        // so they are only valid until the next bufio read.
        private static (slice<byte>, error) readChunkLine(ref bufio.Reader b)
        {
            var (p, err) = b.ReadSlice('\n');
            if (err != null)
            { 
                // We always know when EOF is coming.
                // If the caller asked for a line, there should be a line.
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }
                else if (err == bufio.ErrBufferFull)
                {
                    err = ErrLineTooLong;
                }
                return (null, err);
            }
            if (len(p) >= maxLineLength)
            {
                return (null, ErrLineTooLong);
            }
            p = trimTrailingWhitespace(p);
            p, err = removeChunkExtension(p);
            if (err != null)
            {
                return (null, err);
            }
            return (p, null);
        }

        private static slice<byte> trimTrailingWhitespace(slice<byte> b)
        {
            while (len(b) > 0L && isASCIISpace(b[len(b) - 1L]))
            {
                b = b[..len(b) - 1L];
            }

            return b;
        }

        private static bool isASCIISpace(byte b)
        {
            return b == ' ' || b == '\t' || b == '\n' || b == '\r';
        }

        // removeChunkExtension removes any chunk-extension from p.
        // For example,
        //     "0" => "0"
        //     "0;token" => "0"
        //     "0;token=val" => "0"
        //     `0;token="quoted string"` => "0"
        private static (slice<byte>, error) removeChunkExtension(slice<byte> p)
        {
            var semi = bytes.IndexByte(p, ';');
            if (semi == -1L)
            {
                return (p, null);
            } 
            // TODO: care about exact syntax of chunk extensions? We're
            // ignoring and stripping them anyway. For now just never
            // return an error.
            return (p[..semi], null);
        }

        // NewChunkedWriter returns a new chunkedWriter that translates writes into HTTP
        // "chunked" format before writing them to w. Closing the returned chunkedWriter
        // sends the final 0-length chunk that marks the end of the stream.
        //
        // NewChunkedWriter is not needed by normal applications. The http
        // package adds chunking automatically if handlers don't set a
        // Content-Length header. Using newChunkedWriter inside a handler
        // would result in double chunking or chunking with a Content-Length
        // length, both of which are wrong.
        public static io.WriteCloser NewChunkedWriter(io.Writer w)
        {
            return ref new chunkedWriter(w);
        }

        // Writing to chunkedWriter translates to writing in HTTP chunked Transfer
        // Encoding wire format to the underlying Wire chunkedWriter.
        private partial struct chunkedWriter
        {
            public io.Writer Wire;
        }

        // Write the contents of data as one chunk to Wire.
        // NOTE: Note that the corresponding chunk-writing procedure in Conn.Write has
        // a bug since it does not check for success of io.WriteString
        private static (long, error) Write(this ref chunkedWriter cw, slice<byte> data)
        {
            // Don't send 0-length data. It looks like EOF for chunked encoding.
            if (len(data) == 0L)
            {
                return (0L, null);
            }
            _, err = fmt.Fprintf(cw.Wire, "%x\r\n", len(data));

            if (err != null)
            {
                return (0L, err);
            }
            n, err = cw.Wire.Write(data);

            if (err != null)
            {
                return;
            }
            if (n != len(data))
            {
                err = io.ErrShortWrite;
                return;
            }
            _, err = io.WriteString(cw.Wire, "\r\n");

            if (err != null)
            {
                return;
            }
            {
                ref FlushAfterChunkWriter (bw, ok) = cw.Wire._<ref FlushAfterChunkWriter>();

                if (ok)
                {
                    err = bw.Flush();
                }

            }
            return;
        }

        private static error Close(this ref chunkedWriter cw)
        {
            var (_, err) = io.WriteString(cw.Wire, "0\r\n");
            return error.As(err);
        }

        // FlushAfterChunkWriter signals from the caller of NewChunkedWriter
        // that each chunk should be followed by a flush. It is used by the
        // http.Transport code to keep the buffering behavior for headers and
        // trailers, but flush out chunks aggressively in the middle for
        // request bodies which may be generated slowly. See Issue 6574.
        public partial struct FlushAfterChunkWriter
        {
            public ref bufio.Writer Writer => ref Writer_ptr;
        }

        private static (ulong, error) parseHexUint(slice<byte> v)
        {
            foreach (var (i, b) in v)
            {

                if ('0' <= b && b <= '9') 
                    b = b - '0';
                else if ('a' <= b && b <= 'f') 
                    b = b - 'a' + 10L;
                else if ('A' <= b && b <= 'F') 
                    b = b - 'A' + 10L;
                else 
                    return (0L, errors.New("invalid byte in chunk length"));
                                if (i == 16L)
                {
                    return (0L, errors.New("http chunk length too large"));
                }
                n <<= 4L;
                n |= uint64(b);
            }
            return;
        }
    }
}}}
