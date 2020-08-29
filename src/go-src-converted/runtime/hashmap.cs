// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\hashmap.go
// This file contains the implementation of Go's map type.
//
// A map is just a hash table. The data is arranged
// into an array of buckets. Each bucket contains up to
// 8 key/value pairs. The low-order bits of the hash are
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
// (64-bit, 8 byte keys and values)
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
// bytes/entry = overhead bytes used per key/value pair
// hitprobe    = # of entries to check when looking up a present key
// missprobe   = # of entries to check when looking up an absent key
//
// Keep in mind this data is for maximally loaded tables, i.e. just
// before the table grows. Typical tables will be somewhat less loaded.

using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
 
        // Maximum number of key/value pairs a bucket can hold.
        private static readonly long bucketCntBits = 3L;
        private static readonly long bucketCnt = 1L << (int)(bucketCntBits); 

        // Maximum average load of a bucket that triggers growth is 6.5.
        // Represent as loadFactorNum/loadFactDen, to allow integer math.
        private static readonly long loadFactorNum = 13L;
        private static readonly long loadFactorDen = 2L; 

        // Maximum key or value size to keep inline (instead of mallocing per element).
        // Must fit in a uint8.
        // Fast versions cannot handle big values - the cutoff size for
        // fast versions in ../../cmd/internal/gc/walk.go must be at most this value.
        private static readonly long maxKeySize = 128L;
        private static readonly long maxValueSize = 128L; 

        // data offset should be the size of the bmap struct, but needs to be
        // aligned correctly. For amd64p32 this means 64-bit alignment
        // even though pointers are 32 bit.
        private static readonly var dataOffset = @unsafe.Offsetof(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{bbmapvint64}{}.v); 

        // Possible tophash values. We reserve a few possibilities for special marks.
        // Each bucket (including its overflow buckets, if any) will have either all or none of its
        // entries in the evacuated* states (except during the evacuate() method, which only happens
        // during map writes and thus no one else can observe the map during that time).
        private static readonly long empty = 0L; // cell is empty
        private static readonly long evacuatedEmpty = 1L; // cell is empty, bucket is evacuated.
        private static readonly long evacuatedX = 2L; // key/value is valid.  Entry has been evacuated to first half of larger table.
        private static readonly long evacuatedY = 3L; // same as above, but evacuated to second half of larger table.
        private static readonly long minTopHash = 4L; // minimum tophash for a normal filled cell.

        // flags
        private static readonly long iterator = 1L; // there may be an iterator using buckets
        private static readonly long oldIterator = 2L; // there may be an iterator using oldbuckets
        private static readonly long hashWriting = 4L; // a goroutine is writing to the map
        private static readonly long sameSizeGrow = 8L; // the current map growth is to a new map of the same size

        // sentinel bucket ID for iterator checks
        private static readonly long noCheck = 1L << (int)((8L * sys.PtrSize)) - 1L;

        // A header for a Go map.
        private partial struct hmap
        {
            public long count; // # live cells == size of map.  Must be first (used by len() builtin)
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
        private partial struct mapextra
        {
            public ref slice<ref bmap> overflow;
            public ref slice<ref bmap> oldoverflow; // nextOverflow holds a pointer to a free overflow bucket.
            public ptr<bmap> nextOverflow;
        }

        // A bucket for a Go map.
        private partial struct bmap
        {
            public array<byte> tophash; // Followed by bucketCnt keys and then bucketCnt values.
// NOTE: packing all the keys together and then all the values together makes the
// code a bit more complicated than alternating key/value/key/value/... but it allows
// us to eliminate padding which would be needed for, e.g., map[int64]int8.
// Followed by an overflow pointer.
        }

        // A hash iteration structure.
        // If you modify hiter, also change cmd/internal/gc/reflect.go to indicate
        // the layout of this structure.
        private partial struct hiter
        {
            public unsafe.Pointer key; // Must be in first position.  Write nil to indicate iteration end (see cmd/internal/gc/range.go).
            public unsafe.Pointer value; // Must be in second position (see cmd/internal/gc/range.go).
            public ptr<maptype> t;
            public ptr<hmap> h;
            public unsafe.Pointer buckets; // bucket ptr at hash_iter initialization time
            public ptr<bmap> bptr; // current bucket
            public ref slice<ref bmap> overflow; // keeps overflow buckets of hmap.buckets alive
            public ref slice<ref bmap> oldoverflow; // keeps overflow buckets of hmap.oldbuckets alive
            public System.UIntPtr startBucket; // bucket iteration started at
            public byte offset; // intra-bucket offset to start from during iteration (should be big enough to hold bucketCnt-1)
            public bool wrapped; // already wrapped around from end of bucket array to beginning
            public byte B;
            public byte i;
            public System.UIntPtr bucket;
            public System.UIntPtr checkBucket;
        }

        // bucketShift returns 1<<b, optimized for code generation.
        private static System.UIntPtr bucketShift(byte b)
        {
            if (sys.GoarchAmd64 | sys.GoarchAmd64p32 | sys.Goarch386 != 0L)
            {
                b &= sys.PtrSize * 8L - 1L; // help x86 archs remove shift overflow checks
            }
            return uintptr(1L) << (int)(b);
        }

        // bucketMask returns 1<<b - 1, optimized for code generation.
        private static System.UIntPtr bucketMask(byte b)
        {
            return bucketShift(b) - 1L;
        }

        // tophash calculates the tophash value for hash.
        private static byte tophash(System.UIntPtr hash)
        {
            var top = uint8(hash >> (int)((sys.PtrSize * 8L - 8L)));
            if (top < minTopHash)
            {
                top += minTopHash;
            }
            return top;
        }

        private static bool evacuated(ref bmap b)
        {
            var h = b.tophash[0L];
            return h > empty && h < minTopHash;
        }

        private static ref bmap overflow(this ref bmap b, ref maptype t)
        {
            return new ptr<*(ptr<ptr<bmap>>)>(add(@unsafe.Pointer(b), uintptr(t.bucketsize) - sys.PtrSize));
        }

        private static void setoverflow(this ref bmap b, ref maptype t, ref bmap ovf)
        {
            (bmap.Value.Value)(add(@unsafe.Pointer(b), uintptr(t.bucketsize) - sys.PtrSize)).Value;

            ovf;
        }

        private static unsafe.Pointer keys(this ref bmap b)
        {
            return add(@unsafe.Pointer(b), dataOffset);
        }

        // incrnoverflow increments h.noverflow.
        // noverflow counts the number of overflow buckets.
        // This is used to trigger same-size map growth.
        // See also tooManyOverflowBuckets.
        // To keep hmap small, noverflow is a uint16.
        // When there are few buckets, noverflow is an exact count.
        // When there are many buckets, noverflow is an approximate count.
        private static void incrnoverflow(this ref hmap h)
        { 
            // We trigger same-size map growth if there are
            // as many overflow buckets as buckets.
            // We need to be able to count to 1<<h.B.
            if (h.B < 16L)
            {
                h.noverflow++;
                return;
            } 
            // Increment with probability 1/(1<<(h.B-15)).
            // When we reach 1<<15 - 1, we will have approximately
            // as many overflow buckets as buckets.
            var mask = uint32(1L) << (int)((h.B - 15L)) - 1L; 
            // Example: if h.B == 18, then mask == 7,
            // and fastrand & 7 == 0 with probability 1/8.
            if (fastrand() & mask == 0L)
            {
                h.noverflow++;
            }
        }

        private static ref bmap newoverflow(this ref hmap h, ref maptype t, ref bmap b)
        {
            ref bmap ovf = default;
            if (h.extra != null && h.extra.nextOverflow != null)
            { 
                // We have preallocated overflow buckets available.
                // See makeBucketArray for more details.
                ovf = h.extra.nextOverflow;
                if (ovf.overflow(t) == null)
                { 
                    // We're not at the end of the preallocated overflow buckets. Bump the pointer.
                    h.extra.nextOverflow = (bmap.Value)(add(@unsafe.Pointer(ovf), uintptr(t.bucketsize)));
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
                ovf = (bmap.Value)(newobject(t.bucket));
            }
            h.incrnoverflow();
            if (t.bucket.kind & kindNoPointers != 0L)
            {
                h.createOverflow();
                h.extra.overflow.Value = append(h.extra.overflow.Value, ovf);
            }
            b.setoverflow(t, ovf);
            return ovf;
        }

        private static void createOverflow(this ref hmap h)
        {
            if (h.extra == null)
            {
                h.extra = @new<mapextra>();
            }
            if (h.extra.overflow == null)
            {
                h.extra.overflow = @new<slice<ref bmap>>();
            }
        }

        private static ref hmap makemap64(ref maptype t, long hint, ref hmap h)
        {
            if (int64(int(hint)) != hint)
            {
                hint = 0L;
            }
            return makemap(t, int(hint), h);
        }

        // makehmap_small implements Go map creation for make(map[k]v) and
        // make(map[k]v, hint) when hint is known to be at most bucketCnt
        // at compile time and the map needs to be allocated on the heap.
        private static ref hmap makemap_small()
        {
            ptr<hmap> h = @new<hmap>();
            h.hash0 = fastrand();
            return h;
        }

        // makemap implements Go map creation for make(map[k]v, hint).
        // If the compiler has determined that the map or the first bucket
        // can be created on the stack, h and/or bucket may be non-nil.
        // If h != nil, the map can be created directly in h.
        // If h.buckets != nil, bucket pointed to can be used as the first bucket.
        private static ref hmap makemap(ref maptype t, long hint, ref hmap h)
        { 
            // The size of hmap should be 48 bytes on 64 bit
            // and 28 bytes on 32 bit platforms.
            {
                var sz = @unsafe.Sizeof(new hmap());

                if (sz != 8L + 5L * sys.PtrSize)
                {
                    println("runtime: sizeof(hmap) =", sz, ", t.hmap.size =", t.hmap.size);
                    throw("bad hmap size");
                }

            }

            if (hint < 0L || hint > int(maxSliceCap(t.bucket.size)))
            {
                hint = 0L;
            } 

            // initialize Hmap
            if (h == null)
            {
                h = (hmap.Value)(newobject(t.hmap));
            }
            h.hash0 = fastrand(); 

            // find size parameter which will hold the requested # of elements
            var B = uint8(0L);
            while (overLoadFactor(hint, B))
            {
                B++;
            }

            h.B = B; 

            // allocate initial hash table
            // if B == 0, the buckets field is allocated lazily later (in mapassign)
            // If hint is large zeroing this memory could take a while.
            if (h.B != 0L)
            {
                ref bmap nextOverflow = default;
                h.buckets, nextOverflow = makeBucketArray(t, h.B);
                if (nextOverflow != null)
                {
                    h.extra = @new<mapextra>();
                    h.extra.nextOverflow = nextOverflow;
                }
            }
            return h;
        }

        // mapaccess1 returns a pointer to h[key].  Never returns nil, instead
        // it will return a reference to the zero object for the value type if
        // the key is not in the map.
        // NOTE: The returned pointer may keep the whole map live, so don't
        // hold onto it for very long.
        private static unsafe.Pointer mapaccess1(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(mapaccess1);
                racereadpc(@unsafe.Pointer(h), callerpc, pc);
                raceReadObjectPC(t.key, key, callerpc, pc);
            }
            if (msanenabled && h != null)
            {
                msanread(key, t.key.size);
            }
            if (h == null || h.count == 0L)
            {
                return @unsafe.Pointer(ref zeroVal[0L]);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            var alg = t.key.alg;
            var hash = alg.hash(key, uintptr(h.hash0));
            var m = bucketMask(h.B);
            var b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
            {
                var c = h.oldbuckets;

                if (c != null)
                {
                    if (!h.sameSizeGrow())
                    { 
                        // There used to be half as many buckets; mask down one more power of two.
                        m >>= 1L;
                    }
                    var oldb = (bmap.Value)(add(c, (hash & m) * uintptr(t.bucketsize)));
                    if (!evacuated(oldb))
                    {
                        b = oldb;
                    }
                }

            }
            var top = tophash(hash);
            while (b != null)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        continue;
                    }
                    var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    if (t.indirectkey)
                    {
                        k = ((@unsafe.Pointer.Value)(k)).Value;
                b = b.overflow(t);
                    }
                    if (alg.equal(key, k))
                    {
                        var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                        if (t.indirectvalue)
                        {
                            v = ((@unsafe.Pointer.Value)(v)).Value;
                        }
                        return v;
                    }
                }

            }

            return @unsafe.Pointer(ref zeroVal[0L]);
        }

        private static (unsafe.Pointer, bool) mapaccess2(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(mapaccess2);
                racereadpc(@unsafe.Pointer(h), callerpc, pc);
                raceReadObjectPC(t.key, key, callerpc, pc);
            }
            if (msanenabled && h != null)
            {
                msanread(key, t.key.size);
            }
            if (h == null || h.count == 0L)
            {
                return (@unsafe.Pointer(ref zeroVal[0L]), false);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            var alg = t.key.alg;
            var hash = alg.hash(key, uintptr(h.hash0));
            var m = bucketMask(h.B);
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + (hash & m) * uintptr(t.bucketsize)));
            {
                var c = h.oldbuckets;

                if (c != null)
                {
                    if (!h.sameSizeGrow())
                    { 
                        // There used to be half as many buckets; mask down one more power of two.
                        m >>= 1L;
                    }
                    var oldb = (bmap.Value)(@unsafe.Pointer(uintptr(c) + (hash & m) * uintptr(t.bucketsize)));
                    if (!evacuated(oldb))
                    {
                        b = oldb;
                    }
                }

            }
            var top = tophash(hash);
            while (b != null)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        continue;
                    }
                    var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    if (t.indirectkey)
                    {
                        k = ((@unsafe.Pointer.Value)(k)).Value;
                b = b.overflow(t);
                    }
                    if (alg.equal(key, k))
                    {
                        var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                        if (t.indirectvalue)
                        {
                            v = ((@unsafe.Pointer.Value)(v)).Value;
                        }
                        return (v, true);
                    }
                }

            }

            return (@unsafe.Pointer(ref zeroVal[0L]), false);
        }

        // returns both key and value. Used by map iterator
        private static (unsafe.Pointer, unsafe.Pointer) mapaccessK(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            if (h == null || h.count == 0L)
            {
                return (null, null);
            }
            var alg = t.key.alg;
            var hash = alg.hash(key, uintptr(h.hash0));
            var m = bucketMask(h.B);
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + (hash & m) * uintptr(t.bucketsize)));
            {
                var c = h.oldbuckets;

                if (c != null)
                {
                    if (!h.sameSizeGrow())
                    { 
                        // There used to be half as many buckets; mask down one more power of two.
                        m >>= 1L;
                    }
                    var oldb = (bmap.Value)(@unsafe.Pointer(uintptr(c) + (hash & m) * uintptr(t.bucketsize)));
                    if (!evacuated(oldb))
                    {
                        b = oldb;
                    }
                }

            }
            var top = tophash(hash);
            while (b != null)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        continue;
                    }
                    var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    if (t.indirectkey)
                    {
                        k = ((@unsafe.Pointer.Value)(k)).Value;
                b = b.overflow(t);
                    }
                    if (alg.equal(key, k))
                    {
                        var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                        if (t.indirectvalue)
                        {
                            v = ((@unsafe.Pointer.Value)(v)).Value;
                        }
                        return (k, v);
                    }
                }

            }

            return (null, null);
        }

        private static unsafe.Pointer mapaccess1_fat(ref maptype t, ref hmap h, unsafe.Pointer key, unsafe.Pointer zero)
        {
            var v = mapaccess1(t, h, key);
            if (v == @unsafe.Pointer(ref zeroVal[0L]))
            {
                return zero;
            }
            return v;
        }

        private static (unsafe.Pointer, bool) mapaccess2_fat(ref maptype t, ref hmap h, unsafe.Pointer key, unsafe.Pointer zero)
        {
            var v = mapaccess1(t, h, key);
            if (v == @unsafe.Pointer(ref zeroVal[0L]))
            {
                return (zero, false);
            }
            return (v, true);
        }

        // Like mapaccess, but allocates a slot for the key if it is not present in the map.
        private static unsafe.Pointer mapassign(ref maptype _t, ref hmap _h, unsafe.Pointer key) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(mapassign);
                racewritepc(@unsafe.Pointer(h), callerpc, pc);
                raceReadObjectPC(t.key, key, callerpc, pc);
            }
            if (msanenabled)
            {
                msanread(key, t.key.size);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var alg = t.key.alg;
            var hash = alg.hash(key, uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash, since alg.hash may panic,
            // in which case we have not actually done a write.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));
            var top = tophash(hash);

            ref byte inserti = default;
            unsafe.Pointer insertk = default;
            unsafe.Pointer val = default;
            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        if (b.tophash[i] == empty && inserti == null)
                        {
                            inserti = ref b.tophash[i];
                            insertk = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                            val = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                        }
                        continue;
                    }
                    var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    if (t.indirectkey)
                    {
                        k = ((@unsafe.Pointer.Value)(k)).Value;
                    }
                    if (!alg.equal(key, k))
                    {
                        continue;
                    } 
                    // already have a mapping for key. Update it.
                    if (t.needkeyupdate)
                    {
                        typedmemmove(t.key, k, key);
                    }
                    val = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                    goto done;
                }

                var ovf = b.overflow(t);
                if (ovf == null)
                {
                    break;
                }
                b = ovf;
            } 

            // Did not find mapping for key. Allocate new cell & add entry.

            // If we hit the max load factor or we have too many overflow buckets,
            // and we're not already in the middle of growing, start growing.
 

            // Did not find mapping for key. Allocate new cell & add entry.

            // If we hit the max load factor or we have too many overflow buckets,
            // and we're not already in the middle of growing, start growing.
            if (!h.growing() && (overLoadFactor(h.count + 1L, h.B) || tooManyOverflowBuckets(h.noverflow, h.B)))
            {
                hashGrow(t, h);
                goto again; // Growing the table invalidates everything, so try again
            }
            if (inserti == null)
            { 
                // all current buckets are full, allocate a new one.
                var newb = h.newoverflow(t, b);
                inserti = ref newb.tophash[0L];
                insertk = add(@unsafe.Pointer(newb), dataOffset);
                val = add(insertk, bucketCnt * uintptr(t.keysize));
            } 

            // store new key/value at insert position
            if (t.indirectkey)
            {
                var kmem = newobject(t.key) * (@unsafe.Pointer.Value)(insertk);

                kmem;
                insertk = kmem;
            }
            if (t.indirectvalue)
            {
                var vmem = newobject(t.elem) * (@unsafe.Pointer.Value)(val);

                vmem;
            }
            typedmemmove(t.key, insertk, key);
            inserti.Value = top;
            h.count++;

done:
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            if (t.indirectvalue)
            {
                val = ((@unsafe.Pointer.Value)(val)).Value;
            }
            return val;
        });

        private static void mapdelete(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(mapdelete);
                racewritepc(@unsafe.Pointer(h), callerpc, pc);
                raceReadObjectPC(t.key, key, callerpc, pc);
            }
            if (msanenabled && h != null)
            {
                msanread(key, t.key.size);
            }
            if (h == null || h.count == 0L)
            {
                return;
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var alg = t.key.alg;
            var hash = alg.hash(key, uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash, since alg.hash may panic,
            // in which case we have not actually done a write (delete).
            h.flags |= hashWriting;

            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork(t, h, bucket);
            }
            var b = (bmap.Value)(add(h.buckets, bucket * uintptr(t.bucketsize)));
            var top = tophash(hash);
search:

            while (b != null)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        continue;
                    }
                    var k = add(@unsafe.Pointer(b), dataOffset + i * uintptr(t.keysize));
                    var k2 = k;
                    if (t.indirectkey)
                    {
                        k2 = ((@unsafe.Pointer.Value)(k2)).Value;
                b = b.overflow(t);
                    }
                    if (!alg.equal(key, k2))
                    {
                        continue;
                    } 
                    // Only clear key if there are pointers in it.
                    if (t.indirectkey)
                    {
                        (@unsafe.Pointer.Value)(k).Value;

                        null;
                    }
                    else if (t.key.kind & kindNoPointers == 0L)
                    {
                        memclrHasPointers(k, t.key.size);
                    } 
                    // Only clear value if there are pointers in it.
                    if (t.indirectvalue || t.elem.kind & kindNoPointers == 0L)
                    {
                        var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + i * uintptr(t.valuesize));
                        if (t.indirectvalue)
                        {
                            (@unsafe.Pointer.Value)(v).Value;

                            null;
                        }
                        else
                        {
                            memclrHasPointers(v, t.elem.size);
                        }
                    }
                    b.tophash[i] = empty;
                    h.count--;
                    _breaksearch = true;
                    break;
                }

            }

            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
        }

        // mapiterinit initializes the hiter struct used for ranging over maps.
        // The hiter struct pointed to by 'it' is allocated on the stack
        // by the compilers order pass or on the heap by reflect_mapiterinit.
        // Both need to have zeroed hiter since the struct contains pointers.
        private static void mapiterinit(ref maptype t, ref hmap h, ref hiter it)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapiterinit));
            }
            if (h == null || h.count == 0L)
            {
                return;
            }
            if (@unsafe.Sizeof(new hiter()) / sys.PtrSize != 12L)
            {
                throw("hash_iter size incorrect"); // see ../../cmd/internal/gc/reflect.go
            }
            it.t = t;
            it.h = h; 

            // grab snapshot of bucket state
            it.B = h.B;
            it.buckets = h.buckets;
            if (t.bucket.kind & kindNoPointers != 0L)
            { 
                // Allocate the current slice and remember pointers to both current and old.
                // This preserves all relevant overflow buckets alive even if
                // the table grows and/or overflow buckets are added to the table
                // while we are iterating.
                h.createOverflow();
                it.overflow = h.extra.overflow;
                it.oldoverflow = h.extra.oldoverflow;
            } 

            // decide where to start
            var r = uintptr(fastrand());
            if (h.B > 31L - bucketCntBits)
            {
                r += uintptr(fastrand()) << (int)(31L);
            }
            it.startBucket = r & bucketMask(h.B);
            it.offset = uint8(r >> (int)(h.B) & (bucketCnt - 1L)); 

            // iterator state
            it.bucket = it.startBucket; 

            // Remember we have an iterator.
            // Can run concurrently with another mapiterinit().
            {
                var old = h.flags;

                if (old & (iterator | oldIterator) != iterator | oldIterator)
                {
                    atomic.Or8(ref h.flags, iterator | oldIterator);
                }

            }

            mapiternext(it);
        }

        private static void mapiternext(ref hiter it)
        {
            var h = it.h;
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapiternext));
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map iteration and map write");
            }
            var t = it.t;
            var bucket = it.bucket;
            var b = it.bptr;
            var i = it.i;
            var checkBucket = it.checkBucket;
            var alg = t.key.alg;

next:
            if (b == null)
            {
                if (bucket == it.startBucket && it.wrapped)
                { 
                    // end of iteration
                    it.key = null;
                    it.value = null;
                    return;
                }
                if (h.growing() && it.B == h.B)
                { 
                    // Iterator was started in the middle of a grow, and the grow isn't done yet.
                    // If the bucket we're looking at hasn't been filled in yet (i.e. the old
                    // bucket hasn't been evacuated) then we need to iterate through the old
                    // bucket and only return the ones that will be migrated to this bucket.
                    var oldbucket = bucket & it.h.oldbucketmask();
                    b = (bmap.Value)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
                    if (!evacuated(b))
                    {
                        checkBucket = bucket;
                    }
                    else
                    {
                        b = (bmap.Value)(add(it.buckets, bucket * uintptr(t.bucketsize)));
                        checkBucket = noCheck;
                    }
                }
                else
                {
                    b = (bmap.Value)(add(it.buckets, bucket * uintptr(t.bucketsize)));
                    checkBucket = noCheck;
                }
                bucket++;
                if (bucket == bucketShift(it.B))
                {
                    bucket = 0L;
                    it.wrapped = true;
                }
                i = 0L;
            }
            while (i < bucketCnt)
            {
                var offi = (i + it.offset) & (bucketCnt - 1L);
                if (b.tophash[offi] == empty || b.tophash[offi] == evacuatedEmpty)
                {
                    continue;
                i++;
                }
                var k = add(@unsafe.Pointer(b), dataOffset + uintptr(offi) * uintptr(t.keysize));
                if (t.indirectkey)
                {
                    k = ((@unsafe.Pointer.Value)(k)).Value;
                }
                var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * uintptr(t.keysize) + uintptr(offi) * uintptr(t.valuesize));
                if (checkBucket != noCheck && !h.sameSizeGrow())
                { 
                    // Special case: iterator was started during a grow to a larger size
                    // and the grow is not done yet. We're working on a bucket whose
                    // oldbucket has not been evacuated yet. Or at least, it wasn't
                    // evacuated when we started the bucket. So we're iterating
                    // through the oldbucket, skipping any keys that will go
                    // to the other new bucket (each oldbucket expands to two
                    // buckets during a grow).
                    if (t.reflexivekey || alg.equal(k, k))
                    { 
                        // If the item in the oldbucket is not destined for
                        // the current new bucket in the iteration, skip it.
                        var hash = alg.hash(k, uintptr(h.hash0));
                        if (hash & bucketMask(it.B) != checkBucket)
                        {
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
                        if (checkBucket >> (int)((it.B - 1L)) != uintptr(b.tophash[offi] & 1L))
                        {
                            continue;
                        }
                    }
                }
                if ((b.tophash[offi] != evacuatedX && b.tophash[offi] != evacuatedY) || !(t.reflexivekey || alg.equal(k, k)))
                { 
                    // This is the golden data, we can return it.
                    // OR
                    // key!=key, so the entry can't be deleted or updated, so we can just return it.
                    // That's lucky for us because when key!=key we can't look it up successfully.
                    it.key = k;
                    if (t.indirectvalue)
                    {
                        v = ((@unsafe.Pointer.Value)(v)).Value;
                    }
                    it.value = v;
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
                    var (rk, rv) = mapaccessK(t, h, k);
                    if (rk == null)
                    {
                        continue; // key has been deleted
                    }
                    it.key = rk;
                    it.value = rv;
                }
                it.bucket = bucket;
                if (it.bptr != b)
                { // avoid unnecessary write barrier; see issue 14921
                    it.bptr = b;
                }
                it.i = i + 1L;
                it.checkBucket = checkBucket;
                return;
            }

            b = b.overflow(t);
            i = 0L;
            goto next;
        }

        private static (unsafe.Pointer, ref bmap) makeBucketArray(ref maptype t, byte b)
        {
            var @base = bucketShift(b);
            var nbuckets = base; 
            // For small b, overflow buckets are unlikely.
            // Avoid the overhead of the calculation.
            if (b >= 4L)
            { 
                // Add on the estimated number of overflow buckets
                // required to insert the median number of elements
                // used with this value of b.
                nbuckets += bucketShift(b - 4L);
                var sz = t.bucket.size * nbuckets;
                var up = roundupsize(sz);
                if (up != sz)
                {
                    nbuckets = up / t.bucket.size;
                }
            }
            buckets = newarray(t.bucket, int(nbuckets));
            if (base != nbuckets)
            { 
                // We preallocated some overflow buckets.
                // To keep the overhead of tracking these overflow buckets to a minimum,
                // we use the convention that if a preallocated overflow bucket's overflow
                // pointer is nil, then there are more available by bumping the pointer.
                // We need a safe non-nil pointer for the last overflow bucket; just use buckets.
                nextOverflow = (bmap.Value)(add(buckets, base * uintptr(t.bucketsize)));
                var last = (bmap.Value)(add(buckets, (nbuckets - 1L) * uintptr(t.bucketsize)));
                last.setoverflow(t, (bmap.Value)(buckets));
            }
            return (buckets, nextOverflow);
        }

        private static void hashGrow(ref maptype t, ref hmap h)
        { 
            // If we've hit the load factor, get bigger.
            // Otherwise, there are too many overflow buckets,
            // so keep the same number of buckets and "grow" laterally.
            var bigger = uint8(1L);
            if (!overLoadFactor(h.count + 1L, h.B))
            {
                bigger = 0L;
                h.flags |= sameSizeGrow;
            }
            var oldbuckets = h.buckets;
            var (newbuckets, nextOverflow) = makeBucketArray(t, h.B + bigger);

            var flags = h.flags & ~(iterator | oldIterator);
            if (h.flags & iterator != 0L)
            {
                flags |= oldIterator;
            } 
            // commit the grow (atomic wrt gc)
            h.B += bigger;
            h.flags = flags;
            h.oldbuckets = oldbuckets;
            h.buckets = newbuckets;
            h.nevacuate = 0L;
            h.noverflow = 0L;

            if (h.extra != null && h.extra.overflow != null)
            { 
                // Promote current overflow buckets to the old generation.
                if (h.extra.oldoverflow != null)
                {
                    throw("oldoverflow is not nil");
                }
                h.extra.oldoverflow = h.extra.overflow;
                h.extra.overflow = null;
            }
            if (nextOverflow != null)
            {
                if (h.extra == null)
                {
                    h.extra = @new<mapextra>();
                }
                h.extra.nextOverflow = nextOverflow;
            } 

            // the actual copying of the hash table data is done incrementally
            // by growWork() and evacuate().
        }

        // overLoadFactor reports whether count items placed in 1<<B buckets is over loadFactor.
        private static bool overLoadFactor(long count, byte B)
        {
            return count > bucketCnt && uintptr(count) > loadFactorNum * (bucketShift(B) / loadFactorDen);
        }

        // tooManyOverflowBuckets reports whether noverflow buckets is too many for a map with 1<<B buckets.
        // Note that most of these overflow buckets must be in sparse use;
        // if use was dense, then we'd have already triggered regular map growth.
        private static bool tooManyOverflowBuckets(ushort noverflow, byte B)
        { 
            // If the threshold is too low, we do extraneous work.
            // If the threshold is too high, maps that grow and shrink can hold on to lots of unused memory.
            // "too many" means (approximately) as many overflow buckets as regular buckets.
            // See incrnoverflow for more details.
            if (B > 15L)
            {
                B = 15L;
            } 
            // The compiler doesn't see here that B < 16; mask B to generate shorter shift code.
            return noverflow >= uint16(1L) << (int)((B & 15L));
        }

        // growing reports whether h is growing. The growth may be to the same size or bigger.
        private static bool growing(this ref hmap h)
        {
            return h.oldbuckets != null;
        }

        // sameSizeGrow reports whether the current growth is to a map of the same size.
        private static bool sameSizeGrow(this ref hmap h)
        {
            return h.flags & sameSizeGrow != 0L;
        }

        // noldbuckets calculates the number of buckets prior to the current map growth.
        private static System.UIntPtr noldbuckets(this ref hmap h)
        {
            var oldB = h.B;
            if (!h.sameSizeGrow())
            {
                oldB--;
            }
            return bucketShift(oldB);
        }

        // oldbucketmask provides a mask that can be applied to calculate n % noldbuckets().
        private static System.UIntPtr oldbucketmask(this ref hmap h)
        {
            return h.noldbuckets() - 1L;
        }

        private static void growWork(ref maptype t, ref hmap h, System.UIntPtr bucket)
        { 
            // make sure we evacuate the oldbucket corresponding
            // to the bucket we're about to use
            evacuate(t, h, bucket & h.oldbucketmask()); 

            // evacuate one more oldbucket to make progress on growing
            if (h.growing())
            {
                evacuate(t, h, h.nevacuate);
            }
        }

        private static bool bucketEvacuated(ref maptype t, ref hmap h, System.UIntPtr bucket)
        {
            var b = (bmap.Value)(add(h.oldbuckets, bucket * uintptr(t.bucketsize)));
            return evacuated(b);
        }

        // evacDst is an evacuation destination.
        private partial struct evacDst
        {
            public ptr<bmap> b; // current destination bucket
            public long i; // key/val index into b
            public unsafe.Pointer k; // pointer to current key storage
            public unsafe.Pointer v; // pointer to current value storage
        }

        private static void evacuate(ref maptype t, ref hmap h, System.UIntPtr oldbucket)
        {
            var b = (bmap.Value)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
            var newbit = h.noldbuckets();
            if (!evacuated(b))
            { 
                // TODO: reuse overflow buckets instead of using new ones, if there
                // is no iterator using the old buckets.  (If !oldIterator.)

                // xy contains the x and y (low and high) evacuation destinations.
                array<evacDst> xy = new array<evacDst>(2L);
                var x = ref xy[0L];
                x.b = (bmap.Value)(add(h.buckets, oldbucket * uintptr(t.bucketsize)));
                x.k = add(@unsafe.Pointer(x.b), dataOffset);
                x.v = add(x.k, bucketCnt * uintptr(t.keysize));

                if (!h.sameSizeGrow())
                { 
                    // Only calculate y pointers if we're growing bigger.
                    // Otherwise GC can see bad pointers.
                    var y = ref xy[1L];
                    y.b = (bmap.Value)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
                    y.k = add(@unsafe.Pointer(y.b), dataOffset);
                    y.v = add(y.k, bucketCnt * uintptr(t.keysize));
                }
                while (b != null)
                {
                    var k = add(@unsafe.Pointer(b), dataOffset);
                    var v = add(k, bucketCnt * uintptr(t.keysize));
                    {
                        long i = 0L;

                        while (i < bucketCnt)
                        {
                            var top = b.tophash[i];
                            if (top == empty)
                            {
                                b.tophash[i] = evacuatedEmpty;
                                continue;
                            i = i + 1L;
                        k = add(k, uintptr(t.keysize));
                        v = add(v, uintptr(t.valuesize));
                            }
                            if (top < minTopHash)
                            {
                                throw("bad map state");
                    b = b.overflow(t);
                            }
                            var k2 = k;
                            if (t.indirectkey)
                            {
                                k2 = ((@unsafe.Pointer.Value)(k2)).Value;
                            }
                            byte useY = default;
                            if (!h.sameSizeGrow())
                            { 
                                // Compute hash to make our evacuation decision (whether we need
                                // to send this key/value to bucket x or bucket y).
                                var hash = t.key.alg.hash(k2, uintptr(h.hash0));
                                if (h.flags & iterator != 0L && !t.reflexivekey && !t.key.alg.equal(k2, k2))
                                { 
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
                                    useY = top & 1L;
                                    top = tophash(hash);
                                }
                                else
                                {
                                    if (hash & newbit != 0L)
                                    {
                                        useY = 1L;
                                    }
                                }
                            }
                            if (evacuatedX + 1L != evacuatedY)
                            {
                                throw("bad evacuatedN");
                            }
                            b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY
                            var dst = ref xy[useY]; // evacuation destination

                            if (dst.i == bucketCnt)
                            {
                                dst.b = h.newoverflow(t, dst.b);
                                dst.i = 0L;
                                dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                                dst.v = add(dst.k, bucketCnt * uintptr(t.keysize));
                            }
                            dst.b.tophash[dst.i & (bucketCnt - 1L)] = top; // mask dst.i as an optimization, to avoid a bounds check
                            if (t.indirectkey)
                            {
                                (@unsafe.Pointer.Value)(dst.k).Value;

                                k2; // copy pointer
                            }
                            else
                            {
                                typedmemmove(t.key, dst.k, k); // copy value
                            }
                            if (t.indirectvalue)
                            {
                                (@unsafe.Pointer.Value)(dst.v).Value;

                                v.Value;
                            }
                            else
                            {
                                typedmemmove(t.elem, dst.v, v);
                            }
                            dst.i++; 
                            // These updates might push these pointers past the end of the
                            // key or value arrays.  That's ok, as we have the overflow pointer
                            // at the end of the bucket to protect against pointing past the
                            // end of the bucket.
                            dst.k = add(dst.k, uintptr(t.keysize));
                            dst.v = add(dst.v, uintptr(t.valuesize));
                        }

                    }
                } 
                // Unlink the overflow buckets & clear key/value to help GC.
 
                // Unlink the overflow buckets & clear key/value to help GC.
                if (h.flags & oldIterator == 0L && t.bucket.kind & kindNoPointers == 0L)
                {
                    b = add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)); 
                    // Preserve b.tophash because the evacuation
                    // state is maintained there.
                    var ptr = add(b, dataOffset);
                    var n = uintptr(t.bucketsize) - dataOffset;
                    memclrHasPointers(ptr, n);
                }
            }
            if (oldbucket == h.nevacuate)
            {
                advanceEvacuationMark(h, t, newbit);
            }
        }

        private static void advanceEvacuationMark(ref hmap h, ref maptype t, System.UIntPtr newbit)
        {
            h.nevacuate++; 
            // Experiments suggest that 1024 is overkill by at least an order of magnitude.
            // Put it in there as a safeguard anyway, to ensure O(1) behavior.
            var stop = h.nevacuate + 1024L;
            if (stop > newbit)
            {
                stop = newbit;
            }
            while (h.nevacuate != stop && bucketEvacuated(t, h, h.nevacuate))
            {
                h.nevacuate++;
            }

            if (h.nevacuate == newbit)
            { // newbit == # of oldbuckets
                // Growing is all done. Free old main bucket array.
                h.oldbuckets = null; 
                // Can discard old overflow buckets as well.
                // If they are still referenced by an iterator,
                // then the iterator holds a pointers to the slice.
                if (h.extra != null)
                {
                    h.extra.oldoverflow = null;
                }
                h.flags &= sameSizeGrow;
            }
        }

        private static bool ismapkey(ref _type t)
        {
            return t.alg.hash != null;
        }

        // Reflect stubs. Called from ../reflect/asm_*.s

        //go:linkname reflect_makemap reflect.makemap
        private static ref hmap reflect_makemap(ref maptype t, long cap)
        { 
            // Check invariants and reflects math.
            {
                var sz = @unsafe.Sizeof(new hmap());

                if (sz != t.hmap.size)
                {
                    println("runtime: sizeof(hmap) =", sz, ", t.hmap.size =", t.hmap.size);
                    throw("bad hmap size");
                }

            }
            if (!ismapkey(t.key))
            {
                throw("runtime.reflect_makemap: unsupported map key type");
            }
            if (t.key.size > maxKeySize && (!t.indirectkey || t.keysize != uint8(sys.PtrSize)) || t.key.size <= maxKeySize && (t.indirectkey || t.keysize != uint8(t.key.size)))
            {
                throw("key size wrong");
            }
            if (t.elem.size > maxValueSize && (!t.indirectvalue || t.valuesize != uint8(sys.PtrSize)) || t.elem.size <= maxValueSize && (t.indirectvalue || t.valuesize != uint8(t.elem.size)))
            {
                throw("value size wrong");
            }
            if (t.key.align > bucketCnt)
            {
                throw("key align too big");
            }
            if (t.elem.align > bucketCnt)
            {
                throw("value align too big");
            }
            if (t.key.size % uintptr(t.key.align) != 0L)
            {
                throw("key size not a multiple of key align");
            }
            if (t.elem.size % uintptr(t.elem.align) != 0L)
            {
                throw("value size not a multiple of value align");
            }
            if (bucketCnt < 8L)
            {
                throw("bucketsize too small for proper alignment");
            }
            if (dataOffset % uintptr(t.key.align) != 0L)
            {
                throw("need padding in bucket (key)");
            }
            if (dataOffset % uintptr(t.elem.align) != 0L)
            {
                throw("need padding in bucket (value)");
            }
            return makemap(t, cap, null);
        }

        //go:linkname reflect_mapaccess reflect.mapaccess
        private static unsafe.Pointer reflect_mapaccess(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            var (val, ok) = mapaccess2(t, h, key);
            if (!ok)
            { 
                // reflect wants nil for a missing element
                val = null;
            }
            return val;
        }

        //go:linkname reflect_mapassign reflect.mapassign
        private static void reflect_mapassign(ref maptype t, ref hmap h, unsafe.Pointer key, unsafe.Pointer val)
        {
            var p = mapassign(t, h, key);
            typedmemmove(t.elem, p, val);
        }

        //go:linkname reflect_mapdelete reflect.mapdelete
        private static void reflect_mapdelete(ref maptype t, ref hmap h, unsafe.Pointer key)
        {
            mapdelete(t, h, key);
        }

        //go:linkname reflect_mapiterinit reflect.mapiterinit
        private static ref hiter reflect_mapiterinit(ref maptype t, ref hmap h)
        {
            ptr<hiter> it = @new<hiter>();
            mapiterinit(t, h, it);
            return it;
        }

        //go:linkname reflect_mapiternext reflect.mapiternext
        private static void reflect_mapiternext(ref hiter it)
        {
            mapiternext(it);
        }

        //go:linkname reflect_mapiterkey reflect.mapiterkey
        private static unsafe.Pointer reflect_mapiterkey(ref hiter it)
        {
            return it.key;
        }

        //go:linkname reflect_maplen reflect.maplen
        private static long reflect_maplen(ref hmap h)
        {
            if (h == null)
            {
                return 0L;
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(reflect_maplen));
            }
            return h.count;
        }

        //go:linkname reflect_ismapkey reflect.ismapkey
        private static bool reflect_ismapkey(ref _type t)
        {
            return ismapkey(t);
        }

        private static readonly long maxZero = 1024L; // must match value in ../cmd/compile/internal/gc/walk.go
 // must match value in ../cmd/compile/internal/gc/walk.go
        private static array<byte> zeroVal = new array<byte>(maxZero);
    }
}
