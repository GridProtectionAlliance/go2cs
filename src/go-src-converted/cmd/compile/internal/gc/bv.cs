// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:40:41 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bv.go
using bits = go.math.bits_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static readonly long wordBits = (long)32L;
        private static readonly var wordMask = wordBits - 1L;
        private static readonly long wordShift = (long)5L;


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

        private static bvec next(this ptr<bulkBvec> _addr_b)
        {
            ref bulkBvec b = ref _addr_b.val;

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
            i += int32(bits.TrailingZeros32(w));

            return i;

        }

        private static bool IsEmpty(this bvec bv)
        {
            foreach (var (_, x) in bv.b)
            {
                if (x != 0L)
                {
                    return false;
                }

            }
            return true;

        }

        private static void Not(this bvec bv)
        {
            foreach (var (i, x) in bv.b)
            {
                bv.b[i] = ~x;
            }

        }

        // union
        private static void Or(this bvec dst, bvec src1, bvec src2)
        {
            if (len(src1.b) == 0L)
            {
                return ;
            }

            _ = dst.b[len(src1.b) - 1L];
            _ = src2.b[len(src1.b) - 1L]; // hoist bounds checks out of the loop

            foreach (var (i, x) in src1.b)
            {
                dst.b[i] = x | src2.b[i];
            }

        }

        // intersection
        private static void And(this bvec dst, bvec src1, bvec src2)
        {
            if (len(src1.b) == 0L)
            {
                return ;
            }

            _ = dst.b[len(src1.b) - 1L];
            _ = src2.b[len(src1.b) - 1L]; // hoist bounds checks out of the loop

            foreach (var (i, x) in src1.b)
            {
                dst.b[i] = x & src2.b[i];
            }

        }

        // difference
        private static void AndNot(this bvec dst, bvec src1, bvec src2)
        {
            if (len(src1.b) == 0L)
            {
                return ;
            }

            _ = dst.b[len(src1.b) - 1L];
            _ = src2.b[len(src1.b) - 1L]; // hoist bounds checks out of the loop

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

        // FNV-1 hash function constants.
        public static readonly long H0 = (long)2166136261L;
        public static readonly long Hp = (long)16777619L;


        private static uint hashbitmap(uint h, bvec bv)
        {
            var n = int((bv.n + 31L) / 32L);
            for (long i = 0L; i < n; i++)
            {
                var w = bv.b[i];
                h = (h * Hp) ^ (w & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(8L)) & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(16L)) & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(24L)) & 0xffUL);
            }


            return h;

        }

        // bvecSet is a set of bvecs, in initial insertion order.
        private partial struct bvecSet
        {
            public slice<long> index; // hash -> uniq index. -1 indicates empty slot.
            public slice<bvec> uniq; // unique bvecs, in insertion order
        }

        private static void grow(this ptr<bvecSet> _addr_m)
        {
            ref bvecSet m = ref _addr_m.val;
 
            // Allocate new index.
            var n = len(m.index) * 2L;
            if (n == 0L)
            {
                n = 32L;
            }

            var newIndex = make_slice<long>(n);
            {
                var i__prev1 = i;

                foreach (var (__i) in newIndex)
                {
                    i = __i;
                    newIndex[i] = -1L;
                } 

                // Rehash into newIndex.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i, __bv) in m.uniq)
                {
                    i = __i;
                    bv = __bv;
                    var h = hashbitmap(H0, bv) % uint32(len(newIndex));
                    while (true)
                    {
                        var j = newIndex[h];
                        if (j < 0L)
                        {
                            newIndex[h] = i;
                            break;
                        }

                        h++;
                        if (h == uint32(len(newIndex)))
                        {
                            h = 0L;
                        }

                    }


                }

                i = i__prev1;
            }

            m.index = newIndex;

        }

        // add adds bv to the set and returns its index in m.extractUniqe.
        // The caller must not modify bv after this.
        private static long add(this ptr<bvecSet> _addr_m, bvec bv)
        {
            ref bvecSet m = ref _addr_m.val;

            if (len(m.uniq) * 4L >= len(m.index))
            {
                m.grow();
            }

            var index = m.index;
            var h = hashbitmap(H0, bv) % uint32(len(index));
            while (true)
            {
                var j = index[h];
                if (j < 0L)
                { 
                    // New bvec.
                    index[h] = len(m.uniq);
                    m.uniq = append(m.uniq, bv);
                    return len(m.uniq) - 1L;

                }

                var jlive = m.uniq[j];
                if (bv.Eq(jlive))
                { 
                    // Existing bvec.
                    return j;

                }

                h++;
                if (h == uint32(len(index)))
                {
                    h = 0L;
                }

            }


        }

        // extractUniqe returns this slice of unique bit vectors in m, as
        // indexed by the result of bvecSet.add.
        private static slice<bvec> extractUniqe(this ptr<bvecSet> _addr_m)
        {
            ref bvecSet m = ref _addr_m.val;

            return m.uniq;
        }
    }
}}}}
