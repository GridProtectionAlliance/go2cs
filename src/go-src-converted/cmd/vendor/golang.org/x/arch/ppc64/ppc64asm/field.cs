// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2020 October 08 04:44:43 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\field.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace ppc64
{
    public static partial class ppc64asm_package
    {
        // A BitField is a bit-field in a 32-bit word.
        // Bits are counted from 0 from the MSB to 31 as the LSB.
        public partial struct BitField
        {
            public byte Offs; // the offset of the left-most bit.
            public byte Bits; // length in bits.
        }

        public static @string String(this BitField b)
        {
            if (b.Bits > 1L)
            {
                return fmt.Sprintf("[%d:%d]", b.Offs, int(b.Offs + b.Bits) - 1L);
            }
            else if (b.Bits == 1L)
            {
                return fmt.Sprintf("[%d]", b.Offs);
            }
            else
            {
                return fmt.Sprintf("[%d, len=0]", b.Offs);
            }

        }

        // Parse extracts the bitfield b from i, and return it as an unsigned integer.
        // Parse will panic if b is invalid.
        public static uint Parse(this BitField b, uint i) => func((_, panic, __) =>
        {
            if (b.Bits > 32L || b.Bits == 0L || b.Offs > 31L || b.Offs + b.Bits > 32L)
            {
                panic(fmt.Sprintf("invalid bitfiled %v", b));
            }

            return (i >> (int)((32L - b.Offs - b.Bits))) & ((1L << (int)(b.Bits)) - 1L);

        });

        // ParseSigned extracts the bitfield b from i, and return it as a signed integer.
        // ParseSigned will panic if b is invalid.
        public static int ParseSigned(this BitField b, uint i)
        {
            var u = int32(b.Parse(i));
            return u << (int)((32L - b.Bits)) >> (int)((32L - b.Bits));
        }

        // BitFields is a series of BitFields representing a single number.
        public partial struct BitFields // : slice<BitField>
        {
        }

        public static @string String(this BitFields bs)
        {
            var ss = make_slice<@string>(len(bs));
            foreach (var (i, bf) in bs)
            {
                ss[i] = bf.String();
            }
            return fmt.Sprintf("<%s>", strings.Join(ss, "|"));

        }

        private static void Append(this ptr<BitFields> _addr_bs, BitField b)
        {
            ref BitFields bs = ref _addr_bs.val;

            bs.val = append(bs.val, b);
        }

        // parse extracts the bitfields from i, concatenate them and return the result
        // as an unsigned integer and the total length of all the bitfields.
        // parse will panic if any bitfield in b is invalid, but it doesn't check if
        // the sequence of bitfields is reasonable.
        public static (uint, byte) parse(this BitFields bs, uint i)
        {
            uint u = default;
            byte Bits = default;

            foreach (var (_, b) in bs)
            {
                u = (u << (int)(b.Bits)) | b.Parse(i);
                Bits += b.Bits;
            }
            return (u, Bits);

        }

        // Parse extracts the bitfields from i, concatenate them and return the result
        // as an unsigned integer. Parse will panic if any bitfield in b is invalid.
        public static uint Parse(this BitFields bs, uint i)
        {
            var (u, _) = bs.parse(i);
            return u;
        }

        // Parse extracts the bitfields from i, concatenate them and return the result
        // as a signed integer. Parse will panic if any bitfield in b is invalid.
        public static int ParseSigned(this BitFields bs, uint i)
        {
            var (u, l) = bs.parse(i);
            return int32(u) << (int)((32L - l)) >> (int)((32L - l));
        }
    }
}}}}}}}
