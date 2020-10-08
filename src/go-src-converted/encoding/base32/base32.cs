// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base32 implements base32 encoding as specified by RFC 4648.
// package base32 -- go2cs converted at 2020 October 08 03:42:28 UTC
// import "encoding/base32" ==> using base32 = go.encoding.base32_package
// Original source: C:\Go\src\encoding\base32\base32.go
using io = go.io_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class base32_package
    {
        /*
         * Encodings
         */

        // An Encoding is a radix 32 encoding/decoding scheme, defined by a
        // 32-character alphabet. The most common is the "base32" encoding
        // introduced for SASL GSSAPI and standardized in RFC 4648.
        // The alternate "base32hex" encoding is used in DNSSEC.
        public partial struct Encoding
        {
            public array<byte> encode;
            public array<byte> decodeMap;
            public int padChar;
        }

        public static readonly int StdPadding = (int)'='; // Standard padding character
        public static readonly int NoPadding = (int)-1L; // No padding

        private static readonly @string encodeStd = (@string)"ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static readonly @string encodeHex = (@string)"0123456789ABCDEFGHIJKLMNOPQRSTUV";

        // NewEncoding returns a new Encoding defined by the given alphabet,
        // which must be a 32-byte string.


        // NewEncoding returns a new Encoding defined by the given alphabet,
        // which must be a 32-byte string.
        public static ptr<Encoding> NewEncoding(@string encoder) => func((_, panic, __) =>
        {
            if (len(encoder) != 32L)
            {
                panic("encoding alphabet is not 32-bytes long");
            }

            ptr<Encoding> e = @new<Encoding>();
            copy(e.encode[..], encoder);
            e.padChar = StdPadding;

            {
                long i__prev1 = i;

                for (long i = 0L; i < len(e.decodeMap); i++)
                {
                    e.decodeMap[i] = 0xFFUL;
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < len(encoder); i++)
                {
                    e.decodeMap[encoder[i]] = byte(i);
                }


                i = i__prev1;
            }
            return _addr_e!;

        });

        // StdEncoding is the standard base32 encoding, as defined in
        // RFC 4648.
        public static var StdEncoding = NewEncoding(encodeStd);

        // HexEncoding is the ``Extended Hex Alphabet'' defined in RFC 4648.
        // It is typically used in DNS.
        public static var HexEncoding = NewEncoding(encodeHex);

        // WithPadding creates a new encoding identical to enc except
        // with a specified padding character, or NoPadding to disable padding.
        // The padding character must not be '\r' or '\n', must not
        // be contained in the encoding's alphabet and must be a rune equal or
        // below '\xff'.
        public static ptr<Encoding> WithPadding(this Encoding enc, int padding) => func((_, panic, __) =>
        {
            if (padding == '\r' || padding == '\n' || padding > 0xffUL)
            {
                panic("invalid padding");
            }

            for (long i = 0L; i < len(enc.encode); i++)
            {
                if (rune(enc.encode[i]) == padding)
                {
                    panic("padding contained in alphabet");
                }

            }


            enc.padChar = padding;
            return _addr__addr_enc!;

        });

        /*
         * Encoder
         */

        // Encode encodes src using the encoding enc, writing
        // EncodedLen(len(src)) bytes to dst.
        //
        // The encoding pads the output to a multiple of 8 bytes,
        // so Encode is not appropriate for use on individual blocks
        // of a large data stream. Use NewEncoder() instead.
        private static void Encode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src)
        {
            ref Encoding enc = ref _addr_enc.val;

            while (len(src) > 0L)
            {
                array<byte> b = new array<byte>(8L); 

                // Unpack 8x 5-bit source blocks into a 5 byte
                // destination quantum

                if (len(src) == 4L)
                {
                    b[6L] |= (src[3L] << (int)(3L)) & 0x1FUL;
                    b[5L] = (src[3L] >> (int)(2L)) & 0x1FUL;
                    b[4L] = src[3L] >> (int)(7L);
                    fallthrough = true;
                }
                if (fallthrough || len(src) == 3L)
                {
                    b[4L] |= (src[2L] << (int)(1L)) & 0x1FUL;
                    b[3L] = (src[2L] >> (int)(4L)) & 0x1FUL;
                    fallthrough = true;
                }
                if (fallthrough || len(src) == 2L)
                {
                    b[3L] |= (src[1L] << (int)(4L)) & 0x1FUL;
                    b[2L] = (src[1L] >> (int)(1L)) & 0x1FUL;
                    b[1L] = (src[1L] >> (int)(6L)) & 0x1FUL;
                    fallthrough = true;
                }
                if (fallthrough || len(src) == 1L)
                {
                    b[1L] |= (src[0L] << (int)(2L)) & 0x1FUL;
                    b[0L] = src[0L] >> (int)(3L);
                    goto __switch_break0;
                }
                // default: 
                    b[7L] = src[4L] & 0x1FUL;
                    b[6L] = src[4L] >> (int)(5L);

                __switch_break0:; 

                // Encode 5-bit blocks using the base32 alphabet
                var size = len(dst);
                if (size >= 8L)
                { 
                    // Common case, unrolled for extra performance
                    dst[0L] = enc.encode[b[0L] & 31L];
                    dst[1L] = enc.encode[b[1L] & 31L];
                    dst[2L] = enc.encode[b[2L] & 31L];
                    dst[3L] = enc.encode[b[3L] & 31L];
                    dst[4L] = enc.encode[b[4L] & 31L];
                    dst[5L] = enc.encode[b[5L] & 31L];
                    dst[6L] = enc.encode[b[6L] & 31L];
                    dst[7L] = enc.encode[b[7L] & 31L];

                }
                else
                {
                    for (long i = 0L; i < size; i++)
                    {
                        dst[i] = enc.encode[b[i] & 31L];
                    }


                } 

                // Pad the final quantum
                if (len(src) < 5L)
                {
                    if (enc.padChar == NoPadding)
                    {
                        break;
                    }

                    dst[7L] = byte(enc.padChar);
                    if (len(src) < 4L)
                    {
                        dst[6L] = byte(enc.padChar);
                        dst[5L] = byte(enc.padChar);
                        if (len(src) < 3L)
                        {
                            dst[4L] = byte(enc.padChar);
                            if (len(src) < 2L)
                            {
                                dst[3L] = byte(enc.padChar);
                                dst[2L] = byte(enc.padChar);
                            }

                        }

                    }

                    break;

                }

                src = src[5L..];
                dst = dst[8L..];

            }


        }

        // EncodeToString returns the base32 encoding of src.
        private static @string EncodeToString(this ptr<Encoding> _addr_enc, slice<byte> src)
        {
            ref Encoding enc = ref _addr_enc.val;

            var buf = make_slice<byte>(enc.EncodedLen(len(src)));
            enc.Encode(buf, src);
            return string(buf);
        }

        private partial struct encoder
        {
            public error err;
            public ptr<Encoding> enc;
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
                for (i = 0L; i < len(p) && e.nbuf < 5L; i++)
                {
                    e.buf[e.nbuf] = p[i];
                    e.nbuf++;
                }

                n += i;
                p = p[i..];
                if (e.nbuf < 5L)
                {
                    return ;
                }

                e.enc.Encode(e.@out[0L..], e.buf[0L..]);
                _, e.err = e.w.Write(e.@out[0L..8L]);

                if (e.err != null)
                {
                    return (n, error.As(e.err)!);
                }

                e.nbuf = 0L;

            } 

            // Large interior chunks.
            while (len(p) >= 5L)
            {
                var nn = len(e.@out) / 8L * 5L;
                if (nn > len(p))
                {
                    nn = len(p);
                    nn -= nn % 5L;
                }

                e.enc.Encode(e.@out[0L..], p[0L..nn]);
                _, e.err = e.w.Write(e.@out[0L..nn / 5L * 8L]);

                if (e.err != null)
                {
                    return (n, error.As(e.err)!);
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
                e.enc.Encode(e.@out[0L..], e.buf[0L..e.nbuf]);
                var encodedLen = e.enc.EncodedLen(e.nbuf);
                e.nbuf = 0L;
                _, e.err = e.w.Write(e.@out[0L..encodedLen]);
            }

            return error.As(e.err)!;

        }

        // NewEncoder returns a new base32 stream encoder. Data written to
        // the returned writer will be encoded using enc and then written to w.
        // Base32 encodings operate in 5-byte blocks; when finished
        // writing, the caller must Close the returned encoder to flush any
        // partially written blocks.
        public static io.WriteCloser NewEncoder(ptr<Encoding> _addr_enc, io.Writer w)
        {
            ref Encoding enc = ref _addr_enc.val;

            return addr(new encoder(enc:enc,w:w));
        }

        // EncodedLen returns the length in bytes of the base32 encoding
        // of an input buffer of length n.
        private static long EncodedLen(this ptr<Encoding> _addr_enc, long n)
        {
            ref Encoding enc = ref _addr_enc.val;

            if (enc.padChar == NoPadding)
            {
                return (n * 8L + 4L) / 5L;
            }

            return (n + 4L) / 5L * 8L;

        }

        /*
         * Decoder
         */

        public partial struct CorruptInputError // : long
        {
        }

        public static @string Error(this CorruptInputError e)
        {
            return "illegal base32 data at input byte " + strconv.FormatInt(int64(e), 10L);
        }

        // decode is like Decode but returns an additional 'end' value, which
        // indicates if end-of-message padding was encountered and thus any
        // additional data is an error. This method assumes that src has been
        // stripped of all supported whitespace ('\r' and '\n').
        private static (long, bool, error) decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src)
        {
            long n = default;
            bool end = default;
            error err = default!;
            ref Encoding enc = ref _addr_enc.val;
 
            // Lift the nil check outside of the loop.
            _ = enc.decodeMap;

            long dsti = 0L;
            var olen = len(src);

            while (len(src) > 0L && !end)
            { 
                // Decode quantum using the base32 alphabet
                array<byte> dbuf = new array<byte>(8L);
                long dlen = 8L;

                {
                    long j = 0L;

                    while (j < 8L)
                    {
                        if (len(src) == 0L)
                        {
                            if (enc.padChar != NoPadding)
                            { 
                                // We have reached the end and are missing padding
                                return (n, false, error.As(CorruptInputError(olen - len(src) - j))!);

                            } 
                            // We have reached the end and are not expecting any padding
                            dlen = j;
                            end = true;
                            break;

                        }

                        var @in = src[0L];
                        src = src[1L..];
                        if (in == byte(enc.padChar) && j >= 2L && len(src) < 8L)
                        { 
                            // We've reached the end and there's padding
                            if (len(src) + j < 8L - 1L)
                            { 
                                // not enough padding
                                return (n, false, error.As(CorruptInputError(olen))!);

                            }

                            for (long k = 0L; k < 8L - 1L - j; k++)
                            {
                                if (len(src) > k && src[k] != byte(enc.padChar))
                                { 
                                    // incorrect padding
                                    return (n, false, error.As(CorruptInputError(olen - len(src) + k - 1L))!);

                                }

                            }

                            dlen = j;
                            end = true; 
                            // 7, 5 and 2 are not valid padding lengths, and so 1, 3 and 6 are not
                            // valid dlen values. See RFC 4648 Section 6 "Base 32 Encoding" listing
                            // the five valid padding lengths, and Section 9 "Illustrations and
                            // Examples" for an illustration for how the 1st, 3rd and 6th base32
                            // src bytes do not yield enough information to decode a dst byte.
                            if (dlen == 1L || dlen == 3L || dlen == 6L)
                            {
                                return (n, false, error.As(CorruptInputError(olen - len(src) - 1L))!);
                            }

                            break;

                        }

                        dbuf[j] = enc.decodeMap[in];
                        if (dbuf[j] == 0xFFUL)
                        {
                            return (n, false, error.As(CorruptInputError(olen - len(src) - 1L))!);
                        }

                        j++;

                    } 

                    // Pack 8x 5-bit source blocks into 5 byte destination
                    // quantum

                } 

                // Pack 8x 5-bit source blocks into 5 byte destination
                // quantum

                if (dlen == 8L)
                {
                    dst[dsti + 4L] = dbuf[6L] << (int)(5L) | dbuf[7L];
                    n++;
                    fallthrough = true;
                }
                if (fallthrough || dlen == 7L)
                {
                    dst[dsti + 3L] = dbuf[4L] << (int)(7L) | dbuf[5L] << (int)(2L) | dbuf[6L] >> (int)(3L);
                    n++;
                    fallthrough = true;
                }
                if (fallthrough || dlen == 5L)
                {
                    dst[dsti + 2L] = dbuf[3L] << (int)(4L) | dbuf[4L] >> (int)(1L);
                    n++;
                    fallthrough = true;
                }
                if (fallthrough || dlen == 4L)
                {
                    dst[dsti + 1L] = dbuf[1L] << (int)(6L) | dbuf[2L] << (int)(1L) | dbuf[3L] >> (int)(4L);
                    n++;
                    fallthrough = true;
                }
                if (fallthrough || dlen == 2L)
                {
                    dst[dsti + 0L] = dbuf[0L] << (int)(3L) | dbuf[1L] >> (int)(2L);
                    n++;
                    goto __switch_break1;
                }

                __switch_break1:;
                dsti += 5L;

            }

            return (n, end, error.As(null!)!);

        }

        // Decode decodes src using the encoding enc. It writes at most
        // DecodedLen(len(src)) bytes to dst and returns the number of bytes
        // written. If src contains invalid base32 data, it will return the
        // number of bytes successfully written and CorruptInputError.
        // New line characters (\r and \n) are ignored.
        private static (long, error) Decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src)
        {
            long n = default;
            error err = default!;
            ref Encoding enc = ref _addr_enc.val;

            var buf = make_slice<byte>(len(src));
            var l = stripNewlines(buf, src);
            n, _, err = enc.decode(dst, buf[..l]);
            return ;
        }

        // DecodeString returns the bytes represented by the base32 string s.
        private static (slice<byte>, error) DecodeString(this ptr<Encoding> _addr_enc, @string s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Encoding enc = ref _addr_enc.val;

            slice<byte> buf = (slice<byte>)s;
            var l = stripNewlines(buf, buf);
            var (n, _, err) = enc.decode(buf, buf[..l]);
            return (buf[..n], error.As(err)!);
        }

        private partial struct decoder
        {
            public error err;
            public ptr<Encoding> enc;
            public io.Reader r;
            public bool end; // saw end of message
            public array<byte> buf; // leftover input
            public long nbuf;
            public slice<byte> @out; // leftover decoded output
            public array<byte> outbuf;
        }

        private static (long, error) readEncodedData(io.Reader r, slice<byte> buf, long min, bool expectsPadding)
        {
            long n = default;
            error err = default!;

            while (n < min && err == null)
            {
                long nn = default;
                nn, err = r.Read(buf[n..]);
                n += nn;
            } 
            // data was read, less than min bytes could be read
 
            // data was read, less than min bytes could be read
            if (n < min && n > 0L && err == io.EOF)
            {
                err = io.ErrUnexpectedEOF;
            } 
            // no data was read, the buffer already contains some data
            // when padding is disabled this is not an error, as the message can be of
            // any length
            if (expectsPadding && min < 8L && n == 0L && err == io.EOF)
            {
                err = io.ErrUnexpectedEOF;
            }

            return ;

        }

        private static (long, error) Read(this ptr<decoder> _addr_d, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref decoder d = ref _addr_d.val;
 
            // Use leftover decoded output from last read.
            if (len(d.@out) > 0L)
            {
                n = copy(p, d.@out);
                d.@out = d.@out[n..];
                if (len(d.@out) == 0L)
                {
                    return (n, error.As(d.err)!);
                }

                return (n, error.As(null!)!);

            }

            if (d.err != null)
            {
                return (0L, error.As(d.err)!);
            } 

            // Read a chunk.
            var nn = len(p) / 5L * 8L;
            if (nn < 8L)
            {
                nn = 8L;
            }

            if (nn > len(d.buf))
            {
                nn = len(d.buf);
            } 

            // Minimum amount of bytes that needs to be read each cycle
            long min = default;
            bool expectsPadding = default;
            if (d.enc.padChar == NoPadding)
            {
                min = 1L;
                expectsPadding = false;
            }
            else
            {
                min = 8L - d.nbuf;
                expectsPadding = true;
            }

            nn, d.err = readEncodedData(d.r, d.buf[d.nbuf..nn], min, expectsPadding);
            d.nbuf += nn;
            if (d.nbuf < min)
            {
                return (0L, error.As(d.err)!);
            } 

            // Decode chunk into p, or d.out and then p if p is too small.
            long nr = default;
            if (d.enc.padChar == NoPadding)
            {
                nr = d.nbuf;
            }
            else
            {
                nr = d.nbuf / 8L * 8L;
            }

            var nw = d.enc.DecodedLen(d.nbuf);

            if (nw > len(p))
            {
                nw, d.end, err = d.enc.decode(d.outbuf[0L..], d.buf[0L..nr]);
                d.@out = d.outbuf[0L..nw];
                n = copy(p, d.@out);
                d.@out = d.@out[n..];
            }
            else
            {
                n, d.end, err = d.enc.decode(p, d.buf[0L..nr]);
            }

            d.nbuf -= nr;
            for (long i = 0L; i < d.nbuf; i++)
            {
                d.buf[i] = d.buf[i + nr];
            }


            if (err != null && (d.err == null || d.err == io.EOF))
            {
                d.err = err;
            }

            if (len(d.@out) > 0L)
            { 
                // We cannot return all the decoded bytes to the caller in this
                // invocation of Read, so we return a nil error to ensure that Read
                // will be called again.  The error stored in d.err, if any, will be
                // returned with the last set of decoded bytes.
                return (n, error.As(null!)!);

            }

            return (n, error.As(d.err)!);

        }

        private partial struct newlineFilteringReader
        {
            public io.Reader wrapped;
        }

        // stripNewlines removes newline characters and returns the number
        // of non-newline characters copied to dst.
        private static long stripNewlines(slice<byte> dst, slice<byte> src)
        {
            long offset = 0L;
            foreach (var (_, b) in src)
            {
                if (b == '\r' || b == '\n')
                {
                    continue;
                }

                dst[offset] = b;
                offset++;

            }
            return offset;

        }

        private static (long, error) Read(this ptr<newlineFilteringReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref newlineFilteringReader r = ref _addr_r.val;

            var (n, err) = r.wrapped.Read(p);
            while (n > 0L)
            {
                var s = p[0L..n];
                var offset = stripNewlines(s, s);
                if (err != null || offset > 0L)
                {
                    return (offset, error.As(err)!);
                } 
                // Previous buffer entirely whitespace, read again
                n, err = r.wrapped.Read(p);

            }

            return (n, error.As(err)!);

        }

        // NewDecoder constructs a new base32 stream decoder.
        public static io.Reader NewDecoder(ptr<Encoding> _addr_enc, io.Reader r)
        {
            ref Encoding enc = ref _addr_enc.val;

            return addr(new decoder(enc:enc,r:&newlineFilteringReader{r}));
        }

        // DecodedLen returns the maximum length in bytes of the decoded data
        // corresponding to n bytes of base32-encoded data.
        private static long DecodedLen(this ptr<Encoding> _addr_enc, long n)
        {
            ref Encoding enc = ref _addr_enc.val;

            if (enc.padChar == NoPadding)
            {
                return n * 5L / 8L;
            }

            return n / 8L * 5L;

        }
    }
}}
