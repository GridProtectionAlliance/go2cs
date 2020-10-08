// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base64 implements base64 encoding as specified by RFC 4648.
// package base64 -- go2cs converted at 2020 October 08 03:35:10 UTC
// import "encoding/base64" ==> using base64 = go.encoding.base64_package
// Original source: C:\Go\src\encoding\base64\base64.go
using binary = go.encoding.binary_package;
using io = go.io_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class base64_package
    {
        /*
         * Encodings
         */

        // An Encoding is a radix 64 encoding/decoding scheme, defined by a
        // 64-character alphabet. The most common encoding is the "base64"
        // encoding defined in RFC 4648 and used in MIME (RFC 2045) and PEM
        // (RFC 1421).  RFC 4648 also defines an alternate encoding, which is
        // the standard encoding with - and _ substituted for + and /.
        public partial struct Encoding
        {
            public array<byte> encode;
            public array<byte> decodeMap;
            public int padChar;
            public bool strict;
        }

        public static readonly int StdPadding = (int)'='; // Standard padding character
        public static readonly int NoPadding = (int)-1L; // No padding

        private static readonly @string encodeStd = (@string)"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        private static readonly @string encodeURL = (@string)"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        // NewEncoding returns a new padded Encoding defined by the given alphabet,
        // which must be a 64-byte string that does not contain the padding character
        // or CR / LF ('\r', '\n').
        // The resulting Encoding uses the default padding character ('='),
        // which may be changed or disabled via WithPadding.


        // NewEncoding returns a new padded Encoding defined by the given alphabet,
        // which must be a 64-byte string that does not contain the padding character
        // or CR / LF ('\r', '\n').
        // The resulting Encoding uses the default padding character ('='),
        // which may be changed or disabled via WithPadding.
        public static ptr<Encoding> NewEncoding(@string encoder) => func((_, panic, __) =>
        {
            if (len(encoder) != 64L)
            {
                panic("encoding alphabet is not 64-bytes long");
            }

            {
                long i__prev1 = i;

                for (long i = 0L; i < len(encoder); i++)
                {
                    if (encoder[i] == '\n' || encoder[i] == '\r')
                    {
                        panic("encoding alphabet contains newline character");
                    }

                }


                i = i__prev1;
            }

            ptr<Encoding> e = @new<Encoding>();
            e.padChar = StdPadding;
            copy(e.encode[..], encoder);

            {
                long i__prev1 = i;

                for (i = 0L; i < len(e.decodeMap); i++)
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

        // Strict creates a new encoding identical to enc except with
        // strict decoding enabled. In this mode, the decoder requires that
        // trailing padding bits are zero, as described in RFC 4648 section 3.5.
        //
        // Note that the input is still malleable, as new line characters
        // (CR and LF) are still ignored.
        public static ptr<Encoding> Strict(this Encoding enc)
        {
            enc.strict = true;
            return _addr__addr_enc!;
        }

        // StdEncoding is the standard base64 encoding, as defined in
        // RFC 4648.
        public static var StdEncoding = NewEncoding(encodeStd);

        // URLEncoding is the alternate base64 encoding defined in RFC 4648.
        // It is typically used in URLs and file names.
        public static var URLEncoding = NewEncoding(encodeURL);

        // RawStdEncoding is the standard raw, unpadded base64 encoding,
        // as defined in RFC 4648 section 3.2.
        // This is the same as StdEncoding but omits padding characters.
        public static var RawStdEncoding = StdEncoding.WithPadding(NoPadding);

        // RawURLEncoding is the unpadded alternate base64 encoding defined in RFC 4648.
        // It is typically used in URLs and file names.
        // This is the same as URLEncoding but omits padding characters.
        public static var RawURLEncoding = URLEncoding.WithPadding(NoPadding);

        /*
         * Encoder
         */

        // Encode encodes src using the encoding enc, writing
        // EncodedLen(len(src)) bytes to dst.
        //
        // The encoding pads the output to a multiple of 4 bytes,
        // so Encode is not appropriate for use on individual blocks
        // of a large data stream. Use NewEncoder() instead.
        private static void Encode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src)
        {
            ref Encoding enc = ref _addr_enc.val;

            if (len(src) == 0L)
            {
                return ;
            } 
            // enc is a pointer receiver, so the use of enc.encode within the hot
            // loop below means a nil check at every operation. Lift that nil check
            // outside of the loop to speed up the encoder.
            _ = enc.encode;

            long di = 0L;
            long si = 0L;
            var n = (len(src) / 3L) * 3L;
            while (si < n)
            { 
                // Convert 3x 8bit source bytes into 4 bytes
                var val = uint(src[si + 0L]) << (int)(16L) | uint(src[si + 1L]) << (int)(8L) | uint(src[si + 2L]);

                dst[di + 0L] = enc.encode[val >> (int)(18L) & 0x3FUL];
                dst[di + 1L] = enc.encode[val >> (int)(12L) & 0x3FUL];
                dst[di + 2L] = enc.encode[val >> (int)(6L) & 0x3FUL];
                dst[di + 3L] = enc.encode[val & 0x3FUL];

                si += 3L;
                di += 4L;

            }


            var remain = len(src) - si;
            if (remain == 0L)
            {
                return ;
            } 
            // Add the remaining small block
            val = uint(src[si + 0L]) << (int)(16L);
            if (remain == 2L)
            {
                val |= uint(src[si + 1L]) << (int)(8L);
            }

            dst[di + 0L] = enc.encode[val >> (int)(18L) & 0x3FUL];
            dst[di + 1L] = enc.encode[val >> (int)(12L) & 0x3FUL];

            switch (remain)
            {
                case 2L: 
                    dst[di + 2L] = enc.encode[val >> (int)(6L) & 0x3FUL];
                    if (enc.padChar != NoPadding)
                    {
                        dst[di + 3L] = byte(enc.padChar);
                    }

                    break;
                case 1L: 
                    if (enc.padChar != NoPadding)
                    {
                        dst[di + 2L] = byte(enc.padChar);
                        dst[di + 3L] = byte(enc.padChar);
                    }

                    break;
            }

        }

        // EncodeToString returns the base64 encoding of src.
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
                for (i = 0L; i < len(p) && e.nbuf < 3L; i++)
                {
                    e.buf[e.nbuf] = p[i];
                    e.nbuf++;
                }

                n += i;
                p = p[i..];
                if (e.nbuf < 3L)
                {
                    return ;
                }

                e.enc.Encode(e.@out[..], e.buf[..]);
                _, e.err = e.w.Write(e.@out[..4L]);

                if (e.err != null)
                {
                    return (n, error.As(e.err)!);
                }

                e.nbuf = 0L;

            } 

            // Large interior chunks.
            while (len(p) >= 3L)
            {
                var nn = len(e.@out) / 4L * 3L;
                if (nn > len(p))
                {
                    nn = len(p);
                    nn -= nn % 3L;
                }

                e.enc.Encode(e.@out[..], p[..nn]);
                _, e.err = e.w.Write(e.@out[0L..nn / 3L * 4L]);

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
                e.enc.Encode(e.@out[..], e.buf[..e.nbuf]);
                _, e.err = e.w.Write(e.@out[..e.enc.EncodedLen(e.nbuf)]);
                e.nbuf = 0L;
            }

            return error.As(e.err)!;

        }

        // NewEncoder returns a new base64 stream encoder. Data written to
        // the returned writer will be encoded using enc and then written to w.
        // Base64 encodings operate in 4-byte blocks; when finished
        // writing, the caller must Close the returned encoder to flush any
        // partially written blocks.
        public static io.WriteCloser NewEncoder(ptr<Encoding> _addr_enc, io.Writer w)
        {
            ref Encoding enc = ref _addr_enc.val;

            return addr(new encoder(enc:enc,w:w));
        }

        // EncodedLen returns the length in bytes of the base64 encoding
        // of an input buffer of length n.
        private static long EncodedLen(this ptr<Encoding> _addr_enc, long n)
        {
            ref Encoding enc = ref _addr_enc.val;

            if (enc.padChar == NoPadding)
            {
                return (n * 8L + 5L) / 6L; // minimum # chars at 6 bits per char
            }

            return (n + 2L) / 3L * 4L; // minimum # 4-char quanta, 3 bytes each
        }

        /*
         * Decoder
         */

        public partial struct CorruptInputError // : long
        {
        }

        public static @string Error(this CorruptInputError e)
        {
            return "illegal base64 data at input byte " + strconv.FormatInt(int64(e), 10L);
        }

        // decodeQuantum decodes up to 4 base64 bytes. The received parameters are
        // the destination buffer dst, the source buffer src and an index in the
        // source buffer si.
        // It returns the number of bytes read from src, the number of bytes written
        // to dst, and an error, if any.
        private static (long, long, error) decodeQuantum(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src, long si)
        {
            long nsi = default;
            long n = default;
            error err = default!;
            ref Encoding enc = ref _addr_enc.val;
 
            // Decode quantum using the base64 alphabet
            array<byte> dbuf = new array<byte>(4L);
            long dlen = 4L; 

            // Lift the nil check outside of the loop.
            _ = enc.decodeMap;

            for (long j = 0L; j < len(dbuf); j++)
            {
                if (len(src) == si)
                {

                    if (j == 0L) 
                        return (si, 0L, error.As(null!)!);
                    else if (j == 1L || enc.padChar != NoPadding) 
                        return (si, 0L, error.As(CorruptInputError(si - j))!);
                                        dlen = j;
                    break;

                }

                var @in = src[si];
                si++;

                var @out = enc.decodeMap[in];
                if (out != 0xffUL)
                {
                    dbuf[j] = out;
                    continue;
                }

                if (in == '\n' || in == '\r')
                {
                    j--;
                    continue;
                }

                if (rune(in) != enc.padChar)
                {
                    return (si, 0L, error.As(CorruptInputError(si - 1L))!);
                } 

                // We've reached the end and there's padding
                switch (j)
                {
                    case 0L: 
                        // incorrect padding

                    case 1L: 
                        // incorrect padding
                        return (si, 0L, error.As(CorruptInputError(si - 1L))!);
                        break;
                    case 2L: 
                        // "==" is expected, the first "=" is already consumed.
                        // skip over newlines
                        while (si < len(src) && (src[si] == '\n' || src[si] == '\r'))
                        {
                            si++;
                        }

                        if (si == len(src))
                        { 
                            // not enough padding
                            return (si, 0L, error.As(CorruptInputError(len(src)))!);

                        }

                        if (rune(src[si]) != enc.padChar)
                        { 
                            // incorrect padding
                            return (si, 0L, error.As(CorruptInputError(si - 1L))!);

                        }

                        si++;
                        break;
                } 

                // skip over newlines
                while (si < len(src) && (src[si] == '\n' || src[si] == '\r'))
                {
                    si++;
                }

                if (si < len(src))
                { 
                    // trailing garbage
                    err = CorruptInputError(si);

                }

                dlen = j;
                break;

            } 

            // Convert 4x 6bit source bytes into 3 bytes
 

            // Convert 4x 6bit source bytes into 3 bytes
            var val = uint(dbuf[0L]) << (int)(18L) | uint(dbuf[1L]) << (int)(12L) | uint(dbuf[2L]) << (int)(6L) | uint(dbuf[3L]);
            dbuf[2L] = byte(val >> (int)(0L));
            dbuf[1L] = byte(val >> (int)(8L));
            dbuf[0L] = byte(val >> (int)(16L));

            if (dlen == 4L)
            {
                dst[2L] = dbuf[2L];
                dbuf[2L] = 0L;
                fallthrough = true;
            }
            if (fallthrough || dlen == 3L)
            {
                dst[1L] = dbuf[1L];
                if (enc.strict && dbuf[2L] != 0L)
                {
                    return (si, 0L, error.As(CorruptInputError(si - 1L))!);
                }

                dbuf[1L] = 0L;
                fallthrough = true;
            }
            if (fallthrough || dlen == 2L)
            {
                dst[0L] = dbuf[0L];
                if (enc.strict && (dbuf[1L] != 0L || dbuf[2L] != 0L))
                {
                    return (si, 0L, error.As(CorruptInputError(si - 2L))!);
                }

                goto __switch_break0;
            }

            __switch_break0:;

            return (si, dlen - 1L, error.As(err)!);

        }

        // DecodeString returns the bytes represented by the base64 string s.
        private static (slice<byte>, error) DecodeString(this ptr<Encoding> _addr_enc, @string s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Encoding enc = ref _addr_enc.val;

            var dbuf = make_slice<byte>(enc.DecodedLen(len(s)));
            var (n, err) = enc.Decode(dbuf, (slice<byte>)s);
            return (dbuf[..n], error.As(err)!);
        }

        private partial struct decoder
        {
            public error err;
            public error readErr; // error from r.Read
            public ptr<Encoding> enc;
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
 
            // Use leftover decoded output from last read.
            if (len(d.@out) > 0L)
            {
                n = copy(p, d.@out);
                d.@out = d.@out[n..];
                return (n, error.As(null!)!);
            }

            if (d.err != null)
            {
                return (0L, error.As(d.err)!);
            } 

            // This code assumes that d.r strips supported whitespace ('\r' and '\n').

            // Refill buffer.
            while (d.nbuf < 4L && d.readErr == null)
            {
                var nn = len(p) / 3L * 4L;
                if (nn < 4L)
                {
                    nn = 4L;
                }

                if (nn > len(d.buf))
                {
                    nn = len(d.buf);
                }

                nn, d.readErr = d.r.Read(d.buf[d.nbuf..nn]);
                d.nbuf += nn;

            }


            if (d.nbuf < 4L)
            {
                if (d.enc.padChar == NoPadding && d.nbuf > 0L)
                { 
                    // Decode final fragment, without padding.
                    long nw = default;
                    nw, d.err = d.enc.Decode(d.outbuf[..], d.buf[..d.nbuf]);
                    d.nbuf = 0L;
                    d.@out = d.outbuf[..nw];
                    n = copy(p, d.@out);
                    d.@out = d.@out[n..];
                    if (n > 0L || len(p) == 0L && len(d.@out) > 0L)
                    {
                        return (n, error.As(null!)!);
                    }

                    if (d.err != null)
                    {
                        return (0L, error.As(d.err)!);
                    }

                }

                d.err = d.readErr;
                if (d.err == io.EOF && d.nbuf > 0L)
                {
                    d.err = io.ErrUnexpectedEOF;
                }

                return (0L, error.As(d.err)!);

            } 

            // Decode chunk into p, or d.out and then p if p is too small.
            var nr = d.nbuf / 4L * 4L;
            nw = d.nbuf / 4L * 3L;
            if (nw > len(p))
            {
                nw, d.err = d.enc.Decode(d.outbuf[..], d.buf[..nr]);
                d.@out = d.outbuf[..nw];
                n = copy(p, d.@out);
                d.@out = d.@out[n..];
            }
            else
            {
                n, d.err = d.enc.Decode(p, d.buf[..nr]);
            }

            d.nbuf -= nr;
            copy(d.buf[..d.nbuf], d.buf[nr..]);
            return (n, error.As(d.err)!);

        }

        // Decode decodes src using the encoding enc. It writes at most
        // DecodedLen(len(src)) bytes to dst and returns the number of bytes
        // written. If src contains invalid base64 data, it will return the
        // number of bytes successfully written and CorruptInputError.
        // New line characters (\r and \n) are ignored.
        private static (long, error) Decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src)
        {
            long n = default;
            error err = default!;
            ref Encoding enc = ref _addr_enc.val;

            if (len(src) == 0L)
            {
                return (0L, error.As(null!)!);
            } 

            // Lift the nil check outside of the loop. enc.decodeMap is directly
            // used later in this function, to let the compiler know that the
            // receiver can't be nil.
            _ = enc.decodeMap;

            long si = 0L;
            while (strconv.IntSize >= 64L && len(src) - si >= 8L && len(dst) - n >= 8L)
            {
                var src2 = src[si..si + 8L];
                {
                    var dn__prev1 = dn;

                    var (dn, ok) = assemble64(enc.decodeMap[src2[0L]], enc.decodeMap[src2[1L]], enc.decodeMap[src2[2L]], enc.decodeMap[src2[3L]], enc.decodeMap[src2[4L]], enc.decodeMap[src2[5L]], enc.decodeMap[src2[6L]], enc.decodeMap[src2[7L]]);

                    if (ok)
                    {
                        binary.BigEndian.PutUint64(dst[n..], dn);
                        n += 6L;
                        si += 8L;
                    }
                    else
                    {
                        long ninc = default;
                        si, ninc, err = enc.decodeQuantum(dst[n..], src, si);
                        n += ninc;
                        if (err != null)
                        {
                            return (n, error.As(err)!);
                        }

                    }

                    dn = dn__prev1;

                }

            }


            while (len(src) - si >= 4L && len(dst) - n >= 4L)
            {
                src2 = src[si..si + 4L];
                {
                    var dn__prev1 = dn;

                    (dn, ok) = assemble32(enc.decodeMap[src2[0L]], enc.decodeMap[src2[1L]], enc.decodeMap[src2[2L]], enc.decodeMap[src2[3L]]);

                    if (ok)
                    {
                        binary.BigEndian.PutUint32(dst[n..], dn);
                        n += 3L;
                        si += 4L;
                    }
                    else
                    {
                        ninc = default;
                        si, ninc, err = enc.decodeQuantum(dst[n..], src, si);
                        n += ninc;
                        if (err != null)
                        {
                            return (n, error.As(err)!);
                        }

                    }

                    dn = dn__prev1;

                }

            }


            while (si < len(src))
            {
                ninc = default;
                si, ninc, err = enc.decodeQuantum(dst[n..], src, si);
                n += ninc;
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

            }

            return (n, error.As(err)!);

        }

        // assemble32 assembles 4 base64 digits into 3 bytes.
        // Each digit comes from the decode map, and will be 0xff
        // if it came from an invalid character.
        private static (uint, bool) assemble32(byte n1, byte n2, byte n3, byte n4)
        {
            uint dn = default;
            bool ok = default;
 
            // Check that all the digits are valid. If any of them was 0xff, their
            // bitwise OR will be 0xff.
            if (n1 | n2 | n3 | n4 == 0xffUL)
            {
                return (0L, false);
            }

            return (uint32(n1) << (int)(26L) | uint32(n2) << (int)(20L) | uint32(n3) << (int)(14L) | uint32(n4) << (int)(8L), true);

        }

        // assemble64 assembles 8 base64 digits into 6 bytes.
        // Each digit comes from the decode map, and will be 0xff
        // if it came from an invalid character.
        private static (ulong, bool) assemble64(byte n1, byte n2, byte n3, byte n4, byte n5, byte n6, byte n7, byte n8)
        {
            ulong dn = default;
            bool ok = default;
 
            // Check that all the digits are valid. If any of them was 0xff, their
            // bitwise OR will be 0xff.
            if (n1 | n2 | n3 | n4 | n5 | n6 | n7 | n8 == 0xffUL)
            {
                return (0L, false);
            }

            return (uint64(n1) << (int)(58L) | uint64(n2) << (int)(52L) | uint64(n3) << (int)(46L) | uint64(n4) << (int)(40L) | uint64(n5) << (int)(34L) | uint64(n6) << (int)(28L) | uint64(n7) << (int)(22L) | uint64(n8) << (int)(16L), true);

        }

        private partial struct newlineFilteringReader
        {
            public io.Reader wrapped;
        }

        private static (long, error) Read(this ptr<newlineFilteringReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref newlineFilteringReader r = ref _addr_r.val;

            var (n, err) = r.wrapped.Read(p);
            while (n > 0L)
            {
                long offset = 0L;
                foreach (var (i, b) in p[..n])
                {
                    if (b != '\r' && b != '\n')
                    {
                        if (i != offset)
                        {
                            p[offset] = b;
                        }

                        offset++;

                    }

                }
                if (offset > 0L)
                {
                    return (offset, error.As(err)!);
                } 
                // Previous buffer entirely whitespace, read again
                n, err = r.wrapped.Read(p);

            }

            return (n, error.As(err)!);

        }

        // NewDecoder constructs a new base64 stream decoder.
        public static io.Reader NewDecoder(ptr<Encoding> _addr_enc, io.Reader r)
        {
            ref Encoding enc = ref _addr_enc.val;

            return addr(new decoder(enc:enc,r:&newlineFilteringReader{r}));
        }

        // DecodedLen returns the maximum length in bytes of the decoded data
        // corresponding to n bytes of base64-encoded data.
        private static long DecodedLen(this ptr<Encoding> _addr_enc, long n)
        {
            ref Encoding enc = ref _addr_enc.val;

            if (enc.padChar == NoPadding)
            { 
                // Unpadded data may end with partial block of 2-3 characters.
                return n * 6L / 8L;

            } 
            // Padded base64 should always be a multiple of 4 characters in length.
            return n / 4L * 3L;

        }
    }
}}
