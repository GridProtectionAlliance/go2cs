// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:27:59 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bitset.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private partial struct bitset8 // : byte
        {
        }

        private static void set(this ptr<bitset8> _addr_f, byte mask, bool b)
        {
            ref bitset8 f = ref _addr_f.val;

            if (b)
            {
                (uint8.val)(f).val;

                mask;

            }
            else
            {
                (uint8.val)(f).val;

                mask;

            }

        }

        private partial struct bitset16 // : ushort
        {
        }

        private static void set(this ptr<bitset16> _addr_f, ushort mask, bool b)
        {
            ref bitset16 f = ref _addr_f.val;

            if (b)
            {
                (uint16.val)(f).val;

                mask;

            }
            else
            {
                (uint16.val)(f).val;

                mask;

            }

        }

        private partial struct bitset32 // : uint
        {
        }

        private static void set(this ptr<bitset32> _addr_f, uint mask, bool b)
        {
            ref bitset32 f = ref _addr_f.val;

            if (b)
            {
                (uint32.val)(f).val;

                mask;

            }
            else
            {
                (uint32.val)(f).val;

                mask;

            }

        }

        private static byte get2(this bitset32 f, byte shift)
        {
            return uint8(f >> (int)(shift)) & 3L;
        }

        // set2 sets two bits in f using the bottom two bits of b.
        private static void set2(this ptr<bitset32> _addr_f, byte shift, byte b)
        {
            ref bitset32 f = ref _addr_f.val;
 
            // Clear old bits.
            (uint32.val)(f).val;

            3L << (int)(shift) * (uint32.val)(f);

            uint32(b & 3L) << (int)(shift);

        }

        private static byte get3(this bitset32 f, byte shift)
        {
            return uint8(f >> (int)(shift)) & 7L;
        }

        // set3 sets three bits in f using the bottom three bits of b.
        private static void set3(this ptr<bitset32> _addr_f, byte shift, byte b)
        {
            ref bitset32 f = ref _addr_f.val;
 
            // Clear old bits.
            (uint32.val)(f).val;

            7L << (int)(shift) * (uint32.val)(f);

            uint32(b & 7L) << (int)(shift);

        }
    }
}}}}
