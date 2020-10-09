// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mpallocbits.go
using sys = go.runtime.@internal.sys_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // pageBits is a bitmap representing one bit per page in a palloc chunk.
        private partial struct pageBits // : array<ulong>
        {
        }

        // get returns the value of the i'th bit in the bitmap.
        private static ulong get(this ptr<pageBits> _addr_b, ulong i)
        {
            ref pageBits b = ref _addr_b.val;

            return uint((b[i / 64L] >> (int)((i % 64L))) & 1L);
        }

        // block64 returns the 64-bit aligned block of bits containing the i'th bit.
        private static ulong block64(this ptr<pageBits> _addr_b, ulong i)
        {
            ref pageBits b = ref _addr_b.val;

            return b[i / 64L];
        }

        // set sets bit i of pageBits.
        private static void set(this ptr<pageBits> _addr_b, ulong i)
        {
            ref pageBits b = ref _addr_b.val;

            b[i / 64L] |= 1L << (int)((i % 64L));
        }

        // setRange sets bits in the range [i, i+n).
        private static void setRange(this ptr<pageBits> _addr_b, ulong i, ulong n)
        {
            ref pageBits b = ref _addr_b.val;

            _ = b[i / 64L];
            if (n == 1L)
            { 
                // Fast path for the n == 1 case.
                b.set(i);
                return ;

            } 
            // Set bits [i, j].
            var j = i + n - 1L;
            if (i / 64L == j / 64L)
            {
                b[i / 64L] |= ((uint64(1L) << (int)(n)) - 1L) << (int)((i % 64L));
                return ;
            }

            _ = b[j / 64L]; 
            // Set leading bits.
            b[i / 64L] |= ~uint64(0L) << (int)((i % 64L));
            for (var k = i / 64L + 1L; k < j / 64L; k++)
            {
                b[k] = ~uint64(0L);
            } 
            // Set trailing bits.
 
            // Set trailing bits.
            b[j / 64L] |= (uint64(1L) << (int)((j % 64L + 1L))) - 1L;

        }

        // setAll sets all the bits of b.
        private static void setAll(this ptr<pageBits> _addr_b)
        {
            ref pageBits b = ref _addr_b.val;

            foreach (var (i) in b)
            {
                b[i] = ~uint64(0L);
            }

        }

        // clear clears bit i of pageBits.
        private static void clear(this ptr<pageBits> _addr_b, ulong i)
        {
            ref pageBits b = ref _addr_b.val;

            b[i / 64L] &= 1L << (int)((i % 64L));
        }

        // clearRange clears bits in the range [i, i+n).
        private static void clearRange(this ptr<pageBits> _addr_b, ulong i, ulong n)
        {
            ref pageBits b = ref _addr_b.val;

            _ = b[i / 64L];
            if (n == 1L)
            { 
                // Fast path for the n == 1 case.
                b.clear(i);
                return ;

            } 
            // Clear bits [i, j].
            var j = i + n - 1L;
            if (i / 64L == j / 64L)
            {
                b[i / 64L] &= ((uint64(1L) << (int)(n)) - 1L) << (int)((i % 64L));
                return ;
            }

            _ = b[j / 64L]; 
            // Clear leading bits.
            b[i / 64L] &= ~uint64(0L) << (int)((i % 64L));
            for (var k = i / 64L + 1L; k < j / 64L; k++)
            {
                b[k] = 0L;
            } 
            // Clear trailing bits.
 
            // Clear trailing bits.
            b[j / 64L] &= (uint64(1L) << (int)((j % 64L + 1L))) - 1L;

        }

        // clearAll frees all the bits of b.
        private static void clearAll(this ptr<pageBits> _addr_b)
        {
            ref pageBits b = ref _addr_b.val;

            foreach (var (i) in b)
            {
                b[i] = 0L;
            }

        }

        // popcntRange counts the number of set bits in the
        // range [i, i+n).
        private static ulong popcntRange(this ptr<pageBits> _addr_b, ulong i, ulong n)
        {
            ulong s = default;
            ref pageBits b = ref _addr_b.val;

            if (n == 1L)
            {
                return uint((b[i / 64L] >> (int)((i % 64L))) & 1L);
            }

            _ = b[i / 64L];
            var j = i + n - 1L;
            if (i / 64L == j / 64L)
            {
                return uint(sys.OnesCount64((b[i / 64L] >> (int)((i % 64L))) & ((1L << (int)(n)) - 1L)));
            }

            _ = b[j / 64L];
            s += uint(sys.OnesCount64(b[i / 64L] >> (int)((i % 64L))));
            for (var k = i / 64L + 1L; k < j / 64L; k++)
            {
                s += uint(sys.OnesCount64(b[k]));
            }

            s += uint(sys.OnesCount64(b[j / 64L] & ((1L << (int)((j % 64L + 1L))) - 1L)));
            return ;

        }

        // pallocBits is a bitmap that tracks page allocations for at most one
        // palloc chunk.
        //
        // The precise representation is an implementation detail, but for the
        // sake of documentation, 0s are free pages and 1s are allocated pages.
        private partial struct pallocBits // : pageBits
        {
        }

        // consec8tab is a table containing the number of consecutive
        // zero bits for any uint8 value.
        //
        // The table is generated by calling consec8(i) for each
        // possible uint8 value, which is defined as:
        //
        // // consec8 counts the maximum number of consecutive 0 bits
        // // in a uint8.
        // func consec8(n uint8) int {
        //     n = ^n
        //     i := 0
        //     for n != 0 {
        //         n &= (n << 1)
        //         i++
        //     }
        //     return i
        // }
        private static array<ulong> consec8tab = new array<ulong>(new ulong[] { 8, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 4, 3, 3, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 4, 3, 2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 6, 5, 4, 4, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 4, 3, 2, 2, 2, 1, 1, 1, 3, 2, 1, 1, 2, 1, 1, 1, 5, 4, 3, 3, 2, 2, 2, 2, 3, 2, 1, 1, 2, 1, 1, 1, 4, 3, 2, 2, 2, 1, 1, 1, 3, 2, 1, 1, 2, 1, 1, 1, 7, 6, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 4, 3, 2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 5, 4, 3, 3, 2, 2, 2, 2, 3, 2, 1, 1, 2, 1, 1, 1, 4, 3, 2, 2, 2, 1, 1, 1, 3, 2, 1, 1, 2, 1, 1, 1, 6, 5, 4, 4, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 4, 3, 2, 2, 2, 1, 1, 1, 3, 2, 1, 1, 2, 1, 1, 1, 5, 4, 3, 3, 2, 2, 2, 2, 3, 2, 1, 1, 2, 1, 1, 1, 4, 3, 2, 2, 2, 1, 1, 1, 3, 2, 1, 1, 2, 1, 1, 0 });

        // summarize returns a packed summary of the bitmap in pallocBits.
        private static pallocSum summarize(this ptr<pallocBits> _addr_b)
        {
            ref pallocBits b = ref _addr_b.val;
 
            // TODO(mknyszek): There may be something more clever to be done
            // here to make the summarize operation more efficient. For example,
            // we can compute start and end with 64-bit wide operations easily,
            // but max is a bit more complex. Perhaps there exists some way to
            // leverage the 64-bit start and end to our advantage?
            ulong start = default;            ulong max = default;            ulong end = default;

            for (long i = 0L; i < len(b); i++)
            {
                var a = b[i];
                {
                    long j = 0L;

                    while (j < 64L)
                    {
                        var k = uint8(a >> (int)(j)); 

                        // Compute start.
                        var si = uint(sys.TrailingZeros8(k));
                        if (start == uint(i * 64L + j))
                        {
                            start += si;
                        j += 8L;
                        } 

                        // Compute max.
                        if (end + si > max)
                        {
                            max = end + si;
                        }

                        {
                            var mi = consec8tab[k];

                            if (mi > max)
                            {
                                max = mi;
                            } 

                            // Compute end.

                        } 

                        // Compute end.
                        if (k == 0L)
                        {
                            end += 8L;
                        }
                        else
                        {
                            end = uint(sys.LeadingZeros8(k));
                        }

                    }

                }

            }

            return packPallocSum(start, max, end);

        }

        // find searches for npages contiguous free pages in pallocBits and returns
        // the index where that run starts, as well as the index of the first free page
        // it found in the search. searchIdx represents the first known free page and
        // where to begin the search from.
        //
        // If find fails to find any free space, it returns an index of ^uint(0) and
        // the new searchIdx should be ignored.
        //
        // Note that if npages == 1, the two returned values will always be identical.
        private static (ulong, ulong) find(this ptr<pallocBits> _addr_b, System.UIntPtr npages, ulong searchIdx)
        {
            ulong _p0 = default;
            ulong _p0 = default;
            ref pallocBits b = ref _addr_b.val;

            if (npages == 1L)
            {
                var addr = b.find1(searchIdx);
                return (addr, addr);
            }
            else if (npages <= 64L)
            {
                return b.findSmallN(npages, searchIdx);
            }

            return b.findLargeN(npages, searchIdx);

        }

        // find1 is a helper for find which searches for a single free page
        // in the pallocBits and returns the index.
        //
        // See find for an explanation of the searchIdx parameter.
        private static ulong find1(this ptr<pallocBits> _addr_b, ulong searchIdx)
        {
            ref pallocBits b = ref _addr_b.val;

            for (var i = searchIdx / 64L; i < uint(len(b)); i++)
            {
                var x = b[i];
                if (x == ~uint64(0L))
                {
                    continue;
                }

                return i * 64L + uint(sys.TrailingZeros64(~x));

            }

            return ~uint(0L);

        }

        // findSmallN is a helper for find which searches for npages contiguous free pages
        // in this pallocBits and returns the index where that run of contiguous pages
        // starts as well as the index of the first free page it finds in its search.
        //
        // See find for an explanation of the searchIdx parameter.
        //
        // Returns a ^uint(0) index on failure and the new searchIdx should be ignored.
        //
        // findSmallN assumes npages <= 64, where any such contiguous run of pages
        // crosses at most one aligned 64-bit boundary in the bits.
        private static (ulong, ulong) findSmallN(this ptr<pallocBits> _addr_b, System.UIntPtr npages, ulong searchIdx)
        {
            ulong _p0 = default;
            ulong _p0 = default;
            ref pallocBits b = ref _addr_b.val;

            var end = uint(0L);
            var newSearchIdx = ~uint(0L);
            for (var i = searchIdx / 64L; i < uint(len(b)); i++)
            {
                var bi = b[i];
                if (bi == ~uint64(0L))
                {
                    end = 0L;
                    continue;
                } 
                // First see if we can pack our allocation in the trailing
                // zeros plus the end of the last 64 bits.
                var start = uint(sys.TrailingZeros64(bi));
                if (newSearchIdx == ~uint(0L))
                { 
                    // The new searchIdx is going to be at these 64 bits after any
                    // 1s we file, so count trailing 1s.
                    newSearchIdx = i * 64L + uint(sys.TrailingZeros64(~bi));

                }

                if (end + start >= uint(npages))
                {
                    return (i * 64L - end, newSearchIdx);
                } 
                // Next, check the interior of the 64-bit chunk.
                var j = findBitRange64(~bi, uint(npages));
                if (j < 64L)
                {
                    return (i * 64L + j, newSearchIdx);
                }

                end = uint(sys.LeadingZeros64(bi));

            }

            return (~uint(0L), newSearchIdx);

        }

        // findLargeN is a helper for find which searches for npages contiguous free pages
        // in this pallocBits and returns the index where that run starts, as well as the
        // index of the first free page it found it its search.
        //
        // See alloc for an explanation of the searchIdx parameter.
        //
        // Returns a ^uint(0) index on failure and the new searchIdx should be ignored.
        //
        // findLargeN assumes npages > 64, where any such run of free pages
        // crosses at least one aligned 64-bit boundary in the bits.
        private static (ulong, ulong) findLargeN(this ptr<pallocBits> _addr_b, System.UIntPtr npages, ulong searchIdx)
        {
            ulong _p0 = default;
            ulong _p0 = default;
            ref pallocBits b = ref _addr_b.val;

            var start = ~uint(0L);
            var size = uint(0L);
            var newSearchIdx = ~uint(0L);
            for (var i = searchIdx / 64L; i < uint(len(b)); i++)
            {
                var x = b[i];
                if (x == ~uint64(0L))
                {
                    size = 0L;
                    continue;
                }

                if (newSearchIdx == ~uint(0L))
                { 
                    // The new searchIdx is going to be at these 64 bits after any
                    // 1s we file, so count trailing 1s.
                    newSearchIdx = i * 64L + uint(sys.TrailingZeros64(~x));

                }

                if (size == 0L)
                {
                    size = uint(sys.LeadingZeros64(x));
                    start = i * 64L + 64L - size;
                    continue;
                }

                var s = uint(sys.TrailingZeros64(x));
                if (s + size >= uint(npages))
                {
                    size += s;
                    return (start, newSearchIdx);
                }

                if (s < 64L)
                {
                    size = uint(sys.LeadingZeros64(x));
                    start = i * 64L + 64L - size;
                    continue;
                }

                size += 64L;

            }

            if (size < uint(npages))
            {
                return (~uint(0L), newSearchIdx);
            }

            return (start, newSearchIdx);

        }

        // allocRange allocates the range [i, i+n).
        private static void allocRange(this ptr<pallocBits> _addr_b, ulong i, ulong n)
        {
            ref pallocBits b = ref _addr_b.val;

            (pageBits.val)(b).setRange(i, n);
        }

        // allocAll allocates all the bits of b.
        private static void allocAll(this ptr<pallocBits> _addr_b)
        {
            ref pallocBits b = ref _addr_b.val;

            (pageBits.val)(b).setAll();
        }

        // free1 frees a single page in the pallocBits at i.
        private static void free1(this ptr<pallocBits> _addr_b, ulong i)
        {
            ref pallocBits b = ref _addr_b.val;

            (pageBits.val)(b).clear(i);
        }

        // free frees the range [i, i+n) of pages in the pallocBits.
        private static void free(this ptr<pallocBits> _addr_b, ulong i, ulong n)
        {
            ref pallocBits b = ref _addr_b.val;

            (pageBits.val)(b).clearRange(i, n);
        }

        // freeAll frees all the bits of b.
        private static void freeAll(this ptr<pallocBits> _addr_b)
        {
            ref pallocBits b = ref _addr_b.val;

            (pageBits.val)(b).clearAll();
        }

        // pages64 returns a 64-bit bitmap representing a block of 64 pages aligned
        // to 64 pages. The returned block of pages is the one containing the i'th
        // page in this pallocBits. Each bit represents whether the page is in-use.
        private static ulong pages64(this ptr<pallocBits> _addr_b, ulong i)
        {
            ref pallocBits b = ref _addr_b.val;

            return (pageBits.val)(b).block64(i);
        }

        // findBitRange64 returns the bit index of the first set of
        // n consecutive 1 bits. If no consecutive set of 1 bits of
        // size n may be found in c, then it returns an integer >= 64.
        private static ulong findBitRange64(ulong c, ulong n)
        {
            var i = uint(0L);
            var cont = uint(sys.TrailingZeros64(~c));
            while (cont < n && i < 64L)
            {
                i += cont;
                i += uint(sys.TrailingZeros64(c >> (int)(i)));
                cont = uint(sys.TrailingZeros64(~(c >> (int)(i))));
            }

            return i;

        }

        // pallocData encapsulates pallocBits and a bitmap for
        // whether or not a given page is scavenged in a single
        // structure. It's effectively a pallocBits with
        // additional functionality.
        //
        // Update the comment on (*pageAlloc).chunks should this
        // structure change.
        private partial struct pallocData
        {
            public ref pallocBits pallocBits => ref pallocBits_val;
            public pageBits scavenged;
        }

        // allocRange sets bits [i, i+n) in the bitmap to 1 and
        // updates the scavenged bits appropriately.
        private static void allocRange(this ptr<pallocData> _addr_m, ulong i, ulong n)
        {
            ref pallocData m = ref _addr_m.val;
 
            // Clear the scavenged bits when we alloc the range.
            m.pallocBits.allocRange(i, n);
            m.scavenged.clearRange(i, n);

        }

        // allocAll sets every bit in the bitmap to 1 and updates
        // the scavenged bits appropriately.
        private static void allocAll(this ptr<pallocData> _addr_m)
        {
            ref pallocData m = ref _addr_m.val;
 
            // Clear the scavenged bits when we alloc the range.
            m.pallocBits.allocAll();
            m.scavenged.clearAll();

        }
    }
}
