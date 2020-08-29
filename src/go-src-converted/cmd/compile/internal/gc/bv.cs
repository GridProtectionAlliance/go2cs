// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:25:56 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bv.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static readonly long wordBits = 32L;
        private static readonly var wordMask = wordBits - 1L;
        private static readonly long wordShift = 5L;

        // A bvec is a bit vector.
        private partial struct bvec
        {
            public int n; // number of bits in vector
            public slice<uint> b; // words holding bits
        }

        private static bvec bvalloc(int n)
        {
            var nword = (n + wordBits - 1L) / wordBits;
            return new bvec(n,make([]uint32,nword));
        }

        private partial struct bulkBvec
        {
            public slice<uint> words;
            public int nbit;
            public int nword;
        }

        private static bulkBvec bvbulkalloc(int nbit, int count)
        {
            var nword = (nbit + wordBits - 1L) / wordBits;
            var size = int64(nword) * int64(count);
            if (int64(int32(size * 4L)) != size * 4L)
            {
                Fatalf("bvbulkalloc too big: nbit=%d count=%d nword=%d size=%d", nbit, count, nword, size);
            }
            return new bulkBvec(words:make([]uint32,size),nbit:nbit,nword:nword,);
        }

        private static bvec next(this ref bulkBvec b)
        {
            bvec @out = new bvec(b.nbit,b.words[:b.nword]);
            b.words = b.words[b.nword..];
            return out;
        }

        private static bool Eq(this bvec bv1, bvec bv2)
        {
            if (bv1.n != bv2.n)
            {
                Fatalf("bvequal: lengths %d and %d are not equal", bv1.n, bv2.n);
            }
            foreach (var (i, x) in bv1.b)
            {
                if (x != bv2.b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static void Copy(this bvec dst, bvec src)
        {
            copy(dst.b, src.b);
        }

        private static bool Get(this bvec bv, int i)
        {
            if (i < 0L || i >= bv.n)
            {
                Fatalf("bvget: index %d is out of bounds with length %d\n", i, bv.n);
            }
            var mask = uint32(1L << (int)(uint(i % wordBits)));
            return bv.b[i >> (int)(wordShift)] & mask != 0L;
        }

        private static void Set(this bvec bv, int i)
        {
            if (i < 0L || i >= bv.n)
            {
                Fatalf("bvset: index %d is out of bounds with length %d\n", i, bv.n);
            }
            var mask = uint32(1L << (int)(uint(i % wordBits)));
            bv.b[i / wordBits] |= mask;
        }

        private static void Unset(this bvec bv, int i)
        {
            if (i < 0L || i >= bv.n)
            {
                Fatalf("bvunset: index %d is out of bounds with length %d\n", i, bv.n);
            }
            var mask = uint32(1L << (int)(uint(i % wordBits)));
            bv.b[i / wordBits] &= mask;
        }

        // bvnext returns the smallest index >= i for which bvget(bv, i) == 1.
        // If there is no such index, bvnext returns -1.
        private static int Next(this bvec bv, int i)
        {
            if (i >= bv.n)
            {
                return -1L;
            } 

            // Jump i ahead to next word with bits.
            if (bv.b[i >> (int)(wordShift)] >> (int)(uint(i & wordMask)) == 0L)
            {
                i &= wordMask;
                i += wordBits;
                while (i < bv.n && bv.b[i >> (int)(wordShift)] == 0L)
                {
                    i += wordBits;
                }

            }
            if (i >= bv.n)
            {
                return -1L;
            } 

            // Find 1 bit.
            var w = bv.b[i >> (int)(wordShift)] >> (int)(uint(i & wordMask));

            while (w & 1L == 0L)
            {
                w >>= 1L;
                i++;
            }


            return i;
        }

        private static bool IsEmpty(this bvec bv)
        {
            {
                var i = int32(0L);

                while (i < bv.n)
                {
                    if (bv.b[i >> (int)(wordShift)] != 0L)
                    {
                        return false;
                    i += wordBits;
                    }
                }

            }
            return true;
        }

        private static void Not(this bvec bv)
        {
            var i = int32(0L);
            var w = int32(0L);
            while (i < bv.n)
            {
                bv.b[w] = ~bv.b[w];
                i = i + wordBits;
            w = w + 1L;
            }

        }

        // union
        private static void Or(this bvec dst, bvec src1, bvec src2)
        {
            foreach (var (i, x) in src1.b)
            {
                dst.b[i] = x | src2.b[i];
            }
        }

        // intersection
        private static void And(this bvec dst, bvec src1, bvec src2)
        {
            foreach (var (i, x) in src1.b)
            {
                dst.b[i] = x & src2.b[i];
            }
        }

        // difference
        private static void AndNot(this bvec dst, bvec src1, bvec src2)
        {
            foreach (var (i, x) in src1.b)
            {
                dst.b[i] = x & ~src2.b[i];
            }
        }

        private static @string String(this bvec bv)
        {
            var s = make_slice<byte>(2L + bv.n);
            copy(s, "#*");
            for (var i = int32(0L); i < bv.n; i++)
            {
                var ch = byte('0');
                if (bv.Get(i))
                {
                    ch = '1';
                }
                s[2L + i] = ch;
            }

            return string(s);
        }

        private static void Clear(this bvec bv)
        {
            foreach (var (i) in bv.b)
            {
                bv.b[i] = 0L;
            }
        }
    }
}}}}
