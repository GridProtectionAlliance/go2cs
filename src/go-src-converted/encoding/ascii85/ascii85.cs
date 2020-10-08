// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ascii85 implements the ascii85 data encoding
// as used in the btoa tool and Adobe's PostScript and PDF document formats.
// package ascii85 -- go2cs converted at 2020 October 08 03:42:26 UTC
// import "encoding/ascii85" ==> using ascii85 = go.encoding.ascii85_package
// Original source: C:\Go\src\encoding\ascii85\ascii85.go
using io = go.io_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class ascii85_package
    {
        /*
         * Encoder
         */

        // Encode encodes src into at most MaxEncodedLen(len(src))
        // bytes of dst, returning the actual number of bytes written.
        //
        // The encoding handles 4-byte chunks, using a special encoding
        // for the last fragment, so Encode is not appropriate for use on
        // individual blocks of a large data stream. Use NewEncoder() instead.
        //
        // Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
        // Encode does not add these.
        public static long Encode(slice<byte> dst, slice<byte> src)
        {
            if (len(src) == 0L)
            {
                return 0L;
            }
            long n = 0L;
            while (len(src) > 0L)
            {
                dst[0L] = 0L;
                dst[1L] = 0L;
                dst[2L] = 0L;
                dst[3L] = 0L;
                dst[4L] = 0L; 

                // Unpack 4 bytes into uint32 to repack into base 85 5-byte.
                uint v = default;

                if (len(src) == 3L)
                {
                    v |= uint32(src[2L]) << (int)(8L);
                    fallthrough = true;
                }
                if (fallthrough || len(src) == 2L)
                {
                    v |= uint32(src[1L]) << (int)(16L);
                    fallthrough = true;
                }
                if (fallthrough || len(src) == 1L)
                {
                    v |= uint32(src[0L]) << (int)(24L);
                    goto __switch_break0;
                }
                // default: 
                    v |= uint32(src[3L]);

                __switch_break0:; 

                // Special case: zero (!!!!!) shortens to z.
                if (v == 0L && len(src) >= 4L)
                {
                    dst[0L] = 'z';
                    dst = dst[1L..];
                    src = src[4L..];
                    n++;
                    continue;
                }
                for (long i = 4L; i >= 0L; i--)
                {
                    dst[i] = '!' + byte(v % 85L);
                    v /= 85L;
                } 

                // If src was short, discard the low destination bytes.
                long m = 5L;
                if (len(src) < 4L)
                {
                    m -= 4L - len(src);
                    src = null;
                }
                else
                {
                    src = src[4L..];
                }
                dst = dst[m..];
                n += m;

            }
            return n;

        }

        // MaxEncodedLen returns the maximum length of an encoding of n source bytes.
        public static long MaxEncodedLen(long n)
        {
            return (n + 3L) / 4L * 5L;
        }

        // NewEncoder returns a new ascii85 stream encoder. Data written to
        // the returned writer will be encoded and then written to w.
        // Ascii85 encodings operate in 32-bit blocks; when finished
        // writing, the caller must Close the returned encoder to flush any
        // trailing partial block.
        public static io.WriteCloser NewEncoder(io.Writer w)
        {
            return addr(new encoder(w:w));
        }

        private partial struct encoder
        {
            public error err;
            public io.Writer w;
            public array<byte> buf; // buffered data waiting to be encoded
            public long nbuf; // number of bytes in buf
            public array<byte> @out; // output buffer
        }

        private static (long, error) Write(this ptr<encoder> _addr_e, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref encoder e = ref _addr_e.val;

            if (e.err != null)
            {
                return (0L, error.As(e.err)!);
            } 

            // Leading fringe.
            if (e.nbuf > 0L)
            {
                long i = default;
                for (i = 0L; i < len(p) && e.nbuf < 4L; i++)
                {
                    e.buf[e.nbuf] = p[i];
                    e.nbuf++;
                }

                n += i;
                p = p[i..];
                if (e.nbuf < 4L)
                {
                    return ;
                }

                var nout = Encode(e.@out[0L..], e.buf[0L..]);
                _, e.err = e.w.Write(e.@out[0L..nout]);

                if (e.err != null)
                {
                    return (n, error.As(e.err)!);
                }

                e.nbuf = 0L;

            } 

            // Large interior chunks.
            while (len(p) >= 4L)
            {
                var nn = len(e.@out) / 5L * 4L;
                if (nn > len(p))
                {
                    nn = len(p);
                }

                nn -= nn % 4L;
                if (nn > 0L)
                {
                    nout = Encode(e.@out[0L..], p[0L..nn]);
                    _, e.err = e.w.Write(e.@out[0L..nout]);

                    if (e.err != null)
                    {
                        return (n, error.As(e.err)!);
                    }

                }

                n += nn;
                p = p[nn..];

            } 

            // Trailing fringe.
 

            // Trailing fringe.
            {
                long i__prev1 = i;

                for (i = 0L; i < len(p); i++)
                {
                    e.buf[i] = p[i];
                }


                i = i__prev1;
            }
            e.nbuf = len(p);
            n += len(p);
            return ;

        }

        // Close flushes any pending output from the encoder.
        // It is an error to call Write after calling Close.
        private static error Close(this ptr<encoder> _addr_e)
        {
            ref encoder e = ref _addr_e.val;
 
            // If there's anything left in the buffer, flush it out
            if (e.err == null && e.nbuf > 0L)
            {
                var nout = Encode(e.@out[0L..], e.buf[0L..e.nbuf]);
                e.nbuf = 0L;
                _, e.err = e.w.Write(e.@out[0L..nout]);
            }

            return error.As(e.err)!;

        }

        /*
         * Decoder
         */

        public partial struct CorruptInputError // : long
        {
        }

        public static @string Error(this CorruptInputError e)
        {
            return "illegal ascii85 data at input byte " + strconv.FormatInt(int64(e), 10L);
        }

        // Decode decodes src into dst, returning both the number
        // of bytes written to dst and the number consumed from src.
        // If src contains invalid ascii85 data, Decode will return the
        // number of bytes successfully written and a CorruptInputError.
        // Decode ignores space and control characters in src.
        // Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
        // Decode expects these to have been stripped by the caller.
        //
        // If flush is true, Decode assumes that src represents the
        // end of the input stream and processes it completely rather
        // than wait for the completion of another 32-bit block.
        //
        // NewDecoder wraps an io.Reader interface around Decode.
        //
        public static (long, long, error) Decode(slice<byte> dst, slice<byte> src, bool flush)
        {
            long ndst = default;
            long nsrc = default;
            error err = default!;

            uint v = default;
            long nb = default;
            {
                var i__prev1 = i;

                foreach (var (__i, __b) in src)
                {
                    i = __i;
                    b = __b;
                    if (len(dst) - ndst < 4L)
                    {
                        return ;
                    }


                    if (b <= ' ') 
                        continue;
                    else if (b == 'z' && nb == 0L) 
                        nb = 5L;
                        v = 0L;
                    else if ('!' <= b && b <= 'u') 
                        v = v * 85L + uint32(b - '!');
                        nb++;
                    else 
                        return (0L, 0L, error.As(CorruptInputError(i))!);
                                        if (nb == 5L)
                    {
                        nsrc = i + 1L;
                        dst[ndst] = byte(v >> (int)(24L));
                        dst[ndst + 1L] = byte(v >> (int)(16L));
                        dst[ndst + 2L] = byte(v >> (int)(8L));
                        dst[ndst + 3L] = byte(v);
                        ndst += 4L;
                        nb = 0L;
                        v = 0L;
                    }

                }

                i = i__prev1;
            }

            if (flush)
            {
                nsrc = len(src);
                if (nb > 0L)
                { 
                    // The number of output bytes in the last fragment
                    // is the number of leftover input bytes - 1:
                    // the extra byte provides enough bits to cover
                    // the inefficiency of the encoding for the block.
                    if (nb == 1L)
                    {
                        return (0L, 0L, error.As(CorruptInputError(len(src)))!);
                    }

                    {
                        var i__prev1 = i;

                        for (var i = nb; i < 5L; i++)
                        { 
                            // The short encoding truncated the output value.
                            // We have to assume the worst case values (digit 84)
                            // in order to ensure that the top bits are correct.
                            v = v * 85L + 84L;

                        }


                        i = i__prev1;
                    }
                    {
                        var i__prev1 = i;

                        for (i = 0L; i < nb - 1L; i++)
                        {
                            dst[ndst] = byte(v >> (int)(24L));
                            v <<= 8L;
                            ndst++;
                        }


                        i = i__prev1;
                    }

                }

            }

            return ;

        }

        // NewDecoder constructs a new ascii85 stream decoder.
        public static io.Reader NewDecoder(io.Reader r)
        {
            return addr(new decoder(r:r));
        }

        private partial struct decoder
        {
            public error err;
            public error readErr;
            public io.Reader r;
            public array<byte> buf; // leftover input
            public long nbuf;
            public slice<byte> @out; // leftover decoded output
            public array<byte> outbuf;
        }

        private static (long, error) Read(this ptr<decoder> _addr_d, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref decoder d = ref _addr_d.val;

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            if (d.err != null)
            {
                return (0L, error.As(d.err)!);
            }

            while (true)
            { 
                // Copy leftover output from last decode.
                if (len(d.@out) > 0L)
                {
                    n = copy(p, d.@out);
                    d.@out = d.@out[n..];
                    return ;
                } 

                // Decode leftover input from last read.
                long nn = default;                long nsrc = default;                long ndst = default;

                if (d.nbuf > 0L)
                {
                    ndst, nsrc, d.err = Decode(d.outbuf[0L..], d.buf[0L..d.nbuf], d.readErr != null);
                    if (ndst > 0L)
                    {
                        d.@out = d.outbuf[0L..ndst];
                        d.nbuf = copy(d.buf[0L..], d.buf[nsrc..d.nbuf]);
                        continue; // copy out and return
                    }

                    if (ndst == 0L && d.err == null)
                    { 
                        // Special case: input buffer is mostly filled with non-data bytes.
                        // Filter out such bytes to make room for more input.
                        long off = 0L;
                        for (long i = 0L; i < d.nbuf; i++)
                        {
                            if (d.buf[i] > ' ')
                            {
                                d.buf[off] = d.buf[i];
                                off++;
                            }

                        }

                        d.nbuf = off;

                    }

                } 

                // Out of input, out of decoded output. Check errors.
                if (d.err != null)
                {
                    return (0L, error.As(d.err)!);
                }

                if (d.readErr != null)
                {
                    d.err = d.readErr;
                    return (0L, error.As(d.err)!);
                } 

                // Read more data.
                nn, d.readErr = d.r.Read(d.buf[d.nbuf..]);
                d.nbuf += nn;

            }


        }
    }
}}
