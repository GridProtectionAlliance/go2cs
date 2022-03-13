// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:25:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mpallocbits.go
namespace go;

using sys = runtime.@internal.sys_package;


// pageBits is a bitmap representing one bit per page in a palloc chunk.

public static partial class runtime_package {

private partial struct pageBits { // : array<ulong>
}

// get returns the value of the i'th bit in the bitmap.
private static nuint get(this ptr<pageBits> _addr_b, nuint i) {
    ref pageBits b = ref _addr_b.val;

    return uint((b[i / 64] >> (int)((i % 64))) & 1);
}

// block64 returns the 64-bit aligned block of bits containing the i'th bit.
private static ulong block64(this ptr<pageBits> _addr_b, nuint i) {
    ref pageBits b = ref _addr_b.val;

    return b[i / 64];
}

// set sets bit i of pageBits.
private static void set(this ptr<pageBits> _addr_b, nuint i) {
    ref pageBits b = ref _addr_b.val;

    b[i / 64] |= 1 << (int)((i % 64));
}

// setRange sets bits in the range [i, i+n).
private static void setRange(this ptr<pageBits> _addr_b, nuint i, nuint n) {
    ref pageBits b = ref _addr_b.val;

    _ = b[i / 64];
    if (n == 1) { 
        // Fast path for the n == 1 case.
        b.set(i);
        return ;
    }
    var j = i + n - 1;
    if (i / 64 == j / 64) {
        b[i / 64] |= ((uint64(1) << (int)(n)) - 1) << (int)((i % 64));
        return ;
    }
    _ = b[j / 64]; 
    // Set leading bits.
    b[i / 64] |= ~uint64(0) << (int)((i % 64));
    for (var k = i / 64 + 1; k < j / 64; k++) {
        b[k] = ~uint64(0);
    } 
    // Set trailing bits.
    b[j / 64] |= (uint64(1) << (int)((j % 64 + 1))) - 1;
}

// setAll sets all the bits of b.
private static void setAll(this ptr<pageBits> _addr_b) {
    ref pageBits b = ref _addr_b.val;

    foreach (var (i) in b) {
        b[i] = ~uint64(0);
    }
}

// clear clears bit i of pageBits.
private static void clear(this ptr<pageBits> _addr_b, nuint i) {
    ref pageBits b = ref _addr_b.val;

    b[i / 64] &= 1 << (int)((i % 64));
}

// clearRange clears bits in the range [i, i+n).
private static void clearRange(this ptr<pageBits> _addr_b, nuint i, nuint n) {
    ref pageBits b = ref _addr_b.val;

    _ = b[i / 64];
    if (n == 1) { 
        // Fast path for the n == 1 case.
        b.clear(i);
        return ;
    }
    var j = i + n - 1;
    if (i / 64 == j / 64) {
        b[i / 64] &= ((uint64(1) << (int)(n)) - 1) << (int)((i % 64));
        return ;
    }
    _ = b[j / 64]; 
    // Clear leading bits.
    b[i / 64] &= ~uint64(0) << (int)((i % 64));
    for (var k = i / 64 + 1; k < j / 64; k++) {
        b[k] = 0;
    } 
    // Clear trailing bits.
    b[j / 64] &= (uint64(1) << (int)((j % 64 + 1))) - 1;
}

// clearAll frees all the bits of b.
private static void clearAll(this ptr<pageBits> _addr_b) {
    ref pageBits b = ref _addr_b.val;

    foreach (var (i) in b) {
        b[i] = 0;
    }
}

// popcntRange counts the number of set bits in the
// range [i, i+n).
private static nuint popcntRange(this ptr<pageBits> _addr_b, nuint i, nuint n) {
    nuint s = default;
    ref pageBits b = ref _addr_b.val;

    if (n == 1) {
        return uint((b[i / 64] >> (int)((i % 64))) & 1);
    }
    _ = b[i / 64];
    var j = i + n - 1;
    if (i / 64 == j / 64) {
        return uint(sys.OnesCount64((b[i / 64] >> (int)((i % 64))) & ((1 << (int)(n)) - 1)));
    }
    _ = b[j / 64];
    s += uint(sys.OnesCount64(b[i / 64] >> (int)((i % 64))));
    for (var k = i / 64 + 1; k < j / 64; k++) {
        s += uint(sys.OnesCount64(b[k]));
    }
    s += uint(sys.OnesCount64(b[j / 64] & ((1 << (int)((j % 64 + 1))) - 1)));
    return ;
}

// pallocBits is a bitmap that tracks page allocations for at most one
// palloc chunk.
//
// The precise representation is an implementation detail, but for the
// sake of documentation, 0s are free pages and 1s are allocated pages.
private partial struct pallocBits { // : pageBits
}

// summarize returns a packed summary of the bitmap in pallocBits.
private static pallocSum summarize(this ptr<pallocBits> _addr_b) {
    ref pallocBits b = ref _addr_b.val;

    nuint start = default;    nuint max = default;    nuint cur = default;

    const var notSetYet = ~uint(0); // sentinel for start value
 // sentinel for start value
    start = notSetYet;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(b); i++) {
            var x = b[i];
            if (x == 0) {
                cur += 64;
                continue;
            }
            var t = uint(sys.TrailingZeros64(x));
            var l = uint(sys.LeadingZeros64(x)); 

            // Finish any region spanning the uint64s
            cur += t;
            if (start == notSetYet) {
                start = cur;
            }
            if (cur > max) {
                max = cur;
            } 
            // Final region that might span to next uint64
            cur = l;
        }

        i = i__prev1;
    }
    if (start == notSetYet) { 
        // Made it all the way through without finding a single 1 bit.
        const var n = uint(64 * len(b));

        return packPallocSum(n, n, n);
    }
    if (cur > max) {
        max = cur;
    }
    if (max >= 64 - 2) { 
        // There is no way an internal run of zeros could beat max.
        return packPallocSum(start, max, cur);
    }
outer:
    {
        nint i__prev1 = i;

        for (i = 0; i < len(b); i++) {
            x = b[i]; 

            // Look inside this uint64. We have a pattern like
            // 000000 1xxxxx1 000000
            // We need to look inside the 1xxxxx1 for any contiguous
            // region of zeros.

            // We already know the trailing zeros are no larger than max. Remove them.
            x>>=sys.TrailingZeros64(x) & 63;
            if (x & (x + 1) == 0) { // no more zeros (except at the top).
                continue;
            } 

            // Strategy: shrink all runs of zeros by max. If any runs of zero
            // remain, then we've identified a larger maxiumum zero run.
            var p = max; // number of zeros we still need to shrink by.
            var k = uint(1); // current minimum length of runs of ones in x.
            while (true) { 
                // Shrink all runs of zeros by p places (except the top zeros).
                while (p > 0) {
                    if (p <= k) { 
                        // Shift p ones down into the top of each run of zeros.
                        x |= x >> (int)((p & 63));
                        if (x & (x + 1) == 0) { // no more zeros (except at the top).
                            _continueouter = true;
                            break;
                        }
                        break;
                    } 
                    // Shift k ones down into the top of each run of zeros.
                    x |= x >> (int)((k & 63));
                    if (x & (x + 1) == 0) { // no more zeros (except at the top).
                        _continueouter = true;
                        break;
                    }
                    p -= k; 
                    // We've just doubled the minimum length of 1-runs.
                    // This allows us to shift farther in the next iteration.
                    k *= 2;
                } 

                // The length of the lowest-order zero run is an increment to our maximum.
 

                // The length of the lowest-order zero run is an increment to our maximum.
                var j = uint(sys.TrailingZeros64(~x)); // count contiguous trailing ones
                x>>=j & 63; // remove trailing ones
                j = uint(sys.TrailingZeros64(x)); // count contiguous trailing zeros
                x>>=j & 63; // remove zeros
                max += j; // we have a new maximum!
                if (x & (x + 1) == 0) { // no more zeros (except at the top).
                    _continueouter = true;
                    break;
                }
                p = j; // remove j more zeros from each zero run.
            }
        }

        i = i__prev1;
    }
    return packPallocSum(start, max, cur);
}

// find searches for npages contiguous free pages in pallocBits and returns
// the index where that run starts, as well as the index of the first free page
// it found in the search. searchIdx represents the first known free page and
// where to begin the next search from.
//
// If find fails to find any free space, it returns an index of ^uint(0) and
// the new searchIdx should be ignored.
//
// Note that if npages == 1, the two returned values will always be identical.
private static (nuint, nuint) find(this ptr<pallocBits> _addr_b, System.UIntPtr npages, nuint searchIdx) {
    nuint _p0 = default;
    nuint _p0 = default;
    ref pallocBits b = ref _addr_b.val;

    if (npages == 1) {
        var addr = b.find1(searchIdx);
        return (addr, addr);
    }
    else if (npages <= 64) {
        return b.findSmallN(npages, searchIdx);
    }
    return b.findLargeN(npages, searchIdx);
}

// find1 is a helper for find which searches for a single free page
// in the pallocBits and returns the index.
//
// See find for an explanation of the searchIdx parameter.
private static nuint find1(this ptr<pallocBits> _addr_b, nuint searchIdx) {
    ref pallocBits b = ref _addr_b.val;

    _ = b[0]; // lift nil check out of loop
    for (var i = searchIdx / 64; i < uint(len(b)); i++) {
        var x = b[i];
        if (~x == 0) {
            continue;
        }
        return i * 64 + uint(sys.TrailingZeros64(~x));
    }
    return ~uint(0);
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
private static (nuint, nuint) findSmallN(this ptr<pallocBits> _addr_b, System.UIntPtr npages, nuint searchIdx) {
    nuint _p0 = default;
    nuint _p0 = default;
    ref pallocBits b = ref _addr_b.val;

    var end = uint(0);
    var newSearchIdx = ~uint(0);
    for (var i = searchIdx / 64; i < uint(len(b)); i++) {
        var bi = b[i];
        if (~bi == 0) {
            end = 0;
            continue;
        }
        if (newSearchIdx == ~uint(0)) { 
            // The new searchIdx is going to be at these 64 bits after any
            // 1s we file, so count trailing 1s.
            newSearchIdx = i * 64 + uint(sys.TrailingZeros64(~bi));
        }
        var start = uint(sys.TrailingZeros64(bi));
        if (end + start >= uint(npages)) {
            return (i * 64 - end, newSearchIdx);
        }
        var j = findBitRange64(~bi, uint(npages));
        if (j < 64) {
            return (i * 64 + j, newSearchIdx);
        }
        end = uint(sys.LeadingZeros64(bi));
    }
    return (~uint(0), newSearchIdx);
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
private static (nuint, nuint) findLargeN(this ptr<pallocBits> _addr_b, System.UIntPtr npages, nuint searchIdx) {
    nuint _p0 = default;
    nuint _p0 = default;
    ref pallocBits b = ref _addr_b.val;

    var start = ~uint(0);
    var size = uint(0);
    var newSearchIdx = ~uint(0);
    for (var i = searchIdx / 64; i < uint(len(b)); i++) {
        var x = b[i];
        if (x == ~uint64(0)) {
            size = 0;
            continue;
        }
        if (newSearchIdx == ~uint(0)) { 
            // The new searchIdx is going to be at these 64 bits after any
            // 1s we file, so count trailing 1s.
            newSearchIdx = i * 64 + uint(sys.TrailingZeros64(~x));
        }
        if (size == 0) {
            size = uint(sys.LeadingZeros64(x));
            start = i * 64 + 64 - size;
            continue;
        }
        var s = uint(sys.TrailingZeros64(x));
        if (s + size >= uint(npages)) {
            size += s;
            return (start, newSearchIdx);
        }
        if (s < 64) {
            size = uint(sys.LeadingZeros64(x));
            start = i * 64 + 64 - size;
            continue;
        }
        size += 64;
    }
    if (size < uint(npages)) {
        return (~uint(0), newSearchIdx);
    }
    return (start, newSearchIdx);
}

// allocRange allocates the range [i, i+n).
private static void allocRange(this ptr<pallocBits> _addr_b, nuint i, nuint n) {
    ref pallocBits b = ref _addr_b.val;

    (pageBits.val)(b).setRange(i, n);
}

// allocAll allocates all the bits of b.
private static void allocAll(this ptr<pallocBits> _addr_b) {
    ref pallocBits b = ref _addr_b.val;

    (pageBits.val)(b).setAll();
}

// free1 frees a single page in the pallocBits at i.
private static void free1(this ptr<pallocBits> _addr_b, nuint i) {
    ref pallocBits b = ref _addr_b.val;

    (pageBits.val)(b).clear(i);
}

// free frees the range [i, i+n) of pages in the pallocBits.
private static void free(this ptr<pallocBits> _addr_b, nuint i, nuint n) {
    ref pallocBits b = ref _addr_b.val;

    (pageBits.val)(b).clearRange(i, n);
}

// freeAll frees all the bits of b.
private static void freeAll(this ptr<pallocBits> _addr_b) {
    ref pallocBits b = ref _addr_b.val;

    (pageBits.val)(b).clearAll();
}

// pages64 returns a 64-bit bitmap representing a block of 64 pages aligned
// to 64 pages. The returned block of pages is the one containing the i'th
// page in this pallocBits. Each bit represents whether the page is in-use.
private static ulong pages64(this ptr<pallocBits> _addr_b, nuint i) {
    ref pallocBits b = ref _addr_b.val;

    return (pageBits.val)(b).block64(i);
}

// findBitRange64 returns the bit index of the first set of
// n consecutive 1 bits. If no consecutive set of 1 bits of
// size n may be found in c, then it returns an integer >= 64.
// n must be > 0.
private static nuint findBitRange64(ulong c, nuint n) { 
    // This implementation is based on shrinking the length of
    // runs of contiguous 1 bits. We remove the top n-1 1 bits
    // from each run of 1s, then look for the first remaining 1 bit.
    var p = n - 1; // number of 1s we want to remove.
    var k = uint(1); // current minimum width of runs of 0 in c.
    while (p > 0) {
        if (p <= k) { 
            // Shift p 0s down into the top of each run of 1s.
            c &= c >> (int)((p & 63));
            break;
        }
        c &= c >> (int)((k & 63));
        if (c == 0) {
            return 64;
        }
        p -= k; 
        // We've just doubled the minimum length of 0-runs.
        // This allows us to shift farther in the next iteration.
        k *= 2;
    } 
    // Find first remaining 1.
    // Since we shrunk from the top down, the first 1 is in
    // its correct original position.
    return uint(sys.TrailingZeros64(c));
}

// pallocData encapsulates pallocBits and a bitmap for
// whether or not a given page is scavenged in a single
// structure. It's effectively a pallocBits with
// additional functionality.
//
// Update the comment on (*pageAlloc).chunks should this
// structure change.
private partial struct pallocData {
    public ref pallocBits pallocBits => ref pallocBits_val;
    public pageBits scavenged;
}

// allocRange sets bits [i, i+n) in the bitmap to 1 and
// updates the scavenged bits appropriately.
private static void allocRange(this ptr<pallocData> _addr_m, nuint i, nuint n) {
    ref pallocData m = ref _addr_m.val;
 
    // Clear the scavenged bits when we alloc the range.
    m.pallocBits.allocRange(i, n);
    m.scavenged.clearRange(i, n);
}

// allocAll sets every bit in the bitmap to 1 and updates
// the scavenged bits appropriately.
private static void allocAll(this ptr<pallocData> _addr_m) {
    ref pallocData m = ref _addr_m.val;
 
    // Clear the scavenged bits when we alloc the range.
    m.pallocBits.allocAll();
    m.scavenged.clearAll();
}

} // end runtime_package
