// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:20:21 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\map_faststr.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static unsafe.Pointer mapaccess1_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, @string ky)
        {
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;

            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess1_faststr));
            }
            if (h == null || h.count == 0L)
            {
                return @unsafe.Pointer(_addr_zeroVal[0L]);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            var key = stringStructOf(_addr_ky);
            if (h.B == 0L)
            { 
                // One-bucket table.
                var b = (bmap.val)(h.buckets);
                if (key.len < 32L)
                { 
                    // short key, doing lots of comparisons is ok
                    {
                        var i__prev1 = i;
                        var kptr__prev1 = kptr;

                        var i = uintptr(0L);
                        var kptr = b.keys();

                        while (i < bucketCnt)
                        {
                            var k = (stringStruct.val)(kptr);
                            if (k.len != key.len || isEmpty(b.tophash[i]))
                            {
                                if (b.tophash[i] == emptyRest)
                                {
                                    break;
                            i = i + 1L;
                        kptr = add(kptr, 2L * sys.PtrSize);
                                }
                                continue;

                            }
                            if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                            {
                                return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize));
                            }
                        }

                        i = i__prev1;
                        kptr = kptr__prev1;
                    }
                    return @unsafe.Pointer(_addr_zeroVal[0L]);

                }
                var keymaybe = uintptr(bucketCnt);
                {
                    var i__prev1 = i;
                    var kptr__prev1 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.val)(kptr);
                        if (k.len != key.len || isEmpty(b.tophash[i]))
                        {
                            if (b.tophash[i] == emptyRest)
                            {
                                break;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                            }
                            continue;

                        }
                        if (k.str == key.str)
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize));
                        }
                        if ((new ptr<ptr<array<byte>>>(key.str)) != (new ptr<ptr<array<byte>>>(k.str)).val)
                        {
                            continue;
                        }
                        if ((new ptr<ptr<array<byte>>>(add(key.str, uintptr(key.len) - 4L))) != (new ptr<ptr<array<byte>>>(add(k.str, uintptr(key.len) - 4L))).val)
                        {
                            continue;
                        }
                        if (keymaybe != bucketCnt)
                        { 
                            // Two keys are potential matches. Use hash to distinguish them.
                            goto dohash;

                        }
                        keymaybe = i;

                    }

                    i = i__prev1;
                    kptr = kptr__prev1;
                }
                if (keymaybe != bucketCnt)
                {
                    k = (stringStruct.val)(add(@unsafe.Pointer(b), dataOffset + keymaybe * 2L * sys.PtrSize));
                    if (memequal(k.str, key.str, uintptr(key.len)))
                    {
                        return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + keymaybe * uintptr(t.elemsize));
                    }
                }
                return @unsafe.Pointer(_addr_zeroVal[0L]);

            }
dohash:
            var hash = t.hasher(noescape(@unsafe.Pointer(_addr_ky)), uintptr(h.hash0));
            var m = bucketMask(h.B);
            b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
            {
                var c = h.oldbuckets;

                if (c != null)
                {
                    if (!h.sameSizeGrow())
                    { 
                        // There used to be half as many buckets; mask down one more power of two.
                        m >>= 1L;

                    }
                    var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
                    if (!evacuated(oldb))
                    {
                        b = oldb;
                    }
                }
            }

            var top = tophash(hash);
            while (b != null)
            {
                {
                    var i__prev2 = i;
                    var kptr__prev2 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.val)(kptr);
                        if (k.len != key.len || b.tophash[i] != top)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }
                        if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize));
                b = b.overflow(t);
                        }
                    }

                    i = i__prev2;
                    kptr = kptr__prev2;
                }

            }
            return @unsafe.Pointer(_addr_zeroVal[0L]);

        }

        private static (unsafe.Pointer, bool) mapaccess2_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, @string ky)
        {
            unsafe.Pointer _p0 = default;
            bool _p0 = default;
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;

            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess2_faststr));
            }

            if (h == null || h.count == 0L)
            {
                return (@unsafe.Pointer(_addr_zeroVal[0L]), false);
            }

            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }

            var key = stringStructOf(_addr_ky);
            if (h.B == 0L)
            { 
                // One-bucket table.
                var b = (bmap.val)(h.buckets);
                if (key.len < 32L)
                { 
                    // short key, doing lots of comparisons is ok
                    {
                        var i__prev1 = i;
                        var kptr__prev1 = kptr;

                        var i = uintptr(0L);
                        var kptr = b.keys();

                        while (i < bucketCnt)
                        {
                            var k = (stringStruct.val)(kptr);
                            if (k.len != key.len || isEmpty(b.tophash[i]))
                            {
                                if (b.tophash[i] == emptyRest)
                                {
                                    break;
                            i = i + 1L;
                        kptr = add(kptr, 2L * sys.PtrSize);
                                }

                                continue;

                            }

                            if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                            {
                                return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize)), true);
                            }

                        }


                        i = i__prev1;
                        kptr = kptr__prev1;
                    }
                    return (@unsafe.Pointer(_addr_zeroVal[0L]), false);

                } 
                // long key, try not to do more comparisons than necessary
                var keymaybe = uintptr(bucketCnt);
                {
                    var i__prev1 = i;
                    var kptr__prev1 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.val)(kptr);
                        if (k.len != key.len || isEmpty(b.tophash[i]))
                        {
                            if (b.tophash[i] == emptyRest)
                            {
                                break;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                            }

                            continue;

                        }

                        if (k.str == key.str)
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize)), true);
                        } 
                        // check first 4 bytes
                        if ((new ptr<ptr<array<byte>>>(key.str)) != (new ptr<ptr<array<byte>>>(k.str)).val)
                        {
                            continue;
                        } 
                        // check last 4 bytes
                        if ((new ptr<ptr<array<byte>>>(add(key.str, uintptr(key.len) - 4L))) != (new ptr<ptr<array<byte>>>(add(k.str, uintptr(key.len) - 4L))).val)
                        {
                            continue;
                        }

                        if (keymaybe != bucketCnt)
                        { 
                            // Two keys are potential matches. Use hash to distinguish them.
                            goto dohash;

                        }

                        keymaybe = i;

                    }


                    i = i__prev1;
                    kptr = kptr__prev1;
                }
                if (keymaybe != bucketCnt)
                {
                    k = (stringStruct.val)(add(@unsafe.Pointer(b), dataOffset + keymaybe * 2L * sys.PtrSize));
                    if (memequal(k.str, key.str, uintptr(key.len)))
                    {
                        return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + keymaybe * uintptr(t.elemsize)), true);
                    }

                }

                return (@unsafe.Pointer(_addr_zeroVal[0L]), false);

            }

dohash:
            var hash = t.hasher(noescape(@unsafe.Pointer(_addr_ky)), uintptr(h.hash0));
            var m = bucketMask(h.B);
            b = (bmap.val)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
            {
                var c = h.oldbuckets;

                if (c != null)
                {
                    if (!h.sameSizeGrow())
                    { 
                        // There used to be half as many buckets; mask down one more power of two.
                        m >>= 1L;

                    }

                    var oldb = (bmap.val)(add(c, (hash & m) * uintptr(t.bucketsize)));
                    if (!evacuated(oldb))
                    {
                        b = oldb;
                    }

                }

            }

            var top = tophash(hash);
            while (b != null)
            {
                {
                    var i__prev2 = i;
                    var kptr__prev2 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.val)(kptr);
                        if (k.len != key.len || b.tophash[i] != top)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }

                        if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize)), true);
                b = b.overflow(t);
                        }

                    }


                    i = i__prev2;
                    kptr = kptr__prev2;
                }

            }

            return (@unsafe.Pointer(_addr_zeroVal[0L]), false);

        }

        private static unsafe.Pointer mapassign_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, @string s) => func((_, panic, __) =>
        {
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;

            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }

            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_faststr));
            }

            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }

            var key = stringStructOf(_addr_s);
            var hash = t.hasher(noescape(@unsafe.Pointer(_addr_s)), uintptr(h.hash0)); 

            // Set hashWriting after calling t.hasher for consistency with mapassign.
            h.flags ^= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }

again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_faststr(_addr_t, _addr_h, bucket);
            }

            var b = (bmap.val)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));
            var top = tophash(hash);

            ptr<bmap> insertb;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

bucketloop: 

            // Did not find mapping for key. Allocate new cell & add entry.

            // If we hit the max load factor or we have too many overflow buckets,
            // and we're not already in the middle of growing, start growing.
            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        if (isEmpty(b.tophash[i]) && insertb == null)
                        {
                            insertb = b;
                            inserti = i;
                        }

                        if (b.tophash[i] == emptyRest)
                        {
                            _breakbucketloop = true;
                            break;
                        }

                        continue;

                    }

                    var k = (stringStruct.val)(add(@unsafe.Pointer(b), dataOffset + i * 2L * sys.PtrSize));
                    if (k.len != key.len)
                    {
                        continue;
                    }

                    if (k.str != key.str && !memequal(k.str, key.str, uintptr(key.len)))
                    {
                        continue;
                    } 
                    // already have a mapping for key. Update it.
                    inserti = i;
                    insertb = b;
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

            if (insertb == null)
            { 
                // all current buckets are full, allocate a new one.
                insertb = h.newoverflow(t, b);
                inserti = 0L; // not necessary, but avoids needlessly spilling inserti
            }

            insertb.tophash[inserti & (bucketCnt - 1L)] = top; // mask inserti to avoid bounds checks

            insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 2L * sys.PtrSize); 
            // store new key at insert position
            ((stringStruct.val)(insertk)).val = key.val;
            h.count++;

done:
            var elem = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 2L * sys.PtrSize + inserti * uintptr(t.elemsize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }

            h.flags &= hashWriting;
            return elem;

        });

        private static void mapdelete_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, @string ky)
        {
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;

            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapdelete_faststr));
            }

            if (h == null || h.count == 0L)
            {
                return ;
            }

            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }

            var key = stringStructOf(_addr_ky);
            var hash = t.hasher(noescape(@unsafe.Pointer(_addr_ky)), uintptr(h.hash0)); 

            // Set hashWriting after calling t.hasher for consistency with mapdelete
            h.flags ^= hashWriting;

            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_faststr(_addr_t, _addr_h, bucket);
            }

            var b = (bmap.val)(add(h.buckets, bucket * uintptr(t.bucketsize)));
            var bOrig = b;
            var top = tophash(hash);
search:

            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        var k = (stringStruct.val)(kptr);
                        if (k.len != key.len || b.tophash[i] != top)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }

                        if (k.str != key.str && !memequal(k.str, key.str, uintptr(key.len)))
                        {
                            continue;
                b = b.overflow(t);
                        } 
                        // Clear key's pointer.
                        k.str = null;
                        var e = add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.elemsize));
                        if (t.elem.ptrdata != 0L)
                        {
                            memclrHasPointers(e, t.elem.size);
                        }
                        else
                        {
                            memclrNoHeapPointers(e, t.elem.size);
                        }

                        b.tophash[i] = emptyOne; 
                        // If the bucket now ends in a bunch of emptyOne states,
                        // change those to emptyRest states.
                        if (i == bucketCnt - 1L)
                        {
                            if (b.overflow(t) != null && b.overflow(t).tophash[0L] != emptyRest)
                            {
                                goto notLast;
                            }

                        }
                        else
                        {
                            if (b.tophash[i + 1L] != emptyRest)
                            {
                                goto notLast;
                            }

                        }

                        while (true)
                        {
                            b.tophash[i] = emptyRest;
                            if (i == 0L)
                            {
                                if (b == bOrig)
                                {
                                    break; // beginning of initial bucket, we're done.
                                } 
                                // Find previous bucket, continue at its last entry.
                                var c = b;
                                b = bOrig;

                                while (b.overflow(t) != c)
                                {
                                    b = b.overflow(t);
                                }
                            else

                                i = bucketCnt - 1L;

                            }                            {
                                i--;
                            }

                            if (b.tophash[i] != emptyOne)
                            {
                                break;
                            }

                        }

notLast:
                        h.count--;
                        _breaksearch = true;
                        break;
                    }

                }

            }

            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }

            h.flags &= hashWriting;

        }

        private static void growWork_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr bucket)
        {
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;
 
            // make sure we evacuate the oldbucket corresponding
            // to the bucket we're about to use
            evacuate_faststr(_addr_t, _addr_h, bucket & h.oldbucketmask()); 

            // evacuate one more oldbucket to make progress on growing
            if (h.growing())
            {
                evacuate_faststr(_addr_t, _addr_h, h.nevacuate);
            }

        }

        private static void evacuate_faststr(ptr<maptype> _addr_t, ptr<hmap> _addr_h, System.UIntPtr oldbucket)
        {
            ref maptype t = ref _addr_t.val;
            ref hmap h = ref _addr_h.val;

            var b = (bmap.val)(add(h.oldbuckets, oldbucket * uintptr(t.bucketsize)));
            var newbit = h.noldbuckets();
            if (!evacuated(b))
            { 
                // TODO: reuse overflow buckets instead of using new ones, if there
                // is no iterator using the old buckets.  (If !oldIterator.)

                // xy contains the x and y (low and high) evacuation destinations.
                array<evacDst> xy = new array<evacDst>(2L);
                var x = _addr_xy[0L];
                x.b = (bmap.val)(add(h.buckets, oldbucket * uintptr(t.bucketsize)));
                x.k = add(@unsafe.Pointer(x.b), dataOffset);
                x.e = add(x.k, bucketCnt * 2L * sys.PtrSize);

                if (!h.sameSizeGrow())
                { 
                    // Only calculate y pointers if we're growing bigger.
                    // Otherwise GC can see bad pointers.
                    var y = _addr_xy[1L];
                    y.b = (bmap.val)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
                    y.k = add(@unsafe.Pointer(y.b), dataOffset);
                    y.e = add(y.k, bucketCnt * 2L * sys.PtrSize);

                }

                while (b != null)
                {
                    var k = add(@unsafe.Pointer(b), dataOffset);
                    var e = add(k, bucketCnt * 2L * sys.PtrSize);
                    {
                        long i = 0L;

                        while (i < bucketCnt)
                        {
                            var top = b.tophash[i];
                            if (isEmpty(top))
                            {
                                b.tophash[i] = evacuatedEmpty;
                                continue;
                            i = i + 1L;
                        k = add(k, 2L * sys.PtrSize);
                        e = add(e, uintptr(t.elemsize));
                            }

                            if (top < minTopHash)
                            {
                                throw("bad map state");
                    b = b.overflow(t);
                            }

                            byte useY = default;
                            if (!h.sameSizeGrow())
                            { 
                                // Compute hash to make our evacuation decision (whether we need
                                // to send this key/elem to bucket x or bucket y).
                                var hash = t.hasher(k, uintptr(h.hash0));
                                if (hash & newbit != 0L)
                                {
                                    useY = 1L;
                                }

                            }

                            b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY, enforced in makemap
                            var dst = _addr_xy[useY]; // evacuation destination

                            if (dst.i == bucketCnt)
                            {
                                dst.b = h.newoverflow(t, dst.b);
                                dst.i = 0L;
                                dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                                dst.e = add(dst.k, bucketCnt * 2L * sys.PtrSize);
                            }

                            dst.b.tophash[dst.i & (bucketCnt - 1L)] = top * (string.val)(dst.k);

                            new ptr<ptr<ptr<@string>>>(k);

                            typedmemmove(t.elem, dst.e, e);
                            dst.i++; 
                            // These updates might push these pointers past the end of the
                            // key or elem arrays.  That's ok, as we have the overflow pointer
                            // at the end of the bucket to protect against pointing past the
                            // end of the bucket.
                            dst.k = add(dst.k, 2L * sys.PtrSize);
                            dst.e = add(dst.e, uintptr(t.elemsize));

                        }

                    }

                } 
                // Unlink the overflow buckets & clear key/elem to help GC.
 
                // Unlink the overflow buckets & clear key/elem to help GC.
                if (h.flags & oldIterator == 0L && t.bucket.ptrdata != 0L)
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
    }
}
