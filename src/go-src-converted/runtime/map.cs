// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:24:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\map.go
namespace go;
// This file contains the implementation of Go's map type.
//
// A map is just a hash table. The data is arranged
// into an array of buckets. Each bucket contains up to
// 8 key/elem pairs. The low-order bits of the hash are
// used to select a bucket. Each bucket contains a few
// high-order bits of each hash to distinguish the entries
// within a single bucket.
//
// If more than 8 keys hash to a bucket, we chain on
// extra buckets.
//
// When the hashtable grows, we allocate a new array
// of buckets twice as big. Buckets are incrementally
// copied from the old bucket array to the new bucket array.
//
// Map iterators walk through the array of buckets and
// return the keys in walk order (bucket #, then overflow
// chain order, then bucket index).  To maintain iteration
// semantics, we never move keys within their bucket (if
// we did, keys might be returned 0 or 2 times).  When
// growing the table, iterators remain iterating through the
// old table and must check the new table if the bucket
// they are iterating through has been moved ("evacuated")
// to the new table.

// Picking loadFactor: too large and we have lots of overflow
// buckets, too small and we waste a lot of space. I wrote
// a simple program to check some stats for different loads:
// (64-bit, 8 byte keys and elems)
//  loadFactor    %overflow  bytes/entry     hitprobe    missprobe
//        4.00         2.13        20.77         3.00         4.00
//        4.50         4.05        17.30         3.25         4.50
//        5.00         6.85        14.77         3.50         5.00
//        5.50        10.55        12.94         3.75         5.50
//        6.00        15.27        11.67         4.00         6.00
//        6.50        20.90        10.79         4.25         6.50
//        7.00        27.14        10.15         4.50         7.00
//        7.50        34.03         9.73         4.75         7.50
//        8.00        41.10         9.40         5.00         8.00
//
// %overflow   = percentage of buckets which have an overflow bucket
// bytes/entry = overhead bytes used per key/elem pair
// hitprobe    = # of entries to check when looking up a present key
// missprobe   = # of entries to check when looking up an absent key
//
// Keep in mind this data is for maximally loaded tables, i.e. just
// before the table grows. Typical tables will be somewhat less loaded.


using atomic = runtime.@internal.atomic_package;
using math = runtime.@internal.math_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;

public static partial class runtime_package {

 
// Maximum number of key/elem pairs a bucket can hold.
private static readonly nint bucketCntBits = 3;
private static readonly nint bucketCnt = 1 << (int)(bucketCntBits); 

// Maximum average load of a bucket that triggers growth is 6.5.
// Represent as loadFactorNum/loadFactorDen, to allow integer math.
private static readonly nint loadFactorNum = 13;
private static readonly nint loadFactorDen = 2; 

// Maximum key or elem size to keep inline (instead of mallocing per element).
// Must fit in a uint8.
// Fast versions cannot handle big elems - the cutoff size for
// fast versions in cmd/compile/internal/gc/walk.go must be at most this elem.
private static readonly nint maxKeySize = 128;
private static readonly nint maxElemSize = 128; 

// data offset should be the size of the bmap struct, but needs to be
// aligned correctly. For amd64p32 this means 64-bit alignment
// even though pointers are 32 bit.
private static readonly var dataOffset = @unsafe.Offsetof(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{bbmapvint64}{}.v); 

// Possible tophash values. We reserve a few possibilities for special marks.
// Each bucket (including its overflow buckets, if any) will have either all or none of its
// entries in the evacuated* states (except during the evacuate() method, which only happens
// during map writes and thus no one else can observe the map during that time).
private static readonly nint emptyRest = 0; // this cell is empty, and there are no more non-empty cells at higher indexes or overflows.
private static readonly nint emptyOne = 1; // this cell is empty
private static readonly nint evacuatedX = 2; // key/elem is valid.  Entry has been evacuated to first half of larger table.
private static readonly nint evacuatedY = 3; // same as above, but evacuated to second half of larger table.
private static readonly nint evacuatedEmpty = 4; // cell is empty, bucket is evacuated.
private static readonly nint minTopHash = 5; // minimum tophash for a normal filled cell.

// flags
private static readonly nint iterator = 1; // there may be an iterator using buckets
private static readonly nint oldIterator = 2; // there may be an iterator using oldbuckets
private static readonly nint hashWriting = 4; // a goroutine is writing to the map
private static readonly nint sameSizeGrow = 8; // the current map growth is to a new map of the same size

// sentinel bucket ID for iterator checks
private static readonly nint noCheck = 1 << (int)((8 * sys.PtrSize)) - 1;

// isEmpty reports whether the given tophash array entry represents an empty bucket entry.
private static bool isEmpty(byte x) {
    return x <= emptyOne;
}

// A header for a Go map.
private partial struct hmap {
    public nint count; // # live cells == size of map.  Must be first (used by len() builtin)
    public byte flags;
    public byte B; // log_2 of # of buckets (can hold up to loadFactor * 2^B items)
    public ushort noverflow; // approximate number of overflow buckets; see incrnoverflow for details
    public uint hash0; // hash seed

    public unsafe.Pointer buckets; // array of 2^B Buckets. may be nil if count==0.
    public unsafe.Pointer oldbuckets; // previous bucket array of half the size, non-nil only when growing
    public System.UIntPtr nevacuate; // progress counter for evacuation (buckets less than this have been evacuated)

    public ptr<mapextra> extra; // optional fields
}

// mapextra holds fields that are not present on all maps.
private partial struct mapextra {
    public ptr<slice<ptr<bmap>>> overflow;
    public ptr<slice<ptr<bmap>>> oldoverflow; // nextOverflow holds a pointer to a free overflow bucket.
    public ptr<bmap> nextOverflow;
}

// A bucket for a Go map.
private partial struct bmap {
    public array<byte> tophash; // Followed by bucketCnt keys and then bucketCnt elems.
// NOTE: packing all the keys together and then all the elems together makes the
// code a bit more complicated than alternating key/elem/key/elem/... but it allows
// us to eliminate padding which would be needed for, e.g., map[int64]int8.
// Followed by an overflow pointer.
}

// A hash iteration structure.
// If you modify hiter, also change cmd/compile/internal/reflectdata/reflect.go to indicate
// the layout of this structure.
private partial struct hiter {
    public unsafe.Pointer key; // Must be in first position.  Write nil to indicate iteration end (see cmd/compile/internal/walk/range.go).
    public unsafe.Pointer elem; // Must be in second position (see cmd/compile/internal/walk/range.go).
    public ptr<maptype> t;
    public ptr<hmap> h;
    public unsafe.Pointer buckets; // bucket ptr at hash_iter initialization time
    public ptr<bmap> bptr; // current bucket
    public ptr<slice<ptr<bmap>>> overflow; // keeps overflow buckets of hmap.buckets alive
    public ptr<slice<ptr<bmap>>> oldoverflow; // keeps overflow buckets of hmap.oldbuckets alive
    public System.UIntPtr startBucket; // bucket iteration started at
    public byte offset; // intra-bucket offset to start from during iteration (should be big enough to hold bucketCnt-1)
    public bool wrapped; // already wrapped around from end of bucket array to beginning
    public byte B;
    public byte i;
    public System.UIntPtr bucket;
    public System.UIntPtr checkBucket;
}

// bucketShift returns 1<<b, optimized for code generation.
private static System.UIntPtr bucketShift(byte b) { 
    // Masking the shift amount allows overflow checks to be elided.
    return uintptr(1) << (int)((b & (sys.PtrSize * 8 - 1)));
}

// bucketMask returns 1<<b - 1, optimized for code generation.
private static System.UIntPtr bucketMask(byte b) {
    return bucketShift(b) - 1;
}

// tophash calculates the tophash value for hash.
private static byte tophash(System.UIntPtr hash) {
    var top = uint8(hash >> (int)((sys.PtrSize * 8 - 8)));
    if (top < minTopHash) {
        top += minTopHash;
    }
    return top;
}

private static bool evacuated(ptr<bmap> _addr_b) {
    ref bmap b = ref _addr_b.val;

    var h = b.tophash[0];
    return h > emptyOne && h < minTopHash;
}

private static ptr<bmap> overflow(this ptr<bmap> _addr_b, ptr<maptype> _addr_t) {
    ref bmap b = ref _addr_b.val;
    ref maptype t = ref _addr_t.val;

    return new ptr<ptr<ptr<ptr<bmap>>>>(add(@unsafe.Pointer(b), uintptr(t.bucketsize) - sys.PtrSize));
}

private static void setoverflow(this ptr<bmap> _addr_b, ptr<maptype> _addr_t, ptr<bmap> _addr_ovf) {
    ref bmap b = ref _addr_b.val;
    ref maptype t = ref _addr_t.val;
    ref bmap ovf = ref _addr_ovf.val;

    (bmap.val).val;

    (add(@unsafe.Pointer(b), uintptr(t.bucketsize) - sys.PtrSize)) = ovf;
}

private static unsafe.Pointer keys(this ptr<bmap> _addr_b) {
    ref bmap b = ref _addr_b.val;

    return add(@unsafe.Pointer(b), dataOffset);
}

// incrnoverflow increments h.noverflow.
// noverflow counts the number of overflow buckets.
// This is used to trigger same-size map growth.
// See also tooManyOverflowBuckets.
// To keep hmap small, noverflow is a uint16.
// When there are few buckets, noverflow is an exact count.
// When there are many buckets, noverflow is an approximate count.
private static void incrnoverflow(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;
 
    // We trigger same-size map growth if there are
    // as many overflow buckets as buckets.
    // We need to be able to count to 1<<h.B.
    if (h.B < 16) {
        h.noverflow++;
        return ;
    }
    var mask = uint32(1) << (int)((h.B - 15)) - 1; 
    // Example: if h.B == 18, then mask == 7,
    // and fastrand & 7 == 0 with probability 1/8.
    if (fastrand() & mask == 0) {
        h.noverflow++;
    }
}

private static ptr<bmap> newoverflow(this ptr<hmap> _addr_h, ptr<maptype> _addr_t, ptr<bmap> _addr_b) {
    ref hmap h = ref _addr_h.val;
    ref maptype t = ref _addr_t.val;
    ref bmap b = ref _addr_b.val;

    ptr<bmap> ovf;
    if (h.extra != null && h.extra.nextOverflow != null) { 
        // We have preallocated overflow buckets available.
        // See makeBucketArray for more details.
        ovf = h.extra.nextOverflow;
        if (ovf.overflow(t) == null) { 
            // We're not at the end of the preallocated overflow buckets. Bump the pointer.
            h.extra.nextOverflow = (bmap.val)(add(@unsafe.Pointer(ovf), uintptr(t.bucketsize)));
        }
        else
 { 
            // This is the last preallocated overflow bucket.
            // Reset the overflow pointer on this bucket,
            // which was set to a non-nil sentinel value.
            ovf.setoverflow(t, null);
            h.extra.nextOverflow = null;
        }
    }
    else
 {
        ovf = (bmap.val)(newobject(t.bucket));
    }
    h.incrnoverflow();
    if (t.bucket.ptrdata == 0) {
        h.createOverflow();
        h.extra.overflow.val = append(h.extra.overflow.val, ovf);
    }
    b.setoverflow(t, ovf);
    return _addr_ovf!;
}

private static void createOverflow(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    if (h.extra == null) {
        h.extra = @new<mapextra>();
    }
    if (h.extra.overflow == null) {
        h.extra.overflow = @new<*bmap>();
    }
}

private static ptr<hmap> makemap64(ptr<maptype> _addr_t, long hint, ptr<hmap> _addr_h) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (int64(int(hint)) != hint) {
        hint = 0;
    }
    return _addr_makemap(_addr_t, int(hint), _addr_h)!;
}

// makemap_small implements Go map creation for make(map[k]v) and
// make(map[k]v, hint) when hint is known to be at most bucketCnt
// at compile time and the map needs to be allocated on the heap.
private static ptr<hmap> makemap_small() {
    ptr<hmap> h = @new<hmap>();
    h.hash0 = fastrand();
    return _addr_h!;
}

// makemap implements Go map creation for make(map[k]v, hint).
// If the compiler has determined that the map or the first bucket
// can be created on the stack, h and/or bucket may be non-nil.
// If h != nil, the map can be created directly in h.
// If h.buckets != nil, bucket pointed to can be used as the first bucket.
private static ptr<hmap> makemap(ptr<maptype> _addr_t, nint hint, ptr<hmap> _addr_h) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var (mem, overflow) = math.MulUintptr(uintptr(hint), t.bucket.size);
    if (overflow || mem > maxAlloc) {
        hint = 0;
    }
    if (h == null) {
        h = @new<hmap>();
    }
    h.hash0 = fastrand(); 

    // Find the size parameter B which will hold the requested # of elements.
    // For hint < 0 overLoadFactor returns false since hint < bucketCnt.
    var B = uint8(0);
    while (overLoadFactor(hint, B)) {
        B++;
    }
    h.B = B; 

    // allocate initial hash table
    // if B == 0, the buckets field is allocated lazily later (in mapassign)
    // If hint is large zeroing this memory could take a while.
    if (h.B != 0) {
        ptr<bmap> nextOverflow;
        h.buckets, nextOverflow = makeBucketArray(_addr_t, h.B, null);
        if (nextOverflow != null) {
            h.extra = @new<mapextra>();
            h.extra.nextOverflow = nextOverflow;
        }
    }
    return _addr_h!;
}

// makeBucketArray initializes a backing array for map buckets.
// 1<<b is the minimum number of buckets to allocate.
// dirtyalloc should either be nil or a bucket array previously
// allocated by makeBucketArray with the same t and b parameters.
// If dirtyalloc is nil a new backing array will be alloced and
// otherwise dirtyalloc will be cleared and reused as backing array.
private static (unsafe.Pointer, ptr<bmap>) makeBucketArray(ptr<maptype> _addr_t, byte b, unsafe.Pointer dirtyalloc) {
    unsafe.Pointer buckets = default;
    ptr<bmap> nextOverflow = default!;
    ref maptype t = ref _addr_t.val;

    var @base = bucketShift(b);
    var nbuckets = base; 
    // For small b, overflow buckets are unlikely.
    // Avoid the overhead of the calculation.
    if (b >= 4) { 
        // Add on the estimated number of overflow buckets
        // required to insert the median number of elements
        // used with this value of b.
        nbuckets += bucketShift(b - 4);
        var sz = t.bucket.size * nbuckets;
        var up = roundupsize(sz);
        if (up != sz) {
            nbuckets = up / t.bucket.size;
        }
    }
    if (dirtyalloc == null) {
        buckets = newarray(t.bucket, int(nbuckets));
    }
    else
 { 
        // dirtyalloc was previously generated by
        // the above newarray(t.bucket, int(nbuckets))
        // but may not be empty.
        buckets = dirtyalloc;
        var size = t.bucket.size * nbuckets;
        if (t.bucket.ptrdata != 0) {
            memclrHasPointers(buckets, size);
        }
        else
 {
            memclrNoHeapPointers(buckets, size);
        }
    }
    if (base != nbuckets) { 
        // We preallocated some overflow buckets.
        // To keep the overhead of tracking these overflow buckets to a minimum,
        // we use the convention that if a preallocated overflow bucket's overflow
        // pointer is nil, then there are more available by bumping the pointer.
        // We need a safe non-nil pointer for the last overflow bucket; just use buckets.
        nextOverflow = (bmap.val)(add(buckets, base * uintptr(t.bucketsize)));
        var last = (bmap.val)(add(buckets, (nbuckets - 1) * uintptr(t.bucketsize)));
        last.setoverflow(t, (bmap.val)(buckets));
    }
    return (buckets, _addr_nextOverflow!);
}

// mapaccess1 returns a pointer to h[key].  Never returns nil, instead
// it will return a reference to the zero object for the elem type if
// the key is not in the map.
// NOTE: The returned pointer may keep the whole map live, so don't
// hold onto it for very long.
private static unsafe.Pointer mapaccess1(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        var pc = funcPC(mapaccess1);
        racereadpc(@unsafe.Pointer(h), callerpc, pc);
        raceReadObjectPC(t.key, key, callerpc, pc);
    }
    if (msanenabled && h != null) {
        msanread(key, t.key.size);
    }
    if (h == null || h.count == 0) {
        if (t.hashMightPanic()) {
            t.hasher(key, 0); // see issue 23734
        }
        return @unsafe.Pointer(_addr_zeroVal[0]);
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map read and map write");
    }
    var hash = t.hasher(key, uintptr(h.hash0));
    var m = bucketMask(h.B);
    var b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
    {
        var c = h.oldbuckets;

        if (c != null) {
            if (!h.sameSizeGrow()) { 
                // There used to be half as many buckets; mask down one more power of two.
                m>>=1;
            }
            var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
            if (!evacuated(_addr_oldb)) {
                b = oldb;
            }
        }
    }
    var top = tophash(hash);
bucketloop:
    while (b != null) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (b.tophash[i] != top) {
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }
                continue;
        b = b.overflow(t);
            }
            var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
            if (t.indirectkey()) {
                k = ((@unsafe.Pointer.val)(k)).val;
            }
            if (t.key.equal(key, k)) {
                var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
                if (t.indirectelem()) {
                    e = ((@unsafe.Pointer.val)(e)).val;
                }
                return e;
            }
        }
    }
    return @unsafe.Pointer(_addr_zeroVal[0]);
}

private static (unsafe.Pointer, bool) mapaccess2(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    unsafe.Pointer _p0 = default;
    bool _p0 = default;
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        var pc = funcPC(mapaccess2);
        racereadpc(@unsafe.Pointer(h), callerpc, pc);
        raceReadObjectPC(t.key, key, callerpc, pc);
    }
    if (msanenabled && h != null) {
        msanread(key, t.key.size);
    }
    if (h == null || h.count == 0) {
        if (t.hashMightPanic()) {
            t.hasher(key, 0); // see issue 23734
        }
        return (@unsafe.Pointer(_addr_zeroVal[0]), false);
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map read and map write");
    }
    var hash = t.hasher(key, uintptr(h.hash0));
    var m = bucketMask(h.B);
    var b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
    {
        var c = h.oldbuckets;

        if (c != null) {
            if (!h.sameSizeGrow()) { 
                // There used to be half as many buckets; mask down one more power of two.
                m>>=1;
            }
            var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
            if (!evacuated(_addr_oldb)) {
                b = oldb;
            }
        }
    }
    var top = tophash(hash);
bucketloop:
    while (b != null) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (b.tophash[i] != top) {
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }
                continue;
        b = b.overflow(t);
            }
            var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
            if (t.indirectkey()) {
                k = ((@unsafe.Pointer.val)(k)).val;
            }
            if (t.key.equal(key, k)) {
                var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
                if (t.indirectelem()) {
                    e = ((@unsafe.Pointer.val)(e)).val;
                }
                return (e, true);
            }
        }
    }
    return (@unsafe.Pointer(_addr_zeroVal[0]), false);
}

// returns both key and elem. Used by map iterator
private static (unsafe.Pointer, unsafe.Pointer) mapaccessK(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    unsafe.Pointer _p0 = default;
    unsafe.Pointer _p0 = default;
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (h == null || h.count == 0) {
        return (null, null);
    }
    var hash = t.hasher(key, uintptr(h.hash0));
    var m = bucketMask(h.B);
    var b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
    {
        var c = h.oldbuckets;

        if (c != null) {
            if (!h.sameSizeGrow()) { 
                // There used to be half as many buckets; mask down one more power of two.
                m>>=1;
            }
            var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
            if (!evacuated(_addr_oldb)) {
                b = oldb;
            }
        }
    }
    var top = tophash(hash);
bucketloop:
    while (b != null) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (b.tophash[i] != top) {
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }
                continue;
        b = b.overflow(t);
            }
            var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
            if (t.indirectkey()) {
                k = ((@unsafe.Pointer.val)(k)).val;
            }
            if (t.key.equal(key, k)) {
                var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
                if (t.indirectelem()) {
                    e = ((@unsafe.Pointer.val)(e)).val;
                }
                return (k, e);
            }
        }
    }
    return (null, null);
}

private static unsafe.Pointer mapaccess1_fat(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key, unsafe.Pointer zero) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var e = mapaccess1(_addr_t, _addr_h, key);
    if (e == @unsafe.Pointer(_addr_zeroVal[0])) {
        return zero;
    }
    return e;
}

private static (unsafe.Pointer, bool) mapaccess2_fat(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key, unsafe.Pointer zero) {
    unsafe.Pointer _p0 = default;
    bool _p0 = default;
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var e = mapaccess1(_addr_t, _addr_h, key);
    if (e == @unsafe.Pointer(_addr_zeroVal[0])) {
        return (zero, false);
    }
    return (e, true);
}

// Like mapaccess, but allocates a slot for the key if it is not present in the map.
private static unsafe.Pointer mapassign(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) => func((_, panic, _) => {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (h == null) {
        panic(plainError("assignment to entry in nil map"));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = funcPC(mapassign);
        racewritepc(@unsafe.Pointer(h), callerpc, pc);
        raceReadObjectPC(t.key, key, callerpc, pc);
    }
    if (msanenabled) {
        msanread(key, t.key.size);
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    var hash = t.hasher(key, uintptr(h.hash0)); 

    // Set hashWriting after calling t.hasher, since t.hasher may panic,
    // in which case we have not actually done a write.
    h.flags ^= hashWriting;

    if (h.buckets == null) {
        h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
    }
again:
    var bucket = hash & bucketMask(h.B);
    if (h.growing()) {
        growWork(_addr_t, _addr_h, bucket);
    }
    var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));
    var top = tophash(hash);

    ptr<byte> inserti;
    unsafe.Pointer insertk = default;
    unsafe.Pointer elem = default;
bucketloop: 

    // Did not find mapping for key. Allocate new cell & add entry.

    // If we hit the max load factor or we have too many overflow buckets,
    // and we're not already in the middle of growing, start growing.
    while (true) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (b.tophash[i] != top) {
                if (isEmpty(b.tophash[i]) && inserti == null) {
                    inserti = _addr_b.tophash[i];
                    insertk = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    elem = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
                }
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }
                continue;
            }
            var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
            if (t.indirectkey()) {
                k = ((@unsafe.Pointer.val)(k)).val;
            }
            if (!t.key.equal(key, k)) {
                continue;
            } 
            // already have a mapping for key. Update it.
            if (t.needkeyupdate()) {
                typedmemmove(t.key, k, key);
            }
            elem = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
            goto done;
        }
        var ovf = b.overflow(t);
        if (ovf == null) {
            break;
        }
        b = ovf;
    } 

    // Did not find mapping for key. Allocate new cell & add entry.

    // If we hit the max load factor or we have too many overflow buckets,
    // and we're not already in the middle of growing, start growing.
    if (!h.growing() && (overLoadFactor(h.count + 1, h.B) || tooManyOverflowBuckets(h.noverflow, h.B))) {
        hashGrow(_addr_t, _addr_h);
        goto again; // Growing the table invalidates everything, so try again
    }
    if (inserti == null) { 
        // The current bucket and all the overflow buckets connected to it are full, allocate a new one.
        var newb = h.newoverflow(t, b);
        inserti = _addr_newb.tophash[0];
        insertk = add(@unsafe.Pointer(newb), dataOffset);
        elem = add(insertk, bucketCnt * uintptr(t.keysize));
    }
    if (t.indirectkey()) {
        var kmem = newobject(t.key) * (@unsafe.Pointer.val);

        (insertk) = kmem;
        insertk = kmem;
    }
    if (t.indirectelem()) {
        var vmem = newobject(t.elem) * (@unsafe.Pointer.val);

        (elem) = vmem;
    }
    typedmemmove(t.key, insertk, key);
    inserti.val = top;
    h.count++;

done:
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;
    if (t.indirectelem()) {
        elem = ((@unsafe.Pointer.val)(elem)).val;
    }
    return elem;
});

private static void mapdelete(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        var pc = funcPC(mapdelete);
        racewritepc(@unsafe.Pointer(h), callerpc, pc);
        raceReadObjectPC(t.key, key, callerpc, pc);
    }
    if (msanenabled && h != null) {
        msanread(key, t.key.size);
    }
    if (h == null || h.count == 0) {
        if (t.hashMightPanic()) {
            t.hasher(key, 0); // see issue 23734
        }
        return ;
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    var hash = t.hasher(key, uintptr(h.hash0)); 

    // Set hashWriting after calling t.hasher, since t.hasher may panic,
    // in which case we have not actually done a write (delete).
    h.flags ^= hashWriting;

    var bucket = hash & bucketMask(h.B);
    if (h.growing()) {
        growWork(_addr_t, _addr_h, bucket);
    }
    var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));
    var bOrig = b;
    var top = tophash(hash);
search:

    while (b != null) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (b.tophash[i] != top) {
                if (b.tophash[i] == emptyRest) {
                    _breaksearch = true;
                    break;
                }
                continue;
        b = b.overflow(t);
            }
            var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
            var k2 = k;
            if (t.indirectkey()) {
                k2 = ((@unsafe.Pointer.val)(k2)).val;
            }
            if (!t.key.equal(key, k2)) {
                continue;
            } 
            // Only clear key if there are pointers in it.
            if (t.indirectkey()) {
                (@unsafe.Pointer.val).val;

                (k) = null;
            }
            else if (t.key.ptrdata != 0) {
                memclrHasPointers(k, t.key.size);
            }
            var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.elemsize));
            if (t.indirectelem()) {
                (@unsafe.Pointer.val).val;

                (e) = null;
            }
            else if (t.elem.ptrdata != 0) {
                memclrHasPointers(e, t.elem.size);
            }
            else
 {
                memclrNoHeapPointers(e, t.elem.size);
            }
            b.tophash[i] = emptyOne; 
            // If the bucket now ends in a bunch of emptyOne states,
            // change those to emptyRest states.
            // It would be nice to make this a separate function, but
            // for loops are not currently inlineable.
            if (i == bucketCnt - 1) {
                if (b.overflow(t) != null && b.overflow(t).tophash[0] != emptyRest) {
                    goto notLast;
                }
            }
            else
 {
                if (b.tophash[i + 1] != emptyRest) {
                    goto notLast;
                }
            }
            while (true) {
                b.tophash[i] = emptyRest;
                if (i == 0) {
                    if (b == bOrig) {
                        break; // beginning of initial bucket, we're done.
                    } 
                    // Find previous bucket, continue at its last entry.
                    var c = b;
                    b = bOrig;

                    while (b.overflow(t) != c) {
                        b = b.overflow(t);
                    }
                else

                    i = bucketCnt - 1;
                } {
                    i--;
                }
                if (b.tophash[i] != emptyOne) {
                    break;
                }
            }

notLast: 
            // Reset the hash seed to make it more difficult for attackers to
            // repeatedly trigger hash collisions. See issue 25237.
            h.count--; 
            // Reset the hash seed to make it more difficult for attackers to
            // repeatedly trigger hash collisions. See issue 25237.
            if (h.count == 0) {
                h.hash0 = fastrand();
            }
            _breaksearch = true;
            break;
        }
    }
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;
}

// mapiterinit initializes the hiter struct used for ranging over maps.
// The hiter struct pointed to by 'it' is allocated on the stack
// by the compilers order pass or on the heap by reflect_mapiterinit.
// Both need to have zeroed hiter since the struct contains pointers.
private static void mapiterinit(ptr<maptype> _addr_t, ptr<hmap> _addr_h, ptr<hiter> _addr_it) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;
    ref hiter it = ref _addr_it.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapiterinit));
    }
    if (h == null || h.count == 0) {
        return ;
    }
    if (@unsafe.Sizeof(new hiter()) / sys.PtrSize != 12) {
        throw("hash_iter size incorrect"); // see cmd/compile/internal/reflectdata/reflect.go
    }
    it.t = t;
    it.h = h; 

    // grab snapshot of bucket state
    it.B = h.B;
    it.buckets = h.buckets;
    if (t.bucket.ptrdata == 0) { 
        // Allocate the current slice and remember pointers to both current and old.
        // This preserves all relevant overflow buckets alive even if
        // the table grows and/or overflow buckets are added to the table
        // while we are iterating.
        h.createOverflow();
        it.overflow = h.extra.overflow;
        it.oldoverflow = h.extra.oldoverflow;
    }
    var r = uintptr(fastrand());
    if (h.B > 31 - bucketCntBits) {
        r += uintptr(fastrand()) << 31;
    }
    it.startBucket = r & bucketMask(h.B);
    it.offset = uint8(r >> (int)(h.B) & (bucketCnt - 1)); 

    // iterator state
    it.bucket = it.startBucket; 

    // Remember we have an iterator.
    // Can run concurrently with another mapiterinit().
    {
        var old = h.flags;

        if (old & (iterator | oldIterator) != iterator | oldIterator) {
            atomic.Or8(_addr_h.flags, iterator | oldIterator);
        }
    }

    mapiternext(_addr_it);
}

private static void mapiternext(ptr<hiter> _addr_it) {
    ref hiter it = ref _addr_it.val;

    var h = it.h;
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapiternext));
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map iteration and map write");
    }
    var t = it.t;
    var bucket = it.bucket;
    var b = it.bptr;
    var i = it.i;
    var checkBucket = it.checkBucket;

next:
    if (b == null) {
        if (bucket == it.startBucket && it.wrapped) { 
            // end of iteration
            it.key = null;
            it.elem = null;
            return ;
        }
        if (h.growing() && it.B == h.B) { 
            // Iterator was started in the middle of a grow, and the grow isn't done yet.
            // If the bucket we're looking at hasn't been filled in yet (i.e. the old
            // bucket hasn't been evacuated) then we need to iterate through the old
            // bucket and only return the ones that will be migrated to this bucket.
            var oldbucket = bucket & it.h.oldbucketmask();
            b = (bmap.val)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
            if (!evacuated(_addr_b)) {
                checkBucket = bucket;
            }
            else
 {
                b = (bmap.val)(add(it.buckets, bucket * uintptr(t.bucketsize)));
                checkBucket = noCheck;
            }
        }
        else
 {
            b = (bmap.val)(add(it.buckets, bucket * uintptr(t.bucketsize)));
            checkBucket = noCheck;
        }
        bucket++;
        if (bucket == bucketShift(it.B)) {
            bucket = 0;
            it.wrapped = true;
        }
        i = 0;
    }
    while (i < bucketCnt) {
        var offi = (i + it.offset) & (bucketCnt - 1);
        if (isEmpty(b.tophash[offi]) || b.tophash[offi] == evacuatedEmpty) { 
            // TODO: emptyRest is hard to use here, as we start iterating
            // in the middle of a bucket. It's feasible, just tricky.
            continue;
        i++;
        }
        var k = add(@unsafe.Pointer(b), dataOffset + uintptr(offi) * uintptr(t.keysize));
        if (t.indirectkey()) {
            k = ((@unsafe.Pointer.val)(k)).val;
        }
        var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + uintptr(offi) * uintptr(t.elemsize));
        if (checkBucket != noCheck && !h.sameSizeGrow()) { 
            // Special case: iterator was started during a grow to a larger size
            // and the grow is not done yet. We're working on a bucket whose
            // oldbucket has not been evacuated yet. Or at least, it wasn't
            // evacuated when we started the bucket. So we're iterating
            // through the oldbucket, skipping any keys that will go
            // to the other new bucket (each oldbucket expands to two
            // buckets during a grow).
            if (t.reflexivekey() || t.key.equal(k, k)) { 
                // If the item in the oldbucket is not destined for
                // the current new bucket in the iteration, skip it.
                var hash = t.hasher(k, uintptr(h.hash0));
                if (hash & bucketMask(it.B) != checkBucket) {
                    continue;
                }
            }
            else
 { 
                // Hash isn't repeatable if k != k (NaNs).  We need a
                // repeatable and randomish choice of which direction
                // to send NaNs during evacuation. We'll use the low
                // bit of tophash to decide which way NaNs go.
                // NOTE: this case is why we need two evacuate tophash
                // values, evacuatedX and evacuatedY, that differ in
                // their low bit.
                if (checkBucket >> (int)((it.B - 1)) != uintptr(b.tophash[offi] & 1)) {
                    continue;
                }
            }
        }
        if ((b.tophash[offi] != evacuatedX && b.tophash[offi] != evacuatedY) || !(t.reflexivekey() || t.key.equal(k, k))) { 
            // This is the golden data, we can return it.
            // OR
            // key!=key, so the entry can't be deleted or updated, so we can just return it.
            // That's lucky for us because when key!=key we can't look it up successfully.
            it.key = k;
            if (t.indirectelem()) {
                e = ((@unsafe.Pointer.val)(e)).val;
            }
            it.elem = e;
        }
        else
 { 
            // The hash table has grown since the iterator was started.
            // The golden data for this key is now somewhere else.
            // Check the current hash table for the data.
            // This code handles the case where the key
            // has been deleted, updated, or deleted and reinserted.
            // NOTE: we need to regrab the key as it has potentially been
            // updated to an equal() but not identical key (e.g. +0.0 vs -0.0).
            var (rk, re) = mapaccessK(_addr_t, _addr_h, k);
            if (rk == null) {
                continue; // key has been deleted
            }
            it.key = rk;
            it.elem = re;
        }
        it.bucket = bucket;
        if (it.bptr != b) { // avoid unnecessary write barrier; see issue 14921
            it.bptr = b;
        }
        it.i = i + 1;
        it.checkBucket = checkBucket;
        return ;
    }
    b = b.overflow(t);
    i = 0;
    goto next;
}

// mapclear deletes all keys from a map.
private static void mapclear(ptr<maptype> _addr_t, ptr<hmap> _addr_h) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        var pc = funcPC(mapclear);
        racewritepc(@unsafe.Pointer(h), callerpc, pc);
    }
    if (h == null || h.count == 0) {
        return ;
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    h.flags ^= hashWriting;

    h.flags &= sameSizeGrow;
    h.oldbuckets = null;
    h.nevacuate = 0;
    h.noverflow = 0;
    h.count = 0; 

    // Reset the hash seed to make it more difficult for attackers to
    // repeatedly trigger hash collisions. See issue 25237.
    h.hash0 = fastrand(); 

    // Keep the mapextra allocation but clear any extra information.
    if (h.extra != null) {
        h.extra.val = new mapextra();
    }
    var (_, nextOverflow) = makeBucketArray(_addr_t, h.B, h.buckets);
    if (nextOverflow != null) { 
        // If overflow buckets are created then h.extra
        // will have been allocated during initial bucket creation.
        h.extra.nextOverflow = nextOverflow;
    }
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;
}

private static void hashGrow(ptr<maptype> _addr_t, ptr<hmap> _addr_h) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;
 
    // If we've hit the load factor, get bigger.
    // Otherwise, there are too many overflow buckets,
    // so keep the same number of buckets and "grow" laterally.
    var bigger = uint8(1);
    if (!overLoadFactor(h.count + 1, h.B)) {
        bigger = 0;
        h.flags |= sameSizeGrow;
    }
    var oldbuckets = h.buckets;
    var (newbuckets, nextOverflow) = makeBucketArray(_addr_t, h.B + bigger, null);

    var flags = h.flags & ~(iterator | oldIterator);
    if (h.flags & iterator != 0) {
        flags |= oldIterator;
    }
    h.B += bigger;
    h.flags = flags;
    h.oldbuckets = oldbuckets;
    h.buckets = newbuckets;
    h.nevacuate = 0;
    h.noverflow = 0;

    if (h.extra != null && h.extra.overflow != null) { 
        // Promote current overflow buckets to the old generation.
        if (h.extra.oldoverflow != null) {
            throw("oldoverflow is not nil");
        }
        h.extra.oldoverflow = h.extra.overflow;
        h.extra.overflow = null;
    }
    if (nextOverflow != null) {
        if (h.extra == null) {
            h.extra = @new<mapextra>();
        }
        h.extra.nextOverflow = nextOverflow;
    }
}

// overLoadFactor reports whether count items placed in 1<<B buckets is over loadFactor.
private static bool overLoadFactor(nint count, byte B) {
    return count > bucketCnt && uintptr(count) > loadFactorNum * (bucketShift(B) / loadFactorDen);
}

// tooManyOverflowBuckets reports whether noverflow buckets is too many for a map with 1<<B buckets.
// Note that most of these overflow buckets must be in sparse use;
// if use was dense, then we'd have already triggered regular map growth.
private static bool tooManyOverflowBuckets(ushort noverflow, byte B) { 
    // If the threshold is too low, we do extraneous work.
    // If the threshold is too high, maps that grow and shrink can hold on to lots of unused memory.
    // "too many" means (approximately) as many overflow buckets as regular buckets.
    // See incrnoverflow for more details.
    if (B > 15) {
        B = 15;
    }
    return noverflow >= uint16(1) << (int)((B & 15));
}

// growing reports whether h is growing. The growth may be to the same size or bigger.
private static bool growing(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    return h.oldbuckets != null;
}

// sameSizeGrow reports whether the current growth is to a map of the same size.
private static bool sameSizeGrow(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    return h.flags & sameSizeGrow != 0;
}

// noldbuckets calculates the number of buckets prior to the current map growth.
private static System.UIntPtr noldbuckets(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    var oldB = h.B;
    if (!h.sameSizeGrow()) {
        oldB--;
    }
    return bucketShift(oldB);
}

// oldbucketmask provides a mask that can be applied to calculate n % noldbuckets().
private static System.UIntPtr oldbucketmask(this ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    return h.noldbuckets() - 1;
}

private static void growWork(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr bucket) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;
 
    // make sure we evacuate the oldbucket corresponding
    // to the bucket we're about to use
    evacuate(_addr_t, _addr_h, bucket & h.oldbucketmask()); 

    // evacuate one more oldbucket to make progress on growing
    if (h.growing()) {
        evacuate(_addr_t, _addr_h, h.nevacuate);
    }
}

private static bool bucketEvacuated(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr bucket) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var b = (bmap.val)(add(h.oldbuckets, bucket * uintptr(t.bucketsize)));
    return evacuated(_addr_b);
}

// evacDst is an evacuation destination.
private partial struct evacDst {
    public ptr<bmap> b; // current destination bucket
    public nint i; // key/elem index into b
    public unsafe.Pointer k; // pointer to current key storage
    public unsafe.Pointer e; // pointer to current elem storage
}

private static void evacuate(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr oldbucket) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var b = (bmap.val)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
    var newbit = h.noldbuckets();
    if (!evacuated(_addr_b)) { 
        // TODO: reuse overflow buckets instead of using new ones, if there
        // is no iterator using the old buckets.  (If !oldIterator.)

        // xy contains the x and y (low and high) evacuation destinations.
        array<evacDst> xy = new array<evacDst>(2);
        var x = _addr_xy[0];
        x.b = (bmap.val)(add(h.buckets, oldbucket * uintptr(t.bucketsize)));
        x.k = add(@unsafe.Pointer(x.b), dataOffset);
        x.e = add(x.k, bucketCnt * uintptr(t.keysize));

        if (!h.sameSizeGrow()) { 
            // Only calculate y pointers if we're growing bigger.
            // Otherwise GC can see bad pointers.
            var y = _addr_xy[1];
            y.b = (bmap.val)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
            y.k = add(@unsafe.Pointer(y.b), dataOffset);
            y.e = add(y.k, bucketCnt * uintptr(t.keysize));
        }
        while (b != null) {
            var k = add(@unsafe.Pointer(b), dataOffset);
            var e = add(k, bucketCnt * uintptr(t.keysize));
            {
                nint i = 0;

                while (i < bucketCnt) {
                    var top = b.tophash[i];
                    if (isEmpty(top)) {
                        b.tophash[i] = evacuatedEmpty;
                        continue;
                    (i, k, e) = (i + 1, add(k, uintptr(t.keysize)), add(e, uintptr(t.elemsize)));
                    }
                    if (top < minTopHash) {
                        throw("bad map state");
            b = b.overflow(t);
                    }
                    var k2 = k;
                    if (t.indirectkey()) {
                        k2 = ((@unsafe.Pointer.val)(k2)).val;
                    }
                    byte useY = default;
                    if (!h.sameSizeGrow()) { 
                        // Compute hash to make our evacuation decision (whether we need
                        // to send this key/elem to bucket x or bucket y).
                        var hash = t.hasher(k2, uintptr(h.hash0));
                        if (h.flags & iterator != 0 && !t.reflexivekey() && !t.key.equal(k2, k2)) { 
                            // If key != key (NaNs), then the hash could be (and probably
                            // will be) entirely different from the old hash. Moreover,
                            // it isn't reproducible. Reproducibility is required in the
                            // presence of iterators, as our evacuation decision must
                            // match whatever decision the iterator made.
                            // Fortunately, we have the freedom to send these keys either
                            // way. Also, tophash is meaningless for these kinds of keys.
                            // We let the low bit of tophash drive the evacuation decision.
                            // We recompute a new random tophash for the next level so
                            // these keys will get evenly distributed across all buckets
                            // after multiple grows.
                            useY = top & 1;
                            top = tophash(hash);
                        }
                        else
 {
                            if (hash & newbit != 0) {
                                useY = 1;
                            }
                        }
                    }
                    if (evacuatedX + 1 != evacuatedY || evacuatedX ^ 1 != evacuatedY) {
                        throw("bad evacuatedN");
                    }
                    b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY
                    var dst = _addr_xy[useY]; // evacuation destination

                    if (dst.i == bucketCnt) {
                        dst.b = h.newoverflow(t, dst.b);
                        dst.i = 0;
                        dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                        dst.e = add(dst.k, bucketCnt * uintptr(t.keysize));
                    }
                    dst.b.tophash[dst.i & (bucketCnt - 1)] = top; // mask dst.i as an optimization, to avoid a bounds check
                    if (t.indirectkey()) {
                        (@unsafe.Pointer.val).val;

                        (dst.k) = k2; // copy pointer
                    }
                    else
 {
                        typedmemmove(t.key, dst.k, k); // copy elem
                    }
                    if (t.indirectelem()) {
                        (@unsafe.Pointer.val).val;

                        (dst.e) = new ptr<ptr<ptr<unsafe.Pointer>>>(e);
                    }
                    else
 {
                        typedmemmove(t.elem, dst.e, e);
                    }
                    dst.i++; 
                    // These updates might push these pointers past the end of the
                    // key or elem arrays.  That's ok, as we have the overflow pointer
                    // at the end of the bucket to protect against pointing past the
                    // end of the bucket.
                    dst.k = add(dst.k, uintptr(t.keysize));
                    dst.e = add(dst.e, uintptr(t.elemsize));
                }

            }
        } 
        // Unlink the overflow buckets & clear key/elem to help GC.
        if (h.flags & oldIterator == 0 && t.bucket.ptrdata != 0) {
            b = add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)); 
            // Preserve b.tophash because the evacuation
            // state is maintained there.
            var ptr = add(b, dataOffset);
            var n = uintptr(t.bucketsize) - dataOffset;
            memclrHasPointers(ptr, n);
        }
    }
    if (oldbucket == h.nevacuate) {
        advanceEvacuationMark(_addr_h, _addr_t, newbit);
    }
}

private static void advanceEvacuationMark(ptr<hmap> _addr_h, ptr<maptype> _addr_t, System.UIntPtr newbit) {
    ref hmap h = ref _addr_h.val;
    ref maptype t = ref _addr_t.val;

    h.nevacuate++; 
    // Experiments suggest that 1024 is overkill by at least an order of magnitude.
    // Put it in there as a safeguard anyway, to ensure O(1) behavior.
    var stop = h.nevacuate + 1024;
    if (stop > newbit) {
        stop = newbit;
    }
    while (h.nevacuate != stop && bucketEvacuated(_addr_t, _addr_h, h.nevacuate)) {
        h.nevacuate++;
    }
    if (h.nevacuate == newbit) { // newbit == # of oldbuckets
        // Growing is all done. Free old main bucket array.
        h.oldbuckets = null; 
        // Can discard old overflow buckets as well.
        // If they are still referenced by an iterator,
        // then the iterator holds a pointers to the slice.
        if (h.extra != null) {
            h.extra.oldoverflow = null;
        }
        h.flags &= sameSizeGrow;
    }
}

// Reflect stubs. Called from ../reflect/asm_*.s

//go:linkname reflect_makemap reflect.makemap
private static ptr<hmap> reflect_makemap(ptr<maptype> _addr_t, nint cap) {
    ref maptype t = ref _addr_t.val;
 
    // Check invariants and reflects math.
    if (t.key.equal == null) {
        throw("runtime.reflect_makemap: unsupported map key type");
    }
    if (t.key.size > maxKeySize && (!t.indirectkey() || t.keysize != uint8(sys.PtrSize)) || t.key.size <= maxKeySize && (t.indirectkey() || t.keysize != uint8(t.key.size))) {
        throw("key size wrong");
    }
    if (t.elem.size > maxElemSize && (!t.indirectelem() || t.elemsize != uint8(sys.PtrSize)) || t.elem.size <= maxElemSize && (t.indirectelem() || t.elemsize != uint8(t.elem.size))) {
        throw("elem size wrong");
    }
    if (t.key.align > bucketCnt) {
        throw("key align too big");
    }
    if (t.elem.align > bucketCnt) {
        throw("elem align too big");
    }
    if (t.key.size % uintptr(t.key.align) != 0) {
        throw("key size not a multiple of key align");
    }
    if (t.elem.size % uintptr(t.elem.align) != 0) {
        throw("elem size not a multiple of elem align");
    }
    if (bucketCnt < 8) {
        throw("bucketsize too small for proper alignment");
    }
    if (dataOffset % uintptr(t.key.align) != 0) {
        throw("need padding in bucket (key)");
    }
    if (dataOffset % uintptr(t.elem.align) != 0) {
        throw("need padding in bucket (elem)");
    }
    return _addr_makemap(_addr_t, cap, _addr_null)!;
}

//go:linkname reflect_mapaccess reflect.mapaccess
private static unsafe.Pointer reflect_mapaccess(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var (elem, ok) = mapaccess2(_addr_t, _addr_h, key);
    if (!ok) { 
        // reflect wants nil for a missing element
        elem = null;
    }
    return elem;
}

//go:linkname reflect_mapassign reflect.mapassign
private static void reflect_mapassign(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key, unsafe.Pointer elem) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var p = mapassign(_addr_t, _addr_h, key);
    typedmemmove(t.elem, p, elem);
}

//go:linkname reflect_mapdelete reflect.mapdelete
private static void reflect_mapdelete(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    mapdelete(_addr_t, _addr_h, key);
}

//go:linkname reflect_mapiterinit reflect.mapiterinit
private static ptr<hiter> reflect_mapiterinit(ptr<maptype> _addr_t, ptr<hmap> _addr_h) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    ptr<hiter> it = @new<hiter>();
    mapiterinit(_addr_t, _addr_h, it);
    return _addr_it!;
}

//go:linkname reflect_mapiternext reflect.mapiternext
private static void reflect_mapiternext(ptr<hiter> _addr_it) {
    ref hiter it = ref _addr_it.val;

    mapiternext(_addr_it);
}

//go:linkname reflect_mapiterkey reflect.mapiterkey
private static unsafe.Pointer reflect_mapiterkey(ptr<hiter> _addr_it) {
    ref hiter it = ref _addr_it.val;

    return it.key;
}

//go:linkname reflect_mapiterelem reflect.mapiterelem
private static unsafe.Pointer reflect_mapiterelem(ptr<hiter> _addr_it) {
    ref hiter it = ref _addr_it.val;

    return it.elem;
}

//go:linkname reflect_maplen reflect.maplen
private static nint reflect_maplen(ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    if (h == null) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(reflect_maplen));
    }
    return h.count;
}

//go:linkname reflectlite_maplen internal/reflectlite.maplen
private static nint reflectlite_maplen(ptr<hmap> _addr_h) {
    ref hmap h = ref _addr_h.val;

    if (h == null) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(reflect_maplen));
    }
    return h.count;
}

private static readonly nint maxZero = 1024; // must match value in reflect/value.go:maxZero cmd/compile/internal/gc/walk.go:zeroValSize
 // must match value in reflect/value.go:maxZero cmd/compile/internal/gc/walk.go:zeroValSize
private static array<byte> zeroVal = new array<byte>(maxZero);

} // end runtime_package
