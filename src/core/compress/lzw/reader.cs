// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lzw implements the Lempel-Ziv-Welch compressed data format,
// described in T. A. Welch, “A Technique for High-Performance Data
// Compression”, Computer, 17(6) (June 1984), pp 8-19.
//
// In particular, it implements LZW as used by the GIF and PDF file
// formats, which means variable-width codes up to 12 bits and the first
// two non-literal codes are a clear code and an EOF code.
//
// The TIFF file format uses a similar but incompatible version of the LZW
// algorithm. See the golang.org/x/image/tiff/lzw package for an
// implementation.
namespace go.compress;

// TODO(nigeltao): check that PDF uses LZW in the same way as GIF,
// modulo LSB/MSB packing order.
using bufio = bufio_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;

partial class lzw_package {

[GoType("num:nint")] partial struct Order;

public static readonly Order LSB = /* iota */ 0;
public static readonly Order MSB = 1;

internal static readonly UntypedInt maxWidth = 12;
internal static readonly UntypedInt decoderInvalidCode = /* 0xffff */ 65535;
internal static readonly UntypedInt flushBuffer = /* 1 << maxWidth */ 4096;

// Reader is an io.Reader which can be used to read compressed data in the
// LZW format.
[GoType] partial struct Reader {
    internal io_package.ByteReader r;
    internal uint32 bits;
    internal nuint nBits;
    internal nuint width;
    internal Func<ж<Reader>, (uint16, error)> read; // readLSB or readMSB
    internal nint litWidth;                          // width in bits of literal codes
    internal error err;
    // The first 1<<litWidth codes are literal codes.
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
    internal uint16 clear;
    internal uint16 eof;
    internal uint16 hi;
    internal uint16 overflow;
    internal uint16 last;
    // Each code c in [lo, hi] expands to two or more bytes. For c != hi:
    //   suffix[c] is the last of these bytes.
    //   prefix[c] is the code for all but the last byte.
    //   This code can either be a literal code or another code in [lo, c).
    // The c == hi case is a special case.
    internal array<uint8> suffix = new(1 << (int)(maxWidth));
    internal array<uint16> prefix = new(1 << (int)(maxWidth));
    // output is the temporary output buffer.
    // Literal codes are accumulated from the start of the buffer.
    // Non-literal codes decode to a sequence of suffixes that are first
    // written right-to-left from the end of the buffer before being copied
    // to the start of the buffer.
    // It is flushed when it contains >= 1<<maxWidth bytes,
    // so that there is always room to decode an entire code.
    internal array<byte> output = new(2 * 1 << (int)(maxWidth));
    internal nint o;   // write index into output
    internal slice<byte> toRead; // bytes to return from Read
}

// readLSB returns the next code for "Least Significant Bits first" data.
[GoRecv] internal static (uint16, error) readLSB(this ref Reader r) {
    while (r.nBits < r.width) {
        var (x, err) = r.r.ReadByte();
        if (err != default!) {
            return (0, err);
        }
        r.bits |= (uint32)(((uint32)x) << (int)(r.nBits));
        r.nBits += 8;
    }
    var code = ((uint16)((uint32)(r.bits & (1 << (int)(r.width) - 1))));
    r.bits >>= (nuint)(r.width);
    r.nBits -= r.width;
    return (code, default!);
}

// readMSB returns the next code for "Most Significant Bits first" data.
[GoRecv] internal static (uint16, error) readMSB(this ref Reader r) {
    while (r.nBits < r.width) {
        var (x, err) = r.r.ReadByte();
        if (err != default!) {
            return (0, err);
        }
        r.bits |= (uint32)(((uint32)x) << (int)((24 - r.nBits)));
        r.nBits += 8;
    }
    var code = ((uint16)(r.bits >> (int)((32 - r.width))));
    r.bits <<= (nuint)(r.width);
    r.nBits -= r.width;
    return (code, default!);
}

// Read implements io.Reader, reading uncompressed bytes from its underlying [Reader].
[GoRecv] public static (nint, error) Read(this ref Reader r, slice<byte> b) {
    while (ᐧ) {
        if (len(r.toRead) > 0) {
            nint n = copy(b, r.toRead);
            r.toRead = r.toRead[(int)(n)..];
            return (n, default!);
        }
        if (r.err != default!) {
            return (0, r.err);
        }
        r.decode();
    }
}

// decode decompresses bytes from r and leaves them in d.toRead.
// read specifies how to decode bytes into codes.
// litWidth is the width in bits of literal codes.
[GoRecv] internal static void decode(this ref Reader r) {
    // Loop over the code stream, converting codes into decompressed bytes.
loop:
    while (ᐧ) {
        var (code, err) = r.read(r);
        if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            r.err = err;
            break;
        }
        switch (ᐧ) {
        case {} when code is < r.clear: {
            r.output[r.o] = ((uint8)code);
            r.o++;
            if (r.last != decoderInvalidCode) {
                // We have a literal code.
                // Save what the hi code expands to.
                r.suffix[r.hi] = ((uint8)code);
                r.prefix[r.hi] = r.last;
            }
            break;
        }
        case {} when code is r.clear: {
            r.width = 1 + ((nuint)r.litWidth);
            r.hi = r.eof;
            r.overflow = 1 << (int)(r.width);
            r.last = decoderInvalidCode;
            continue;
            break;
        }
        case {} when code is r.eof: {
            r.err = io.EOF;
            goto break_loop;
            break;
        }
        case {} when code is <= r.hi: {
            var c = code;
            nint i = len(r.output) - 1;
            if (code == r.hi && r.last != decoderInvalidCode) {
                // code == hi is a special case which expands to the last expansion
                // followed by the head of the last expansion. To find the head, we walk
                // the prefix chain until we find a literal code.
                c = r.last;
                while (c >= r.clear) {
                    c = r.prefix[c];
                }
                r.output[i] = ((uint8)c);
                i--;
                c = r.last;
            }
            while (c >= r.clear) {
                // Copy the suffix chain into output and then write that to w.
                r.output[i] = r.suffix[c];
                i--;
                c = r.prefix[c];
            }
            r.output[i] = ((uint8)c);
            r.o += copy(r.output[(int)(r.o)..], r.output[(int)(i)..]);
            if (r.last != decoderInvalidCode) {
                // Save what the hi code expands to.
                r.suffix[r.hi] = ((uint8)c);
                r.prefix[r.hi] = r.last;
            }
            break;
        }
        default: {
            r.err = errors.New("lzw: invalid code"u8);
            goto break_loop;
            break;
        }}

        (r.last, r.hi) = (code, r.hi + 1);
        if (r.hi >= r.overflow) {
            if (r.hi > r.overflow) {
                throw panic("unreachable");
            }
            if (r.width == maxWidth){
                r.last = decoderInvalidCode;
                // Undo the d.hi++ a few lines above, so that (1) we maintain
                // the invariant that d.hi < d.overflow, and (2) d.hi does not
                // eventually overflow a uint16.
                r.hi--;
            } else {
                r.width++;
                r.overflow = 1 << (int)(r.width);
            }
        }
        if (r.o >= flushBuffer) {
            break;
        }
continue_loop:;
    }
break_loop:;
    // Flush pending output.
    r.toRead = r.output[..(int)(r.o)];
    r.o = 0;
}

internal static error errClosed = errors.New("lzw: reader/writer is closed"u8);

// Close closes the [Reader] and returns an error for any future read operation.
// It does not close the underlying [io.Reader].
[GoRecv] public static error Close(this ref Reader r) {
    r.err = errClosed;
    // in case any Reads come along
    return default!;
}

// Reset clears the [Reader]'s state and allows it to be reused again
// as a new [Reader].
[GoRecv] public static void Reset(this ref Reader r, io.Reader src, Order order, nint litWidth) {
    r = new Reader(nil);
    r.init(src, order, litWidth);
}

// NewReader creates a new [io.ReadCloser].
// Reads from the returned [io.ReadCloser] read and decompress data from r.
// If r does not also implement [io.ByteReader],
// the decompressor may read more data than necessary from r.
// It is the caller's responsibility to call Close on the ReadCloser when
// finished reading.
// The number of bits to use for literal codes, litWidth, must be in the
// range [2,8] and is typically 8. It must equal the litWidth
// used during compression.
//
// It is guaranteed that the underlying type of the returned [io.ReadCloser]
// is a *[Reader].
public static io.ReadCloser NewReader(io.Reader r, Order order, nint litWidth) {
    return ~newReader(r, order, litWidth);
}

internal static ж<Reader> newReader(io.Reader src, Order order, nint litWidth) {
    var r = @new<Reader>();
    r.init(src, order, litWidth);
    return r;
}

[GoRecv] internal static void init(this ref Reader r, io.Reader src, Order order, nint litWidth) {
    var exprᴛ1 = order;
    if (exprᴛ1 == LSB) {
        r.read = () => (ж<Reader>).readLSB();
    }
    else if (exprᴛ1 == MSB) {
        r.read = () => (ж<Reader>).readMSB();
    }
    else { /* default: */
        r.err = errors.New("lzw: unknown order"u8);
        return;
    }

    if (litWidth < 2 || 8 < litWidth) {
        r.err = fmt.Errorf("lzw: litWidth %d out of range"u8, litWidth);
        return;
    }
    var (br, ok) = src._<io.ByteReader>(ᐧ);
    if (!ok && src != default!) {
        br = ~bufio.NewReader(src);
    }
    r.r = br;
    r.litWidth = litWidth;
    r.width = 1 + ((nuint)litWidth);
    r.clear = ((uint16)1) << (int)(((nuint)litWidth));
    (r.eof, r.hi) = (r.clear + 1, r.clear + 1);
    r.overflow = ((uint16)1) << (int)(r.width);
    r.last = decoderInvalidCode;
}

} // end lzw_package
