// Code generated from sort.go using genzfunc.go; DO NOT EDIT.

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sort -- go2cs converted at 2022 March 06 22:12:37 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Program Files\Go\src\sort\zfuncversion.go


namespace go;

public static partial class sort_package {

    // Auto-generated variant of sort.go:insertionSort
private static void insertionSort_func(lessSwap data, nint a, nint b) {
    for (var i = a + 1; i < b; i++) {
        for (var j = i; j > a && data.Less(j, j - 1); j--) {
            data.Swap(j, j - 1);
        }
    }
}

// Auto-generated variant of sort.go:siftDown
private static void siftDown_func(lessSwap data, nint lo, nint hi, nint first) {
    var root = lo;
    while (true) {
        nint child = 2 * root + 1;
        if (child >= hi) {
            break;
        }
        if (child + 1 < hi && data.Less(first + child, first + child + 1)) {
            child++;
        }
        if (!data.Less(first + root, first + child)) {
            return ;
        }
        data.Swap(first + root, first + child);
        root = child;

    }

}

// Auto-generated variant of sort.go:heapSort
private static void heapSort_func(lessSwap data, nint a, nint b) {
    var first = a;
    nint lo = 0;
    var hi = b - a;
    {
        var i__prev1 = i;

        for (var i = (hi - 1) / 2; i >= 0; i--) {
            siftDown_func(data, i, hi, first);
        }

        i = i__prev1;
    }
    {
        var i__prev1 = i;

        for (i = hi - 1; i >= 0; i--) {
            data.Swap(first, first + i);
            siftDown_func(data, lo, i, first);
        }

        i = i__prev1;
    }

}

// Auto-generated variant of sort.go:medianOfThree
private static void medianOfThree_func(lessSwap data, nint m1, nint m0, nint m2) {
    if (data.Less(m1, m0)) {
        data.Swap(m1, m0);
    }
    if (data.Less(m2, m1)) {
        data.Swap(m2, m1);
        if (data.Less(m1, m0)) {
            data.Swap(m1, m0);
        }
    }
}

// Auto-generated variant of sort.go:swapRange
private static void swapRange_func(lessSwap data, nint a, nint b, nint n) {
    for (nint i = 0; i < n; i++) {
        data.Swap(a + i, b + i);
    }
}

// Auto-generated variant of sort.go:doPivot
private static (nint, nint) doPivot_func(lessSwap data, nint lo, nint hi) {
    nint midlo = default;
    nint midhi = default;

    var m = int(uint(lo + hi) >> 1);
    if (hi - lo > 40) {
        var s = (hi - lo) / 8;
        medianOfThree_func(data, lo, lo + s, lo + 2 * s);
        medianOfThree_func(data, m, m - s, m + s);
        medianOfThree_func(data, hi - 1, hi - 1 - s, hi - 1 - 2 * s);
    }
    medianOfThree_func(data, lo, m, hi - 1);
    var pivot = lo;
    var a = lo + 1;
    var c = hi - 1;
    while (a < c && data.Less(a, pivot)) {
        a++;
    }
    var b = a;
    while (true) {
        while (b < c && !data.Less(pivot, b)) {
            b++;
        }
        while (b < c && data.Less(pivot, c - 1)) {
            c--;
        }
        if (b >= c) {
            break;
        }
        data.Swap(b, c - 1);
        b++;
        c--;

    }
    var protect = hi - c < 5;
    if (!protect && hi - c < (hi - lo) / 4) {
        nint dups = 0;
        if (!data.Less(pivot, hi - 1)) {
            data.Swap(c, hi - 1);
            c++;
            dups++;
        }
        if (!data.Less(b - 1, pivot)) {
            b--;
            dups++;
        }
        if (!data.Less(m, pivot)) {
            data.Swap(m, b - 1);
            b--;
            dups++;
        }
        protect = dups > 1;

    }
    if (protect) {
        while (true) {
            while (a < b && !data.Less(b - 1, pivot)) {
                b--;
            }

            while (a < b && data.Less(a, pivot)) {
                a++;
            }

            if (a >= b) {
                break;
            }

            data.Swap(a, b - 1);
            a++;
            b--;

        }

    }
    data.Swap(pivot, b - 1);
    return (b - 1, c);

}

// Auto-generated variant of sort.go:quickSort
private static void quickSort_func(lessSwap data, nint a, nint b, nint maxDepth) {
    while (b - a > 12) {
        if (maxDepth == 0) {
            heapSort_func(data, a, b);
            return ;
        }
        maxDepth--;
        var (mlo, mhi) = doPivot_func(data, a, b);
        if (mlo - a < b - mhi) {
            quickSort_func(data, a, mlo, maxDepth);
            a = mhi;
        }
        else
 {
            quickSort_func(data, mhi, b, maxDepth);
            b = mlo;
        }
    }
    if (b - a > 1) {
        for (var i = a + 6; i < b; i++) {
            if (data.Less(i, i - 6)) {
                data.Swap(i, i - 6);
            }
        }
        insertionSort_func(data, a, b);
    }
}

// Auto-generated variant of sort.go:stable
private static void stable_func(lessSwap data, nint n) {
    nint blockSize = 20;
    nint a = 0;
    var b = blockSize;
    while (b <= n) {
        insertionSort_func(data, a, b);
        a = b;
        b += blockSize;
    }
    insertionSort_func(data, a, n);
    while (blockSize < n) {
        (a, b) = (0, 2 * blockSize);        while (b <= n) {
            symMerge_func(data, a, a + blockSize, b);
            a = b;
            b += 2 * blockSize;
        }
        {
            var m = a + blockSize;

            if (m < n) {
                symMerge_func(data, a, m, n);
            }

        }

        blockSize *= 2;

    }

}

// Auto-generated variant of sort.go:symMerge
private static void symMerge_func(lessSwap data, nint a, nint m, nint b) {
    if (m - a == 1) {
        var i = m;
        var j = b;
        while (i < j) {
            var h = int(uint(i + j) >> 1);
            if (data.Less(h, a)) {
                i = h + 1;
            }
            else
 {
                j = h;
            }

        }
        {
            var k__prev1 = k;

            for (var k = a; k < i - 1; k++) {
                data.Swap(k, k + 1);
            }


            k = k__prev1;
        }
        return ;

    }
    if (b - m == 1) {
        i = a;
        j = m;
        while (i < j) {
            h = int(uint(i + j) >> 1);
            if (!data.Less(m, h)) {
                i = h + 1;
            }
            else
 {
                j = h;
            }

        }
        {
            var k__prev1 = k;

            for (k = m; k > i; k--) {
                data.Swap(k, k - 1);
            }


            k = k__prev1;
        }
        return ;

    }
    var mid = int(uint(a + b) >> 1);
    var n = mid + m;
    nint start = default;    nint r = default;

    if (m > mid) {
        start = n - b;
        r = mid;
    }
    else
 {
        start = a;
        r = m;
    }
    var p = n - 1;
    while (start < r) {
        var c = int(uint(start + r) >> 1);
        if (!data.Less(p - c, c)) {
            start = c + 1;
        }
        else
 {
            r = c;
        }
    }
    var end = n - start;
    if (start < m && m < end) {
        rotate_func(data, start, m, end);
    }
    if (a < start && start < mid) {
        symMerge_func(data, a, start, mid);
    }
    if (mid < end && end < b) {
        symMerge_func(data, mid, end, b);
    }
}

// Auto-generated variant of sort.go:rotate
private static void rotate_func(lessSwap data, nint a, nint m, nint b) {
    var i = m - a;
    var j = b - m;
    while (i != j) {
        if (i > j) {
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

} // end sort_package
