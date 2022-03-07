// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lzw -- go2cs converted at 2022 March 06 23:35:26 UTC
// import "compress/lzw" ==> using lzw = go.compress.lzw_package
// Original source: C:\Program Files\Go\src\compress\lzw\writer.go
using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using System;


namespace go.compress;

public static partial class lzw_package {

    // A writer is a buffered, flushable writer.
private partial interface writer {
    error Flush();
}

 
// A code is a 12 bit value, stored as a uint32 when encoding to avoid
// type conversions when shifting bits.
private static readonly nint maxCode = 1 << 12 - 1;
private static readonly nint invalidCode = 1 << 32 - 1; 
// There are 1<<12 possible codes, which is an upper bound on the number of
// valid hash table entries at any given point in time. tableSize is 4x that.
private static readonly nint tableSize = 4 * 1 << 12;
private static readonly var tableMask = tableSize - 1; 
// A hash table entry is a uint32. Zero is an invalid entry since the
// lower 12 bits of a valid entry must be a non-literal code.
private static readonly nint invalidEntry = 0;


// Writer is an LZW compressor. It writes the compressed form of the data
// to an underlying writer (see NewWriter).
public partial struct Writer {
    public writer w; // order, write, bits, nBits and width are the state for
// converting a code stream into a byte stream.
    public Order order;
    public Func<ptr<Writer>, uint, error> write;
    public uint bits;
    public nuint nBits;
    public nuint width; // litWidth is the width in bits of literal codes.
    public nuint litWidth; // hi is the code implied by the next code emission.
// overflow is the code at which hi overflows the code width.
    public uint hi; // savedCode is the accumulated code at the end of the most recent Write
// call. It is equal to invalidCode if there was no such call.
    public uint overflow; // savedCode is the accumulated code at the end of the most recent Write
// call. It is equal to invalidCode if there was no such call.
    public uint savedCode; // err is the first error encountered during writing. Closing the writer
// will make any future Write calls return errClosed
    public error err; // table is the hash table from 20-bit keys to 12-bit values. Each table
// entry contains key<<12|val and collisions resolve by linear probing.
// The keys consist of a 12-bit code prefix and an 8-bit byte suffix.
// The values are a 12-bit code.
    public array<uint> table;
}

// writeLSB writes the code c for "Least Significant Bits first" data.
private static error writeLSB(this ptr<Writer> _addr_w, uint c) {
    ref Writer w = ref _addr_w.val;

    w.bits |= c << (int)(w.nBits);
    w.nBits += w.width;
    while (w.nBits >= 8) {
        {
            var err = w.w.WriteByte(uint8(w.bits));

            if (err != null) {
                return error.As(err)!;
            }

        }

        w.bits>>=8;
        w.nBits -= 8;

    }
    return error.As(null!)!;

}

// writeMSB writes the code c for "Most Significant Bits first" data.
private static error writeMSB(this ptr<Writer> _addr_w, uint c) {
    ref Writer w = ref _addr_w.val;

    w.bits |= c << (int)((32 - w.width - w.nBits));
    w.nBits += w.width;
    while (w.nBits >= 8) {
        {
            var err = w.w.WriteByte(uint8(w.bits >> 24));

            if (err != null) {
                return error.As(err)!;
            }

        }

        w.bits<<=8;
        w.nBits -= 8;

    }
    return error.As(null!)!;

}

// errOutOfCodes is an internal error that means that the writer has run out
// of unused codes and a clear code needs to be sent next.
private static var errOutOfCodes = errors.New("lzw: out of codes");

// incHi increments e.hi and checks for both overflow and running out of
// unused codes. In the latter case, incHi sends a clear code, resets the
// writer state and returns errOutOfCodes.
private static error incHi(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    w.hi++;
    if (w.hi == w.overflow) {
        w.width++;
        w.overflow<<=1;
    }
    if (w.hi == maxCode) {
        var clear = uint32(1) << (int)(w.litWidth);
        {
            var err = w.write(w, clear);

            if (err != null) {
                return error.As(err)!;
            }

        }

        w.width = w.litWidth + 1;
        w.hi = clear + 1;
        w.overflow = clear << 1;
        foreach (var (i) in w.table) {
            w.table[i] = invalidEntry;
        }        return error.As(errOutOfCodes)!;

    }
    return error.As(null!)!;

}

// Write writes a compressed representation of p to w's underlying writer.
private static (nint, error) Write(this ptr<Writer> _addr_w, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Writer w = ref _addr_w.val;

    if (w.err != null) {
        return (0, error.As(w.err)!);
    }
    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    {
        var maxLit = uint8(1 << (int)(w.litWidth) - 1);

        if (maxLit != 0xff) {
            {
                var x__prev1 = x;

                foreach (var (_, __x) in p) {
                    x = __x;
                    if (x > maxLit) {
                        w.err = errors.New("lzw: input byte too large for the litWidth");
                        return (0, error.As(w.err)!);
                    }
                }

                x = x__prev1;
            }
        }
    }

    n = len(p);
    var code = w.savedCode;
    if (code == invalidCode) { 
        // The first code sent is always a literal code.
        (code, p) = (uint32(p[0]), p[(int)1..]);
    }
loop:
    {
        var x__prev1 = x;

        foreach (var (_, __x) in p) {
            x = __x;
            var literal = uint32(x);
            var key = code << 8 | literal; 
            // If there is a hash table hit for this key then we continue the loop
            // and do not emit a code yet.
            var hash = (key >> 12 ^ key) & tableMask;
            {
                var h = hash;
                var t = w.table[hash];

                while (t != invalidEntry) {
                    if (key == t >> 12) {
                        code = t & maxCode;
                        _continueloop = true;
                        break;
                    }

                    h = (h + 1) & tableMask;
                    t = w.table[h];

                } 
                // Otherwise, write the current code, and literal becomes the start of
                // the next emitted code.

            } 
            // Otherwise, write the current code, and literal becomes the start of
            // the next emitted code.
            w.err = w.write(w, code);

            if (w.err != null) {
                return (0, error.As(w.err)!);
            }

            code = literal; 
            // Increment e.hi, the next implied code. If we run out of codes, reset
            // the writer state (including clearing the hash table) and continue.
            {
                var err1 = w.incHi();

                if (err1 != null) {
                    if (err1 == errOutOfCodes) {
                        continue;
                    }
                    w.err = err1;
                    return (0, error.As(w.err)!);
                } 
                // Otherwise, insert key -> e.hi into the map that e.table represents.

            } 
            // Otherwise, insert key -> e.hi into the map that e.table represents.
            while (true) {
                if (w.table[hash] == invalidEntry) {
                    w.table[hash] = (key << 12) | w.hi;
                    break;
                }
                hash = (hash + 1) & tableMask;
            }


        }
        x = x__prev1;
    }
    w.savedCode = code;
    return (n, error.As(null!)!);

}

// Close closes the Writer, flushing any pending output. It does not close
// w's underlying writer.
private static error Close(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    if (w.err != null) {
        if (w.err == errClosed) {
            return error.As(null!)!;
        }
        return error.As(w.err)!;

    }
    w.err = errClosed; 
    // Write the savedCode if valid.
    if (w.savedCode != invalidCode) {
        {
            var err__prev2 = err;

            var err = w.write(w, w.savedCode);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

        {
            var err__prev2 = err;

            err = w.incHi();

            if (err != null && err != errOutOfCodes) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

    }
    var eof = uint32(1) << (int)(w.litWidth) + 1;
    {
        var err__prev1 = err;

        err = w.write(w, eof);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 
    // Write the final bits.
    if (w.nBits > 0) {
        if (w.order == MSB) {
            w.bits>>=24;
        }
        {
            var err__prev2 = err;

            err = w.w.WriteByte(uint8(w.bits));

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

    }
    return error.As(w.w.Flush())!;

}

// Reset clears the Writer's state and allows it to be reused again
// as a new Writer.
private static void Reset(this ptr<Writer> _addr_w, io.Writer dst, Order order, nint litWidth) {
    ref Writer w = ref _addr_w.val;

    w.val = new Writer();
    w.init(dst, order, litWidth);
}

// NewWriter creates a new io.WriteCloser.
// Writes to the returned io.WriteCloser are compressed and written to w.
// It is the caller's responsibility to call Close on the WriteCloser when
// finished writing.
// The number of bits to use for literal codes, litWidth, must be in the
// range [2,8] and is typically 8. Input bytes must be less than 1<<litWidth.
//
// It is guaranteed that the underlying type of the returned io.WriteCloser
// is a *Writer.
public static io.WriteCloser NewWriter(io.Writer w, Order order, nint litWidth) {
    return newWriter(w, order, litWidth);
}

private static ptr<Writer> newWriter(io.Writer dst, Order order, nint litWidth) {
    ptr<Writer> w = @new<Writer>();
    w.init(dst, order, litWidth);
    return _addr_w!;
}

private static void init(this ptr<Writer> _addr_w, io.Writer dst, Order order, nint litWidth) {
    ref Writer w = ref _addr_w.val;


    if (order == LSB) 
        w.write = (Writer.val).writeLSB;
    else if (order == MSB) 
        w.write = (Writer.val).writeMSB;
    else 
        w.err = errors.New("lzw: unknown order");
        return ;
        if (litWidth < 2 || 8 < litWidth) {
        w.err = fmt.Errorf("lzw: litWidth %d out of range", litWidth);
        return ;
    }
    writer (bw, ok) = writer.As(dst._<writer>())!;
    if (!ok && dst != null) {
        bw = bufio.NewWriter(dst);
    }
    w.w = bw;
    var lw = uint(litWidth);
    w.order = order;
    w.width = 1 + lw;
    w.litWidth = lw;
    w.hi = 1 << (int)(lw) + 1;
    w.overflow = 1 << (int)((lw + 1));
    w.savedCode = invalidCode;

}

} // end lzw_package
