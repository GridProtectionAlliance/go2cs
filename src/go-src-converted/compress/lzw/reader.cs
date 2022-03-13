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

// package lzw -- go2cs converted at 2022 March 13 06:43:22 UTC
// import "compress/lzw" ==> using lzw = go.compress.lzw_package
// Original source: C:\Program Files\Go\src\compress\lzw\reader.go
namespace go.compress;
// TODO(nigeltao): check that PDF uses LZW in the same way as GIF,
// modulo LSB/MSB packing order.


using bufio = bufio_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;


// Order specifies the bit ordering in an LZW data stream.

using System;
public static partial class lzw_package {

public partial struct Order { // : nint
}

 
// LSB means Least Significant Bits first, as used in the GIF file format.
public static readonly Order LSB = iota; 
// MSB means Most Significant Bits first, as used in the TIFF and PDF
// file formats.
public static readonly var MSB = 0;

private static readonly nint maxWidth = 12;
private static readonly nuint decoderInvalidCode = 0xffff;
private static readonly nint flushBuffer = 1 << (int)(maxWidth);

// Reader is an io.Reader which can be used to read compressed data in the
// LZW format.
public partial struct Reader {
    public io.ByteReader r;
    public uint bits;
    public nuint nBits;
    public nuint width;
    public Func<ptr<Reader>, (ushort, error)> read; // readLSB or readMSB
    public nint litWidth; // width in bits of literal codes
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
    public nint o; // write index into output
    public slice<byte> toRead; // bytes to return from Read
}

// readLSB returns the next code for "Least Significant Bits first" data.
private static (ushort, error) readLSB(this ptr<Reader> _addr_r) {
    ushort _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    while (r.nBits < r.width) {
        var (x, err) = r.r.ReadByte();
        if (err != null) {
            return (0, error.As(err)!);
        }
        r.bits |= uint32(x) << (int)(r.nBits);
        r.nBits += 8;
    }
    var code = uint16(r.bits & (1 << (int)(r.width) - 1));
    r.bits>>=r.width;
    r.nBits -= r.width;
    return (code, error.As(null!)!);
}

// readMSB returns the next code for "Most Significant Bits first" data.
private static (ushort, error) readMSB(this ptr<Reader> _addr_r) {
    ushort _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    while (r.nBits < r.width) {
        var (x, err) = r.r.ReadByte();
        if (err != null) {
            return (0, error.As(err)!);
        }
        r.bits |= uint32(x) << (int)((24 - r.nBits));
        r.nBits += 8;
    }
    var code = uint16(r.bits >> (int)((32 - r.width)));
    r.bits<<=r.width;
    r.nBits -= r.width;
    return (code, error.As(null!)!);
}

// Read implements io.Reader, reading uncompressed bytes from its underlying Reader.
private static (nint, error) Read(this ptr<Reader> _addr_r, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    while (true) {
        if (len(r.toRead) > 0) {
            var n = copy(b, r.toRead);
            r.toRead = r.toRead[(int)n..];
            return (n, error.As(null!)!);
        }
        if (r.err != null) {
            return (0, error.As(r.err)!);
        }
        r.decode();
    }
}

// decode decompresses bytes from r and leaves them in d.toRead.
// read specifies how to decode bytes into codes.
// litWidth is the width in bits of literal codes.
private static void decode(this ptr<Reader> _addr_r) => func((_, panic, _) => {
    ref Reader r = ref _addr_r.val;
 
    // Loop over the code stream, converting codes into decompressed bytes.
loop: 
    // Flush pending output.
    while (true) {
        var (code, err) = r.read(r);
        if (err != null) {
            if (err == io.EOF) {
                err = io.ErrUnexpectedEOF;
            }
            r.err = err;
            break;
        }

        if (code < r.clear) 
            // We have a literal code.
            r.output[r.o] = uint8(code);
            r.o++;
            if (r.last != decoderInvalidCode) { 
                // Save what the hi code expands to.
                r.suffix[r.hi] = uint8(code);
                r.prefix[r.hi] = r.last;
            }
        else if (code == r.clear) 
            r.width = 1 + uint(r.litWidth);
            r.hi = r.eof;
            r.overflow = 1 << (int)(r.width);
            r.last = decoderInvalidCode;
            continue;
        else if (code == r.eof) 
            r.err = io.EOF;
            _breakloop = true;
            break;
        else if (code <= r.hi) 
            var c = code;
            var i = len(r.output) - 1;
            if (code == r.hi && r.last != decoderInvalidCode) { 
                // code == hi is a special case which expands to the last expansion
                // followed by the head of the last expansion. To find the head, we walk
                // the prefix chain until we find a literal code.
                c = r.last;
                while (c >= r.clear) {
                    c = r.prefix[c];
                }

                r.output[i] = uint8(c);
                i--;
                c = r.last;
            } 
            // Copy the suffix chain into output and then write that to w.
            while (c >= r.clear) {
                r.output[i] = r.suffix[c];
                i--;
                c = r.prefix[c];
            }

            r.output[i] = uint8(c);
            r.o += copy(r.output[(int)r.o..], r.output[(int)i..]);
            if (r.last != decoderInvalidCode) { 
                // Save what the hi code expands to.
                r.suffix[r.hi] = uint8(c);
                r.prefix[r.hi] = r.last;
            }
        else 
            r.err = errors.New("lzw: invalid code");
            _breakloop = true;
            break;
                (r.last, r.hi) = (code, r.hi + 1);        if (r.hi >= r.overflow) {
            if (r.hi > r.overflow) {
                panic("unreachable");
            }
            if (r.width == maxWidth) {
                r.last = decoderInvalidCode; 
                // Undo the d.hi++ a few lines above, so that (1) we maintain
                // the invariant that d.hi < d.overflow, and (2) d.hi does not
                // eventually overflow a uint16.
                r.hi--;
            }
            else
 {
                r.width++;
                r.overflow = 1 << (int)(r.width);
            }
        }
        if (r.o >= flushBuffer) {
            break;
        }
    } 
    // Flush pending output.
    r.toRead = r.output[..(int)r.o];
    r.o = 0;
});

private static var errClosed = errors.New("lzw: reader/writer is closed");

// Close closes the Reader and returns an error for any future read operation.
// It does not close the underlying io.Reader.
private static error Close(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    r.err = errClosed; // in case any Reads come along
    return error.As(null!)!;
}

// Reset clears the Reader's state and allows it to be reused again
// as a new Reader.
private static void Reset(this ptr<Reader> _addr_r, io.Reader src, Order order, nint litWidth) {
    ref Reader r = ref _addr_r.val;

    r.val = new Reader();
    r.init(src, order, litWidth);
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
//
// It is guaranteed that the underlying type of the returned io.ReadCloser
// is a *Reader.
public static io.ReadCloser NewReader(io.Reader r, Order order, nint litWidth) {
    return newReader(r, order, litWidth);
}

private static ptr<Reader> newReader(io.Reader src, Order order, nint litWidth) {
    ptr<Reader> r = @new<Reader>();
    r.init(src, order, litWidth);
    return _addr_r!;
}

private static void init(this ptr<Reader> _addr_r, io.Reader src, Order order, nint litWidth) {
    ref Reader r = ref _addr_r.val;


    if (order == LSB) 
        r.read = (Reader.val).readLSB;
    else if (order == MSB) 
        r.read = (Reader.val).readMSB;
    else 
        r.err = errors.New("lzw: unknown order");
        return ;
        if (litWidth < 2 || 8 < litWidth) {
        r.err = fmt.Errorf("lzw: litWidth %d out of range", litWidth);
        return ;
    }
    io.ByteReader (br, ok) = src._<io.ByteReader>();
    if (!ok && src != null) {
        br = bufio.NewReader(src);
    }
    r.r = br;
    r.litWidth = litWidth;
    r.width = 1 + uint(litWidth);
    r.clear = uint16(1) << (int)(uint(litWidth));
    (r.eof, r.hi) = (r.clear + 1, r.clear + 1);    r.overflow = uint16(1) << (int)(r.width);
    r.last = decoderInvalidCode;
}

} // end lzw_package
