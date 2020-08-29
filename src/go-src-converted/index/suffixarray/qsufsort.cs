// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This algorithm is based on "Faster Suffix Sorting"
//   by N. Jesper Larsson and Kunihiko Sadakane
// paper: http://www.larsson.dogma.net/ssrev-tr.pdf
// code:  http://www.larsson.dogma.net/qsufsort.c

// This algorithm computes the suffix array sa by computing its inverse.
// Consecutive groups of suffixes in sa are labeled as sorted groups or
// unsorted groups. For a given pass of the sorter, all suffixes are ordered
// up to their first h characters, and sa is h-ordered. Suffixes in their
// final positions and unambiguously sorted in h-order are in a sorted group.
// Consecutive groups of suffixes with identical first h characters are an
// unsorted group. In each pass of the algorithm, unsorted groups are sorted
// according to the group number of their following suffix.

// In the implementation, if sa[i] is negative, it indicates that i is
// the first element of a sorted group of length -sa[i], and can be skipped.
// An unsorted group sa[i:k] is given the group number of the index of its
// last element, k-1. The group numbers are stored in the inverse slice (inv),
// and when all groups are sorted, this slice is the inverse suffix array.

// package suffixarray -- go2cs converted at 2020 August 29 10:11:05 UTC
// import "index/suffixarray" ==> using suffixarray = go.index.suffixarray_package
// Original source: C:\Go\src\index\suffixarray\qsufsort.go
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace index
{
    public static partial class suffixarray_package
    {
        private static slice<long> qsufsort(slice<byte> data)
        { 
            // initial sorting by first byte of suffix
            var sa = sortedByFirstByte(data);
            if (len(sa) < 2L)
            {
                return sa;
            }
            var inv = initGroups(sa, data); 

            // the index starts 1-ordered
            suffixSortable sufSortable = ref new suffixSortable(sa:sa,inv:inv,h:1);

            while (sa[0L] > -len(sa))
            { // until all suffixes are one big sorted group
                // The suffixes are h-ordered, make them 2*h-ordered
                long pi = 0L; // pi is first position of first group
                long sl = 0L; // sl is negated length of sorted groups
                while (pi < len(sa))
                {
                    {
                        var s = sa[pi];

                        if (s < 0L)
                        { // if pi starts sorted group
                            pi -= s; // skip over sorted group
                            sl += s; // add negated length to sl
                        }
                        else
                        { // if pi starts unsorted group
                            if (sl != 0L)
                            {
                                sa[pi + sl] = sl; // combine sorted groups before pi
                                sl = 0L;
                            }
                            var pk = inv[s] + 1L; // pk-1 is last position of unsorted group
                            sufSortable.sa = sa[pi..pk];
                            sort.Sort(sufSortable);
                            sufSortable.updateGroups(pi);
                            pi = pk; // next group
                        }
                    }
                }
                if (sl != 0L)
                { // if the array ends with a sorted group
                    sa[pi + sl] = sl; // combine sorted groups at end of sa
                }
                sufSortable.h *= 2L; // double sorted depth
            }

            foreach (var (i) in sa)
            { // reconstruct suffix array from inverse
                sa[inv[i]] = i;
            }            return sa;
        }

        private static slice<long> sortedByFirstByte(slice<byte> data)
        { 
            // total byte counts
            array<long> count = new array<long>(256L);
            {
                var b__prev1 = b;

                foreach (var (_, __b) in data)
                {
                    b = __b;
                    count[b]++;
                } 
                // make count[b] equal index of first occurrence of b in sorted array

                b = b__prev1;
            }

            long sum = 0L;
            {
                var b__prev1 = b;

                foreach (var (__b) in count)
                {
                    b = __b;
                    count[b] = sum;
                    sum = count[b] + sum;
                } 
                // iterate through bytes, placing index into the correct spot in sa

                b = b__prev1;
            }

            var sa = make_slice<long>(len(data));
            {
                var b__prev1 = b;

                foreach (var (__i, __b) in data)
                {
                    i = __i;
                    b = __b;
                    sa[count[b]] = i;
                    count[b]++;
                }

                b = b__prev1;
            }

            return sa;
        }

        private static slice<long> initGroups(slice<long> sa, slice<byte> data)
        { 
            // label contiguous same-letter groups with the same group number
            var inv = make_slice<long>(len(data));
            var prevGroup = len(sa) - 1L;
            var groupByte = data[sa[prevGroup]];
            {
                var i__prev1 = i;

                for (var i = len(sa) - 1L; i >= 0L; i--)
                {
                    {
                        var b = data[sa[i]];

                        if (b < groupByte)
                        {
                            if (prevGroup == i + 1L)
                            {
                                sa[i + 1L] = -1L;
                            }
                            groupByte = b;
                            prevGroup = i;
                        }

                    }
                    inv[sa[i]] = prevGroup;
                    if (prevGroup == 0L)
                    {
                        sa[0L] = -1L;
                    }
                } 
                // Separate out the final suffix to the start of its group.
                // This is necessary to ensure the suffix "a" is before "aba"
                // when using a potentially unstable sort.


                i = i__prev1;
            } 
            // Separate out the final suffix to the start of its group.
            // This is necessary to ensure the suffix "a" is before "aba"
            // when using a potentially unstable sort.
            var lastByte = data[len(data) - 1L];
            long s = -1L;
            {
                var i__prev1 = i;

                foreach (var (__i) in sa)
                {
                    i = __i;
                    if (sa[i] >= 0L)
                    {
                        if (data[sa[i]] == lastByte && s == -1L)
                        {
                            s = i;
                        }
                        if (sa[i] == len(sa) - 1L)
                        {
                            sa[i] = sa[s];
                            sa[s] = sa[i];
                            inv[sa[s]] = s;
                            sa[s] = -1L; // mark it as an isolated sorted group
                            break;
                        }
                    }
                }

                i = i__prev1;
            }

            return inv;
        }

        private partial struct suffixSortable
        {
            public slice<long> sa;
            public slice<long> inv;
            public long h;
            public slice<long> buf; // common scratch space
        }

        private static long Len(this ref suffixSortable x)
        {
            return len(x.sa);
        }
        private static bool Less(this ref suffixSortable x, long i, long j)
        {
            return x.inv[x.sa[i] + x.h] < x.inv[x.sa[j] + x.h];
        }
        private static void Swap(this ref suffixSortable x, long i, long j)
        {
            x.sa[i] = x.sa[j];
            x.sa[j] = x.sa[i];

        }

        private static void updateGroups(this ref suffixSortable x, long offset)
        {
            var bounds = x.buf[0L..0L];
            var group = x.inv[x.sa[0L] + x.h];
            {
                long i__prev1 = i;

                for (long i = 1L; i < len(x.sa); i++)
                {
                    {
                        var g = x.inv[x.sa[i] + x.h];

                        if (g > group)
                        {
                            bounds = append(bounds, i);
                            group = g;
                        }

                    }
                }


                i = i__prev1;
            }
            bounds = append(bounds, len(x.sa));
            x.buf = bounds; 

            // update the group numberings after all new groups are determined
            long prev = 0L;
            foreach (var (_, b) in bounds)
            {
                {
                    long i__prev2 = i;

                    for (i = prev; i < b; i++)
                    {
                        x.inv[x.sa[i]] = offset + b - 1L;
                    }


                    i = i__prev2;
                }
                if (b - prev == 1L)
                {
                    x.sa[prev] = -1L;
                }
                prev = b;
            }
        }
    }
}}
