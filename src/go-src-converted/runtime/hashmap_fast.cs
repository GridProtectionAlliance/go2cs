// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:12 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\hashmap_fast.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static unsafe.Pointer mapaccess1_fast32(ref maptype t, ref hmap h, uint key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess1_fast32));
            }
            if (h == null || h.count == 0L)
            {
                return @unsafe.Pointer(ref zeroVal[0L]);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            ref bmap b = default;
            if (h.B == 0L)
            { 
                // One-bucket table. No need to hash.
                b = (bmap.Value)(h.buckets);
            }
            else
            {
                var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0));
                var m = bucketMask(h.B);
                b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
            }
            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (k.Value == key && b.tophash[i] != empty)
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 4L + i * uintptr(t.valuesize));
                        i = i + 1L;
                    k = add(k, 4L);
                        }
                b = b.overflow(t);
                    }
                }
            }
            return @unsafe.Pointer(ref zeroVal[0L]);
        }

        private static (unsafe.Pointer, bool) mapaccess2_fast32(ref maptype t, ref hmap h, uint key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess2_fast32));
            }
            if (h == null || h.count == 0L)
            {
                return (@unsafe.Pointer(ref zeroVal[0L]), false);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            ref bmap b = default;
            if (h.B == 0L)
            { 
                // One-bucket table. No need to hash.
                b = (bmap.Value)(h.buckets);
            }
            else
            {
                var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0));
                var m = bucketMask(h.B);
                b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
            }
            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (k.Value == key && b.tophash[i] != empty)
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 4L + i * uintptr(t.valuesize)), true);
                        i = i + 1L;
                    k = add(k, 4L);
                        }
                b = b.overflow(t);
                    }

                }
            }

            return (@unsafe.Pointer(ref zeroVal[0L]), false);
        }

        private static unsafe.Pointer mapaccess1_fast64(ref maptype t, ref hmap h, ulong key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess1_fast64));
            }
            if (h == null || h.count == 0L)
            {
                return @unsafe.Pointer(ref zeroVal[0L]);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            ref bmap b = default;
            if (h.B == 0L)
            { 
                // One-bucket table. No need to hash.
                b = (bmap.Value)(h.buckets);
            }
            else
            {
                var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0));
                var m = bucketMask(h.B);
                b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
            }
            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (k.Value == key && b.tophash[i] != empty)
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8L + i * uintptr(t.valuesize));
                        i = i + 1L;
                    k = add(k, 8L);
                        }
                b = b.overflow(t);
                    }

                }
            }

            return @unsafe.Pointer(ref zeroVal[0L]);
        }

        private static (unsafe.Pointer, bool) mapaccess2_fast64(ref maptype t, ref hmap h, ulong key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess2_fast64));
            }
            if (h == null || h.count == 0L)
            {
                return (@unsafe.Pointer(ref zeroVal[0L]), false);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            ref bmap b = default;
            if (h.B == 0L)
            { 
                // One-bucket table. No need to hash.
                b = (bmap.Value)(h.buckets);
            }
            else
            {
                var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0));
                var m = bucketMask(h.B);
                b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
            }
            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (k.Value == key && b.tophash[i] != empty)
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8L + i * uintptr(t.valuesize)), true);
                        i = i + 1L;
                    k = add(k, 8L);
                        }
                b = b.overflow(t);
                    }

                }
            }

            return (@unsafe.Pointer(ref zeroVal[0L]), false);
        }

        private static unsafe.Pointer mapaccess1_faststr(ref maptype t, ref hmap h, @string ky)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess1_faststr));
            }
            if (h == null || h.count == 0L)
            {
                return @unsafe.Pointer(ref zeroVal[0L]);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            var key = stringStructOf(ref ky);
            if (h.B == 0L)
            { 
                // One-bucket table.
                var b = (bmap.Value)(h.buckets);
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
                            var k = (stringStruct.Value)(kptr);
                            if (k.len != key.len || b.tophash[i] == empty)
                            {
                                continue;
                            i = i + 1L;
                        kptr = add(kptr, 2L * sys.PtrSize);
                            }
                            if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                            {
                                return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize));
                            }
                        }


                        i = i__prev1;
                        kptr = kptr__prev1;
                    }
                    return @unsafe.Pointer(ref zeroVal[0L]);
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
                        k = (stringStruct.Value)(kptr);
                        if (k.len != key.len || b.tophash[i] == empty)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }
                        if (k.str == key.str)
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize));
                        } 
                        // check first 4 bytes
                        if ((new ptr<ref array<byte>>(key.str)) != (new ptr<ref array<byte>>(k.str)).Value.Value)
                        {
                            continue;
                        } 
                        // check last 4 bytes
                        if ((new ptr<ref array<byte>>(add(key.str, uintptr(key.len) - 4L))) != (new ptr<ref array<byte>>(add(k.str, uintptr(key.len) - 4L))).Value.Value)
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
                    k = (stringStruct.Value)(add(@unsafe.Pointer(b), dataOffset + keymaybe * 2L * sys.PtrSize));
                    if (memequal(k.str, key.str, uintptr(key.len)))
                    {
                        return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + keymaybe * uintptr(t.valuesize));
                    }
                }
                return @unsafe.Pointer(ref zeroVal[0L]);
            }
dohash:
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref ky)), uintptr(h.hash0));
            var m = bucketMask(h.B);
            b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
                {
                    var i__prev2 = i;
                    var kptr__prev2 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.Value)(kptr);
                        if (k.len != key.len || b.tophash[i] != top)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }
                        if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                        {
                            return add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize));
                b = b.overflow(t);
                        }
                    }


                    i = i__prev2;
                    kptr = kptr__prev2;
                }
            }

            return @unsafe.Pointer(ref zeroVal[0L]);
        }

        private static (unsafe.Pointer, bool) mapaccess2_faststr(ref maptype t, ref hmap h, @string ky)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racereadpc(@unsafe.Pointer(h), callerpc, funcPC(mapaccess2_faststr));
            }
            if (h == null || h.count == 0L)
            {
                return (@unsafe.Pointer(ref zeroVal[0L]), false);
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map read and map write");
            }
            var key = stringStructOf(ref ky);
            if (h.B == 0L)
            { 
                // One-bucket table.
                var b = (bmap.Value)(h.buckets);
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
                            var k = (stringStruct.Value)(kptr);
                            if (k.len != key.len || b.tophash[i] == empty)
                            {
                                continue;
                            i = i + 1L;
                        kptr = add(kptr, 2L * sys.PtrSize);
                            }
                            if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                            {
                                return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize)), true);
                            }
                        }


                        i = i__prev1;
                        kptr = kptr__prev1;
                    }
                    return (@unsafe.Pointer(ref zeroVal[0L]), false);
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
                        k = (stringStruct.Value)(kptr);
                        if (k.len != key.len || b.tophash[i] == empty)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }
                        if (k.str == key.str)
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize)), true);
                        } 
                        // check first 4 bytes
                        if ((new ptr<ref array<byte>>(key.str)) != (new ptr<ref array<byte>>(k.str)).Value.Value)
                        {
                            continue;
                        } 
                        // check last 4 bytes
                        if ((new ptr<ref array<byte>>(add(key.str, uintptr(key.len) - 4L))) != (new ptr<ref array<byte>>(add(k.str, uintptr(key.len) - 4L))).Value.Value)
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
                    k = (stringStruct.Value)(add(@unsafe.Pointer(b), dataOffset + keymaybe * 2L * sys.PtrSize));
                    if (memequal(k.str, key.str, uintptr(key.len)))
                    {
                        return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + keymaybe * uintptr(t.valuesize)), true);
                    }
                }
                return (@unsafe.Pointer(ref zeroVal[0L]), false);
            }
dohash:
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref ky)), uintptr(h.hash0));
            var m = bucketMask(h.B);
            b = (bmap.Value)(add(h.buckets, (hash & m) * uintptr(t.bucketsize)));
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
                {
                    var i__prev2 = i;
                    var kptr__prev2 = kptr;

                    i = uintptr(0L);
                    kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        k = (stringStruct.Value)(kptr);
                        if (k.len != key.len || b.tophash[i] != top)
                        {
                            continue;
                        i = i + 1L;
                    kptr = add(kptr, 2L * sys.PtrSize);
                        }
                        if (k.str == key.str || memequal(k.str, key.str, uintptr(key.len)))
                        {
                            return (add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize)), true);
                b = b.overflow(t);
                        }
                    }


                    i = i__prev2;
                    kptr = kptr__prev2;
                }
            }

            return (@unsafe.Pointer(ref zeroVal[0L]), false);
        }

        private static unsafe.Pointer mapassign_fast32(ref maptype _t, ref hmap _h, uint key) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast32));
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapassign.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast32(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));

            ref bmap insertb = default;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] == empty)
                    {
                        if (insertb == null)
                        {
                            inserti = i;
                            insertb = b;
                        }
                        continue;
                    }
                    var k = ((uint32.Value)(add(@unsafe.Pointer(b), dataOffset + i * 4L))).Value;
                    if (k != key)
                    {
                        continue;
                    }
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
            insertb.tophash[inserti & (bucketCnt - 1L)] = tophash(hash); // mask inserti to avoid bounds checks

            insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 4L) * (uint32.Value)(insertk);

            key;

            h.count++;

done:
            var val = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 4L + inserti * uintptr(t.valuesize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            return val;
        });

        private static unsafe.Pointer mapassign_fast32ptr(ref maptype _t, ref hmap _h, unsafe.Pointer key) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast32));
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapassign.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast32(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));

            ref bmap insertb = default;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] == empty)
                    {
                        if (insertb == null)
                        {
                            inserti = i;
                            insertb = b;
                        }
                        continue;
                    }
                    var k = ((@unsafe.Pointer.Value)(add(@unsafe.Pointer(b), dataOffset + i * 4L))).Value;
                    if (k != key)
                    {
                        continue;
                    }
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
            insertb.tophash[inserti & (bucketCnt - 1L)] = tophash(hash); // mask inserti to avoid bounds checks

            insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 4L) * (@unsafe.Pointer.Value)(insertk);

            key;

            h.count++;

done:
            var val = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 4L + inserti * uintptr(t.valuesize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            return val;
        });

        private static unsafe.Pointer mapassign_fast64(ref maptype _t, ref hmap _h, ulong key) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast64));
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapassign.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast64(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));

            ref bmap insertb = default;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] == empty)
                    {
                        if (insertb == null)
                        {
                            insertb = b;
                            inserti = i;
                        }
                        continue;
                    }
                    var k = ((uint64.Value)(add(@unsafe.Pointer(b), dataOffset + i * 8L))).Value;
                    if (k != key)
                    {
                        continue;
                    }
                    insertb = b;
                    inserti = i;
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
            insertb.tophash[inserti & (bucketCnt - 1L)] = tophash(hash); // mask inserti to avoid bounds checks

            insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 8L) * (uint64.Value)(insertk);

            key;

            h.count++;

done:
            var val = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 8L + inserti * uintptr(t.valuesize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            return val;
        });

        private static unsafe.Pointer mapassign_fast64ptr(ref maptype _t, ref hmap _h, unsafe.Pointer key) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
            if (h == null)
            {
                panic(plainError("assignment to entry in nil map"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapassign_fast64));
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapassign.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast64(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));

            ref bmap insertb = default;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] == empty)
                    {
                        if (insertb == null)
                        {
                            insertb = b;
                            inserti = i;
                        }
                        continue;
                    }
                    var k = ((@unsafe.Pointer.Value)(add(@unsafe.Pointer(b), dataOffset + i * 8L))).Value;
                    if (k != key)
                    {
                        continue;
                    }
                    insertb = b;
                    inserti = i;
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
            insertb.tophash[inserti & (bucketCnt - 1L)] = tophash(hash); // mask inserti to avoid bounds checks

            insertk = add(@unsafe.Pointer(insertb), dataOffset + inserti * 8L) * (@unsafe.Pointer.Value)(insertk);

            key;

            h.count++;

done:
            var val = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 8L + inserti * uintptr(t.valuesize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            return val;
        });

        private static unsafe.Pointer mapassign_faststr(ref maptype _t, ref hmap _h, @string s) => func(_t, _h, (ref maptype t, ref hmap h, Defer _, Panic panic, Recover __) =>
        {
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
            var key = stringStructOf(ref s);
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref s)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapassign.
            h.flags |= hashWriting;

            if (h.buckets == null)
            {
                h.buckets = newobject(t.bucket); // newarray(t.bucket, 1)
            }
again:
            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_faststr(t, h, bucket);
            }
            var b = (bmap.Value)(@unsafe.Pointer(uintptr(h.buckets) + bucket * uintptr(t.bucketsize)));
            var top = tophash(hash);

            ref bmap insertb = default;
            System.UIntPtr inserti = default;
            unsafe.Pointer insertk = default;

            while (true)
            {
                for (var i = uintptr(0L); i < bucketCnt; i++)
                {
                    if (b.tophash[i] != top)
                    {
                        if (b.tophash[i] == empty && insertb == null)
                        {
                            insertb = b;
                            inserti = i;
                        }
                        continue;
                    }
                    var k = (stringStruct.Value)(add(@unsafe.Pointer(b), dataOffset + i * 2L * sys.PtrSize));
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
            ((stringStruct.Value)(insertk)).Value = key.Value;
            h.count++;

done:
            var val = add(@unsafe.Pointer(insertb), dataOffset + bucketCnt * 2L * sys.PtrSize + inserti * uintptr(t.valuesize));
            if (h.flags & hashWriting == 0L)
            {
                throw("concurrent map writes");
            }
            h.flags &= hashWriting;
            return val;
        });

        private static void mapdelete_fast32(ref maptype t, ref hmap h, uint key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapdelete_fast32));
            }
            if (h == null || h.count == 0L)
            {
                return;
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapdelete
            h.flags |= hashWriting;

            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast32(t, h, bucket);
            }
            var b = (bmap.Value)(add(h.buckets, bucket * uintptr(t.bucketsize)));
search:

            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (key != k.Value || b.tophash[i] == empty)
                        {
                            continue;
                        i = i + 1L;
                    k = add(k, 4L);
                        } 
                        // Only clear key if there are pointers in it.
                        if (t.key.kind & kindNoPointers == 0L)
                        {
                            memclrHasPointers(k, t.key.size);
                b = b.overflow(t);
                        } 
                        // Only clear value if there are pointers in it.
                        if (t.elem.kind & kindNoPointers == 0L)
                        {
                            var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * 4L + i * uintptr(t.valuesize));
                            memclrHasPointers(v, t.elem.size);
                        }
                        b.tophash[i] = empty;
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

        private static void mapdelete_fast64(ref maptype t, ref hmap h, ulong key)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapdelete_fast64));
            }
            if (h == null || h.count == 0L)
            {
                return;
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref key)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapdelete
            h.flags |= hashWriting;

            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_fast64(t, h, bucket);
            }
            var b = (bmap.Value)(add(h.buckets, bucket * uintptr(t.bucketsize)));
search:

            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var k = b.keys();

                    while (i < bucketCnt)
                    {
                        if (key != k.Value || b.tophash[i] == empty)
                        {
                            continue;
                        i = i + 1L;
                    k = add(k, 8L);
                        } 
                        // Only clear key if there are pointers in it.
                        if (t.key.kind & kindNoPointers == 0L)
                        {
                            memclrHasPointers(k, t.key.size);
                b = b.overflow(t);
                        } 
                        // Only clear value if there are pointers in it.
                        if (t.elem.kind & kindNoPointers == 0L)
                        {
                            var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * 8L + i * uintptr(t.valuesize));
                            memclrHasPointers(v, t.elem.size);
                        }
                        b.tophash[i] = empty;
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

        private static void mapdelete_faststr(ref maptype t, ref hmap h, @string ky)
        {
            if (raceenabled && h != null)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(h), callerpc, funcPC(mapdelete_faststr));
            }
            if (h == null || h.count == 0L)
            {
                return;
            }
            if (h.flags & hashWriting != 0L)
            {
                throw("concurrent map writes");
            }
            var key = stringStructOf(ref ky);
            var hash = t.key.alg.hash(noescape(@unsafe.Pointer(ref ky)), uintptr(h.hash0)); 

            // Set hashWriting after calling alg.hash for consistency with mapdelete
            h.flags |= hashWriting;

            var bucket = hash & bucketMask(h.B);
            if (h.growing())
            {
                growWork_faststr(t, h, bucket);
            }
            var b = (bmap.Value)(add(h.buckets, bucket * uintptr(t.bucketsize)));
            var top = tophash(hash);
search:

            while (b != null)
            {
                {
                    var i = uintptr(0L);
                    var kptr = b.keys();

                    while (i < bucketCnt)
                    {
                        var k = (stringStruct.Value)(kptr);
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
                        // Only clear value if there are pointers in it.
                        if (t.elem.kind & kindNoPointers == 0L)
                        {
                            var v = add(@unsafe.Pointer(b), dataOffset + bucketCnt * 2L * sys.PtrSize + i * uintptr(t.valuesize));
                            memclrHasPointers(v, t.elem.size);
                        }
                        b.tophash[i] = empty;
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

        private static void growWork_fast32(ref maptype t, ref hmap h, System.UIntPtr bucket)
        { 
            // make sure we evacuate the oldbucket corresponding
            // to the bucket we're about to use
            evacuate_fast32(t, h, bucket & h.oldbucketmask()); 

            // evacuate one more oldbucket to make progress on growing
            if (h.growing())
            {
                evacuate_fast32(t, h, h.nevacuate);
            }
        }

        private static void evacuate_fast32(ref maptype t, ref hmap h, System.UIntPtr oldbucket)
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
                x.v = add(x.k, bucketCnt * 4L);

                if (!h.sameSizeGrow())
                { 
                    // Only calculate y pointers if we're growing bigger.
                    // Otherwise GC can see bad pointers.
                    var y = ref xy[1L];
                    y.b = (bmap.Value)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
                    y.k = add(@unsafe.Pointer(y.b), dataOffset);
                    y.v = add(y.k, bucketCnt * 4L);
                }
                while (b != null)
                {
                    var k = add(@unsafe.Pointer(b), dataOffset);
                    var v = add(k, bucketCnt * 4L);
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
                        k = add(k, 4L);
                        v = add(v, uintptr(t.valuesize));
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
                                // to send this key/value to bucket x or bucket y).
                                var hash = t.key.alg.hash(k, uintptr(h.hash0));
                                if (hash & newbit != 0L)
                                {
                                    useY = 1L;
                                }
                            }
                            b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY, enforced in makemap
                            var dst = ref xy[useY]; // evacuation destination

                            if (dst.i == bucketCnt)
                            {
                                dst.b = h.newoverflow(t, dst.b);
                                dst.i = 0L;
                                dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                                dst.v = add(dst.k, bucketCnt * 4L);
                            }
                            dst.b.tophash[dst.i & (bucketCnt - 1L)] = top; // mask dst.i as an optimization, to avoid a bounds check

                            // Copy key.
                            if (sys.PtrSize == 4L && t.key.kind & kindNoPointers == 0L && writeBarrier.enabled)
                            {
                                writebarrierptr((uintptr.Value)(dst.k), k.Value);
                            }
                            else
                            {
                                (uint32.Value)(dst.k).Value;

                                k.Value;
                            }
                            typedmemmove(t.elem, dst.v, v);
                            dst.i++; 
                            // These updates might push these pointers past the end of the
                            // key or value arrays.  That's ok, as we have the overflow pointer
                            // at the end of the bucket to protect against pointing past the
                            // end of the bucket.
                            dst.k = add(dst.k, 4L);
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

        private static void growWork_fast64(ref maptype t, ref hmap h, System.UIntPtr bucket)
        { 
            // make sure we evacuate the oldbucket corresponding
            // to the bucket we're about to use
            evacuate_fast64(t, h, bucket & h.oldbucketmask()); 

            // evacuate one more oldbucket to make progress on growing
            if (h.growing())
            {
                evacuate_fast64(t, h, h.nevacuate);
            }
        }

        private static void evacuate_fast64(ref maptype t, ref hmap h, System.UIntPtr oldbucket)
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
                x.v = add(x.k, bucketCnt * 8L);

                if (!h.sameSizeGrow())
                { 
                    // Only calculate y pointers if we're growing bigger.
                    // Otherwise GC can see bad pointers.
                    var y = ref xy[1L];
                    y.b = (bmap.Value)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
                    y.k = add(@unsafe.Pointer(y.b), dataOffset);
                    y.v = add(y.k, bucketCnt * 8L);
                }
                while (b != null)
                {
                    var k = add(@unsafe.Pointer(b), dataOffset);
                    var v = add(k, bucketCnt * 8L);
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
                        k = add(k, 8L);
                        v = add(v, uintptr(t.valuesize));
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
                                // to send this key/value to bucket x or bucket y).
                                var hash = t.key.alg.hash(k, uintptr(h.hash0));
                                if (hash & newbit != 0L)
                                {
                                    useY = 1L;
                                }
                            }
                            b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY, enforced in makemap
                            var dst = ref xy[useY]; // evacuation destination

                            if (dst.i == bucketCnt)
                            {
                                dst.b = h.newoverflow(t, dst.b);
                                dst.i = 0L;
                                dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                                dst.v = add(dst.k, bucketCnt * 8L);
                            }
                            dst.b.tophash[dst.i & (bucketCnt - 1L)] = top; // mask dst.i as an optimization, to avoid a bounds check

                            // Copy key.
                            if (t.key.kind & kindNoPointers == 0L && writeBarrier.enabled)
                            {
                                if (sys.PtrSize == 8L)
                                {
                                    writebarrierptr((uintptr.Value)(dst.k), k.Value);
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
                                (uint64.Value)(dst.k).Value;

                                k.Value;
                            }
                            typedmemmove(t.elem, dst.v, v);
                            dst.i++; 
                            // These updates might push these pointers past the end of the
                            // key or value arrays.  That's ok, as we have the overflow pointer
                            // at the end of the bucket to protect against pointing past the
                            // end of the bucket.
                            dst.k = add(dst.k, 8L);
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

        private static void growWork_faststr(ref maptype t, ref hmap h, System.UIntPtr bucket)
        { 
            // make sure we evacuate the oldbucket corresponding
            // to the bucket we're about to use
            evacuate_faststr(t, h, bucket & h.oldbucketmask()); 

            // evacuate one more oldbucket to make progress on growing
            if (h.growing())
            {
                evacuate_faststr(t, h, h.nevacuate);
            }
        }

        private static void evacuate_faststr(ref maptype t, ref hmap h, System.UIntPtr oldbucket)
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
                x.v = add(x.k, bucketCnt * 2L * sys.PtrSize);

                if (!h.sameSizeGrow())
                { 
                    // Only calculate y pointers if we're growing bigger.
                    // Otherwise GC can see bad pointers.
                    var y = ref xy[1L];
                    y.b = (bmap.Value)(add(h.buckets, (oldbucket + newbit) * uintptr(t.bucketsize)));
                    y.k = add(@unsafe.Pointer(y.b), dataOffset);
                    y.v = add(y.k, bucketCnt * 2L * sys.PtrSize);
                }
                while (b != null)
                {
                    var k = add(@unsafe.Pointer(b), dataOffset);
                    var v = add(k, bucketCnt * 2L * sys.PtrSize);
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
                        k = add(k, 2L * sys.PtrSize);
                        v = add(v, uintptr(t.valuesize));
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
                                // to send this key/value to bucket x or bucket y).
                                var hash = t.key.alg.hash(k, uintptr(h.hash0));
                                if (hash & newbit != 0L)
                                {
                                    useY = 1L;
                                }
                            }
                            b.tophash[i] = evacuatedX + useY; // evacuatedX + 1 == evacuatedY, enforced in makemap
                            var dst = ref xy[useY]; // evacuation destination

                            if (dst.i == bucketCnt)
                            {
                                dst.b = h.newoverflow(t, dst.b);
                                dst.i = 0L;
                                dst.k = add(@unsafe.Pointer(dst.b), dataOffset);
                                dst.v = add(dst.k, bucketCnt * 2L * sys.PtrSize);
                            }
                            dst.b.tophash[dst.i & (bucketCnt - 1L)] = top * (string.Value)(dst.k);

                            k.Value;

                            typedmemmove(t.elem, dst.v, v);
                            dst.i++; 
                            // These updates might push these pointers past the end of the
                            // key or value arrays.  That's ok, as we have the overflow pointer
                            // at the end of the bucket to protect against pointing past the
                            // end of the bucket.
                            dst.k = add(dst.k, 2L * sys.PtrSize);
                            dst.v = add(dst.v, uintptr(t.valuesize));
                        }

                    }
                } 
                // Unlink the overflow buckets & clear key/value to help GC.
                // Unlink the overflow buckets & clear key/value to help GC.
 
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
    }
}
