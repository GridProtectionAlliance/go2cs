// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:46 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\slice.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private partial struct slice
        {
            public unsafe.Pointer array;
            public long len;
            public long cap;
        }

        // An notInHeapSlice is a slice backed by go:notinheap memory.
        private partial struct notInHeapSlice
        {
            public ptr<notInHeap> array;
            public long len;
            public long cap;
        }

        // maxElems is a lookup table containing the maximum capacity for a slice.
        // The index is the size of the slice element.
        private static array<System.UIntPtr> maxElems = new array<System.UIntPtr>(new System.UIntPtr[] { ^uintptr(0), _MaxMem/1, _MaxMem/2, _MaxMem/3, _MaxMem/4, _MaxMem/5, _MaxMem/6, _MaxMem/7, _MaxMem/8, _MaxMem/9, _MaxMem/10, _MaxMem/11, _MaxMem/12, _MaxMem/13, _MaxMem/14, _MaxMem/15, _MaxMem/16, _MaxMem/17, _MaxMem/18, _MaxMem/19, _MaxMem/20, _MaxMem/21, _MaxMem/22, _MaxMem/23, _MaxMem/24, _MaxMem/25, _MaxMem/26, _MaxMem/27, _MaxMem/28, _MaxMem/29, _MaxMem/30, _MaxMem/31, _MaxMem/32 });

        // maxSliceCap returns the maximum capacity for a slice.
        private static System.UIntPtr maxSliceCap(System.UIntPtr elemsize)
        {
            if (elemsize < uintptr(len(maxElems)))
            {
                return maxElems[elemsize];
            }
            return _MaxMem / elemsize;
        }

        private static slice makeslice(ref _type _et, long len, long cap) => func(_et, (ref _type et, Defer _, Panic panic, Recover __) =>
        { 
            // NOTE: The len > maxElements check here is not strictly necessary,
            // but it produces a 'len out of range' error instead of a 'cap out of range' error
            // when someone does make([]T, bignumber). 'cap out of range' is true too,
            // but since the cap is only being supplied implicitly, saying len is clearer.
            // See issue 4085.
            var maxElements = maxSliceCap(et.size);
            if (len < 0L || uintptr(len) > maxElements)
            {
                panic(errorString("makeslice: len out of range"));
            }
            if (cap < len || uintptr(cap) > maxElements)
            {
                panic(errorString("makeslice: cap out of range"));
            }
            var p = mallocgc(et.size * uintptr(cap), et, true);
            return new slice(p,len,cap);
        });

        private static slice makeslice64(ref _type _et, long len64, long cap64) => func(_et, (ref _type et, Defer _, Panic panic, Recover __) =>
        {
            var len = int(len64);
            if (int64(len) != len64)
            {
                panic(errorString("makeslice: len out of range"));
            }
            var cap = int(cap64);
            if (int64(cap) != cap64)
            {
                panic(errorString("makeslice: cap out of range"));
            }
            return makeslice(et, len, cap);
        });

        // growslice handles slice growth during append.
        // It is passed the slice element type, the old slice, and the desired new minimum capacity,
        // and it returns a new slice with at least that capacity, with the old data
        // copied into it.
        // The new slice's length is set to the old slice's length,
        // NOT to the new requested capacity.
        // This is for codegen convenience. The old slice's length is used immediately
        // to calculate where to write new values during an append.
        // TODO: When the old backend is gone, reconsider this decision.
        // The SSA backend might prefer the new length or to return only ptr/cap and save stack space.
        private static slice growslice(ref _type _et, slice old, long cap) => func(_et, (ref _type et, Defer _, Panic panic, Recover __) =>
        {
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racereadrangepc(old.array, uintptr(old.len * int(et.size)), callerpc, funcPC(growslice));
            }
            if (msanenabled)
            {
                msanread(old.array, uintptr(old.len * int(et.size)));
            }
            if (et.size == 0L)
            {
                if (cap < old.cap)
                {
                    panic(errorString("growslice: cap out of range"));
                } 
                // append should not create a slice with nil pointer but non-zero len.
                // We assume that append doesn't need to preserve old.array in this case.
                return new slice(unsafe.Pointer(&zerobase),old.len,cap);
            }
            var newcap = old.cap;
            var doublecap = newcap + newcap;
            if (cap > doublecap)
            {
                newcap = cap;
            }
            else
            {
                if (old.len < 1024L)
                {
                    newcap = doublecap;
                }
                else
                { 
                    // Check 0 < newcap to detect overflow
                    // and prevent an infinite loop.
                    while (0L < newcap && newcap < cap)
                    {
                        newcap += newcap / 4L;
                    } 
                    // Set newcap to the requested cap when
                    // the newcap calculation overflowed.
 
                    // Set newcap to the requested cap when
                    // the newcap calculation overflowed.
                    if (newcap <= 0L)
                    {
                        newcap = cap;
                    }
                }
            }
            bool overflow = default;
            System.UIntPtr lenmem = default;            System.UIntPtr newlenmem = default;            System.UIntPtr capmem = default;

            const var ptrSize = @unsafe.Sizeof((byte.Value)(null));


            if (et.size == 1L) 
                lenmem = uintptr(old.len);
                newlenmem = uintptr(cap);
                capmem = roundupsize(uintptr(newcap));
                overflow = uintptr(newcap) > _MaxMem;
                newcap = int(capmem);
            else if (et.size == ptrSize) 
                lenmem = uintptr(old.len) * ptrSize;
                newlenmem = uintptr(cap) * ptrSize;
                capmem = roundupsize(uintptr(newcap) * ptrSize);
                overflow = uintptr(newcap) > _MaxMem / ptrSize;
                newcap = int(capmem / ptrSize);
            else 
                lenmem = uintptr(old.len) * et.size;
                newlenmem = uintptr(cap) * et.size;
                capmem = roundupsize(uintptr(newcap) * et.size);
                overflow = uintptr(newcap) > maxSliceCap(et.size);
                newcap = int(capmem / et.size);
            // The check of overflow (uintptr(newcap) > maxSliceCap(et.size))
            // in addition to capmem > _MaxMem is needed to prevent an overflow
            // which can be used to trigger a segfault on 32bit architectures
            // with this example program:
            //
            // type T [1<<27 + 1]int64
            //
            // var d T
            // var s []T
            //
            // func main() {
            //   s = append(s, d, d, d, d)
            //   print(len(s), "\n")
            // }
            if (cap < old.cap || overflow || capmem > _MaxMem)
            {
                panic(errorString("growslice: cap out of range"));
            }
            unsafe.Pointer p = default;
            if (et.kind & kindNoPointers != 0L)
            {
                p = mallocgc(capmem, null, false);
                memmove(p, old.array, lenmem); 
                // The append() that calls growslice is going to overwrite from old.len to cap (which will be the new length).
                // Only clear the part that will not be overwritten.
                memclrNoHeapPointers(add(p, newlenmem), capmem - newlenmem);
            }
            else
            { 
                // Note: can't use rawmem (which avoids zeroing of memory), because then GC can scan uninitialized memory.
                p = mallocgc(capmem, et, true);
                if (!writeBarrier.enabled)
                {
                    memmove(p, old.array, lenmem);
                }
                else
                {
                    {
                        var i = uintptr(0L);

                        while (i < lenmem)
                        {
                            typedmemmove(et, add(p, i), add(old.array, i));
                            i += et.size;
                        }

                    }
                }
            }
            return new slice(p,old.len,newcap);
        });

        private static long slicecopy(slice to, slice fm, System.UIntPtr width)
        {
            if (fm.len == 0L || to.len == 0L)
            {
                return 0L;
            }
            var n = fm.len;
            if (to.len < n)
            {
                n = to.len;
            }
            if (width == 0L)
            {
                return n;
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(slicecopy);
                racewriterangepc(to.array, uintptr(n * int(width)), callerpc, pc);
                racereadrangepc(fm.array, uintptr(n * int(width)), callerpc, pc);
            }
            if (msanenabled)
            {
                msanwrite(to.array, uintptr(n * int(width)));
                msanread(fm.array, uintptr(n * int(width)));
            }
            var size = uintptr(n) * width;
            if (size == 1L)
            { // common case worth about 2x to do here
                // TODO: is this still worth it with new memmove impl?
                (byte.Value)(to.array).Value;

                fm.array.Value; // known to be a byte pointer
            }
            else
            {
                memmove(to.array, fm.array, size);
            }
            return n;
        }

        private static long slicestringcopy(slice<byte> to, @string fm)
        {
            if (len(fm) == 0L || len(to) == 0L)
            {
                return 0L;
            }
            var n = len(fm);
            if (len(to) < n)
            {
                n = len(to);
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(slicestringcopy);
                racewriterangepc(@unsafe.Pointer(ref to[0L]), uintptr(n), callerpc, pc);
            }
            if (msanenabled)
            {
                msanwrite(@unsafe.Pointer(ref to[0L]), uintptr(n));
            }
            memmove(@unsafe.Pointer(ref to[0L]), stringStructOf(ref fm).str, uintptr(n));
            return n;
        }
    }
}
