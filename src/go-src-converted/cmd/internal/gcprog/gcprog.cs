// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcprog implements an encoder for packed GC pointer bitmaps,
// known as GC programs.
//
// Program Format
//
// The GC program encodes a sequence of 0 and 1 bits indicating scalar or pointer words in an object.
// The encoding is a simple Lempel-Ziv program, with codes to emit literal bits and to repeat the
// last n bits c times.
//
// The possible codes are:
//
//    00000000: stop
//    0nnnnnnn: emit n bits copied from the next (n+7)/8 bytes, least significant bit first
//    10000000 n c: repeat the previous n bits c times; n, c are varints
//    1nnnnnnn c: repeat the previous n bits c times; c is a varint
//
// The numbers n and c, when they follow a code, are encoded as varints
// using the same encoding as encoding/binary's Uvarint.
//
// package gcprog -- go2cs converted at 2020 August 29 09:28:22 UTC
// import "cmd/internal/gcprog" ==> using gcprog = go.cmd.@internal.gcprog_package
// Original source: C:\Go\src\cmd\internal\gcprog\gcprog.go
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class gcprog_package
    {
        private static readonly long progMaxLiteral = 127L; // maximum n for literal n bit code

        // A Writer is an encoder for GC programs.
        //
        // The typical use of a Writer is to call Init, maybe call Debug,
        // make a sequence of Ptr, Advance, Repeat, and Append calls
        // to describe the data type, and then finally call End.
 // maximum n for literal n bit code

        // A Writer is an encoder for GC programs.
        //
        // The typical use of a Writer is to call Init, maybe call Debug,
        // make a sequence of Ptr, Advance, Repeat, and Append calls
        // to describe the data type, and then finally call End.
        public partial struct Writer
        {
            public Action<byte> writeByte;
            public long index;
            public array<byte> b;
            public long nb;
            public io.Writer debug;
            public slice<byte> debugBuf;
        }

        // Init initializes w to write a new GC program
        // by calling writeByte for each byte in the program.
        private static void Init(this ref Writer w, Action<byte> writeByte)
        {
            w.writeByte = writeByte;
        }

        // Debug causes the writer to print a debugging trace to out
        // during future calls to methods like Ptr, Advance, and End.
        // It also enables debugging checks during the encoding.
        private static void Debug(this ref Writer w, io.Writer @out)
        {
            w.debug = out;
        }

        // BitIndex returns the number of bits written to the bit stream so far.
        private static long BitIndex(this ref Writer w)
        {
            return w.index;
        }

        // byte writes the byte x to the output.
        private static void @byte(this ref Writer w, byte x)
        {
            if (w.debug != null)
            {
                w.debugBuf = append(w.debugBuf, x);
            }
            w.writeByte(x);
        }

        // End marks the end of the program, writing any remaining bytes.
        private static void End(this ref Writer _w) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            w.flushlit();
            w.@byte(0L);
            if (w.debug != null)
            {
                var index = progbits(w.debugBuf);
                if (index != w.index)
                {
                    println("gcprog: End wrote program for", index, "bits, but current index is", w.index);
                    panic("gcprog: out of sync");
                }
            }
        });

        // Ptr emits a 1 into the bit stream at the given bit index.
        // that is, it records that the index'th word in the object memory is a pointer.
        // Any bits between the current index and the new index
        // are set to zero, meaning the corresponding words are scalars.
        private static void Ptr(this ref Writer _w, long index) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            if (index < w.index)
            {
                println("gcprog: Ptr at index", index, "but current index is", w.index);
                panic("gcprog: invalid Ptr index");
            }
            w.ZeroUntil(index);
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "gcprog: ptr at %d\n", index);
            }
            w.lit(1L);
        });

        // ShouldRepeat reports whether it would be worthwhile to
        // use a Repeat to describe c elements of n bits each,
        // compared to just emitting c copies of the n-bit description.
        private static bool ShouldRepeat(this ref Writer w, long n, long c)
        { 
            // Should we lay out the bits directly instead of
            // encoding them as a repetition? Certainly if count==1,
            // since there's nothing to repeat, but also if the total
            // size of the plain pointer bits for the type will fit in
            // 4 or fewer bytes, since using a repetition will require
            // flushing the current bits plus at least one byte for
            // the repeat size and one for the repeat count.
            return c > 1L && c * n > 4L * 8L;
        }

        // Repeat emits an instruction to repeat the description
        // of the last n words c times (including the initial description, c+1 times in total).
        private static void Repeat(this ref Writer w, long n, long c)
        {
            if (n == 0L || c == 0L)
            {
                return;
            }
            w.flushlit();
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "gcprog: repeat %d × %d\n", n, c);
            }
            if (n < 128L)
            {
                w.@byte(0x80UL | byte(n));
            }
            else
            {
                w.@byte(0x80UL);
                w.varint(n);
            }
            w.varint(c);
            w.index += n * c;
        }

        // ZeroUntil adds zeros to the bit stream until reaching the given index;
        // that is, it records that the words from the most recent pointer until
        // the index'th word are scalars.
        // ZeroUntil is usually called in preparation for a call to Repeat, Append, or End.
        private static void ZeroUntil(this ref Writer _w, long index) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            if (index < w.index)
            {
                println("gcprog: Advance", index, "but index is", w.index);
                panic("gcprog: invalid Advance index");
            }
            var skip = (index - w.index);
            if (skip == 0L)
            {
                return;
            }
            if (skip < 4L * 8L)
            {
                if (w.debug != null)
                {
                    fmt.Fprintf(w.debug, "gcprog: advance to %d by literals\n", index);
                }
                for (var i = int64(0L); i < skip; i++)
                {
                    w.lit(0L);
                }

                return;
            }
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "gcprog: advance to %d by repeat\n", index);
            }
            w.lit(0L);
            w.flushlit();
            w.Repeat(1L, skip - 1L);
        });

        // Append emits the given GC program into the current output.
        // The caller asserts that the program emits n bits (describes n words),
        // and Append panics if that is not true.
        private static void Append(this ref Writer _w, slice<byte> prog, long n) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            w.flushlit();
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "gcprog: append prog for %d ptrs\n", n);
                fmt.Fprintf(w.debug, "\t");
            }
            var n1 = progbits(prog);
            if (n1 != n)
            {
                panic("gcprog: wrong bit count in append");
            } 
            // The last byte of the prog terminates the program.
            // Don't emit that, or else our own program will end.
            foreach (var (i, x) in prog[..len(prog) - 1L])
            {
                if (w.debug != null)
                {
                    if (i > 0L)
                    {
                        fmt.Fprintf(w.debug, " ");
                    }
                    fmt.Fprintf(w.debug, "%02x", x);
                }
                w.@byte(x);
            }
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "\n");
            }
            w.index += n;
        });

        // progbits returns the length of the bit stream encoded by the program p.
        private static long progbits(slice<byte> p) => func((_, panic, __) =>
        {
            long n = default;
            while (len(p) > 0L)
            {
                var x = p[0L];
                p = p[1L..];
                if (x == 0L)
                {
                    break;
                }
                if (x & 0x80UL == 0L)
                {
                    var count = x & ~0x80UL;
                    n += int64(count);
                    p = p[(count + 7L) / 8L..];
                    continue;
                }
                var nbit = int64(x & ~0x80UL);
                if (nbit == 0L)
                {
                    nbit, p = readvarint(p);
                }
                count = default;
                count, p = readvarint(p);
                n += nbit * count;
            }

            if (len(p) > 0L)
            {
                println("gcprog: found end instruction after", n, "ptrs, with", len(p), "bytes remaining");
                panic("gcprog: extra data at end of program");
            }
            return n;
        });

        // readvarint reads a varint from p, returning the value and the remainder of p.
        private static (long, slice<byte>) readvarint(slice<byte> p)
        {
            long v = default;
            ulong nb = default;
            while (true)
            {
                var c = p[0L];
                p = p[1L..];
                v |= int64(c & ~0x80UL) << (int)(nb);
                nb += 7L;
                if (c & 0x80UL == 0L)
                {
                    break;
                }
            }

            return (v, p);
        }

        // lit adds a single literal bit to w.
        private static void lit(this ref Writer w, byte x)
        {
            if (w.nb == progMaxLiteral)
            {
                w.flushlit();
            }
            w.b[w.nb] = x;
            w.nb++;
            w.index++;
        }

        // varint emits the varint encoding of x.
        private static void varint(this ref Writer _w, long x) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            if (x < 0L)
            {
                panic("gcprog: negative varint");
            }
            while (x >= 0x80UL)
            {
                w.@byte(byte(0x80UL | x));
                x >>= 7L;
            }

            w.@byte(byte(x));
        });

        // flushlit flushes any pending literal bits.
        private static void flushlit(this ref Writer w)
        {
            if (w.nb == 0L)
            {
                return;
            }
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "gcprog: flush %d literals\n", w.nb);
                fmt.Fprintf(w.debug, "\t%v\n", w.b[..w.nb]);
                fmt.Fprintf(w.debug, "\t%02x", byte(w.nb));
            }
            w.@byte(byte(w.nb));
            byte bits = default;
            for (long i = 0L; i < w.nb; i++)
            {
                bits |= w.b[i] << (int)(uint(i % 8L));
                if ((i + 1L) % 8L == 0L)
                {
                    if (w.debug != null)
                    {
                        fmt.Fprintf(w.debug, " %02x", bits);
                    }
                    w.@byte(bits);
                    bits = 0L;
                }
            }

            if (w.nb % 8L != 0L)
            {
                if (w.debug != null)
                {
                    fmt.Fprintf(w.debug, " %02x", bits);
                }
                w.@byte(bits);
            }
            if (w.debug != null)
            {
                fmt.Fprintf(w.debug, "\n");
            }
            w.nb = 0L;
        }
    }
}}}
