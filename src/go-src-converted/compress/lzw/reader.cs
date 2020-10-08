// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lzw implements the Lempel-Ziv-Welch compressed data format,
// described in T. A. Welch, ``A Technique for High-Performance Data
// Compression'', Computer, 17(6) (June 1984), pp 8-19.
//
// In particular, it implements LZW as used by the GIF and PDF file
// formats, which means variable-width codes up to 12 bits and the first
// two non-literal codes are a clear code and an EOF code.
//
// The TIFF file format uses a similar but incompatible version of the LZW
// algorithm. See the golang.org/x/image/tiff/lzw package for an
// implementation.
// package lzw -- go2cs converted at 2020 October 08 04:58:43 UTC
// import "compress/lzw" ==> using lzw = go.compress.lzw_package
// Original source: C:\Go\src\compress\lzw\reader.go
// TODO(nigeltao): check that PDF uses LZW in the same way as GIF,
// modulo LSB/MSB packing order.

using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;
using System;

namespace go {
namespace compress
{
    public static partial class lzw_package
    {
        // Order specifies the bit ordering in an LZW data stream.
        public partial struct Order // : long
        {
        }

 
        // LSB means Least Significant Bits first, as used in the GIF file format.
        public static readonly Order LSB = (Order)iota; 
        // MSB means Most Significant Bits first, as used in the TIFF and PDF
        // file formats.
        public static readonly var MSB = (var)0;


        private static readonly long maxWidth = (long)12L;
        private static readonly ulong decoderInvalidCode = (ulong)0xffffUL;
        private static readonly long flushBuffer = (long)1L << (int)(maxWidth);


        // decoder is the state from which the readXxx method converts a byte
        // stream into a code stream.
        private partial struct decoder
        {
            public io.ByteReader r;
            public uint bits;
            public ulong nBits;
            public ulong width;
            public Func<ptr<decoder>, (ushort, error)> read; // readLSB or readMSB
            public long litWidth; // width in bits of literal codes
            public error err; // The first 1<<litWidth codes are literal codes.
// The next two codes mean clear and EOF.
// Other valid codes are in the range [lo, hi] where lo := clear + 2,
// with the upper bound incrementing on each code seen.
//
// overflow is the code at which hi overflows the code width. It always
// equals 1 << width.
//
// last is the most recently seen code, or decoderInvalidCode.
//
// An invariant is that hi < overflow.
            public ushort clear; // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
//   suffix[c] is the last of these bytes.
//   prefix[c] is the code for all but the last byte.
//   This code can either be a literal code or another code in [lo, c).
// The c == hi case is a special case.
            public ushort eof; // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
//   suffix[c] is the last of these bytes.
//   prefix[c] is the code for all but the last byte.
//   This code can either be a literal code or another code in [lo, c).
// The c == hi case is a special case.
            public ushort hi; // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
//   suffix[c] is the last of these bytes.
//   prefix[c] is the code for all but the last byte.
//   This code can either be a literal code or another code in [lo, c).
// The c == hi case is a special case.
            public ushort overflow; // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
//   suffix[c] is the last of these bytes.
//   prefix[c] is the code for all but the last byte.
//   This code can either be a literal code or another code in [lo, c).
// The c == hi case is a special case.
            public ushort last; // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
//   suffix[c] is the last of these bytes.
//   prefix[c] is the code for all but the last byte.
//   This code can either be a literal code or another code in [lo, c).
// The c == hi case is a special case.
            public array<byte> suffix;
            public array<ushort> prefix; // output is the temporary output buffer.
// Literal codes are accumulated from the start of the buffer.
// Non-literal codes decode to a sequence of suffixes that are first
// written right-to-left from the end of the buffer before being copied
// to the start of the buffer.
// It is flushed when it contains >= 1<<maxWidth bytes,
// so that there is always room to decode an entire code.
            public array<byte> output;
            public long o; // write index into output
            public slice<byte> toRead; // bytes to return from Read
        }

        // readLSB returns the next code for "Least Significant Bits first" data.
        private static (ushort, error) readLSB(this ptr<decoder> _addr_d)
        {
            ushort _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            while (d.nBits < d.width)
            {
                var (x, err) = d.r.ReadByte();
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                d.bits |= uint32(x) << (int)(d.nBits);
                d.nBits += 8L;

            }

            var code = uint16(d.bits & (1L << (int)(d.width) - 1L));
            d.bits >>= d.width;
            d.nBits -= d.width;
            return (code, error.As(null!)!);

        }

        // readMSB returns the next code for "Most Significant Bits first" data.
        private static (ushort, error) readMSB(this ptr<decoder> _addr_d)
        {
            ushort _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            while (d.nBits < d.width)
            {
                var (x, err) = d.r.ReadByte();
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                d.bits |= uint32(x) << (int)((24L - d.nBits));
                d.nBits += 8L;

            }

            var code = uint16(d.bits >> (int)((32L - d.width)));
            d.bits <<= d.width;
            d.nBits -= d.width;
            return (code, error.As(null!)!);

        }

        private static (long, error) Read(this ptr<decoder> _addr_d, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            while (true)
            {
                if (len(d.toRead) > 0L)
                {
                    var n = copy(b, d.toRead);
                    d.toRead = d.toRead[n..];
                    return (n, error.As(null!)!);
                }

                if (d.err != null)
                {
                    return (0L, error.As(d.err)!);
                }

                d.decode();

            }


        }

        // decode decompresses bytes from r and leaves them in d.toRead.
        // read specifies how to decode bytes into codes.
        // litWidth is the width in bits of literal codes.
        private static void decode(this ptr<decoder> _addr_d) => func((_, panic, __) =>
        {
            ref decoder d = ref _addr_d.val;
 
            // Loop over the code stream, converting codes into decompressed bytes.
loop: 
            // Flush pending output.
            while (true)
            {
                var (code, err) = d.read(d);
                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }

                    d.err = err;
                    break;

                }


                if (code < d.clear) 
                    // We have a literal code.
                    d.output[d.o] = uint8(code);
                    d.o++;
                    if (d.last != decoderInvalidCode)
                    { 
                        // Save what the hi code expands to.
                        d.suffix[d.hi] = uint8(code);
                        d.prefix[d.hi] = d.last;

                    }

                else if (code == d.clear) 
                    d.width = 1L + uint(d.litWidth);
                    d.hi = d.eof;
                    d.overflow = 1L << (int)(d.width);
                    d.last = decoderInvalidCode;
                    continue;
                else if (code == d.eof) 
                    d.err = io.EOF;
                    _breakloop = true;
                    break;
                else if (code <= d.hi) 
                    var c = code;
                    var i = len(d.output) - 1L;
                    if (code == d.hi && d.last != decoderInvalidCode)
                    { 
                        // code == hi is a special case which expands to the last expansion
                        // followed by the head of the last expansion. To find the head, we walk
                        // the prefix chain until we find a literal code.
                        c = d.last;
                        while (c >= d.clear)
                        {
                            c = d.prefix[c];
                        }

                        d.output[i] = uint8(c);
                        i--;
                        c = d.last;

                    } 
                    // Copy the suffix chain into output and then write that to w.
                    while (c >= d.clear)
                    {
                        d.output[i] = d.suffix[c];
                        i--;
                        c = d.prefix[c];
                    }

                    d.output[i] = uint8(c);
                    d.o += copy(d.output[d.o..], d.output[i..]);
                    if (d.last != decoderInvalidCode)
                    { 
                        // Save what the hi code expands to.
                        d.suffix[d.hi] = uint8(c);
                        d.prefix[d.hi] = d.last;

                    }

                else 
                    d.err = errors.New("lzw: invalid code");
                    _breakloop = true;
                    break;
                                d.last = code;
                d.hi = d.hi + 1L;
                if (d.hi >= d.overflow)
                {
                    if (d.hi > d.overflow)
                    {
                        panic("unreachable");
                    }

                    if (d.width == maxWidth)
                    {
                        d.last = decoderInvalidCode; 
                        // Undo the d.hi++ a few lines above, so that (1) we maintain
                        // the invariant that d.hi < d.overflow, and (2) d.hi does not
                        // eventually overflow a uint16.
                        d.hi--;

                    }
                    else
                    {
                        d.width++;
                        d.overflow = 1L << (int)(d.width);
                    }

                }

                if (d.o >= flushBuffer)
                {
                    break;
                }

            } 
            // Flush pending output.
 
            // Flush pending output.
            d.toRead = d.output[..d.o];
            d.o = 0L;

        });

        private static var errClosed = errors.New("lzw: reader/writer is closed");

        private static error Close(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;

            d.err = errClosed; // in case any Reads come along
            return error.As(null!)!;

        }

        // NewReader creates a new io.ReadCloser.
        // Reads from the returned io.ReadCloser read and decompress data from r.
        // If r does not also implement io.ByteReader,
        // the decompressor may read more data than necessary from r.
        // It is the caller's responsibility to call Close on the ReadCloser when
        // finished reading.
        // The number of bits to use for literal codes, litWidth, must be in the
        // range [2,8] and is typically 8. It must equal the litWidth
        // used during compression.
        public static io.ReadCloser NewReader(io.Reader r, Order order, long litWidth)
        {
            ptr<decoder> d = @new<decoder>();

            if (order == LSB) 
                d.read = ptr<decoder>;
            else if (order == MSB) 
                d.read = ptr<decoder>;
            else 
                d.err = errors.New("lzw: unknown order");
                return d;
                        if (litWidth < 2L || 8L < litWidth)
            {
                d.err = fmt.Errorf("lzw: litWidth %d out of range", litWidth);
                return d;
            }

            {
                io.ByteReader (br, ok) = r._<io.ByteReader>();

                if (ok)
                {
                    d.r = br;
                }
                else
                {
                    d.r = bufio.NewReader(r);
                }

            }

            d.litWidth = litWidth;
            d.width = 1L + uint(litWidth);
            d.clear = uint16(1L) << (int)(uint(litWidth));
            d.eof = d.clear + 1L;
            d.hi = d.clear + 1L;
            d.overflow = uint16(1L) << (int)(d.width);
            d.last = decoderInvalidCode;

            return d;

        }
    }
}}
