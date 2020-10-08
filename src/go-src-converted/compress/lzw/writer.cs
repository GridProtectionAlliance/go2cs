// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lzw -- go2cs converted at 2020 October 08 04:58:43 UTC
// import "compress/lzw" ==> using lzw = go.compress.lzw_package
// Original source: C:\Go\src\compress\lzw\writer.go
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
        // A writer is a buffered, flushable writer.
        private partial interface writer : io.ByteWriter
        {
            error Flush();
        }

        // An errWriteCloser is an io.WriteCloser that always returns a given error.
        private partial struct errWriteCloser
        {
            public error err;
        }

        private static (long, error) Write(this ptr<errWriteCloser> _addr_e, slice<byte> _p0)
        {
            long _p0 = default;
            error _p0 = default!;
            ref errWriteCloser e = ref _addr_e.val;

            return (0L, error.As(e.err)!);
        }

        private static error Close(this ptr<errWriteCloser> _addr_e)
        {
            ref errWriteCloser e = ref _addr_e.val;

            return error.As(e.err)!;
        }

 
        // A code is a 12 bit value, stored as a uint32 when encoding to avoid
        // type conversions when shifting bits.
        private static readonly long maxCode = (long)1L << (int)(12L) - 1L;
        private static readonly long invalidCode = (long)1L << (int)(32L) - 1L; 
        // There are 1<<12 possible codes, which is an upper bound on the number of
        // valid hash table entries at any given point in time. tableSize is 4x that.
        private static readonly long tableSize = (long)4L * 1L << (int)(12L);
        private static readonly var tableMask = (var)tableSize - 1L; 
        // A hash table entry is a uint32. Zero is an invalid entry since the
        // lower 12 bits of a valid entry must be a non-literal code.
        private static readonly long invalidEntry = (long)0L;


        // encoder is LZW compressor.
        private partial struct encoder
        {
            public writer w; // order, write, bits, nBits and width are the state for
// converting a code stream into a byte stream.
            public Order order;
            public Func<ptr<encoder>, uint, error> write;
            public uint bits;
            public ulong nBits;
            public ulong width; // litWidth is the width in bits of literal codes.
            public ulong litWidth; // hi is the code implied by the next code emission.
// overflow is the code at which hi overflows the code width.
            public uint hi; // savedCode is the accumulated code at the end of the most recent Write
// call. It is equal to invalidCode if there was no such call.
            public uint overflow; // savedCode is the accumulated code at the end of the most recent Write
// call. It is equal to invalidCode if there was no such call.
            public uint savedCode; // err is the first error encountered during writing. Closing the encoder
// will make any future Write calls return errClosed
            public error err; // table is the hash table from 20-bit keys to 12-bit values. Each table
// entry contains key<<12|val and collisions resolve by linear probing.
// The keys consist of a 12-bit code prefix and an 8-bit byte suffix.
// The values are a 12-bit code.
            public array<uint> table;
        }

        // writeLSB writes the code c for "Least Significant Bits first" data.
        private static error writeLSB(this ptr<encoder> _addr_e, uint c)
        {
            ref encoder e = ref _addr_e.val;

            e.bits |= c << (int)(e.nBits);
            e.nBits += e.width;
            while (e.nBits >= 8L)
            {
                {
                    var err = e.w.WriteByte(uint8(e.bits));

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                e.bits >>= 8L;
                e.nBits -= 8L;

            }

            return error.As(null!)!;

        }

        // writeMSB writes the code c for "Most Significant Bits first" data.
        private static error writeMSB(this ptr<encoder> _addr_e, uint c)
        {
            ref encoder e = ref _addr_e.val;

            e.bits |= c << (int)((32L - e.width - e.nBits));
            e.nBits += e.width;
            while (e.nBits >= 8L)
            {
                {
                    var err = e.w.WriteByte(uint8(e.bits >> (int)(24L)));

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                e.bits <<= 8L;
                e.nBits -= 8L;

            }

            return error.As(null!)!;

        }

        // errOutOfCodes is an internal error that means that the encoder has run out
        // of unused codes and a clear code needs to be sent next.
        private static var errOutOfCodes = errors.New("lzw: out of codes");

        // incHi increments e.hi and checks for both overflow and running out of
        // unused codes. In the latter case, incHi sends a clear code, resets the
        // encoder state and returns errOutOfCodes.
        private static error incHi(this ptr<encoder> _addr_e)
        {
            ref encoder e = ref _addr_e.val;

            e.hi++;
            if (e.hi == e.overflow)
            {
                e.width++;
                e.overflow <<= 1L;
            }

            if (e.hi == maxCode)
            {
                var clear = uint32(1L) << (int)(e.litWidth);
                {
                    var err = e.write(e, clear);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                e.width = e.litWidth + 1L;
                e.hi = clear + 1L;
                e.overflow = clear << (int)(1L);
                foreach (var (i) in e.table)
                {
                    e.table[i] = invalidEntry;
                }
                return error.As(errOutOfCodes)!;

            }

            return error.As(null!)!;

        }

        // Write writes a compressed representation of p to e's underlying writer.
        private static (long, error) Write(this ptr<encoder> _addr_e, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref encoder e = ref _addr_e.val;

            if (e.err != null)
            {
                return (0L, error.As(e.err)!);
            }

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            {
                var maxLit = uint8(1L << (int)(e.litWidth) - 1L);

                if (maxLit != 0xffUL)
                {
                    {
                        var x__prev1 = x;

                        foreach (var (_, __x) in p)
                        {
                            x = __x;
                            if (x > maxLit)
                            {
                                e.err = errors.New("lzw: input byte too large for the litWidth");
                                return (0L, error.As(e.err)!);
                            }

                        }

                        x = x__prev1;
                    }
                }

            }

            n = len(p);
            var code = e.savedCode;
            if (code == invalidCode)
            { 
                // The first code sent is always a literal code.
                code = uint32(p[0L]);
                p = p[1L..];

            }

loop:
            {
                var x__prev1 = x;

                foreach (var (_, __x) in p)
                {
                    x = __x;
                    var literal = uint32(x);
                    var key = code << (int)(8L) | literal; 
                    // If there is a hash table hit for this key then we continue the loop
                    // and do not emit a code yet.
                    var hash = (key >> (int)(12L) ^ key) & tableMask;
                    {
                        var h = hash;
                        var t = e.table[hash];

                        while (t != invalidEntry)
                        {
                            if (key == t >> (int)(12L))
                            {
                                code = t & maxCode;
                                _continueloop = true;
                                break;
                            }

                            h = (h + 1L) & tableMask;
                            t = e.table[h];

                        } 
                        // Otherwise, write the current code, and literal becomes the start of
                        // the next emitted code.

                    } 
                    // Otherwise, write the current code, and literal becomes the start of
                    // the next emitted code.
                    e.err = e.write(e, code);

                    if (e.err != null)
                    {
                        return (0L, error.As(e.err)!);
                    }

                    code = literal; 
                    // Increment e.hi, the next implied code. If we run out of codes, reset
                    // the encoder state (including clearing the hash table) and continue.
                    {
                        var err1 = e.incHi();

                        if (err1 != null)
                        {
                            if (err1 == errOutOfCodes)
                            {
                                continue;
                            }

                            e.err = err1;
                            return (0L, error.As(e.err)!);

                        } 
                        // Otherwise, insert key -> e.hi into the map that e.table represents.

                    } 
                    // Otherwise, insert key -> e.hi into the map that e.table represents.
                    while (true)
                    {
                        if (e.table[hash] == invalidEntry)
                        {
                            e.table[hash] = (key << (int)(12L)) | e.hi;
                            break;
                        }

                        hash = (hash + 1L) & tableMask;

                    }


                }

                x = x__prev1;
            }
            e.savedCode = code;
            return (n, error.As(null!)!);

        }

        // Close closes the encoder, flushing any pending output. It does not close or
        // flush e's underlying writer.
        private static error Close(this ptr<encoder> _addr_e)
        {
            ref encoder e = ref _addr_e.val;

            if (e.err != null)
            {
                if (e.err == errClosed)
                {
                    return error.As(null!)!;
                }

                return error.As(e.err)!;

            } 
            // Make any future calls to Write return errClosed.
            e.err = errClosed; 
            // Write the savedCode if valid.
            if (e.savedCode != invalidCode)
            {
                {
                    var err__prev2 = err;

                    var err = e.write(e, e.savedCode);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = e.incHi();

                    if (err != null && err != errOutOfCodes)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            } 
            // Write the eof code.
            var eof = uint32(1L) << (int)(e.litWidth) + 1L;
            {
                var err__prev1 = err;

                err = e.write(e, eof);

                if (err != null)
                {
                    return error.As(err)!;
                } 
                // Write the final bits.

                err = err__prev1;

            } 
            // Write the final bits.
            if (e.nBits > 0L)
            {
                if (e.order == MSB)
                {
                    e.bits >>= 24L;
                }

                {
                    var err__prev2 = err;

                    err = e.w.WriteByte(uint8(e.bits));

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            return error.As(e.w.Flush())!;

        }

        // NewWriter creates a new io.WriteCloser.
        // Writes to the returned io.WriteCloser are compressed and written to w.
        // It is the caller's responsibility to call Close on the WriteCloser when
        // finished writing.
        // The number of bits to use for literal codes, litWidth, must be in the
        // range [2,8] and is typically 8. Input bytes must be less than 1<<litWidth.
        public static io.WriteCloser NewWriter(io.Writer w, Order order, long litWidth)
        {
            Func<ptr<encoder>, uint, error> write = default;

            if (order == LSB) 
                write = ptr<encoder>;
            else if (order == MSB) 
                write = ptr<encoder>;
            else 
                return addr(new errWriteCloser(errors.New("lzw: unknown order")));
                        if (litWidth < 2L || 8L < litWidth)
            {
                return addr(new errWriteCloser(fmt.Errorf("lzw: litWidth %d out of range",litWidth)));
            }

            writer (bw, ok) = writer.As(w._<writer>())!;
            if (!ok)
            {
                bw = bufio.NewWriter(w);
            }

            var lw = uint(litWidth);
            return addr(new encoder(w:bw,order:order,write:write,width:1+lw,litWidth:lw,hi:1<<lw+1,overflow:1<<(lw+1),savedCode:invalidCode,));

        }
    }
}}
