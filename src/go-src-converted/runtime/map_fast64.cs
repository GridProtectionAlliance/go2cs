// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\map_fast64.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static unsafe.Pointer mapaccess1_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, ulong key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess1_fast64));
    }
    if (h == null || h.count == 0) {
        return @unsafe.Pointer(_addr_zeroVal[0]);
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map read and map write");
    }
    ptr<bmap> b;
    if (h.B == 0) { 
        // One-bucket table. No need to hash.
        b = (bmap.val)(h.buckets);

    }
    else
 {
        var hash = t.hasher(noescape(@unsafe.Pointer(_addr_key)), uintptr(h.hash0));
        var m = bucketMask(h.B);
        b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
        {
            var c = h.oldbuckets;

            if (c != null) {
                if (!h.sameSizeGrow()) { 
                    // There used to be half as many buckets; mask down one more power of two.
                    m>>=1;

                }
                var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
                if (!evacuated(oldb)) {
                    b = oldb;
                }
            }
        }

    }
    while (b != null) {
        {
            var i = uintptr(0);
            var k = b.keys();

            while (i < bucketCnt) {
                if (new ptr<ptr<ptr<ulong>>>(k) == key && !isEmpty(b.tophash[i])) {
                    return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8 + i * uintptr(t.elemsize));
                (i, k) = (i + 1, add(k, 8));
                }
        b = b.overflow(t);
            }
        }

    }
    return @unsafe.Pointer(_addr_zeroVal[0]);

}

private static (unsafe.Pointer, bool) mapaccess2_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, ulong key) {
    unsafe.Pointer _p0 = default;
    bool _p0 = default;
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess2_fast64));
    }
    if (h == null || h.count == 0) {
        return (@unsafe.Pointer(_addr_zeroVal[0]), false);
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map read and map write");
    }
    ptr<bmap> b;
    if (h.B == 0) { 
        // One-bucket table. No need to hash.
        b = (bmap.val)(h.buckets);

    }
    else
 {
        var hash = t.hasher(noescape(@unsafe.Pointer(_addr_key)), uintptr(h.hash0));
        var m = bucketMask(h.B);
        b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
        {
            var c = h.oldbuckets;

            if (c != null) {
                if (!h.sameSizeGrow()) { 
                    // There used to be half as many buckets; mask down one more power of two.
                    m>>=1;

                }

                var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
                if (!evacuated(oldb)) {
                    b = oldb;
                }

            }

        }

    }
    while (b != null) {
        {
            var i = uintptr(0);
            var k = b.keys();

            while (i < bucketCnt) {
                if (new ptr<ptr<ptr<ulong>>>(k) == key && !isEmpty(b.tophash[i])) {
                    return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8 + i * uintptr(t.elemsize)), true);
                (i, k) = (i + 1, add(k, 8));
                }

        b = b.overflow(t);
            }

        }

    }
    return (@unsafe.Pointer(_addr_zeroVal[0]), false);

}

private static unsafe.Pointer mapassign_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, ulong key) => func((_, panic, _) => {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (h == null) {
        panic(plainError("assignment to entry in nil map"));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast64));
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    var hash = t.hasher(noescape(@unsafe.Pointer(_addr_key)), uintptr(h.hash0)); 

    // Set hashWriting after calling t.hasher for consistency with mapassign.
    h.flags ^= hashWriting;

    if (h.buckets == null) {
        h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
    }
again:
    var bucket = hash & bucketMask(h.B);
    if (h.growing()) {
        growWork_fast64(_addr_t, _addr_h, bucket);
    }
    var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));

    ptr<bmap> insertb;
    System.UIntPtr inserti = default;
    unsafe.Pointer insertk = default;

bucketloop: 

    // Did not find mapping for key. Allocate new cell & add entry.

    // If we hit the max load factor or we have too many overflow buckets,
    // and we're not already in the middle of growing, start growing.
    while (true) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (isEmpty(b.tophash[i])) {
                if (insertb == null) {
                    insertb = b;
                    inserti = i;
                }
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }

                continue;

            }

            var k = ((uint64.val)(add(@unsafe.Pointer(b), dataOffset + i * 8))).val;
            if (k != key) {
                continue;
            }

            insertb = b;
            inserti = i;
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
        hashGrow(t, h);
        goto again; // Growing the table invalidates everything, so try again
    }
    if (insertb == null) { 
        // The current bucket and all the overflow buckets connected to it are full, allocate a new one.
        insertb = h.newoverflow(t, b);
        inserti = 0; // not necessary, but avoids needlessly spilling inserti
    }
    insertb.tophash[inserti & (bucketCnt - 1)] = tophash(hash); // mask inserti to avoid bounds checks

    insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 8) * (uint64.val)(insertk);

    key;

    h.count++;

done:
    var elem = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 8 + inserti * uintptr(t.elemsize));
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;
    return elem;

});

private static unsafe.Pointer mapassign_fast64ptr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, unsafe.Pointer key) => func((_, panic, _) => {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (h == null) {
        panic(plainError("assignment to entry in nil map"));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast64));
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    var hash = t.hasher(noescape(@unsafe.Pointer(_addr_key)), uintptr(h.hash0)); 

    // Set hashWriting after calling t.hasher for consistency with mapassign.
    h.flags ^= hashWriting;

    if (h.buckets == null) {
        h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
    }
again:
    var bucket = hash & bucketMask(h.B);
    if (h.growing()) {
        growWork_fast64(_addr_t, _addr_h, bucket);
    }
    var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));

    ptr<bmap> insertb;
    System.UIntPtr inserti = default;
    unsafe.Pointer insertk = default;

bucketloop: 

    // Did not find mapping for key. Allocate new cell & add entry.

    // If we hit the max load factor or we have too many overflow buckets,
    // and we're not already in the middle of growing, start growing.
    while (true) {
        for (var i = uintptr(0); i < bucketCnt; i++) {
            if (isEmpty(b.tophash[i])) {
                if (insertb == null) {
                    insertb = b;
                    inserti = i;
                }
                if (b.tophash[i] == emptyRest) {
                    _breakbucketloop = true;
                    break;
                }

                continue;

            }

            var k = ((@unsafe.Pointer.val)(add(@unsafe.Pointer(b), dataOffset + i * 8))).val;
            if (k != key) {
                continue;
            }

            insertb = b;
            inserti = i;
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
        hashGrow(t, h);
        goto again; // Growing the table invalidates everything, so try again
    }
    if (insertb == null) { 
        // The current bucket and all the overflow buckets connected to it are full, allocate a new one.
        insertb = h.newoverflow(t, b);
        inserti = 0; // not necessary, but avoids needlessly spilling inserti
    }
    insertb.tophash[inserti & (bucketCnt - 1)] = tophash(hash); // mask inserti to avoid bounds checks

    insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 8) * (@unsafe.Pointer.val)(insertk);

    key;

    h.count++;

done:
    var elem = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 8 + inserti * uintptr(t.elemsize));
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;
    return elem;

});

private static void mapdelete_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, ulong key) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    if (raceenabled && h != null) {
        var callerpc = getcallerpc();
        racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapdelete_fast64));
    }
    if (h == null || h.count == 0) {
        return ;
    }
    if (h.flags & hashWriting != 0) {
        throw("concurrent map writes");
    }
    var hash = t.hasher(noescape(@unsafe.Pointer(_addr_key)), uintptr(h.hash0)); 

    // Set hashWriting after calling t.hasher for consistency with mapdelete
    h.flags ^= hashWriting;

    var bucket = hash & bucketMask(h.B);
    if (h.growing()) {
        growWork_fast64(_addr_t, _addr_h, bucket);
    }
    var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));
    var bOrig = b;
search:

    while (b != null) {
        {
            var i = uintptr(0);
            var k = b.keys();

            while (i < bucketCnt) {
                if (key != new ptr<ptr<ptr<ulong>>>(k) || isEmpty(b.tophash[i])) {
                    continue;
                (i, k) = (i + 1, add(k, 8));
                } 
                // Only clear key if there are pointers in it.
                if (t.key.ptrdata != 0) {
                    if (sys.PtrSize == 8) {
                        (@unsafe.Pointer.val)(k).val;

                        null;
        b = b.overflow(t);
                    }
                    else
 { 
                        // There are three ways to squeeze at one ore more 32 bit pointers into 64 bits.
                        // Just call memclrHasPointers instead of trying to handle all cases here.
                        memclrHasPointers(k, 8);

                    }

                }

                var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8 + i * uintptr(t.elemsize));
                if (t.elem.ptrdata != 0) {
                    memclrHasPointers(e, t.elem.size);
                }
                else
 {
                    memclrNoHeapPointers(e, t.elem.size);
                }

                b.tophash[i] = emptyOne; 
                // If the bucket now ends in a bunch of emptyOne states,
                // change those to emptyRest states.
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

    }
    if (h.flags & hashWriting == 0) {
        throw("concurrent map writes");
    }
    h.flags &= hashWriting;

}

private static void growWork_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr bucket) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;
 
    // make sure we evacuate the oldbucket corresponding
    // to the bucket we're about to use
    evacuate_fast64(_addr_t, _addr_h, bucket & h.oldbucketmask()); 

    // evacuate one more oldbucket to make progress on growing
    if (h.growing()) {
        evacuate_fast64(_addr_t, _addr_h, h.nevacuate);
    }
}

private static void evacuate_fast64(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr oldbucket) {
    ref maptype t = ref _addr_t.val;
    ref hmap h = ref _addr_h.val;

    var b = (bmap.val)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
    var newbit = h.noldbuckets();
    if (!evacuated(b)) { 
        // TODO: reuse overflow buckets instead of using new ones, if there
        // is no iterator using the old buckets.  (If !oldIterator.)

        // xy contains the x and y (low and high) evacuation destinations.
        array<evacDst> xy = new array<evacDst>(2);
        var x = _addr_xy[0];
        x.b = (bmap.val)(add(h.buckets, oldbucket * uintptr(t.bucketsize)));
        x.k = add(@unsafe.Pointer(x.b), dataOffset);
        x.e = add(x.k, bucketCnt * 8);

        if (!h.sameSizeGrow()) { 
            // Only calculate y pointers if we're growing bigger.
            // Otherwise GC can see bad pointers.
            var y = _addr_xy[1];
            y.b = (bmap.val)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
            y.k = add(@unsafe.Pointer(y.b), dataOffset);
            y.e = add(y.k, bucketCnt * 8);

        }
        while (b != null) {
            var k = add(@unsafe.Pointer(b), dataOffset);
            var e = add(k, bucketCnt * 8);
            {
                nint i = 0;

                while (i < bucketCnt) {
                    var top = b.tophash[i];
                    if (isEmpty(top)) {
                        b.tophash[i] = evacuatedEmpty;
                        continue;
                    (i, k, e) = (i + 1, add(k, 8), add(e, uintptr(t.elemsize)));
                    }

                    if (top < minTopHash) {
                        throw("bad map state");
            b = b.overflow(t);
                    }

                    byte useY = default;
                    if (!h.sameSizeGrow()) { 
                        // Compute hash to make our evacuation decision (whether we need
                        // to send this key/elem to bucket x or bucket y).
                        var hash = t.hasher(k, uintptr(h.hash0));
                        if (hash & newbit != 0) {
                            useY = 1;
                        }

                    }

                    b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY, enforced in makemap
                    var dst = _addr_xy[useY]; // evacuation destination

                    if (dst.i == bucketCnt) {
                        dst.b = h.newoverflow(t, dst.b);
                        dst.i = 0;
                        dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                        dst.e = add(dst.k, bucketCnt * 8);
                    }

                    dst.b.tophash[dst.i & (bucketCnt - 1)] = top; // mask dst.i as an optimization, to avoid a bounds check

                    // Copy key.
                    if (t.key.ptrdata != 0 && writeBarrier.enabled) {
                        if (sys.PtrSize == 8) { 
                            // Write with a write barrier.
                            (@unsafe.Pointer.val)(dst.k).val;

                            new ptr<ptr<ptr<unsafe.Pointer>>>(k);

                        }
                        else
 { 
                            // There are three ways to squeeze at least one 32 bit pointer into 64 bits.
                            // Give up and call typedmemmove.
                            typedmemmove(t.key, dst.k, k);

                        }

                    }
                    else
 {
                        (uint64.val)(dst.k).val;

                        new ptr<ptr<ptr<ulong>>>(k);

                    }

                    typedmemmove(t.elem, dst.e, e);
                    dst.i++; 
                    // These updates might push these pointers past the end of the
                    // key or elem arrays.  That's ok, as we have the overflow pointer
                    // at the end of the bucket to protect against pointing past the
                    // end of the bucket.
                    dst.k = add(dst.k, 8);
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
        advanceEvacuationMark(h, t, newbit);
    }
}

} // end runtime_package
