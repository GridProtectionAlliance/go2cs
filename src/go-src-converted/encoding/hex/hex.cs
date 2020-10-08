// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hex implements hexadecimal encoding and decoding.
// package hex -- go2cs converted at 2020 October 08 03:36:48 UTC
// import "encoding/hex" ==> using hex = go.encoding.hex_package
// Original source: C:\Go\src\encoding\hex\hex.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class hex_package
    {
        private static readonly @string hextable = (@string)"0123456789abcdef";

        // EncodedLen returns the length of an encoding of n source bytes.
        // Specifically, it returns n * 2.


        // EncodedLen returns the length of an encoding of n source bytes.
        // Specifically, it returns n * 2.
        public static long EncodedLen(long n)
        {
            return n * 2L;
        }

        // Encode encodes src into EncodedLen(len(src))
        // bytes of dst. As a convenience, it returns the number
        // of bytes written to dst, but this value is always EncodedLen(len(src)).
        // Encode implements hexadecimal encoding.
        public static long Encode(slice<byte> dst, slice<byte> src)
        {
            long j = 0L;
            foreach (var (_, v) in src)
            {
                dst[j] = hextable[v >> (int)(4L)];
                dst[j + 1L] = hextable[v & 0x0fUL];
                j += 2L;
            }
            return len(src) * 2L;

        }

        // ErrLength reports an attempt to decode an odd-length input
        // using Decode or DecodeString.
        // The stream-based Decoder returns io.ErrUnexpectedEOF instead of ErrLength.
        public static var ErrLength = errors.New("encoding/hex: odd length hex string");

        // InvalidByteError values describe errors resulting from an invalid byte in a hex string.
        public partial struct InvalidByteError // : byte
        {
        }

        public static @string Error(this InvalidByteError e)
        {
            return fmt.Sprintf("encoding/hex: invalid byte: %#U", rune(e));
        }

        // DecodedLen returns the length of a decoding of x source bytes.
        // Specifically, it returns x / 2.
        public static long DecodedLen(long x)
        {
            return x / 2L;
        }

        // Decode decodes src into DecodedLen(len(src)) bytes,
        // returning the actual number of bytes written to dst.
        //
        // Decode expects that src contains only hexadecimal
        // characters and that src has even length.
        // If the input is malformed, Decode returns the number
        // of bytes decoded before the error.
        public static (long, error) Decode(slice<byte> dst, slice<byte> src)
        {
            long _p0 = default;
            error _p0 = default!;

            long i = 0L;
            long j = 1L;
            while (j < len(src))
            {
                var (a, ok) = fromHexChar(src[j - 1L]);
                if (!ok)
                {
                    return (i, error.As(InvalidByteError(src[j - 1L]))!);
                j += 2L;
                }

                var (b, ok) = fromHexChar(src[j]);
                if (!ok)
                {
                    return (i, error.As(InvalidByteError(src[j]))!);
                }

                dst[i] = (a << (int)(4L)) | b;
                i++;

            }

            if (len(src) % 2L == 1L)
            { 
                // Check for invalid char before reporting bad length,
                // since the invalid char (if present) is an earlier problem.
                {
                    var (_, ok) = fromHexChar(src[j - 1L]);

                    if (!ok)
                    {
                        return (i, error.As(InvalidByteError(src[j - 1L]))!);
                    }

                }

                return (i, error.As(ErrLength)!);

            }

            return (i, error.As(null!)!);

        }

        // fromHexChar converts a hex character into its value and a success flag.
        private static (byte, bool) fromHexChar(byte c)
        {
            byte _p0 = default;
            bool _p0 = default;


            if ('0' <= c && c <= '9') 
                return (c - '0', true);
            else if ('a' <= c && c <= 'f') 
                return (c - 'a' + 10L, true);
            else if ('A' <= c && c <= 'F') 
                return (c - 'A' + 10L, true);
                        return (0L, false);

        }

        // EncodeToString returns the hexadecimal encoding of src.
        public static @string EncodeToString(slice<byte> src)
        {
            var dst = make_slice<byte>(EncodedLen(len(src)));
            Encode(dst, src);
            return string(dst);
        }

        // DecodeString returns the bytes represented by the hexadecimal string s.
        //
        // DecodeString expects that src contains only hexadecimal
        // characters and that src has even length.
        // If the input is malformed, DecodeString returns
        // the bytes decoded before the error.
        public static (slice<byte>, error) DecodeString(@string s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            slice<byte> src = (slice<byte>)s; 
            // We can use the source slice itself as the destination
            // because the decode loop increments by one and then the 'seen' byte is not used anymore.
            var (n, err) = Decode(src, src);
            return (src[..n], error.As(err)!);

        }

        // Dump returns a string that contains a hex dump of the given data. The format
        // of the hex dump matches the output of `hexdump -C` on the command line.
        public static @string Dump(slice<byte> data)
        {
            if (len(data) == 0L)
            {
                return "";
            }

            ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf); 
            // Dumper will write 79 bytes per complete 16 byte chunk, and at least
            // 64 bytes for whatever remains. Round the allocation up, since only a
            // maximum of 15 bytes will be wasted.
            buf.Grow((1L + ((len(data) - 1L) / 16L)) * 79L);

            var dumper = Dumper(_addr_buf);
            dumper.Write(data);
            dumper.Close();
            return buf.String();

        }

        // bufferSize is the number of hexadecimal characters to buffer in encoder and decoder.
        private static readonly long bufferSize = (long)1024L;



        private partial struct encoder
        {
            public io.Writer w;
            public error err;
            public array<byte> @out; // output buffer
        }

        // NewEncoder returns an io.Writer that writes lowercase hexadecimal characters to w.
        public static io.Writer NewEncoder(io.Writer w)
        {
            return addr(new encoder(w:w));
        }

        private static (long, error) Write(this ptr<encoder> _addr_e, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref encoder e = ref _addr_e.val;

            while (len(p) > 0L && e.err == null)
            {
                var chunkSize = bufferSize / 2L;
                if (len(p) < chunkSize)
                {
                    chunkSize = len(p);
                }

                long written = default;
                var encoded = Encode(e.@out[..], p[..chunkSize]);
                written, e.err = e.w.Write(e.@out[..encoded]);
                n += written / 2L;
                p = p[chunkSize..];

            }

            return (n, error.As(e.err)!);

        }

        private partial struct decoder
        {
            public io.Reader r;
            public error err;
            public slice<byte> @in; // input buffer (encoded form)
            public array<byte> arr; // backing array for in
        }

        // NewDecoder returns an io.Reader that decodes hexadecimal characters from r.
        // NewDecoder expects that r contain only an even number of hexadecimal characters.
        public static io.Reader NewDecoder(io.Reader r)
        {
            return addr(new decoder(r:r));
        }

        private static (long, error) Read(this ptr<decoder> _addr_d, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref decoder d = ref _addr_d.val;
 
            // Fill internal buffer with sufficient bytes to decode
            if (len(d.@in) < 2L && d.err == null)
            {
                long numCopy = default;                long numRead = default;

                numCopy = copy(d.arr[..], d.@in); // Copies either 0 or 1 bytes
                numRead, d.err = d.r.Read(d.arr[numCopy..]);
                d.@in = d.arr[..numCopy + numRead];
                if (d.err == io.EOF && len(d.@in) % 2L != 0L)
                {
                    {
                        var (_, ok) = fromHexChar(d.@in[len(d.@in) - 1L]);

                        if (!ok)
                        {
                            d.err = InvalidByteError(d.@in[len(d.@in) - 1L]);
                        }
                        else
                        {
                            d.err = io.ErrUnexpectedEOF;
                        }

                    }

                }

            } 

            // Decode internal buffer into output buffer
            {
                var numAvail = len(d.@in) / 2L;

                if (len(p) > numAvail)
                {
                    p = p[..numAvail];
                }

            }

            var (numDec, err) = Decode(p, d.@in[..len(p) * 2L]);
            d.@in = d.@in[2L * numDec..];
            if (err != null)
            {
                d.@in = null;
                d.err = err; // Decode error; discard input remainder
            }

            if (len(d.@in) < 2L)
            {
                return (numDec, error.As(d.err)!); // Only expose errors when buffer fully consumed
            }

            return (numDec, error.As(null!)!);

        }

        // Dumper returns a WriteCloser that writes a hex dump of all written data to
        // w. The format of the dump matches the output of `hexdump -C` on the command
        // line.
        public static io.WriteCloser Dumper(io.Writer w)
        {
            return addr(new dumper(w:w));
        }

        private partial struct dumper
        {
            public io.Writer w;
            public array<byte> rightChars;
            public array<byte> buf;
            public long used; // number of bytes in the current line
            public ulong n; // number of bytes, total
            public bool closed;
        }

        private static byte toChar(byte b)
        {
            if (b < 32L || b > 126L)
            {
                return '.';
            }

            return b;

        }

        private static (long, error) Write(this ptr<dumper> _addr_h, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref dumper h = ref _addr_h.val;

            if (h.closed)
            {
                return (0L, error.As(errors.New("encoding/hex: dumper closed"))!);
            } 

            // Output lines look like:
            // 00000010  2e 2f 30 31 32 33 34 35  36 37 38 39 3a 3b 3c 3d  |./0123456789:;<=|
            // ^ offset                          ^ extra space              ^ ASCII of line.
            foreach (var (i) in data)
            {
                if (h.used == 0L)
                { 
                    // At the beginning of a line we print the current
                    // offset in hex.
                    h.buf[0L] = byte(h.n >> (int)(24L));
                    h.buf[1L] = byte(h.n >> (int)(16L));
                    h.buf[2L] = byte(h.n >> (int)(8L));
                    h.buf[3L] = byte(h.n);
                    Encode(h.buf[4L..], h.buf[..4L]);
                    h.buf[12L] = ' ';
                    h.buf[13L] = ' ';
                    _, err = h.w.Write(h.buf[4L..]);
                    if (err != null)
                    {
                        return ;
                    }

                }

                Encode(h.buf[..], data[i..i + 1L]);
                h.buf[2L] = ' ';
                long l = 3L;
                if (h.used == 7L)
                { 
                    // There's an additional space after the 8th byte.
                    h.buf[3L] = ' ';
                    l = 4L;

                }
                else if (h.used == 15L)
                { 
                    // At the end of the line there's an extra space and
                    // the bar for the right column.
                    h.buf[3L] = ' ';
                    h.buf[4L] = '|';
                    l = 5L;

                }

                _, err = h.w.Write(h.buf[..l]);
                if (err != null)
                {
                    return ;
                }

                n++;
                h.rightChars[h.used] = toChar(data[i]);
                h.used++;
                h.n++;
                if (h.used == 16L)
                {
                    h.rightChars[16L] = '|';
                    h.rightChars[17L] = '\n';
                    _, err = h.w.Write(h.rightChars[..]);
                    if (err != null)
                    {
                        return ;
                    }

                    h.used = 0L;

                }

            }
            return ;

        }

        private static error Close(this ptr<dumper> _addr_h)
        {
            error err = default!;
            ref dumper h = ref _addr_h.val;
 
            // See the comments in Write() for the details of this format.
            if (h.closed)
            {
                return ;
            }

            h.closed = true;
            if (h.used == 0L)
            {
                return ;
            }

            h.buf[0L] = ' ';
            h.buf[1L] = ' ';
            h.buf[2L] = ' ';
            h.buf[3L] = ' ';
            h.buf[4L] = '|';
            var nBytes = h.used;
            while (h.used < 16L)
            {
                long l = 3L;
                if (h.used == 7L)
                {
                    l = 4L;
                }
                else if (h.used == 15L)
                {
                    l = 5L;
                }

                _, err = h.w.Write(h.buf[..l]);
                if (err != null)
                {
                    return ;
                }

                h.used++;

            }

            h.rightChars[nBytes] = '|';
            h.rightChars[nBytes + 1L] = '\n';
            _, err = h.w.Write(h.rightChars[..nBytes + 2L]);
            return ;

        }
    }
}}
