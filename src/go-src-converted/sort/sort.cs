// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run genzfunc.go

// Package sort provides primitives for sorting slices and user-defined collections.
// package sort -- go2cs converted at 2022 March 06 22:12:37 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Program Files\Go\src\sort\sort.go

using System;


namespace go;

public static partial class sort_package {

    // An implementation of Interface can be sorted by the routines in this package.
    // The methods refer to elements of the underlying collection by integer index.
public partial interface Interface {
    bool Len(); // Less reports whether the element with index i
// must sort before the element with index j.
//
// If both Less(i, j) and Less(j, i) are false,
// then the elements at index i and j are considered equal.
// Sort may place equal elements in any order in the final result,
// while Stable preserves the original input order of equal elements.
//
// Less must describe a transitive ordering:
//  - if both Less(i, j) and Less(j, k) are true, then Less(i, k) must be true as well.
//  - if both Less(i, j) and Less(j, k) are false, then Less(i, k) must be false as well.
//
// Note that floating-point comparison (the < operator on float32 or float64 values)
// is not a transitive ordering when not-a-number (NaN) values are involved.
// See Float64Slice.Less for a correct implementation for floating-point values.
    bool Less(nint i, nint j); // Swap swaps the elements with indexes i and j.
    bool Swap(nint i, nint j);
}

// insertionSort sorts data[a:b] using insertion sort.
private static void insertionSort(Interface data, nint a, nint b) {
    for (var i = a + 1; i < b; i++) {
        for (var j = i; j > a && data.Less(j, j - 1); j--) {
            data.Swap(j, j - 1);
        }
    }
}

// siftDown implements the heap property on data[lo:hi].
// first is an offset into the array where the root of the heap lies.
private static void siftDown(Interface data, nint lo, nint hi, nint first) {
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

private static void heapSort(Interface data, nint a, nint b) {
    var first = a;
    nint lo = 0;
    var hi = b - a; 

    // Build heap with greatest element at top.
    {
        var i__prev1 = i;

        for (var i = (hi - 1) / 2; i >= 0; i--) {
            siftDown(data, i, hi, first);
        }

        i = i__prev1;
    } 

    // Pop elements, largest first, into end of data.
    {
        var i__prev1 = i;

        for (i = hi - 1; i >= 0; i--) {
            data.Swap(first, first + i);
            siftDown(data, lo, i, first);
        }

        i = i__prev1;
    }

}

// Quicksort, loosely following Bentley and McIlroy,
// ``Engineering a Sort Function,'' SP&E November 1993.

// medianOfThree moves the median of the three values data[m0], data[m1], data[m2] into data[m1].
private static void medianOfThree(Interface data, nint m1, nint m0, nint m2) { 
    // sort 3 elements
    if (data.Less(m1, m0)) {
        data.Swap(m1, m0);
    }
    if (data.Less(m2, m1)) {
        data.Swap(m2, m1); 
        // data[m0] <= data[m2] && data[m1] < data[m2]
        if (data.Less(m1, m0)) {
            data.Swap(m1, m0);
        }
    }
}

private static void swapRange(Interface data, nint a, nint b, nint n) {
    for (nint i = 0; i < n; i++) {
        data.Swap(a + i, b + i);
    }
}

private static (nint, nint) doPivot(Interface data, nint lo, nint hi) {
    nint midlo = default;
    nint midhi = default;

    var m = int(uint(lo + hi) >> 1); // Written like this to avoid integer overflow.
    if (hi - lo > 40) { 
        // Tukey's ``Ninther,'' median of three medians of three.
        var s = (hi - lo) / 8;
        medianOfThree(data, lo, lo + s, lo + 2 * s);
        medianOfThree(data, m, m - s, m + s);
        medianOfThree(data, hi - 1, hi - 1 - s, hi - 1 - 2 * s);

    }
    medianOfThree(data, lo, m, hi - 1); 

    // Invariants are:
    //    data[lo] = pivot (set up by ChoosePivot)
    //    data[lo < i < a] < pivot
    //    data[a <= i < b] <= pivot
    //    data[b <= i < c] unexamined
    //    data[c <= i < hi-1] > pivot
    //    data[hi-1] >= pivot
    var pivot = lo;
    var a = lo + 1;
    var c = hi - 1;

    while (a < c && data.Less(a, pivot)) {
        a++;
    }
    var b = a;
    while (true) {
        while (b < c && !data.Less(pivot, b)) { // data[b] <= pivot
            b++;
        }
        while (b < c && data.Less(pivot, c - 1)) { // data[c-1] > pivot
            c--;
        }
        if (b >= c) {
            break;
        }
        data.Swap(b, c - 1);
        b++;
        c--;

    } 
    // If hi-c<3 then there are duplicates (by property of median of nine).
    // Let's be a bit more conservative, and set border to 5.
    var protect = hi - c < 5;
    if (!protect && hi - c < (hi - lo) / 4) { 
        // Lets test some points for equality to pivot
        nint dups = 0;
        if (!data.Less(pivot, hi - 1)) { // data[hi-1] = pivot
            data.Swap(c, hi - 1);
            c++;
            dups++;

        }
        if (!data.Less(b - 1, pivot)) { // data[b-1] = pivot
            b--;
            dups++;

        }
        if (!data.Less(m, pivot)) { // data[m] = pivot
            data.Swap(m, b - 1);
            b--;
            dups++;

        }
        protect = dups > 1;

    }
    if (protect) { 
        // Protect against a lot of duplicates
        // Add invariant:
        //    data[a <= i < b] unexamined
        //    data[b <= i < c] = pivot
        while (true) {
            while (a < b && !data.Less(b - 1, pivot)) { // data[b] == pivot
                b--;
            }

            while (a < b && data.Less(a, pivot)) { // data[a] < pivot
                a++;
            }

            if (a >= b) {
                break;
            } 
            // data[a] == pivot; data[b-1] < pivot
            data.Swap(a, b - 1);
            a++;
            b--;

        }

    }
    data.Swap(pivot, b - 1);
    return (b - 1, c);

}

private static void quickSort(Interface data, nint a, nint b, nint maxDepth) {
    while (b - a > 12) { // Use ShellSort for slices <= 12 elements
        if (maxDepth == 0) {
            heapSort(data, a, b);
            return ;
        }
        maxDepth--;
        var (mlo, mhi) = doPivot(data, a, b); 
        // Avoiding recursion on the larger subproblem guarantees
        // a stack depth of at most lg(b-a).
        if (mlo - a < b - mhi) {
            quickSort(data, a, mlo, maxDepth);
            a = mhi; // i.e., quickSort(data, mhi, b)
        }
        else
 {
            quickSort(data, mhi, b, maxDepth);
            b = mlo; // i.e., quickSort(data, a, mlo)
        }
    }
    if (b - a > 1) { 
        // Do ShellSort pass with gap 6
        // It could be written in this simplified form cause b-a <= 12
        for (var i = a + 6; i < b; i++) {
            if (data.Less(i, i - 6)) {
                data.Swap(i, i - 6);
            }
        }
        insertionSort(data, a, b);

    }
}

// Sort sorts data.
// It makes one call to data.Len to determine n and O(n*log(n)) calls to
// data.Less and data.Swap. The sort is not guaranteed to be stable.
public static void Sort(Interface data) {
    var n = data.Len();
    quickSort(data, 0, n, maxDepth(n));
}

// maxDepth returns a threshold at which quicksort should switch
// to heapsort. It returns 2*ceil(lg(n+1)).
private static nint maxDepth(nint n) {
    nint depth = default;
    {
        var i = n;

        while (i > 0) {
            depth++;
            i>>=1;
        }
    }
    return depth * 2;

}

// lessSwap is a pair of Less and Swap function for use with the
// auto-generated func-optimized variant of sort.go in
// zfuncversion.go.
private partial struct lessSwap {
    public Func<nint, nint, bool> Less;
    public Action<nint, nint> Swap;
}

private partial struct reverse : Interface {
    public Interface Interface;
}

// Less returns the opposite of the embedded implementation's Less method.
private static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}

// Reverse returns the reverse order for data.
public static Interface Reverse(Interface data) {
    return addr(new reverse(data));
}

// IsSorted reports whether data is sorted.
public static bool IsSorted(Interface data) {
    var n = data.Len();
    for (var i = n - 1; i > 0; i--) {
        if (data.Less(i, i - 1)) {
            return false;
        }
    }
    return true;

}

// Convenience types for common cases

// IntSlice attaches the methods of Interface to []int, sorting in increasing order.
public partial struct IntSlice { // : slice<nint>
}

public static nint Len(this IntSlice x) {
    return len(x);
}
public static bool Less(this IntSlice x, nint i, nint j) {
    return x[i] < x[j];
}
public static void Swap(this IntSlice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this IntSlice x) {
    Sort(x);
}

// Float64Slice implements Interface for a []float64, sorting in increasing order,
// with not-a-number (NaN) values ordered before other values.
public partial struct Float64Slice { // : slice<double>
}

public static nint Len(this Float64Slice x) {
    return len(x);
}

// Less reports whether x[i] should be ordered before x[j], as required by the sort Interface.
// Note that floating-point comparison by itself is not a transitive relation: it does not
// report a consistent ordering for not-a-number (NaN) values.
// This implementation of Less places NaN values before any others, by using:
//
//    x[i] < x[j] || (math.IsNaN(x[i]) && !math.IsNaN(x[j]))
//
public static bool Less(this Float64Slice x, nint i, nint j) {
    return x[i] < x[j] || (isNaN(x[i]) && !isNaN(x[j]));
}
public static void Swap(this Float64Slice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// isNaN is a copy of math.IsNaN to avoid a dependency on the math package.
private static bool isNaN(double f) {
    return f != f;
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this Float64Slice x) {
    Sort(x);
}

// StringSlice attaches the methods of Interface to []string, sorting in increasing order.
public partial struct StringSlice { // : slice<@string>
}

public static nint Len(this StringSlice x) {
    return len(x);
}
public static bool Less(this StringSlice x, nint i, nint j) {
    return x[i] < x[j];
}
public static void Swap(this StringSlice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this StringSlice x) {
    Sort(x);
}

// Convenience wrappers for common cases

// Ints sorts a slice of ints in increasing order.
public static void Ints(slice<nint> x) {
    Sort(IntSlice(x));
}

// Float64s sorts a slice of float64s in increasing order.
// Not-a-number (NaN) values are ordered before other values.
public static void Float64s(slice<double> x) {
    Sort(Float64Slice(x));
}

// Strings sorts a slice of strings in increasing order.
public static void Strings(slice<@string> x) {
    Sort(StringSlice(x));
}

// IntsAreSorted reports whether the slice x is sorted in increasing order.
public static bool IntsAreSorted(slice<nint> x) {
    return IsSorted(IntSlice(x));
}

// Float64sAreSorted reports whether the slice x is sorted in increasing order,
// with not-a-number (NaN) values before any other values.
public static bool Float64sAreSorted(slice<double> x) {
    return IsSorted(Float64Slice(x));
}

// StringsAreSorted reports whether the slice x is sorted in increasing order.
public static bool StringsAreSorted(slice<@string> x) {
    return IsSorted(StringSlice(x));
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
public static void Stable(Interface data) {
    stable(data, data.Len());
}

private static void stable(Interface data, nint n) {
    nint blockSize = 20; // must be > 0
    nint a = 0;
    var b = blockSize;
    while (b <= n) {
        insertionSort(data, a, b);
        a = b;
        b += blockSize;
    }
    insertionSort(data, a, n);

    while (blockSize < n) {
        (a, b) = (0, 2 * blockSize);        while (b <= n) {
            symMerge(data, a, a + blockSize, b);
            a = b;
            b += 2 * blockSize;
        }
        {
            var m = a + blockSize;

            if (m < n) {
                symMerge(data, a, m, n);
            }

        }

        blockSize *= 2;

    }

}

// symMerge merges the two sorted subsequences data[a:m] and data[m:b] using
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
private static void symMerge(Interface data, nint a, nint m, nint b) { 
    // Avoid unnecessary recursions of symMerge
    // by direct insertion of data[a] into data[m:b]
    // if data[a:m] only contains one element.
    if (m - a == 1) { 
        // Use binary search to find the lowest index i
        // such that data[i] >= data[a] for m <= i < b.
        // Exit the search loop with i == b in case no such index exists.
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
        // Swap values until data[a] reaches the position before i.
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
        // Use binary search to find the lowest index i
        // such that data[i] > data[m] for a <= i < m.
        // Exit the search loop with i == m in case no such index exists.
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
        // Swap values until data[m] reaches the position i.
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
        rotate(data, start, m, end);
    }
    if (a < start && start < mid) {
        symMerge(data, a, start, mid);
    }
    if (mid < end && end < b) {
        symMerge(data, mid, end, b);
    }
}

// rotate rotates two consecutive blocks u = data[a:m] and v = data[m:b] in data:
// Data of the form 'x u v y' is changed to 'x v u y'.
// rotate performs at most b-a many calls to data.Swap,
// and it assumes non-degenerate arguments: a < m && m < b.
private static void rotate(Interface data, nint a, nint m, nint b) {
    var i = m - a;
    var j = b - m;

    while (i != j) {
        if (i > j) {
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

} // end sort_package
