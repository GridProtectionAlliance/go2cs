// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2022 March 06 23:25:04 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\field.go
using fmt = go.fmt_package;
using strings = go.strings_package;

namespace go.cmd.vendor.golang.org.x.arch.ppc64;

public static partial class ppc64asm_package {

    // A BitField is a bit-field in a 32-bit word.
    // Bits are counted from 0 from the MSB to 31 as the LSB.
public partial struct BitField {
    public byte Offs; // the offset of the left-most bit.
    public byte Bits; // length in bits.
// This instruction word holding this field.
// It is always 0 for ISA < 3.1 instructions. It is
// in decoding order. (0 == prefix, 1 == suffix on ISA 3.1)
    public byte Word;
}

public static @string String(this BitField b) {
    if (b.Bits > 1) {
        return fmt.Sprintf("[%d:%d]", b.Offs, int(b.Offs + b.Bits) - 1);
    }
    else if (b.Bits == 1) {
        return fmt.Sprintf("[%d]", b.Offs);
    }
    else
 {
        return fmt.Sprintf("[%d, len=0]", b.Offs);
    }
}

// Parse extracts the bitfield b from i, and return it as an unsigned integer.
// Parse will panic if b is invalid.
public static uint Parse(this BitField b, array<uint> i) => func((_, panic, _) => {
    i = i.Clone();

    if (b.Bits > 32 || b.Bits == 0 || b.Offs > 31 || b.Offs + b.Bits > 32) {
        panic(fmt.Sprintf("invalid bitfiled %v", b));
    }
    return (i[b.Word] >> (int)((32 - b.Offs - b.Bits))) & ((1 << (int)(b.Bits)) - 1);

});

// ParseSigned extracts the bitfield b from i, and return it as a signed integer.
// ParseSigned will panic if b is invalid.
public static int ParseSigned(this BitField b, array<uint> i) {
    i = i.Clone();

    var u = int32(b.Parse(i));
    return u << (int)((32 - b.Bits)) >> (int)((32 - b.Bits));
}

// BitFields is a series of BitFields representing a single number.
public partial struct BitFields { // : slice<BitField>
}

public static @string String(this BitFields bs) {
    var ss = make_slice<@string>(len(bs));
    foreach (var (i, bf) in bs) {
        ss[i] = bf.String();
    }    return fmt.Sprintf("<%s>", strings.Join(ss, "|"));
}

private static void Append(this ptr<BitFields> _addr_bs, BitField b) {
    ref BitFields bs = ref _addr_bs.val;

    bs.val = append(bs.val, b);
}

// parse extracts the bitfields from i, concatenate them and return the result
// as an unsigned integer and the total length of all the bitfields.
// parse will panic if any bitfield in b is invalid, but it doesn't check if
// the sequence of bitfields is reasonable.
public static (ulong, byte) parse(this BitFields bs, array<uint> i) {
    ulong u = default;
    byte Bits = default;
    i = i.Clone();

    foreach (var (_, b) in bs) {
        u = (uint64(u) << (int)(b.Bits)) | uint64(b.Parse(i));
        Bits += b.Bits;
    }    return (u, Bits);
}

// Parse extracts the bitfields from i, concatenate them and return the result
// as an unsigned integer. Parse will panic if any bitfield in b is invalid.
public static ulong Parse(this BitFields bs, array<uint> i) {
    i = i.Clone();

    var (u, _) = bs.parse(i);
    return u;
}

// Parse extracts the bitfields from i, concatenate them and return the result
// as a signed integer. Parse will panic if any bitfield in b is invalid.
public static long ParseSigned(this BitFields bs, array<uint> i) {
    i = i.Clone();

    var (u, l) = bs.parse(i);
    return int64(u) << (int)((64 - l)) >> (int)((64 - l));
}

} // end ppc64asm_package
