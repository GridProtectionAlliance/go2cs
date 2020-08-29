// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:25:51 UTC
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

        private static void set(this ref bitset8 f, byte mask, bool b)
        {
            if (b)
            {
                (uint8.Value)(f).Value;

                mask;
            }
            else
            {
                (uint8.Value)(f).Value;

                mask;
            }
        }

        private partial struct bitset16 // : ushort
        {
        }

        private static void set(this ref bitset16 f, ushort mask, bool b)
        {
            if (b)
            {
                (uint16.Value)(f).Value;

                mask;
            }
            else
            {
                (uint16.Value)(f).Value;

                mask;
            }
        }

        private partial struct bitset32 // : uint
        {
        }

        private static void set(this ref bitset32 f, uint mask, bool b)
        {
            if (b)
            {
                (uint32.Value)(f).Value;

                mask;
            }
            else
            {
                (uint32.Value)(f).Value;

                mask;
            }
        }

        private static byte get2(this bitset32 f, byte shift)
        {
            return uint8(f >> (int)(shift)) & 3L;
        }

        // set2 sets two bits in f using the bottom two bits of b.
        private static void set2(this ref bitset32 f, byte shift, byte b)
        { 
            // Clear old bits.
            (uint32.Value)(f).Value;

            3L << (int)(shift) * (uint32.Value)(f);

            uint32(b & 3L) << (int)(shift);
        }

        private static byte get3(this bitset32 f, byte shift)
        {
            return uint8(f >> (int)(shift)) & 7L;
        }

        // set3 sets three bits in f using the bottom three bits of b.
        private static void set3(this ref bitset32 f, byte shift, byte b)
        { 
            // Clear old bits.
            (uint32.Value)(f).Value;

            7L << (int)(shift) * (uint32.Value)(f);

            uint32(b & 7L) << (int)(shift);
        }
    }
}}}}
