// DO NOT EDIT; AUTO-GENERATED from sort.go using genzfunc.go

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sort -- go2cs converted at 2020 August 29 08:21:46 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Go\src\sort\zfuncversion.go

using static go.builtin;

namespace go
{
    public static partial class sort_package
    {
        // Auto-generated variant of sort.go:insertionSort
        private static void insertionSort_func(lessSwap data, long a, long b)
        {
            for (var i = a + 1L; i < b; i++)
            {
                for (var j = i; j > a && data.Less(j, j - 1L); j--)
                {
                    data.Swap(j, j - 1L);
                }
            }
        }

        // Auto-generated variant of sort.go:siftDown
        private static void siftDown_func(lessSwap data, long lo, long hi, long first)
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
                    return;
                }
                data.Swap(first + root, first + child);
                root = child;
            }

        }

        // Auto-generated variant of sort.go:heapSort
        private static void heapSort_func(lessSwap data, long a, long b)
        {
            var first = a;
            long lo = 0L;
            var hi = b - a;
            {
                var i__prev1 = i;

                for (var i = (hi - 1L) / 2L; i >= 0L; i--)
                {
                    siftDown_func(data, i, hi, first);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = hi - 1L; i >= 0L; i--)
                {
                    data.Swap(first, first + i);
                    siftDown_func(data, lo, i, first);
                }


                i = i__prev1;
            }
        }

        // Auto-generated variant of sort.go:medianOfThree
        private static void medianOfThree_func(lessSwap data, long m1, long m0, long m2)
        {
            if (data.Less(m1, m0))
            {
                data.Swap(m1, m0);
            }
            if (data.Less(m2, m1))
            {
                data.Swap(m2, m1);
                if (data.Less(m1, m0))
                {
                    data.Swap(m1, m0);
                }
            }
        }

        // Auto-generated variant of sort.go:swapRange
        private static void swapRange_func(lessSwap data, long a, long b, long n)
        {
            for (long i = 0L; i < n; i++)
            {
                data.Swap(a + i, b + i);
            }

        }

        // Auto-generated variant of sort.go:doPivot
        private static (long, long) doPivot_func(lessSwap data, long lo, long hi)
        {
            var m = int(uint(lo + hi) >> (int)(1L));
            if (hi - lo > 40L)
            {
                var s = (hi - lo) / 8L;
                medianOfThree_func(data, lo, lo + s, lo + 2L * s);
                medianOfThree_func(data, m, m - s, m + s);
                medianOfThree_func(data, hi - 1L, hi - 1L - s, hi - 1L - 2L * s);
            }
            medianOfThree_func(data, lo, m, hi - 1L);
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
                {
                    b++;
                }

                while (b < c && data.Less(pivot, c - 1L))
                {
                    c--;
                }

                if (b >= c)
                {
                    break;
                }
                data.Swap(b, c - 1L);
                b++;
                c--;
            }

            var protect = hi - c < 5L;
            if (!protect && hi - c < (hi - lo) / 4L)
            {
                long dups = 0L;
                if (!data.Less(pivot, hi - 1L))
                {
                    data.Swap(c, hi - 1L);
                    c++;
                    dups++;
                }
                if (!data.Less(b - 1L, pivot))
                {
                    b--;
                    dups++;
                }
                if (!data.Less(m, pivot))
                {
                    data.Swap(m, b - 1L);
                    b--;
                    dups++;
                }
                protect = dups > 1L;
            }
            if (protect)
            {
                while (true)
                {
                    while (a < b && !data.Less(b - 1L, pivot))
                    {
                        b--;
                    }

                    while (a < b && data.Less(a, pivot))
                    {
                        a++;
                    }

                    if (a >= b)
                    {
                        break;
                    }
                    data.Swap(a, b - 1L);
                    a++;
                    b--;
                }

            }
            data.Swap(pivot, b - 1L);
            return (b - 1L, c);
        }

        // Auto-generated variant of sort.go:quickSort
        private static void quickSort_func(lessSwap data, long a, long b, long maxDepth)
        {
            while (b - a > 12L)
            {
                if (maxDepth == 0L)
                {
                    heapSort_func(data, a, b);
                    return;
                }
                maxDepth--;
                var (mlo, mhi) = doPivot_func(data, a, b);
                if (mlo - a < b - mhi)
                {
                    quickSort_func(data, a, mlo, maxDepth);
                    a = mhi;
                }
                else
                {
                    quickSort_func(data, mhi, b, maxDepth);
                    b = mlo;
                }
            }

            if (b - a > 1L)
            {
                for (var i = a + 6L; i < b; i++)
                {
                    if (data.Less(i, i - 6L))
                    {
                        data.Swap(i, i - 6L);
                    }
                }

                insertionSort_func(data, a, b);
            }
        }

        // Auto-generated variant of sort.go:stable
        private static void stable_func(lessSwap data, long n)
        {
            long blockSize = 20L;
            long a = 0L;
            var b = blockSize;
            while (b <= n)
            {
                insertionSort_func(data, a, b);
                a = b;
                b += blockSize;
            }

            insertionSort_func(data, a, n);
            while (blockSize < n)
            {
                a = 0L;
                b = 2L * blockSize;
                while (b <= n)
                {
                    symMerge_func(data, a, a + blockSize, b);
                    a = b;
                    b += 2L * blockSize;
                }

                {
                    var m = a + blockSize;

                    if (m < n)
                    {
                        symMerge_func(data, a, m, n);
                    }

                }
                blockSize *= 2L;
            }

        }

        // Auto-generated variant of sort.go:symMerge
        private static void symMerge_func(lessSwap data, long a, long m, long b)
        {
            if (m - a == 1L)
            {
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

                {
                    var k__prev1 = k;

                    for (var k = a; k < i - 1L; k++)
                    {
                        data.Swap(k, k + 1L);
                    }


                    k = k__prev1;
                }
                return;
            }
            if (b - m == 1L)
            {
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

                {
                    var k__prev1 = k;

                    for (k = m; k > i; k--)
                    {
                        data.Swap(k, k - 1L);
                    }


                    k = k__prev1;
                }
                return;
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
                rotate_func(data, start, m, end);
            }
            if (a < start && start < mid)
            {
                symMerge_func(data, a, start, mid);
            }
            if (mid < end && end < b)
            {
                symMerge_func(data, mid, end, b);
            }
        }

        // Auto-generated variant of sort.go:rotate
        private static void rotate_func(lessSwap data, long a, long m, long b)
        {
            var i = m - a;
            var j = b - m;
            while (i != j)
            {
                if (i > j)
                {
                    swapRange_func(data, m - i, m, j);
                    i -= j;
                }
                else
                {
                    swapRange_func(data, m - i, m + j - i, i);
                    j -= i;
                }
            }

            swapRange_func(data, m - i, m, i);
        }
    }
}
