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

// package gcprog -- go2cs converted at 2022 March 13 06:22:40 UTC
// import "cmd/internal/gcprog" ==> using gcprog = go.cmd.@internal.gcprog_package
// Original source: C:\Program Files\Go\src\cmd\internal\gcprog\gcprog.go
namespace go.cmd.@internal;

using fmt = fmt_package;
using io = io_package;
using System;

public static partial class gcprog_package {

private static readonly nint progMaxLiteral = 127; // maximum n for literal n bit code

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
public partial struct Writer {
    public Action<byte> writeByte;
    public long index;
    public array<byte> b;
    public nint nb;
    public io.Writer debug;
    public slice<byte> debugBuf;
}

// Init initializes w to write a new GC program
// by calling writeByte for each byte in the program.
private static void Init(this ptr<Writer> _addr_w, Action<byte> writeByte) {
    ref Writer w = ref _addr_w.val;

    w.writeByte = writeByte;
}

// Debug causes the writer to print a debugging trace to out
// during future calls to methods like Ptr, Advance, and End.
// It also enables debugging checks during the encoding.
private static void Debug(this ptr<Writer> _addr_w, io.Writer @out) {
    ref Writer w = ref _addr_w.val;

    w.debug = out;
}

// BitIndex returns the number of bits written to the bit stream so far.
private static long BitIndex(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    return w.index;
}

// byte writes the byte x to the output.
private static void @byte(this ptr<Writer> _addr_w, byte x) {
    ref Writer w = ref _addr_w.val;

    if (w.debug != null) {
        w.debugBuf = append(w.debugBuf, x);
    }
    w.writeByte(x);
}

// End marks the end of the program, writing any remaining bytes.
private static void End(this ptr<Writer> _addr_w) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    w.flushlit();
    w.@byte(0);
    if (w.debug != null) {
        var index = progbits(w.debugBuf);
        if (index != w.index) {
            println("gcprog: End wrote program for", index, "bits, but current index is", w.index);
            panic("gcprog: out of sync");
        }
    }
});

// Ptr emits a 1 into the bit stream at the given bit index.
// that is, it records that the index'th word in the object memory is a pointer.
// Any bits between the current index and the new index
// are set to zero, meaning the corresponding words are scalars.
private static void Ptr(this ptr<Writer> _addr_w, long index) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    if (index < w.index) {
        println("gcprog: Ptr at index", index, "but current index is", w.index);
        panic("gcprog: invalid Ptr index");
    }
    w.ZeroUntil(index);
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "gcprog: ptr at %d\n", index);
    }
    w.lit(1);
});

// ShouldRepeat reports whether it would be worthwhile to
// use a Repeat to describe c elements of n bits each,
// compared to just emitting c copies of the n-bit description.
private static bool ShouldRepeat(this ptr<Writer> _addr_w, long n, long c) {
    ref Writer w = ref _addr_w.val;
 
    // Should we lay out the bits directly instead of
    // encoding them as a repetition? Certainly if count==1,
    // since there's nothing to repeat, but also if the total
    // size of the plain pointer bits for the type will fit in
    // 4 or fewer bytes, since using a repetition will require
    // flushing the current bits plus at least one byte for
    // the repeat size and one for the repeat count.
    return c > 1 && c * n > 4 * 8;
}

// Repeat emits an instruction to repeat the description
// of the last n words c times (including the initial description, c+1 times in total).
private static void Repeat(this ptr<Writer> _addr_w, long n, long c) {
    ref Writer w = ref _addr_w.val;

    if (n == 0 || c == 0) {
        return ;
    }
    w.flushlit();
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "gcprog: repeat %d × %d\n", n, c);
    }
    if (n < 128) {
        w.@byte(0x80 | byte(n));
    }
    else
 {
        w.@byte(0x80);
        w.varint(n);
    }
    w.varint(c);
    w.index += n * c;
}

// ZeroUntil adds zeros to the bit stream until reaching the given index;
// that is, it records that the words from the most recent pointer until
// the index'th word are scalars.
// ZeroUntil is usually called in preparation for a call to Repeat, Append, or End.
private static void ZeroUntil(this ptr<Writer> _addr_w, long index) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    if (index < w.index) {
        println("gcprog: Advance", index, "but index is", w.index);
        panic("gcprog: invalid Advance index");
    }
    var skip = (index - w.index);
    if (skip == 0) {
        return ;
    }
    if (skip < 4 * 8) {
        if (w.debug != null) {
            fmt.Fprintf(w.debug, "gcprog: advance to %d by literals\n", index);
        }
        for (var i = int64(0); i < skip; i++) {
            w.lit(0);
        }
        return ;
    }
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "gcprog: advance to %d by repeat\n", index);
    }
    w.lit(0);
    w.flushlit();
    w.Repeat(1, skip - 1);
});

// Append emits the given GC program into the current output.
// The caller asserts that the program emits n bits (describes n words),
// and Append panics if that is not true.
private static void Append(this ptr<Writer> _addr_w, slice<byte> prog, long n) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    w.flushlit();
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "gcprog: append prog for %d ptrs\n", n);
        fmt.Fprintf(w.debug, "\t");
    }
    var n1 = progbits(prog);
    if (n1 != n) {
        panic("gcprog: wrong bit count in append");
    }
    foreach (var (i, x) in prog[..(int)len(prog) - 1]) {
        if (w.debug != null) {
            if (i > 0) {
                fmt.Fprintf(w.debug, " ");
            }
            fmt.Fprintf(w.debug, "%02x", x);
        }
        w.@byte(x);
    }    if (w.debug != null) {
        fmt.Fprintf(w.debug, "\n");
    }
    w.index += n;
});

// progbits returns the length of the bit stream encoded by the program p.
private static long progbits(slice<byte> p) => func((_, panic, _) => {
    long n = default;
    while (len(p) > 0) {
        var x = p[0];
        p = p[(int)1..];
        if (x == 0) {
            break;
        }
        if (x & 0x80 == 0) {
            var count = x & ~0x80;
            n += int64(count);
            p = p[(int)(count + 7) / 8..];
            continue;
        }
        var nbit = int64(x & ~0x80);
        if (nbit == 0) {
            nbit, p = readvarint(p);
        }
        count = default;
        count, p = readvarint(p);
        n += nbit * count;
    }
    if (len(p) > 0) {
        println("gcprog: found end instruction after", n, "ptrs, with", len(p), "bytes remaining");
        panic("gcprog: extra data at end of program");
    }
    return n;
});

// readvarint reads a varint from p, returning the value and the remainder of p.
private static (long, slice<byte>) readvarint(slice<byte> p) {
    long _p0 = default;
    slice<byte> _p0 = default;

    long v = default;
    nuint nb = default;
    while (true) {
        var c = p[0];
        p = p[(int)1..];
        v |= int64(c & ~0x80) << (int)(nb);
        nb += 7;
        if (c & 0x80 == 0) {
            break;
        }
    }
    return (v, p);
}

// lit adds a single literal bit to w.
private static void lit(this ptr<Writer> _addr_w, byte x) {
    ref Writer w = ref _addr_w.val;

    if (w.nb == progMaxLiteral) {
        w.flushlit();
    }
    w.b[w.nb] = x;
    w.nb++;
    w.index++;
}

// varint emits the varint encoding of x.
private static void varint(this ptr<Writer> _addr_w, long x) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    if (x < 0) {
        panic("gcprog: negative varint");
    }
    while (x >= 0x80) {
        w.@byte(byte(0x80 | x));
        x>>=7;
    }
    w.@byte(byte(x));
});

// flushlit flushes any pending literal bits.
private static void flushlit(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    if (w.nb == 0) {
        return ;
    }
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "gcprog: flush %d literals\n", w.nb);
        fmt.Fprintf(w.debug, "\t%v\n", w.b[..(int)w.nb]);
        fmt.Fprintf(w.debug, "\t%02x", byte(w.nb));
    }
    w.@byte(byte(w.nb));
    byte bits = default;
    for (nint i = 0; i < w.nb; i++) {
        bits |= w.b[i] << (int)(uint(i % 8));
        if ((i + 1) % 8 == 0) {
            if (w.debug != null) {
                fmt.Fprintf(w.debug, " %02x", bits);
            }
            w.@byte(bits);
            bits = 0;
        }
    }
    if (w.nb % 8 != 0) {
        if (w.debug != null) {
            fmt.Fprintf(w.debug, " %02x", bits);
        }
        w.@byte(bits);
    }
    if (w.debug != null) {
        fmt.Fprintf(w.debug, "\n");
    }
    w.nb = 0;
}

} // end gcprog_package
