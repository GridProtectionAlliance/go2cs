// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run genzfunc.go

// Package sort provides primitives for sorting slices and user-defined
// collections.
// package sort -- go2cs converted at 2020 October 08 03:44:12 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Go\src\sort\sort.go

using static go.builtin;
using System;

namespace go
{
    public static partial class sort_package
    {
        // A type, typically a collection, that satisfies sort.Interface can be
        // sorted by the routines in this package. The methods require that the
        // elements of the collection be enumerated by an integer index.
        public partial interface Interface
        {
            bool Len(); // Less reports whether the element with
// index i should sort before the element with index j.
            bool Less(long i, long j); // Swap swaps the elements with indexes i and j.
            bool Swap(long i, long j);
        }

        // Insertion sort
        private static void insertionSort(Interface data, long a, long b)
        {
            for (var i = a + 1L; i < b; i++)
            {
                for (var j = i; j > a && data.Less(j, j - 1L); j--)
                {
                    data.Swap(j, j - 1L);
                }


            }


        }

        // siftDown implements the heap property on data[lo, hi).
        // first is an offset into the array where the root of the heap lies.
        private static void siftDown(Interface data, long lo, long hi, long first)
        {
            var root = lo;
            while (true)
            {
                long child = 2L * root + 1L;
                if (child >= hi)
                {
                    break;
                }

                if (child + 1L < hi && data.Less(first + child, first + child + 1L))
                {
                    child++;
                }

                if (!data.Less(first + root, first + child))
                {
                    return ;
                }

                data.Swap(first + root, first + child);
                root = child;

            }


        }

        private static void heapSort(Interface data, long a, long b)
        {
            var first = a;
            long lo = 0L;
            var hi = b - a; 

            // Build heap with greatest element at top.
            {
                var i__prev1 = i;

                for (var i = (hi - 1L) / 2L; i >= 0L; i--)
                {
                    siftDown(data, i, hi, first);
                } 

                // Pop elements, largest first, into end of data.


                i = i__prev1;
            } 

            // Pop elements, largest first, into end of data.
            {
                var i__prev1 = i;

                for (i = hi - 1L; i >= 0L; i--)
                {
                    data.Swap(first, first + i);
                    siftDown(data, lo, i, first);
                }


                i = i__prev1;
            }

        }

        // Quicksort, loosely following Bentley and McIlroy,
        // ``Engineering a Sort Function,'' SP&E November 1993.

        // medianOfThree moves the median of the three values data[m0], data[m1], data[m2] into data[m1].
        private static void medianOfThree(Interface data, long m1, long m0, long m2)
        { 
            // sort 3 elements
            if (data.Less(m1, m0))
            {
                data.Swap(m1, m0);
            } 
            // data[m0] <= data[m1]
            if (data.Less(m2, m1))
            {
                data.Swap(m2, m1); 
                // data[m0] <= data[m2] && data[m1] < data[m2]
                if (data.Less(m1, m0))
                {
                    data.Swap(m1, m0);
                }

            } 
            // now data[m0] <= data[m1] <= data[m2]
        }

        private static void swapRange(Interface data, long a, long b, long n)
        {
            for (long i = 0L; i < n; i++)
            {
                data.Swap(a + i, b + i);
            }


        }

        private static (long, long) doPivot(Interface data, long lo, long hi)
        {
            long midlo = default;
            long midhi = default;

            var m = int(uint(lo + hi) >> (int)(1L)); // Written like this to avoid integer overflow.
            if (hi - lo > 40L)
            { 
                // Tukey's ``Ninther,'' median of three medians of three.
                var s = (hi - lo) / 8L;
                medianOfThree(data, lo, lo + s, lo + 2L * s);
                medianOfThree(data, m, m - s, m + s);
                medianOfThree(data, hi - 1L, hi - 1L - s, hi - 1L - 2L * s);

            }

            medianOfThree(data, lo, m, hi - 1L); 

            // Invariants are:
            //    data[lo] = pivot (set up by ChoosePivot)
            //    data[lo < i < a] < pivot
            //    data[a <= i < b] <= pivot
            //    data[b <= i < c] unexamined
            //    data[c <= i < hi-1] > pivot
            //    data[hi-1] >= pivot
            var pivot = lo;
            var a = lo + 1L;
            var c = hi - 1L;

            while (a < c && data.Less(a, pivot))
            {
                a++;
            }

            var b = a;
            while (true)
            {
                while (b < c && !data.Less(pivot, b))
                { // data[b] <= pivot
                    b++;
                }

                while (b < c && data.Less(pivot, c - 1L))
                { // data[c-1] > pivot
                    c--;
                }

                if (b >= c)
                {
                    break;
                } 
                // data[b] > pivot; data[c-1] <= pivot
                data.Swap(b, c - 1L);
                b++;
                c--;

            } 
            // If hi-c<3 then there are duplicates (by property of median of nine).
            // Let's be a bit more conservative, and set border to 5.
 
            // If hi-c<3 then there are duplicates (by property of median of nine).
            // Let's be a bit more conservative, and set border to 5.
            var protect = hi - c < 5L;
            if (!protect && hi - c < (hi - lo) / 4L)
            { 
                // Lets test some points for equality to pivot
                long dups = 0L;
                if (!data.Less(pivot, hi - 1L))
                { // data[hi-1] = pivot
                    data.Swap(c, hi - 1L);
                    c++;
                    dups++;

                }

                if (!data.Less(b - 1L, pivot))
                { // data[b-1] = pivot
                    b--;
                    dups++;

                } 
                // m-lo = (hi-lo)/2 > 6
                // b-lo > (hi-lo)*3/4-1 > 8
                // ==> m < b ==> data[m] <= pivot
                if (!data.Less(m, pivot))
                { // data[m] = pivot
                    data.Swap(m, b - 1L);
                    b--;
                    dups++;

                } 
                // if at least 2 points are equal to pivot, assume skewed distribution
                protect = dups > 1L;

            }

            if (protect)
            { 
                // Protect against a lot of duplicates
                // Add invariant:
                //    data[a <= i < b] unexamined
                //    data[b <= i < c] = pivot
                while (true)
                {
                    while (a < b && !data.Less(b - 1L, pivot))
                    { // data[b] == pivot
                        b--;
                    }

                    while (a < b && data.Less(a, pivot))
                    { // data[a] < pivot
                        a++;
                    }

                    if (a >= b)
                    {
                        break;
                    } 
                    // data[a] == pivot; data[b-1] < pivot
                    data.Swap(a, b - 1L);
                    a++;
                    b--;

                }


            } 
            // Swap pivot into middle
            data.Swap(pivot, b - 1L);
            return (b - 1L, c);

        }

        private static void quickSort(Interface data, long a, long b, long maxDepth)
        {
            while (b - a > 12L)
            { // Use ShellSort for slices <= 12 elements
                if (maxDepth == 0L)
                {
                    heapSort(data, a, b);
                    return ;
                }

                maxDepth--;
                var (mlo, mhi) = doPivot(data, a, b); 
                // Avoiding recursion on the larger subproblem guarantees
                // a stack depth of at most lg(b-a).
                if (mlo - a < b - mhi)
                {
                    quickSort(data, a, mlo, maxDepth);
                    a = mhi; // i.e., quickSort(data, mhi, b)
                }
                else
                {
                    quickSort(data, mhi, b, maxDepth);
                    b = mlo; // i.e., quickSort(data, a, mlo)
                }

            }

            if (b - a > 1L)
            { 
                // Do ShellSort pass with gap 6
                // It could be written in this simplified form cause b-a <= 12
                for (var i = a + 6L; i < b; i++)
                {
                    if (data.Less(i, i - 6L))
                    {
                        data.Swap(i, i - 6L);
                    }

                }

                insertionSort(data, a, b);

            }

        }

        // Sort sorts data.
        // It makes one call to data.Len to determine n, and O(n*log(n)) calls to
        // data.Less and data.Swap. The sort is not guaranteed to be stable.
        public static void Sort(Interface data)
        {
            var n = data.Len();
            quickSort(data, 0L, n, maxDepth(n));
        }

        // maxDepth returns a threshold at which quicksort should switch
        // to heapsort. It returns 2*ceil(lg(n+1)).
        private static long maxDepth(long n)
        {
            long depth = default;
            {
                var i = n;

                while (i > 0L)
                {
                    depth++;
                    i >>= 1L;
                }

            }
            return depth * 2L;

        }

        // lessSwap is a pair of Less and Swap function for use with the
        // auto-generated func-optimized variant of sort.go in
        // zfuncversion.go.
        private partial struct lessSwap
        {
            public Func<long, long, bool> Less;
            public Action<long, long> Swap;
        }

        private partial struct reverse : Interface
        {
            public Interface Interface;
        }

        // Less returns the opposite of the embedded implementation's Less method.
        private static bool Less(this reverse r, long i, long j)
        {
            return r.Interface.Less(j, i);
        }

        // Reverse returns the reverse order for data.
        public static Interface Reverse(Interface data)
        {
            return addr(new reverse(data));
        }

        // IsSorted reports whether data is sorted.
        public static bool IsSorted(Interface data)
        {
            var n = data.Len();
            for (var i = n - 1L; i > 0L; i--)
            {
                if (data.Less(i, i - 1L))
                {
                    return false;
                }

            }

            return true;

        }

        // Convenience types for common cases

        // IntSlice attaches the methods of Interface to []int, sorting in increasing order.
        public partial struct IntSlice // : slice<long>
        {
        }

        public static long Len(this IntSlice p)
        {
            return len(p);
        }
        public static bool Less(this IntSlice p, long i, long j)
        {
            return p[i] < p[j];
        }
        public static void Swap(this IntSlice p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];
        }

        // Sort is a convenience method.
        public static void Sort(this IntSlice p)
        {
            Sort(p);
        }

        // Float64Slice attaches the methods of Interface to []float64, sorting in increasing order
        // (not-a-number values are treated as less than other values).
        public partial struct Float64Slice // : slice<double>
        {
        }

        public static long Len(this Float64Slice p)
        {
            return len(p);
        }
        public static bool Less(this Float64Slice p, long i, long j)
        {
            return p[i] < p[j] || isNaN(p[i]) && !isNaN(p[j]);
        }
        public static void Swap(this Float64Slice p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];
        }

        // isNaN is a copy of math.IsNaN to avoid a dependency on the math package.
        private static bool isNaN(double f)
        {
            return f != f;
        }

        // Sort is a convenience method.
        public static void Sort(this Float64Slice p)
        {
            Sort(p);
        }

        // StringSlice attaches the methods of Interface to []string, sorting in increasing order.
        public partial struct StringSlice // : slice<@string>
        {
        }

        public static long Len(this StringSlice p)
        {
            return len(p);
        }
        public static bool Less(this StringSlice p, long i, long j)
        {
            return p[i] < p[j];
        }
        public static void Swap(this StringSlice p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];
        }

        // Sort is a convenience method.
        public static void Sort(this StringSlice p)
        {
            Sort(p);
        }

        // Convenience wrappers for common cases

        // Ints sorts a slice of ints in increasing order.
        public static void Ints(slice<long> a)
        {
            Sort(IntSlice(a));
        }

        // Float64s sorts a slice of float64s in increasing order
        // (not-a-number values are treated as less than other values).
        public static void Float64s(slice<double> a)
        {
            Sort(Float64Slice(a));
        }

        // Strings sorts a slice of strings in increasing order.
        public static void Strings(slice<@string> a)
        {
            Sort(StringSlice(a));
        }

        // IntsAreSorted tests whether a slice of ints is sorted in increasing order.
        public static bool IntsAreSorted(slice<long> a)
        {
            return IsSorted(IntSlice(a));
        }

        // Float64sAreSorted tests whether a slice of float64s is sorted in increasing order
        // (not-a-number values are treated as less than other values).
        public static bool Float64sAreSorted(slice<double> a)
        {
            return IsSorted(Float64Slice(a));
        }

        // StringsAreSorted tests whether a slice of strings is sorted in increasing order.
        public static bool StringsAreSorted(slice<@string> a)
        {
            return IsSorted(StringSlice(a));
        }

        // Notes on stable sorting:
        // The used algorithms are simple and provable correct on all input and use
        // only logarithmic additional stack space. They perform well if compared
        // experimentally to other stable in-place sorting algorithms.
        //
        // Remarks on other algorithms evaluated:
        //  - GCC's 4.6.3 stable_sort with merge_without_buffer from libstdc++:
        //    Not faster.
        //  - GCC's __rotate for block rotations: Not faster.
        //  - "Practical in-place mergesort" from  Jyrki Katajainen, Tomi A. Pasanen
        //    and Jukka Teuhola; Nordic Journal of Computing 3,1 (1996), 27-40:
        //    The given algorithms are in-place, number of Swap and Assignments
        //    grow as n log n but the algorithm is not stable.
        //  - "Fast Stable In-Place Sorting with O(n) Data Moves" J.I. Munro and
        //    V. Raman in Algorithmica (1996) 16, 115-160:
        //    This algorithm either needs additional 2n bits or works only if there
        //    are enough different elements available to encode some permutations
        //    which have to be undone later (so not stable on any input).
        //  - All the optimal in-place sorting/merging algorithms I found are either
        //    unstable or rely on enough different elements in each step to encode the
        //    performed block rearrangements. See also "In-Place Merging Algorithms",
        //    Denham Coates-Evely, Department of Computer Science, Kings College,
        //    January 2004 and the references in there.
        //  - Often "optimal" algorithms are optimal in the number of assignments
        //    but Interface has only Swap as operation.

        // Stable sorts data while keeping the original order of equal elements.
        //
        // It makes one call to data.Len to determine n, O(n*log(n)) calls to
        // data.Less and O(n*log(n)*log(n)) calls to data.Swap.
        public static void Stable(Interface data)
        {
            stable(data, data.Len());
        }

        private static void stable(Interface data, long n)
        {
            long blockSize = 20L; // must be > 0
            long a = 0L;
            var b = blockSize;
            while (b <= n)
            {
                insertionSort(data, a, b);
                a = b;
                b += blockSize;
            }

            insertionSort(data, a, n);

            while (blockSize < n)
            {
                a = 0L;
                b = 2L * blockSize;
                while (b <= n)
                {
                    symMerge(data, a, a + blockSize, b);
                    a = b;
                    b += 2L * blockSize;
                }

                {
                    var m = a + blockSize;

                    if (m < n)
                    {
                        symMerge(data, a, m, n);
                    }

                }

                blockSize *= 2L;

            }


        }

        // SymMerge merges the two sorted subsequences data[a:m] and data[m:b] using
        // the SymMerge algorithm from Pok-Son Kim and Arne Kutzner, "Stable Minimum
        // Storage Merging by Symmetric Comparisons", in Susanne Albers and Tomasz
        // Radzik, editors, Algorithms - ESA 2004, volume 3221 of Lecture Notes in
        // Computer Science, pages 714-723. Springer, 2004.
        //
        // Let M = m-a and N = b-n. Wolog M < N.
        // The recursion depth is bound by ceil(log(N+M)).
        // The algorithm needs O(M*log(N/M + 1)) calls to data.Less.
        // The algorithm needs O((M+N)*log(M)) calls to data.Swap.
        //
        // The paper gives O((M+N)*log(M)) as the number of assignments assuming a
        // rotation algorithm which uses O(M+N+gcd(M+N)) assignments. The argumentation
        // in the paper carries through for Swap operations, especially as the block
        // swapping rotate uses only O(M+N) Swaps.
        //
        // symMerge assumes non-degenerate arguments: a < m && m < b.
        // Having the caller check this condition eliminates many leaf recursion calls,
        // which improves performance.
        private static void symMerge(Interface data, long a, long m, long b)
        { 
            // Avoid unnecessary recursions of symMerge
            // by direct insertion of data[a] into data[m:b]
            // if data[a:m] only contains one element.
            if (m - a == 1L)
            { 
                // Use binary search to find the lowest index i
                // such that data[i] >= data[a] for m <= i < b.
                // Exit the search loop with i == b in case no such index exists.
                var i = m;
                var j = b;
                while (i < j)
                {
                    var h = int(uint(i + j) >> (int)(1L));
                    if (data.Less(h, a))
                    {
                        i = h + 1L;
                    }
                    else
                    {
                        j = h;
                    }

                } 
                // Swap values until data[a] reaches the position before i.
 
                // Swap values until data[a] reaches the position before i.
                {
                    var k__prev1 = k;

                    for (var k = a; k < i - 1L; k++)
                    {
                        data.Swap(k, k + 1L);
                    }


                    k = k__prev1;
                }
                return ;

            } 

            // Avoid unnecessary recursions of symMerge
            // by direct insertion of data[m] into data[a:m]
            // if data[m:b] only contains one element.
            if (b - m == 1L)
            { 
                // Use binary search to find the lowest index i
                // such that data[i] > data[m] for a <= i < m.
                // Exit the search loop with i == m in case no such index exists.
                i = a;
                j = m;
                while (i < j)
                {
                    h = int(uint(i + j) >> (int)(1L));
                    if (!data.Less(m, h))
                    {
                        i = h + 1L;
                    }
                    else
                    {
                        j = h;
                    }

                } 
                // Swap values until data[m] reaches the position i.
 
                // Swap values until data[m] reaches the position i.
                {
                    var k__prev1 = k;

                    for (k = m; k > i; k--)
                    {
                        data.Swap(k, k - 1L);
                    }


                    k = k__prev1;
                }
                return ;

            }

            var mid = int(uint(a + b) >> (int)(1L));
            var n = mid + m;
            long start = default;            long r = default;

            if (m > mid)
            {
                start = n - b;
                r = mid;
            }
            else
            {
                start = a;
                r = m;
            }

            var p = n - 1L;

            while (start < r)
            {
                var c = int(uint(start + r) >> (int)(1L));
                if (!data.Less(p - c, c))
                {
                    start = c + 1L;
                }
                else
                {
                    r = c;
                }

            }


            var end = n - start;
            if (start < m && m < end)
            {
                rotate(data, start, m, end);
            }

            if (a < start && start < mid)
            {
                symMerge(data, a, start, mid);
            }

            if (mid < end && end < b)
            {
                symMerge(data, mid, end, b);
            }

        }

        // Rotate two consecutive blocks u = data[a:m] and v = data[m:b] in data:
        // Data of the form 'x u v y' is changed to 'x v u y'.
        // Rotate performs at most b-a many calls to data.Swap.
        // Rotate assumes non-degenerate arguments: a < m && m < b.
        private static void rotate(Interface data, long a, long m, long b)
        {
            var i = m - a;
            var j = b - m;

            while (i != j)
            {
                if (i > j)
                {
                    swapRange(data, m - i, m, j);
                    i -= j;
                }
                else
                {
                    swapRange(data, m - i, m + j - i, i);
                    j -= i;
                }

            } 
            // i == j
 
            // i == j
            swapRange(data, m - i, m, i);

        }

        /*
        Complexity of Stable Sorting


        Complexity of block swapping rotation

        Each Swap puts one new element into its correct, final position.
        Elements which reach their final position are no longer moved.
        Thus block swapping rotation needs |u|+|v| calls to Swaps.
        This is best possible as each element might need a move.

        Pay attention when comparing to other optimal algorithms which
        typically count the number of assignments instead of swaps:
        E.g. the optimal algorithm of Dudzinski and Dydek for in-place
        rotations uses O(u + v + gcd(u,v)) assignments which is
        better than our O(3 * (u+v)) as gcd(u,v) <= u.


        Stable sorting by SymMerge and BlockSwap rotations

        SymMerg complexity for same size input M = N:
        Calls to Less:  O(M*log(N/M+1)) = O(N*log(2)) = O(N)
        Calls to Swap:  O((M+N)*log(M)) = O(2*N*log(N)) = O(N*log(N))

        (The following argument does not fuzz over a missing -1 or
        other stuff which does not impact the final result).

        Let n = data.Len(). Assume n = 2^k.

        Plain merge sort performs log(n) = k iterations.
        On iteration i the algorithm merges 2^(k-i) blocks, each of size 2^i.

        Thus iteration i of merge sort performs:
        Calls to Less  O(2^(k-i) * 2^i) = O(2^k) = O(2^log(n)) = O(n)
        Calls to Swap  O(2^(k-i) * 2^i * log(2^i)) = O(2^k * i) = O(n*i)

        In total k = log(n) iterations are performed; so in total:
        Calls to Less O(log(n) * n)
        Calls to Swap O(n + 2*n + 3*n + ... + (k-1)*n + k*n)
           = O((k/2) * k * n) = O(n * k^2) = O(n * log^2(n))


        Above results should generalize to arbitrary n = 2^k + p
        and should not be influenced by the initial insertion sort phase:
        Insertion sort is O(n^2) on Swap and Less, thus O(bs^2) per block of
        size bs at n/bs blocks:  O(bs*n) Swaps and Less during insertion sort.
        Merge sort iterations start at i = log(bs). With t = log(bs) constant:
        Calls to Less O((log(n)-t) * n + bs*n) = O(log(n)*n + (bs-t)*n)
           = O(n * log(n))
        Calls to Swap O(n * log^2(n) - (t^2+t)/2*n) = O(n * log^2(n))

        */
    }
}
