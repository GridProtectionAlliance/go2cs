// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Address range data structure.
//
// This file contains an implementation of a data structure which
// manages ordered address ranges.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// addrRange represents a region of address space.
//
// An addrRange must never span a gap in the address space.
[GoType] partial struct addrRange {
    // base and limit together represent the region of address space
    // [base, limit). That is, base is inclusive, limit is exclusive.
    // These are address over an offset view of the address space on
    // platforms with a segmented address space, that is, on platforms
    // where arenaBaseOffset != 0.
    internal offAddr @base;
    internal offAddr limit;
}

// makeAddrRange creates a new address range from two virtual addresses.
//
// Throws if the base and limit are not in the same memory segment.
internal static addrRange makeAddrRange(uintptr @base, uintptr limit) {
    var r = new addrRange(new offAddr(@base), new offAddr(limit));
    if ((@base - arenaBaseOffset >= @base) != (limit - arenaBaseOffset >= limit)) {
        @throw("addr range base and limit are not in the same memory segment"u8);
    }
    return r;
}

// size returns the size of the range represented in bytes.
internal static uintptr size(this addrRange a) {
    if (!a.@base.lessThan(a.limit)) {
        return 0;
    }
    // Subtraction is safe because limit and base must be in the same
    // segment of the address space.
    return a.limit.diff(a.@base);
}

// contains returns whether or not the range contains a given address.
internal static bool contains(this addrRange a, uintptr addr) {
    return a.@base.lessEqual(new offAddr(addr)) && (new offAddr(addr)).lessThan(a.limit);
}

// subtract takes the addrRange toPrune and cuts out any overlap with
// from, then returns the new range. subtract assumes that a and b
// either don't overlap at all, only overlap on one side, or are equal.
// If b is strictly contained in a, thus forcing a split, it will throw.
internal static addrRange subtract(this addrRange a, addrRange b) {
    if (b.@base.lessEqual(a.@base) && a.limit.lessEqual(b.limit)){
        return new addrRange(nil);
    } else 
    if (a.@base.lessThan(b.@base) && b.limit.lessThan(a.limit)){
        @throw("bad prune"u8);
    } else 
    if (b.limit.lessThan(a.limit) && a.@base.lessThan(b.limit)){
        a.@base = b.limit;
    } else 
    if (a.@base.lessThan(b.@base) && b.@base.lessThan(a.limit)) {
        a.limit = b.@base;
    }
    return a;
}

// takeFromFront takes len bytes from the front of the address range, aligning
// the base to align first. On success, returns the aligned start of the region
// taken and true.
[GoRecv] internal static (uintptr, bool) takeFromFront(this ref addrRange a, uintptr len, uint8 align) {
    var @base = alignUp(a.@base.addr(), ((uintptr)align)) + len;
    if (@base > a.limit.addr()) {
        return (0, false);
    }
    a.@base = new offAddr(@base);
    return (@base - len, true);
}

// takeFromBack takes len bytes from the end of the address range, aligning
// the limit to align after subtracting len. On success, returns the aligned
// start of the region taken and true.
[GoRecv] internal static (uintptr, bool) takeFromBack(this ref addrRange a, uintptr len, uint8 align) {
    var limit = alignDown(a.limit.addr() - len, ((uintptr)align));
    if (a.@base.addr() > limit) {
        return (0, false);
    }
    a.limit = new offAddr(limit);
    return (limit, true);
}

// removeGreaterEqual removes all addresses in a greater than or equal
// to addr and returns the new range.
internal static addrRange removeGreaterEqual(this addrRange a, uintptr addr) {
    if ((new offAddr(addr)).lessEqual(a.@base)) {
        return new addrRange(nil);
    }
    if (a.limit.lessEqual(new offAddr(addr))) {
        return a;
    }
    return makeAddrRange(a.@base.addr(), addr);
}

internal static offAddr minOffAddr = new offAddr(arenaBaseOffset);
internal static offAddr maxOffAddr = new offAddr((uintptr)((((1 << (int)(heapAddrBits)) - 1) + arenaBaseOffset) & uintptrMask));

// offAddr represents an address in a contiguous view
// of the address space on systems where the address space is
// segmented. On other systems, it's just a normal address.
[GoType] partial struct offAddr {
    // a is just the virtual address, but should never be used
    // directly. Call addr() to get this value instead.
    internal uintptr a;
}

// add adds a uintptr offset to the offAddr.
internal static offAddr add(this offAddr l, uintptr bytes) {
    return new offAddr(a: l.a + bytes);
}

// sub subtracts a uintptr offset from the offAddr.
internal static offAddr sub(this offAddr l, uintptr bytes) {
    return new offAddr(a: l.a - bytes);
}

// diff returns the amount of bytes in between the
// two offAddrs.
internal static uintptr diff(this offAddr l1, offAddr l2) {
    return l1.a - l2.a;
}

// lessThan returns true if l1 is less than l2 in the offset
// address space.
internal static bool lessThan(this offAddr l1, offAddr l2) {
    return (l1.a - arenaBaseOffset) < (l2.a - arenaBaseOffset);
}

// lessEqual returns true if l1 is less than or equal to l2 in
// the offset address space.
internal static bool lessEqual(this offAddr l1, offAddr l2) {
    return (l1.a - arenaBaseOffset) <= (l2.a - arenaBaseOffset);
}

// equal returns true if the two offAddr values are equal.
internal static bool equal(this offAddr l1, offAddr l2) {
    // No need to compare in the offset space, it
    // means the same thing.
    return l1 == l2;
}

// addr returns the virtual address for this offset address.
internal static uintptr addr(this offAddr l) {
    return l.a;
}

// atomicOffAddr is like offAddr, but operations on it are atomic.
// It also contains operations to be able to store marked addresses
// to ensure that they're not overridden until they've been seen.
[GoType] partial struct atomicOffAddr {
    // a contains the offset address, unlike offAddr.
    internal @internal.runtime.atomic_package.Int64 a;
}

// Clear attempts to store minOffAddr in atomicOffAddr. It may fail
// if a marked value is placed in the box in the meanwhile.
[GoRecv] internal static void Clear(this ref atomicOffAddr b) {
    while (ᐧ) {
        var old = b.a.Load();
        if (old < 0) {
            return;
        }
        if (b.a.CompareAndSwap(old, ((int64)(minOffAddr.addr() - arenaBaseOffset)))) {
            return;
        }
    }
}

// StoreMin stores addr if it's less than the current value in the
// offset address space if the current value is not marked.
[GoRecv] internal static void StoreMin(this ref atomicOffAddr b, uintptr addr) {
    var @new = ((int64)(addr - arenaBaseOffset));
    while (ᐧ) {
        var old = b.a.Load();
        if (old < @new) {
            return;
        }
        if (b.a.CompareAndSwap(old, @new)) {
            return;
        }
    }
}

// StoreUnmark attempts to unmark the value in atomicOffAddr and
// replace it with newAddr. markedAddr must be a marked address
// returned by Load. This function will not store newAddr if the
// box no longer contains markedAddr.
[GoRecv] internal static void StoreUnmark(this ref atomicOffAddr b, uintptr markedAddr, uintptr newAddr) {
    b.a.CompareAndSwap(-((int64)(markedAddr - arenaBaseOffset)), ((int64)(newAddr - arenaBaseOffset)));
}

// StoreMarked stores addr but first converted to the offset address
// space and then negated.
[GoRecv] internal static void StoreMarked(this ref atomicOffAddr b, uintptr addr) {
    b.a.Store(-((int64)(addr - arenaBaseOffset)));
}

// Load returns the address in the box as a virtual address. It also
// returns if the value was marked or not.
[GoRecv] internal static (uintptr, bool) Load(this ref atomicOffAddr b) {
    var v = b.a.Load();
    var wasMarked = false;
    if (v < 0) {
        wasMarked = true;
        v = -v;
    }
    return (((uintptr)v) + arenaBaseOffset, wasMarked);
}

// addrRanges is a data structure holding a collection of ranges of
// address space.
//
// The ranges are coalesced eagerly to reduce the
// number ranges it holds.
//
// The slice backing store for this field is persistentalloc'd
// and thus there is no way to free it.
//
// addrRanges is not thread-safe.
[GoType] partial struct addrRanges {
    // ranges is a slice of ranges sorted by base.
    internal slice<addrRange> ranges;
    // totalBytes is the total amount of address space in bytes counted by
    // this addrRanges.
    internal uintptr totalBytes;
    // sysStat is the stat to track allocations by this type
    internal ж<sysMemStat> sysStat;
}

[GoRecv] internal static void init(this ref addrRanges a, ж<sysMemStat> ᏑsysStat) {
    ref var sysStat = ref ᏑsysStat.val;

    var ranges = (ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ(a.ranges)));
    ranges.val.len = 0;
    ranges.val.cap = 16;
    ranges.val.Δarray = (ж<notInHeap>)(uintptr)(persistentalloc(@unsafe.Sizeof(new addrRange(nil)) * ((uintptr)(~ranges).cap), goarch.PtrSize, ᏑsysStat));
    a.sysStat = sysStat;
    a.totalBytes = 0;
}

// findSucc returns the first index in a such that addr is
// less than the base of the addrRange at that index.
[GoRecv] internal static nint findSucc(this ref addrRanges a, uintptr addr) {
    var @base = new offAddr(addr);
    // Narrow down the search space via a binary search
    // for large addrRanges until we have at most iterMax
    // candidates left.
    static readonly UntypedInt iterMax = 8;
    nint bot = 0;
    nint top = len(a.ranges);
    while (top - bot > iterMax) {
        nint iΔ1 = ((nint)(((nuint)(bot + top)) >> (int)(1)));
        if (a.ranges[iΔ1].contains(@base.addr())) {
            // a.ranges[i] contains base, so
            // its successor is the next index.
            return iΔ1 + 1;
        }
        if (@base.lessThan(a.ranges[iΔ1].@base)){
            // In this case i might actually be
            // the successor, but we can't be sure
            // until we check the ones before it.
            top = iΔ1;
        } else {
            // In this case we know base is
            // greater than or equal to a.ranges[i].limit-1,
            // so i is definitely not the successor.
            // We already checked i, so pick the next
            // one.
            bot = iΔ1 + 1;
        }
    }
    // There are top-bot candidates left, so
    // iterate over them and find the first that
    // base is strictly less than.
    for (nint i = bot; i < top; i++) {
        if (@base.lessThan(a.ranges[i].@base)) {
            return i;
        }
    }
    return top;
}

// findAddrGreaterEqual returns the smallest address represented by a
// that is >= addr. Thus, if the address is represented by a,
// then it returns addr. The second return value indicates whether
// such an address exists for addr in a. That is, if addr is larger than
// any address known to a, the second return value will be false.
[GoRecv] internal static (uintptr, bool) findAddrGreaterEqual(this ref addrRanges a, uintptr addr) {
    nint i = a.findSucc(addr);
    if (i == 0) {
        return (a.ranges[0].@base.addr(), true);
    }
    if (a.ranges[i - 1].contains(addr)) {
        return (addr, true);
    }
    if (i < len(a.ranges)) {
        return (a.ranges[i].@base.addr(), true);
    }
    return (0, false);
}

// contains returns true if a covers the address addr.
[GoRecv] internal static bool contains(this ref addrRanges a, uintptr addr) {
    nint i = a.findSucc(addr);
    if (i == 0) {
        return false;
    }
    return a.ranges[i - 1].contains(addr);
}

// add inserts a new address range to a.
//
// r must not overlap with any address range in a and r.size() must be > 0.
[GoRecv] internal static void add(this ref addrRanges a, addrRange r) {
    // The copies in this function are potentially expensive, but this data
    // structure is meant to represent the Go heap. At worst, copying this
    // would take ~160µs assuming a conservative copying rate of 25 GiB/s (the
    // copy will almost never trigger a page fault) for a 1 TiB heap with 4 MiB
    // arenas which is completely discontiguous. ~160µs is still a lot, but in
    // practice most platforms have 64 MiB arenas (which cuts this by a factor
    // of 16) and Go heaps are usually mostly contiguous, so the chance that
    // an addrRanges even grows to that size is extremely low.
    // An empty range has no effect on the set of addresses represented
    // by a, but passing a zero-sized range is almost always a bug.
    if (r.size() == 0) {
        print("runtime: range = {", ((Δhex)r.@base.addr()), ", ", ((Δhex)r.limit.addr()), "}\n");
        @throw("attempted to add zero-sized address range"u8);
    }
    // Because we assume r is not currently represented in a,
    // findSucc gives us our insertion index.
    nint i = a.findSucc(r.@base.addr());
    var coalescesDown = i > 0 && a.ranges[i - 1].limit.equal(r.@base);
    var coalescesUp = i < len(a.ranges) && r.limit.equal(a.ranges[i].@base);
    if (coalescesUp && coalescesDown){
        // We have neighbors and they both border us.
        // Merge a.ranges[i-1], r, and a.ranges[i] together into a.ranges[i-1].
        a.ranges[i - 1].limit = a.ranges[i].limit;
        // Delete a.ranges[i].
        copy(a.ranges[(int)(i)..], a.ranges[(int)(i + 1)..]);
        a.ranges = a.ranges[..(int)(len(a.ranges) - 1)];
    } else 
    if (coalescesDown){
        // We have a neighbor at a lower address only and it borders us.
        // Merge the new space into a.ranges[i-1].
        a.ranges[i - 1].limit = r.limit;
    } else 
    if (coalescesUp){
        // We have a neighbor at a higher address only and it borders us.
        // Merge the new space into a.ranges[i].
        a.ranges[i].@base = r.@base;
    } else {
        // We may or may not have neighbors which don't border us.
        // Add the new range.
        if (len(a.ranges) + 1 > cap(a.ranges)){
            // Grow the array. Note that this leaks the old array, but since
            // we're doubling we have at most 2x waste. For a 1 TiB heap and
            // 4 MiB arenas which are all discontiguous (both very conservative
            // assumptions), this would waste at most 4 MiB of memory.
            var oldRanges = a.ranges;
            var ranges = (ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ(a.ranges)));
            ranges.val.len = len(oldRanges) + 1;
            ranges.val.cap = cap(oldRanges) * 2;
            ranges.val.Δarray = (ж<notInHeap>)(uintptr)(persistentalloc(@unsafe.Sizeof(new addrRange(nil)) * ((uintptr)(~ranges).cap), goarch.PtrSize, a.sysStat));
            // Copy in the old array, but make space for the new range.
            copy(a.ranges[..(int)(i)], oldRanges[..(int)(i)]);
            copy(a.ranges[(int)(i + 1)..], oldRanges[(int)(i)..]);
        } else {
            a.ranges = a.ranges[..(int)(len(a.ranges) + 1)];
            copy(a.ranges[(int)(i + 1)..], a.ranges[(int)(i)..]);
        }
        a.ranges[i] = r;
    }
    a.totalBytes += r.size();
}

// removeLast removes and returns the highest-addressed contiguous range
// of a, or the last nBytes of that range, whichever is smaller. If a is
// empty, it returns an empty range.
[GoRecv] internal static addrRange removeLast(this ref addrRanges a, uintptr nBytes) {
    if (len(a.ranges) == 0) {
        return new addrRange(nil);
    }
    var r = a.ranges[len(a.ranges) - 1];
    var size = r.size();
    if (size > nBytes) {
        var newEnd = r.limit.sub(nBytes);
        a.ranges[len(a.ranges) - 1].limit = newEnd;
        a.totalBytes -= nBytes;
        return new addrRange(newEnd, r.limit);
    }
    a.ranges = a.ranges[..(int)(len(a.ranges) - 1)];
    a.totalBytes -= size;
    return r;
}

// removeGreaterEqual removes the ranges of a which are above addr, and additionally
// splits any range containing addr.
[GoRecv] internal static void removeGreaterEqual(this ref addrRanges a, uintptr addr) {
    nint pivot = a.findSucc(addr);
    if (pivot == 0) {
        // addr is before all ranges in a.
        a.totalBytes = 0;
        a.ranges = a.ranges[..0];
        return;
    }
    var removed = ((uintptr)0);
    foreach (var (_, r) in a.ranges[(int)(pivot)..]) {
        removed += r.size();
    }
    {
        var r = a.ranges[pivot - 1]; if (r.contains(addr)) {
            removed += r.size();
            r = r.removeGreaterEqual(addr);
            if (r.size() == 0){
                pivot--;
            } else {
                removed -= r.size();
                a.ranges[pivot - 1] = r;
            }
        }
    }
    a.ranges = a.ranges[..(int)(pivot)];
    a.totalBytes -= removed;
}

// cloneInto makes a deep clone of a's state into b, re-using
// b's ranges if able.
[GoRecv] internal static void cloneInto(this ref addrRanges a, ж<addrRanges> Ꮡb) {
    ref var b = ref Ꮡb.val;

    if (len(a.ranges) > cap(b.ranges)) {
        // Grow the array.
        var ranges = (ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ(b.ranges)));
        ranges.val.len = 0;
        ranges.val.cap = cap(a.ranges);
        ranges.val.Δarray = (ж<notInHeap>)(uintptr)(persistentalloc(@unsafe.Sizeof(new addrRange(nil)) * ((uintptr)(~ranges).cap), goarch.PtrSize, b.sysStat));
    }
    b.ranges = b.ranges[..(int)(len(a.ranges))];
    b.totalBytes = a.totalBytes;
    copy(b.ranges, a.ranges);
}

} // end runtime_package
