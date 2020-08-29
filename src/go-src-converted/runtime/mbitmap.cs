// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: type and heap bitmaps.
//
// Stack, data, and bss bitmaps
//
// Stack frames and global variables in the data and bss sections are described
// by 1-bit bitmaps in which 0 means uninteresting and 1 means live pointer
// to be visited during GC. The bits in each byte are consumed starting with
// the low bit: 1<<0, 1<<1, and so on.
//
// Heap bitmap
//
// The allocated heap comes from a subset of the memory in the range [start, used),
// where start == mheap_.arena_start and used == mheap_.arena_used.
// The heap bitmap comprises 2 bits for each pointer-sized word in that range,
// stored in bytes indexed backward in memory from start.
// That is, the byte at address start-1 holds the 2-bit entries for the four words
// start through start+3*ptrSize, the byte at start-2 holds the entries for
// start+4*ptrSize through start+7*ptrSize, and so on.
//
// In each 2-bit entry, the lower bit holds the same information as in the 1-bit
// bitmaps: 0 means uninteresting and 1 means live pointer to be visited during GC.
// The meaning of the high bit depends on the position of the word being described
// in its allocated object. In all words *except* the second word, the
// high bit indicates that the object is still being described. In
// these words, if a bit pair with a high bit 0 is encountered, the
// low bit can also be assumed to be 0, and the object description is
// over. This 00 is called the ``dead'' encoding: it signals that the
// rest of the words in the object are uninteresting to the garbage
// collector.
//
// In the second word, the high bit is the GC ``checkmarked'' bit (see below).
//
// The 2-bit entries are split when written into the byte, so that the top half
// of the byte contains 4 high bits and the bottom half contains 4 low (pointer)
// bits.
// This form allows a copy from the 1-bit to the 4-bit form to keep the
// pointer bits contiguous, instead of having to space them out.
//
// The code makes use of the fact that the zero value for a heap bitmap
// has no live pointer bit set and is (depending on position), not used,
// not checkmarked, and is the dead encoding.
// These properties must be preserved when modifying the encoding.
//
// The bitmap for noscan spans is not maintained. Code must ensure
// that an object is scannable before consulting its bitmap by
// checking either the noscan bit in the span or by consulting its
// type's information.
//
// Checkmarks
//
// In a concurrent garbage collector, one worries about failing to mark
// a live object due to mutations without write barriers or bugs in the
// collector implementation. As a sanity check, the GC has a 'checkmark'
// mode that retraverses the object graph with the world stopped, to make
// sure that everything that should be marked is marked.
// In checkmark mode, in the heap bitmap, the high bit of the 2-bit entry
// for the second word of the object holds the checkmark bit.
// When not in checkmark mode, this bit is set to 1.
//
// The smallest possible allocation is 8 bytes. On a 32-bit machine, that
// means every allocated object has two words, so there is room for the
// checkmark bit. On a 64-bit machine, however, the 8-byte allocation is
// just one word, so the second bit pair is not available for encoding the
// checkmark. However, because non-pointer allocations are combined
// into larger 16-byte (maxTinySize) allocations, a plain 8-byte allocation
// must be a pointer, so the type bit in the first word is not actually needed.
// It is still used in general, except in checkmark the type bit is repurposed
// as the checkmark bit and then reinitialized (to 1) as the type bit when
// finished.
//

// package runtime -- go2cs converted at 2020 August 29 08:17:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mbitmap.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long bitPointer = 1L << (int)(0L);
        private static readonly long bitScan = 1L << (int)(4L);

        private static readonly long heapBitsShift = 1L; // shift offset between successive bitPointer or bitScan entries
        private static readonly var heapBitmapScale = sys.PtrSize * (8L / 2L); // number of data bytes described by one heap bitmap byte

        // all scan/pointer bits in a byte
        private static readonly var bitScanAll = bitScan | bitScan << (int)(heapBitsShift) | bitScan << (int)((2L * heapBitsShift)) | bitScan << (int)((3L * heapBitsShift));
        private static readonly var bitPointerAll = bitPointer | bitPointer << (int)(heapBitsShift) | bitPointer << (int)((2L * heapBitsShift)) | bitPointer << (int)((3L * heapBitsShift));

        // addb returns the byte pointer p+n.
        //go:nowritebarrier
        //go:nosplit
        private static ref byte addb(ref byte p, System.UIntPtr n)
        { 
            // Note: wrote out full expression instead of calling add(p, n)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return (byte.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + n));
        }

        // subtractb returns the byte pointer p-n.
        // subtractb is typically used when traversing the pointer tables referred to by hbits
        // which are arranged in reverse order.
        //go:nowritebarrier
        //go:nosplit
        private static ref byte subtractb(ref byte p, System.UIntPtr n)
        { 
            // Note: wrote out full expression instead of calling add(p, -n)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return (byte.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) - n));
        }

        // add1 returns the byte pointer p+1.
        //go:nowritebarrier
        //go:nosplit
        private static ref byte add1(ref byte p)
        { 
            // Note: wrote out full expression instead of calling addb(p, 1)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return (byte.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + 1L));
        }

        // subtract1 returns the byte pointer p-1.
        // subtract1 is typically used when traversing the pointer tables referred to by hbits
        // which are arranged in reverse order.
        //go:nowritebarrier
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static ref byte subtract1(ref byte p)
        { 
            // Note: wrote out full expression instead of calling subtractb(p, 1)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return (byte.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) - 1L));
        }

        // mapBits maps any additional bitmap memory needed for the new arena memory.
        //
        // Don't call this directly. Call mheap.setArenaUsed.
        //
        //go:nowritebarrier
        private static void mapBits(this ref mheap h, System.UIntPtr arena_used)
        { 
            // Caller has added extra mappings to the arena.
            // Add extra mappings of bitmap words as needed.
            // We allocate extra bitmap pieces in chunks of bitmapChunk.
            const long bitmapChunk = 8192L;



            var n = (arena_used - mheap_.arena_start) / heapBitmapScale;
            n = round(n, bitmapChunk);
            n = round(n, physPageSize);
            if (h.bitmap_mapped >= n)
            {
                return;
            }
            sysMap(@unsafe.Pointer(h.bitmap - n), n - h.bitmap_mapped, h.arena_reserved, ref memstats.gc_sys);
            h.bitmap_mapped = n;
        }

        // heapBits provides access to the bitmap bits for a single heap word.
        // The methods on heapBits take value receivers so that the compiler
        // can more easily inline calls to those methods and registerize the
        // struct fields independently.
        private partial struct heapBits
        {
            public ptr<byte> bitp;
            public uint shift;
        }

        // markBits provides access to the mark bit for an object in the heap.
        // bytep points to the byte holding the mark bit.
        // mask is a byte with a single bit set that can be &ed with *bytep
        // to see if the bit has been set.
        // *m.byte&m.mask != 0 indicates the mark bit is set.
        // index can be used along with span information to generate
        // the address of the object in the heap.
        // We maintain one set of mark bits for allocation and one for
        // marking purposes.
        private partial struct markBits
        {
            public ptr<byte> bytep;
            public byte mask;
            public System.UIntPtr index;
        }

        //go:nosplit
        private static markBits allocBitsForIndex(this ref mspan s, System.UIntPtr allocBitIndex)
        {
            var (bytep, mask) = s.allocBits.bitp(allocBitIndex);
            return new markBits(bytep,mask,allocBitIndex);
        }

        // refillaCache takes 8 bytes s.allocBits starting at whichByte
        // and negates them so that ctz (count trailing zeros) instructions
        // can be used. It then places these 8 bytes into the cached 64 bit
        // s.allocCache.
        private static void refillAllocCache(this ref mspan s, System.UIntPtr whichByte)
        {
            ref array<byte> bytes = new ptr<ref array<byte>>(@unsafe.Pointer(s.allocBits.bytep(whichByte)));
            var aCache = uint64(0L);
            aCache |= uint64(bytes[0L]);
            aCache |= uint64(bytes[1L]) << (int)((1L * 8L));
            aCache |= uint64(bytes[2L]) << (int)((2L * 8L));
            aCache |= uint64(bytes[3L]) << (int)((3L * 8L));
            aCache |= uint64(bytes[4L]) << (int)((4L * 8L));
            aCache |= uint64(bytes[5L]) << (int)((5L * 8L));
            aCache |= uint64(bytes[6L]) << (int)((6L * 8L));
            aCache |= uint64(bytes[7L]) << (int)((7L * 8L));
            s.allocCache = ~aCache;
        }

        // nextFreeIndex returns the index of the next free object in s at
        // or after s.freeindex.
        // There are hardware instructions that can be used to make this
        // faster if profiling warrants it.
        private static System.UIntPtr nextFreeIndex(this ref mspan s)
        {
            var sfreeindex = s.freeindex;
            var snelems = s.nelems;
            if (sfreeindex == snelems)
            {
                return sfreeindex;
            }
            if (sfreeindex > snelems)
            {
                throw("s.freeindex > s.nelems");
            }
            var aCache = s.allocCache;

            var bitIndex = sys.Ctz64(aCache);
            while (bitIndex == 64L)
            { 
                // Move index to start of next cached bits.
                sfreeindex = (sfreeindex + 64L) & ~(64L - 1L);
                if (sfreeindex >= snelems)
                {
                    s.freeindex = snelems;
                    return snelems;
                }
                var whichByte = sfreeindex / 8L; 
                // Refill s.allocCache with the next 64 alloc bits.
                s.refillAllocCache(whichByte);
                aCache = s.allocCache;
                bitIndex = sys.Ctz64(aCache); 
                // nothing available in cached bits
                // grab the next 8 bytes and try again.
            }

            var result = sfreeindex + uintptr(bitIndex);
            if (result >= snelems)
            {
                s.freeindex = snelems;
                return snelems;
            }
            s.allocCache >>= uint(bitIndex + 1L);
            sfreeindex = result + 1L;

            if (sfreeindex % 64L == 0L && sfreeindex != snelems)
            { 
                // We just incremented s.freeindex so it isn't 0.
                // As each 1 in s.allocCache was encountered and used for allocation
                // it was shifted away. At this point s.allocCache contains all 0s.
                // Refill s.allocCache so that it corresponds
                // to the bits at s.allocBits starting at s.freeindex.
                whichByte = sfreeindex / 8L;
                s.refillAllocCache(whichByte);
            }
            s.freeindex = sfreeindex;
            return result;
        }

        // isFree returns whether the index'th object in s is unallocated.
        private static bool isFree(this ref mspan s, System.UIntPtr index)
        {
            if (index < s.freeindex)
            {
                return false;
            }
            var (bytep, mask) = s.allocBits.bitp(index);
            return bytep & mask == 0L.Value;
        }

        private static System.UIntPtr objIndex(this ref mspan s, System.UIntPtr p)
        {
            var byteOffset = p - s.@base();
            if (byteOffset == 0L)
            {
                return 0L;
            }
            if (s.baseMask != 0L)
            { 
                // s.baseMask is 0, elemsize is a power of two, so shift by s.divShift
                return byteOffset >> (int)(s.divShift);
            }
            return uintptr(((uint64(byteOffset) >> (int)(s.divShift)) * uint64(s.divMul)) >> (int)(s.divShift2));
        }

        private static markBits markBitsForAddr(System.UIntPtr p)
        {
            var s = spanOf(p);
            var objIndex = s.objIndex(p);
            return s.markBitsForIndex(objIndex);
        }

        private static markBits markBitsForIndex(this ref mspan s, System.UIntPtr objIndex)
        {
            var (bytep, mask) = s.gcmarkBits.bitp(objIndex);
            return new markBits(bytep,mask,objIndex);
        }

        private static markBits markBitsForBase(this ref mspan s)
        {
            return new markBits((*uint8)(s.gcmarkBits),uint8(1),0);
        }

        // isMarked reports whether mark bit m is set.
        private static bool isMarked(this markBits m)
        {
            return m.bytep & m.mask != 0L.Value;
        }

        // setMarked sets the marked bit in the markbits, atomically. Some compilers
        // are not able to inline atomic.Or8 function so if it appears as a hot spot consider
        // inlining it manually.
        private static void setMarked(this markBits m)
        { 
            // Might be racing with other updates, so use atomic update always.
            // We used to be clever here and use a non-atomic update in certain
            // cases, but it's not worth the risk.
            atomic.Or8(m.bytep, m.mask);
        }

        // setMarkedNonAtomic sets the marked bit in the markbits, non-atomically.
        private static void setMarkedNonAtomic(this markBits m)
        {
            m.bytep.Value |= m.mask;
        }

        // clearMarked clears the marked bit in the markbits, atomically.
        private static void clearMarked(this markBits m)
        { 
            // Might be racing with other updates, so use atomic update always.
            // We used to be clever here and use a non-atomic update in certain
            // cases, but it's not worth the risk.
            atomic.And8(m.bytep, ~m.mask);
        }

        // markBitsForSpan returns the markBits for the span base address base.
        private static markBits markBitsForSpan(System.UIntPtr @base)
        {
            if (base < mheap_.arena_start || base >= mheap_.arena_used)
            {
                throw("markBitsForSpan: base out of range");
            }
            mbits = markBitsForAddr(base);
            if (mbits.mask != 1L)
            {
                throw("markBitsForSpan: unaligned start");
            }
            return mbits;
        }

        // advance advances the markBits to the next object in the span.
        private static void advance(this ref markBits m)
        {
            if (m.mask == 1L << (int)(7L))
            {
                m.bytep = (uint8.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(m.bytep)) + 1L));
                m.mask = 1L;
            }
            else
            {
                m.mask = m.mask << (int)(1L);
            }
            m.index++;
        }

        // heapBitsForAddr returns the heapBits for the address addr.
        // The caller must have already checked that addr is in the range [mheap_.arena_start, mheap_.arena_used).
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static heapBits heapBitsForAddr(System.UIntPtr addr)
        { 
            // 2 bits per work, 4 pairs per byte, and a mask is hard coded.
            var off = (addr - mheap_.arena_start) / sys.PtrSize;
            return new heapBits((*uint8)(unsafe.Pointer(mheap_.bitmap-off/4-1)),uint32(off&3));
        }

        // heapBitsForSpan returns the heapBits for the span base address base.
        private static heapBits heapBitsForSpan(System.UIntPtr @base)
        {
            if (base < mheap_.arena_start || base >= mheap_.arena_used)
            {
                print("runtime: base ", hex(base), " not in range [", hex(mheap_.arena_start), ",", hex(mheap_.arena_used), ")\n");
                throw("heapBitsForSpan: base out of range");
            }
            return heapBitsForAddr(base);
        }

        // heapBitsForObject returns the base address for the heap object
        // containing the address p, the heapBits for base,
        // the object's span, and of the index of the object in s.
        // If p does not point into a heap object,
        // return base == 0
        // otherwise return the base of the object.
        //
        // refBase and refOff optionally give the base address of the object
        // in which the pointer p was found and the byte offset at which it
        // was found. These are used for error reporting.
        private static (System.UIntPtr, heapBits, ref mspan, System.UIntPtr) heapBitsForObject(System.UIntPtr p, System.UIntPtr refBase, System.UIntPtr refOff)
        {
            var arenaStart = mheap_.arena_start;
            if (p < arenaStart || p >= mheap_.arena_used)
            {
                return;
            }
            var off = p - arenaStart;
            var idx = off >> (int)(_PageShift); 
            // p points into the heap, but possibly to the middle of an object.
            // Consult the span table to find the block beginning.
            s = mheap_.spans[idx];
            if (s == null || p < s.@base() || p >= s.limit || s.state != mSpanInUse)
            {
                if (s == null || s.state == _MSpanManual)
                { 
                    // If s is nil, the virtual address has never been part of the heap.
                    // This pointer may be to some mmap'd region, so we allow it.
                    // Pointers into stacks are also ok, the runtime manages these explicitly.
                    return;
                } 

                // The following ensures that we are rigorous about what data
                // structures hold valid pointers.
                if (debug.invalidptr != 0L)
                { 
                    // Typically this indicates an incorrect use
                    // of unsafe or cgo to store a bad pointer in
                    // the Go heap. It may also indicate a runtime
                    // bug.
                    //
                    // TODO(austin): We could be more aggressive
                    // and detect pointers to unallocated objects
                    // in allocated spans.
                    printlock();
                    print("runtime: pointer ", hex(p));
                    if (s.state != mSpanInUse)
                    {
                        print(" to unallocated span");
                    }
                    else
                    {
                        print(" to unused region of span");
                    }
                    print(" idx=", hex(idx), " span.base()=", hex(s.@base()), " span.limit=", hex(s.limit), " span.state=", s.state, "\n");
                    if (refBase != 0L)
                    {
                        print("runtime: found in object at *(", hex(refBase), "+", hex(refOff), ")\n");
                        gcDumpObject("object", refBase, refOff);
                    }
                    getg().m.traceback = 2L;
                    throw("found bad pointer in Go heap (incorrect use of unsafe or cgo?)");
                }
                return;
            } 
            // If this span holds object of a power of 2 size, just mask off the bits to
            // the interior of the object. Otherwise use the size to get the base.
            if (s.baseMask != 0L)
            { 
                // optimize for power of 2 sized objects.
                base = s.@base();
                base = base + (p - base) & uintptr(s.baseMask);
                objIndex = (base - s.@base()) >> (int)(s.divShift); 
                // base = p & s.baseMask is faster for small spans,
                // but doesn't work for large spans.
                // Overall, it's faster to use the more general computation above.
            }
            else
            {
                base = s.@base();
                if (p - base >= s.elemsize)
                { 
                    // n := (p - base) / s.elemsize, using division by multiplication
                    objIndex = uintptr(p - base) >> (int)(s.divShift) * uintptr(s.divMul) >> (int)(s.divShift2);
                    base += objIndex * s.elemsize;
                }
            } 
            // Now that we know the actual base, compute heapBits to return to caller.
            hbits = heapBitsForAddr(base);
            return;
        }

        // next returns the heapBits describing the next pointer-sized word in memory.
        // That is, if h describes address p, h.next() describes p+ptrSize.
        // Note that next does not modify h. The caller must record the result.
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static heapBits next(this heapBits h)
        {
            if (h.shift < 3L * heapBitsShift)
            {
                return new heapBits(h.bitp,h.shift+heapBitsShift);
            }
            return new heapBits(subtract1(h.bitp),0);
        }

        // forward returns the heapBits describing n pointer-sized words ahead of h in memory.
        // That is, if h describes address p, h.forward(n) describes p+n*ptrSize.
        // h.forward(1) is equivalent to h.next(), just slower.
        // Note that forward does not modify h. The caller must record the result.
        // bits returns the heap bits for the current word.
        private static heapBits forward(this heapBits h, System.UIntPtr n)
        {
            n += uintptr(h.shift) / heapBitsShift;
            return new heapBits(subtractb(h.bitp,n/4),uint32(n%4)*heapBitsShift);
        }

        // The caller can test morePointers and isPointer by &-ing with bitScan and bitPointer.
        // The result includes in its higher bits the bits for subsequent words
        // described by the same bitmap byte.
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static uint bits(this heapBits h)
        { 
            // The (shift & 31) eliminates a test and conditional branch
            // from the generated code.
            return uint32(h.bitp.Value) >> (int)((h.shift & 31L));
        }

        // morePointers returns true if this word and all remaining words in this object
        // are scalars.
        // h must not describe the second word of the object.
        private static bool morePointers(this heapBits h)
        {
            return h.bits() & bitScan != 0L;
        }

        // isPointer reports whether the heap bits describe a pointer word.
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static bool isPointer(this heapBits h)
        {
            return h.bits() & bitPointer != 0L;
        }

        // isCheckmarked reports whether the heap bits have the checkmarked bit set.
        // It must be told how large the object at h is, because the encoding of the
        // checkmark bit varies by size.
        // h must describe the initial word of the object.
        private static bool isCheckmarked(this heapBits h, System.UIntPtr size)
        {
            if (size == sys.PtrSize)
            {
                return (h.bitp >> (int)(h.shift).Value) & bitPointer != 0L;
            } 
            // All multiword objects are 2-word aligned,
            // so we know that the initial word's 2-bit pair
            // and the second word's 2-bit pair are in the
            // same heap bitmap byte, *h.bitp.
            return (h.bitp >> (int)((heapBitsShift + h.shift)).Value) & bitScan != 0L;
        }

        // setCheckmarked sets the checkmarked bit.
        // It must be told how large the object at h is, because the encoding of the
        // checkmark bit varies by size.
        // h must describe the initial word of the object.
        private static void setCheckmarked(this heapBits h, System.UIntPtr size)
        {
            if (size == sys.PtrSize)
            {
                atomic.Or8(h.bitp, bitPointer << (int)(h.shift));
                return;
            }
            atomic.Or8(h.bitp, bitScan << (int)((heapBitsShift + h.shift)));
        }

        // bulkBarrierPreWrite executes a write barrier
        // for every pointer slot in the memory range [src, src+size),
        // using pointer/scalar information from [dst, dst+size).
        // This executes the write barriers necessary before a memmove.
        // src, dst, and size must be pointer-aligned.
        // The range [dst, dst+size) must lie within a single object.
        // It does not perform the actual writes.
        //
        // As a special case, src == 0 indicates that this is being used for a
        // memclr. bulkBarrierPreWrite will pass 0 for the src of each write
        // barrier.
        //
        // Callers should call bulkBarrierPreWrite immediately before
        // calling memmove(dst, src, size). This function is marked nosplit
        // to avoid being preempted; the GC must not stop the goroutine
        // between the memmove and the execution of the barriers.
        // The caller is also responsible for cgo pointer checks if this
        // may be writing Go pointers into non-Go memory.
        //
        // The pointer bitmap is not maintained for allocations containing
        // no pointers at all; any caller of bulkBarrierPreWrite must first
        // make sure the underlying allocation contains pointers, usually
        // by checking typ.kind&kindNoPointers.
        //
        //go:nosplit
        private static void bulkBarrierPreWrite(System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size)
        {
            if ((dst | src | size) & (sys.PtrSize - 1L) != 0L)
            {
                throw("bulkBarrierPreWrite: unaligned arguments");
            }
            if (!writeBarrier.needed)
            {
                return;
            }
            if (!inheap(dst))
            {
                var gp = getg().m.curg;
                if (gp != null && gp.stack.lo <= dst && dst < gp.stack.hi)
                { 
                    // Destination is our own stack. No need for barriers.
                    return;
                } 

                // If dst is a global, use the data or BSS bitmaps to
                // execute write barriers.
                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        if (datap.data <= dst && dst < datap.edata)
                        {
                            bulkBarrierBitmap(dst, src, size, dst - datap.data, datap.gcdatamask.bytedata);
                            return;
                        }
                    }

                    datap = datap__prev1;
                }

                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        if (datap.bss <= dst && dst < datap.ebss)
                        {
                            bulkBarrierBitmap(dst, src, size, dst - datap.bss, datap.gcbssmask.bytedata);
                            return;
                        }
                    }

                    datap = datap__prev1;
                }

                return;
            }
            var buf = ref getg().m.p.ptr().wbBuf;
            var h = heapBitsForAddr(dst);
            if (src == 0L)
            {
                {
                    var i__prev1 = i;

                    var i = uintptr(0L);

                    while (i < size)
                    {
                        if (h.isPointer())
                        {
                            var dstx = (uintptr.Value)(@unsafe.Pointer(dst + i));
                            if (!buf.putFast(dstx.Value, 0L))
                            {
                                wbBufFlush(null, 0L);
                        i += sys.PtrSize;
                            }
                        }
                        h = h.next();
                    }
            else


                    i = i__prev1;
                }
            }            {
                {
                    var i__prev1 = i;

                    i = uintptr(0L);

                    while (i < size)
                    {
                        if (h.isPointer())
                        {
                            dstx = (uintptr.Value)(@unsafe.Pointer(dst + i));
                            var srcx = (uintptr.Value)(@unsafe.Pointer(src + i));
                            if (!buf.putFast(dstx.Value, srcx.Value))
                            {
                                wbBufFlush(null, 0L);
                        i += sys.PtrSize;
                            }
                        }
                        h = h.next();
                    }


                    i = i__prev1;
                }
            }
        }

        // bulkBarrierBitmap executes write barriers for copying from [src,
        // src+size) to [dst, dst+size) using a 1-bit pointer bitmap. src is
        // assumed to start maskOffset bytes into the data covered by the
        // bitmap in bits (which may not be a multiple of 8).
        //
        // This is used by bulkBarrierPreWrite for writes to data and BSS.
        //
        //go:nosplit
        private static void bulkBarrierBitmap(System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size, System.UIntPtr maskOffset, ref byte bits)
        {
            var word = maskOffset / sys.PtrSize;
            bits = addb(bits, word / 8L);
            var mask = uint8(1L) << (int)((word % 8L));

            var buf = ref getg().m.p.ptr().wbBuf;
            {
                var i = uintptr(0L);

                while (i < size)
                {
                    if (mask == 0L)
                    {
                        bits = addb(bits, 1L);
                        if (bits == 0L.Value)
                        { 
                            // Skip 8 words.
                            i += 7L * sys.PtrSize;
                            continue;
                    i += sys.PtrSize;
                        }
                        mask = 1L;
                    }
                    if (bits & mask != 0L.Value)
                    {
                        var dstx = (uintptr.Value)(@unsafe.Pointer(dst + i));
                        if (src == 0L)
                        {
                            if (!buf.putFast(dstx.Value, 0L))
                            {
                                wbBufFlush(null, 0L);
                            }
                        }
                        else
                        {
                            var srcx = (uintptr.Value)(@unsafe.Pointer(src + i));
                            if (!buf.putFast(dstx.Value, srcx.Value))
                            {
                                wbBufFlush(null, 0L);
                            }
                        }
                    }
                    mask <<= 1L;
                }

            }
        }

        // typeBitsBulkBarrier executes writebarrierptr_prewrite for every
        // pointer that would be copied from [src, src+size) to [dst,
        // dst+size) by a memmove using the type bitmap to locate those
        // pointer slots.
        //
        // The type typ must correspond exactly to [src, src+size) and [dst, dst+size).
        // dst, src, and size must be pointer-aligned.
        // The type typ must have a plain bitmap, not a GC program.
        // The only use of this function is in channel sends, and the
        // 64 kB channel element limit takes care of this for us.
        //
        // Must not be preempted because it typically runs right before memmove,
        // and the GC must observe them as an atomic action.
        //
        //go:nosplit
        private static void typeBitsBulkBarrier(ref _type typ, System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size)
        {
            if (typ == null)
            {
                throw("runtime: typeBitsBulkBarrier without type");
            }
            if (typ.size != size)
            {
                println("runtime: typeBitsBulkBarrier with type ", typ.@string(), " of size ", typ.size, " but memory size", size);
                throw("runtime: invalid typeBitsBulkBarrier");
            }
            if (typ.kind & kindGCProg != 0L)
            {
                println("runtime: typeBitsBulkBarrier with type ", typ.@string(), " with GC prog");
                throw("runtime: invalid typeBitsBulkBarrier");
            }
            if (!writeBarrier.needed)
            {
                return;
            }
            var ptrmask = typ.gcdata;
            uint bits = default;
            {
                var i = uintptr(0L);

                while (i < typ.ptrdata)
                {
                    if (i & (sys.PtrSize * 8L - 1L) == 0L)
                    {
                        bits = uint32(ptrmask.Value);
                        ptrmask = addb(ptrmask, 1L);
                    i += sys.PtrSize;
                    }
                    else
                    {
                        bits = bits >> (int)(1L);
                    }
                    if (bits & 1L != 0L)
                    {
                        var dstx = (uintptr.Value)(@unsafe.Pointer(dst + i));
                        var srcx = (uintptr.Value)(@unsafe.Pointer(src + i));
                        writebarrierptr_prewrite(dstx, srcx.Value);
                    }
                }

            }
        }

        // The methods operating on spans all require that h has been returned
        // by heapBitsForSpan and that size, n, total are the span layout description
        // returned by the mspan's layout method.
        // If total > size*n, it means that there is extra leftover memory in the span,
        // usually due to rounding.
        //
        // TODO(rsc): Perhaps introduce a different heapBitsSpan type.

        // initSpan initializes the heap bitmap for a span.
        // It clears all checkmark bits.
        // If this is a span of pointer-sized objects, it initializes all
        // words to pointer/scan.
        // Otherwise, it initializes all words to scalar/dead.
        private static void initSpan(this heapBits h, ref mspan s)
        {
            var (size, n, total) = s.layout(); 

            // Init the markbit structures
            s.freeindex = 0L;
            s.allocCache = ~uint64(0L); // all 1s indicating all free.
            s.nelems = n;
            s.allocBits = null;
            s.gcmarkBits = null;
            s.gcmarkBits = newMarkBits(s.nelems);
            s.allocBits = newAllocBits(s.nelems); 

            // Clear bits corresponding to objects.
            if (total % heapBitmapScale != 0L)
            {
                throw("initSpan: unaligned length");
            }
            var nbyte = total / heapBitmapScale;
            if (sys.PtrSize == 8L && size == sys.PtrSize)
            {
                var end = h.bitp;
                var bitp = subtractb(end, nbyte - 1L);
                while (true)
                {
                    bitp.Value = bitPointerAll | bitScanAll;
                    if (bitp == end)
                    {
                        break;
                    }
                    bitp = add1(bitp);
                }

                return;
            }
            memclrNoHeapPointers(@unsafe.Pointer(subtractb(h.bitp, nbyte - 1L)), nbyte);
        }

        // initCheckmarkSpan initializes a span for being checkmarked.
        // It clears the checkmark bits, which are set to 1 in normal operation.
        private static void initCheckmarkSpan(this heapBits h, System.UIntPtr size, System.UIntPtr n, System.UIntPtr total)
        { 
            // The ptrSize == 8 is a compile-time constant false on 32-bit and eliminates this code entirely.
            if (sys.PtrSize == 8L && size == sys.PtrSize)
            { 
                // Checkmark bit is type bit, bottom bit of every 2-bit entry.
                // Only possible on 64-bit system, since minimum size is 8.
                // Must clear type bit (checkmark bit) of every word.
                // The type bit is the lower of every two-bit pair.
                var bitp = h.bitp;
                {
                    var i__prev1 = i;

                    var i = uintptr(0L);

                    while (i < n)
                    {
                        bitp.Value &= bitPointerAll;
                        bitp = subtract1(bitp);
                        i += 4L;
                    }


                    i = i__prev1;
                }
                return;
            }
            {
                var i__prev1 = i;

                for (i = uintptr(0L); i < n; i++)
                {
                    h.bitp.Value &= bitScan << (int)((heapBitsShift + h.shift));
                    h = h.forward(size / sys.PtrSize);
                }


                i = i__prev1;
            }
        }

        // clearCheckmarkSpan undoes all the checkmarking in a span.
        // The actual checkmark bits are ignored, so the only work to do
        // is to fix the pointer bits. (Pointer bits are ignored by scanobject
        // but consulted by typedmemmove.)
        private static void clearCheckmarkSpan(this heapBits h, System.UIntPtr size, System.UIntPtr n, System.UIntPtr total)
        { 
            // The ptrSize == 8 is a compile-time constant false on 32-bit and eliminates this code entirely.
            if (sys.PtrSize == 8L && size == sys.PtrSize)
            { 
                // Checkmark bit is type bit, bottom bit of every 2-bit entry.
                // Only possible on 64-bit system, since minimum size is 8.
                // Must clear type bit (checkmark bit) of every word.
                // The type bit is the lower of every two-bit pair.
                var bitp = h.bitp;
                {
                    var i = uintptr(0L);

                    while (i < n)
                    {
                        bitp.Value |= bitPointerAll;
                        bitp = subtract1(bitp);
                        i += 4L;
                    }

                }
            }
        }

        // oneBitCount is indexed by byte and produces the
        // number of 1 bits in that byte. For example 128 has 1 bit set
        // and oneBitCount[128] will holds 1.
        private static array<byte> oneBitCount = new array<byte>(new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 });

        // countAlloc returns the number of objects allocated in span s by
        // scanning the allocation bitmap.
        // TODO:(rlh) Use popcount intrinsic.
        private static long countAlloc(this ref mspan s)
        {
            long count = 0L;
            var maxIndex = s.nelems / 8L;
            for (var i = uintptr(0L); i < maxIndex; i++)
            {
                var mrkBits = s.gcmarkBits.bytep(i).Value;
                count += int(oneBitCount[mrkBits]);
            }

            {
                var bitsInLastByte = s.nelems % 8L;

                if (bitsInLastByte != 0L)
                {
                    mrkBits = s.gcmarkBits.bytep(maxIndex).Value;
                    var mask = uint8((1L << (int)(bitsInLastByte)) - 1L);
                    var bits = mrkBits & mask;
                    count += int(oneBitCount[bits]);
                }

            }
            return count;
        }

        // heapBitsSetType records that the new allocation [x, x+size)
        // holds in [x, x+dataSize) one or more values of type typ.
        // (The number of values is given by dataSize / typ.size.)
        // If dataSize < size, the fragment [x+dataSize, x+size) is
        // recorded as non-pointer data.
        // It is known that the type has pointers somewhere;
        // malloc does not call heapBitsSetType when there are no pointers,
        // because all free objects are marked as noscan during
        // heapBitsSweepSpan.
        //
        // There can only be one allocation from a given span active at a time,
        // and the bitmap for a span always falls on byte boundaries,
        // so there are no write-write races for access to the heap bitmap.
        // Hence, heapBitsSetType can access the bitmap without atomics.
        //
        // There can be read-write races between heapBitsSetType and things
        // that read the heap bitmap like scanobject. However, since
        // heapBitsSetType is only used for objects that have not yet been
        // made reachable, readers will ignore bits being modified by this
        // function. This does mean this function cannot transiently modify
        // bits that belong to neighboring objects. Also, on weakly-ordered
        // machines, callers must execute a store/store (publication) barrier
        // between calling this function and making the object reachable.
        private static void heapBitsSetType(System.UIntPtr x, System.UIntPtr size, System.UIntPtr dataSize, ref _type typ)
        {
            const var doubleCheck = false; // slow but helpful; enable to test modifications to this code

            // dataSize is always size rounded up to the next malloc size class,
            // except in the case of allocating a defer block, in which case
            // size is sizeof(_defer{}) (at least 6 words) and dataSize may be
            // arbitrarily larger.
            //
            // The checks for size == sys.PtrSize and size == 2*sys.PtrSize can therefore
            // assume that dataSize == size without checking it explicitly.

 // slow but helpful; enable to test modifications to this code

            // dataSize is always size rounded up to the next malloc size class,
            // except in the case of allocating a defer block, in which case
            // size is sizeof(_defer{}) (at least 6 words) and dataSize may be
            // arbitrarily larger.
            //
            // The checks for size == sys.PtrSize and size == 2*sys.PtrSize can therefore
            // assume that dataSize == size without checking it explicitly.

            if (sys.PtrSize == 8L && size == sys.PtrSize)
            { 
                // It's one word and it has pointers, it must be a pointer.
                // Since all allocated one-word objects are pointers
                // (non-pointers are aggregated into tinySize allocations),
                // initSpan sets the pointer bits for us. Nothing to do here.
                if (doubleCheck)
                {
                    var h = heapBitsForAddr(x);
                    if (!h.isPointer())
                    {
                        throw("heapBitsSetType: pointer bit missing");
                    }
                    if (!h.morePointers())
                    {
                        throw("heapBitsSetType: scan bit missing");
                    }
                }
                return;
            }
            h = heapBitsForAddr(x);
            var ptrmask = typ.gcdata; // start of 1-bit pointer mask (or GC program, handled below)

            // Heap bitmap bits for 2-word object are only 4 bits,
            // so also shared with objects next to it.
            // This is called out as a special case primarily for 32-bit systems,
            // so that on 32-bit systems the code below can assume all objects
            // are 4-word aligned (because they're all 16-byte aligned).
            if (size == 2L * sys.PtrSize)
            {
                if (typ.size == sys.PtrSize)
                { 
                    // We're allocating a block big enough to hold two pointers.
                    // On 64-bit, that means the actual object must be two pointers,
                    // or else we'd have used the one-pointer-sized block.
                    // On 32-bit, however, this is the 8-byte block, the smallest one.
                    // So it could be that we're allocating one pointer and this was
                    // just the smallest block available. Distinguish by checking dataSize.
                    // (In general the number of instances of typ being allocated is
                    // dataSize/typ.size.)
                    if (sys.PtrSize == 4L && dataSize == sys.PtrSize)
                    { 
                        // 1 pointer object. On 32-bit machines clear the bit for the
                        // unused second word.
                        h.bitp.Value &= (bitPointer | bitScan | ((bitPointer | bitScan) << (int)(heapBitsShift))) << (int)(h.shift);
                        h.bitp.Value |= (bitPointer | bitScan) << (int)(h.shift);
                    }
                    else
                    { 
                        // 2-element slice of pointer.
                        h.bitp.Value |= (bitPointer | bitScan | bitPointer << (int)(heapBitsShift)) << (int)(h.shift);
                    }
                    return;
                } 
                // Otherwise typ.size must be 2*sys.PtrSize,
                // and typ.kind&kindGCProg == 0.
                if (doubleCheck)
                {
                    if (typ.size != 2L * sys.PtrSize || typ.kind & kindGCProg != 0L)
                    {
                        print("runtime: heapBitsSetType size=", size, " but typ.size=", typ.size, " gcprog=", typ.kind & kindGCProg != 0L, "\n");
                        throw("heapBitsSetType");
                    }
                }
                var b = uint32(ptrmask.Value);
                var hb = (b & 3L) | bitScan; 
                // bitPointer == 1, bitScan is 1 << 4, heapBitsShift is 1.
                // 110011 is shifted h.shift and complemented.
                // This clears out the bits that are about to be
                // ored into *h.hbitp in the next instructions.
                h.bitp.Value &= (bitPointer | bitScan | ((bitPointer | bitScan) << (int)(heapBitsShift))) << (int)(h.shift);
                h.bitp.Value |= uint8(hb << (int)(h.shift));
                return;
            } 

            // Copy from 1-bit ptrmask into 2-bit bitmap.
            // The basic approach is to use a single uintptr as a bit buffer,
            // alternating between reloading the buffer and writing bitmap bytes.
            // In general, one load can supply two bitmap byte writes.
            // This is a lot of lines of code, but it compiles into relatively few
            // machine instructions.
 
            // Ptrmask input.
            ref byte p = default;            b = default;            System.UIntPtr nb = default;            ref byte endp = default;            System.UIntPtr endnb = default;            System.UIntPtr pbits = default;            System.UIntPtr w = default;            System.UIntPtr nw = default;            ref byte hbitp = default;            hb = default;

            hbitp = h.bitp; 

            // Handle GC program. Delayed until this part of the code
            // so that we can use the same double-checking mechanism
            // as the 1-bit case. Nothing above could have encountered
            // GC programs: the cases were all too small.
            if (typ.kind & kindGCProg != 0L)
            {
                heapBitsSetTypeGCProg(h, typ.ptrdata, typ.size, dataSize, size, addb(typ.gcdata, 4L));
                if (doubleCheck)
                { 
                    // Double-check the heap bits written by GC program
                    // by running the GC program to create a 1-bit pointer mask
                    // and then jumping to the double-check code below.
                    // This doesn't catch bugs shared between the 1-bit and 4-bit
                    // GC program execution, but it does catch mistakes specific
                    // to just one of those and bugs in heapBitsSetTypeGCProg's
                    // implementation of arrays.
                    lock(ref debugPtrmask.@lock);
                    if (debugPtrmask.data == null)
                    {
                        debugPtrmask.data = (byte.Value)(persistentalloc(1L << (int)(20L), 1L, ref memstats.other_sys));
                    }
                    ptrmask = debugPtrmask.data;
                    runGCProg(addb(typ.gcdata, 4L), null, ptrmask, 1L);
                    goto Phase4;
                }
                return;
            } 

            // Note about sizes:
            //
            // typ.size is the number of words in the object,
            // and typ.ptrdata is the number of words in the prefix
            // of the object that contains pointers. That is, the final
            // typ.size - typ.ptrdata words contain no pointers.
            // This allows optimization of a common pattern where
            // an object has a small header followed by a large scalar
            // buffer. If we know the pointers are over, we don't have
            // to scan the buffer's heap bitmap at all.
            // The 1-bit ptrmasks are sized to contain only bits for
            // the typ.ptrdata prefix, zero padded out to a full byte
            // of bitmap. This code sets nw (below) so that heap bitmap
            // bits are only written for the typ.ptrdata prefix; if there is
            // more room in the allocated object, the next heap bitmap
            // entry is a 00, indicating that there are no more pointers
            // to scan. So only the ptrmask for the ptrdata bytes is needed.
            //
            // Replicated copies are not as nice: if there is an array of
            // objects with scalar tails, all but the last tail does have to
            // be initialized, because there is no way to say "skip forward".
            // However, because of the possibility of a repeated type with
            // size not a multiple of 4 pointers (one heap bitmap byte),
            // the code already must handle the last ptrmask byte specially
            // by treating it as containing only the bits for endnb pointers,
            // where endnb <= 4. We represent large scalar tails that must
            // be expanded in the replication by setting endnb larger than 4.
            // This will have the effect of reading many bits out of b,
            // but once the real bits are shifted out, b will supply as many
            // zero bits as we try to read, which is exactly what we need.
            p = ptrmask;
            if (typ.size < dataSize)
            { 
                // Filling in bits for an array of typ.
                // Set up for repetition of ptrmask during main loop.
                // Note that ptrmask describes only a prefix of
                const var maxBits = sys.PtrSize * 8L - 7L;

                if (typ.ptrdata / sys.PtrSize <= maxBits)
                { 
                    // Entire ptrmask fits in uintptr with room for a byte fragment.
                    // Load into pbits and never read from ptrmask again.
                    // This is especially important when the ptrmask has
                    // fewer than 8 bits in it; otherwise the reload in the middle
                    // of the Phase 2 loop would itself need to loop to gather
                    // at least 8 bits.

                    // Accumulate ptrmask into b.
                    // ptrmask is sized to describe only typ.ptrdata, but we record
                    // it as describing typ.size bytes, since all the high bits are zero.
                    nb = typ.ptrdata / sys.PtrSize;
                    {
                        var i__prev1 = i;

                        var i = uintptr(0L);

                        while (i < nb)
                        {
                            b |= uintptr(p.Value) << (int)(i);
                            p = add1(p);
                            i += 8L;
                        }
                else


                        i = i__prev1;
                    }
                    nb = typ.size / sys.PtrSize; 

                    // Replicate ptrmask to fill entire pbits uintptr.
                    // Doubling and truncating is fewer steps than
                    // iterating by nb each time. (nb could be 1.)
                    // Since we loaded typ.ptrdata/sys.PtrSize bits
                    // but are pretending to have typ.size/sys.PtrSize,
                    // there might be no replication necessary/possible.
                    pbits = b;
                    endnb = nb;
                    if (nb + nb <= maxBits)
                    {
                        while (endnb <= sys.PtrSize * 8L)
                        {
                            pbits |= pbits << (int)(endnb);
                            endnb += endnb;
                        } 
                        // Truncate to a multiple of original ptrmask.
                        // Because nb+nb <= maxBits, nb fits in a byte.
                        // Byte division is cheaper than uintptr division.
 
                        // Truncate to a multiple of original ptrmask.
                        // Because nb+nb <= maxBits, nb fits in a byte.
                        // Byte division is cheaper than uintptr division.
                        endnb = uintptr(maxBits / byte(nb)) * nb;
                        pbits &= 1L << (int)(endnb) - 1L;
                        b = pbits;
                        nb = endnb;
                    } 

                    // Clear p and endp as sentinel for using pbits.
                    // Checked during Phase 2 loop.
                    p = null;
                    endp = null;
                }                { 
                    // Ptrmask is larger. Read it multiple times.
                    var n = (typ.ptrdata / sys.PtrSize + 7L) / 8L - 1L;
                    endp = addb(ptrmask, n);
                    endnb = typ.size / sys.PtrSize - n * 8L;
                }
            }
            if (p != null)
            {
                b = uintptr(p.Value);
                p = add1(p);
                nb = 8L;
            }
            if (typ.size == dataSize)
            { 
                // Single entry: can stop once we reach the non-pointer data.
                nw = typ.ptrdata / sys.PtrSize;
            }
            else
            { 
                // Repeated instances of typ in an array.
                // Have to process first N-1 entries in full, but can stop
                // once we reach the non-pointer data in the final entry.
                nw = ((dataSize / typ.size - 1L) * typ.size + typ.ptrdata) / sys.PtrSize;
            }
            if (nw == 0L)
            { 
                // No pointers! Caller was supposed to check.
                println("runtime: invalid type ", typ.@string());
                throw("heapBitsSetType: called with non-pointer type");
                return;
            }
            if (nw < 2L)
            { 
                // Must write at least 2 words, because the "no scan"
                // encoding doesn't take effect until the third word.
                nw = 2L;
            } 

            // Phase 1: Special case for leading byte (shift==0) or half-byte (shift==4).
            // The leading byte is special because it contains the bits for word 1,
            // which does not have the scan bit set.
            // The leading half-byte is special because it's a half a byte,
            // so we have to be careful with the bits already there.

            if (h.shift == 0L) 
                // Ptrmask and heap bitmap are aligned.
                // Handle first byte of bitmap specially.
                //
                // The first byte we write out covers the first four
                // words of the object. The scan/dead bit on the first
                // word must be set to scan since there are pointers
                // somewhere in the object. The scan/dead bit on the
                // second word is the checkmark, so we don't set it.
                // In all following words, we set the scan/dead
                // appropriately to indicate that the object contains
                // to the next 2-bit entry in the bitmap.
                //
                // TODO: It doesn't matter if we set the checkmark, so
                // maybe this case isn't needed any more.
                hb = b & bitPointerAll;
                hb |= bitScan | bitScan << (int)((2L * heapBitsShift)) | bitScan << (int)((3L * heapBitsShift));
                w += 4L;

                if (w >= nw)
                {
                    goto Phase3;
                }
                hbitp.Value = uint8(hb);
                hbitp = subtract1(hbitp);
                b >>= 4L;
                nb -= 4L;
            else if (sys.PtrSize == 8L && h.shift == 2L) 
                // Ptrmask and heap bitmap are misaligned.
                // The bits for the first two words are in a byte shared
                // with another object, so we must be careful with the bits
                // already there.
                // We took care of 1-word and 2-word objects above,
                // so this is at least a 6-word object.
                hb = (b & (bitPointer | bitPointer << (int)(heapBitsShift))) << (int)((2L * heapBitsShift)); 
                // This is not noscan, so set the scan bit in the
                // first word.
                hb |= bitScan << (int)((2L * heapBitsShift));
                b >>= 2L;
                nb -= 2L; 
                // Note: no bitScan for second word because that's
                // the checkmark.
                hbitp.Value &= uint8((bitPointer | bitScan | (bitPointer << (int)(heapBitsShift))) << (int)((2L * heapBitsShift)));
                hbitp.Value |= uint8(hb);
                hbitp = subtract1(hbitp);
                w += 2L;

                if (w >= nw)
                { 
                    // We know that there is more data, because we handled 2-word objects above.
                    // This must be at least a 6-word object. If we're out of pointer words,
                    // mark no scan in next bitmap byte and finish.
                    hb = 0L;
                    w += 4L;
                    goto Phase3;
                }
            else 
                throw("heapBitsSetType: unexpected shift");
            // Phase 2: Full bytes in bitmap, up to but not including write to last byte (full or partial) in bitmap.
            // The loop computes the bits for that last write but does not execute the write;
            // it leaves the bits in hb for processing by phase 3.
            // To avoid repeated adjustment of nb, we subtract out the 4 bits we're going to
            // use in the first half of the loop right now, and then we only adjust nb explicitly
            // if the 8 bits used by each iteration isn't balanced by 8 bits loaded mid-loop.
            nb -= 4L;
            while (true)
            { 
                // Emit bitmap byte.
                // b has at least nb+4 bits, with one exception:
                // if w+4 >= nw, then b has only nw-w bits,
                // but we'll stop at the break and then truncate
                // appropriately in Phase 3.
                hb = b & bitPointerAll;
                hb |= bitScanAll;
                w += 4L;

                if (w >= nw)
                {
                    break;
                }
                hbitp.Value = uint8(hb);
                hbitp = subtract1(hbitp);
                b >>= 4L; 

                // Load more bits. b has nb right now.
                if (p != endp)
                { 
                    // Fast path: keep reading from ptrmask.
                    // nb unmodified: we just loaded 8 bits,
                    // and the next iteration will consume 8 bits,
                    // leaving us with the same nb the next time we're here.
                    if (nb < 8L)
                    {
                        b |= uintptr(p.Value) << (int)(nb);
                        p = add1(p);
                    }
                    else
                    { 
                        // Reduce the number of bits in b.
                        // This is important if we skipped
                        // over a scalar tail, since nb could
                        // be larger than the bit width of b.
                        nb -= 8L;
                    }
                }
                else if (p == null)
                { 
                    // Almost as fast path: track bit count and refill from pbits.
                    // For short repetitions.
                    if (nb < 8L)
                    {
                        b |= pbits << (int)(nb);
                        nb += endnb;
                    }
                    nb -= 8L; // for next iteration
                }
                else
                { 
                    // Slow path: reached end of ptrmask.
                    // Process final partial byte and rewind to start.
                    b |= uintptr(p.Value) << (int)(nb);
                    nb += endnb;
                    if (nb < 8L)
                    {
                        b |= uintptr(ptrmask.Value) << (int)(nb);
                        p = add1(ptrmask);
                    }
                    else
                    {
                        nb -= 8L;
                        p = ptrmask;
                    }
                } 

                // Emit bitmap byte.
                hb = b & bitPointerAll;
                hb |= bitScanAll;
                w += 4L;

                if (w >= nw)
                {
                    break;
                }
                hbitp.Value = uint8(hb);
                hbitp = subtract1(hbitp);
                b >>= 4L;
            }


Phase3: 

            // Change nw from counting possibly-pointer words to total words in allocation.
            if (w > nw)
            { 
                // Counting the 4 entries in hb not yet written to memory,
                // there are more entries than possible pointer slots.
                // Discard the excess entries (can't be more than 3).
                var mask = uintptr(1L) << (int)((4L - (w - nw))) - 1L;
                hb &= mask | mask << (int)(4L); // apply mask to both pointer bits and scan bits
            } 

            // Change nw from counting possibly-pointer words to total words in allocation.
            nw = size / sys.PtrSize; 

            // Write whole bitmap bytes.
            // The first is hb, the rest are zero.
            if (w <= nw)
            {
                hbitp.Value = uint8(hb);
                hbitp = subtract1(hbitp);
                hb = 0L; // for possible final half-byte below
                w += 4L;

                while (w <= nw)
                {
                    hbitp.Value = 0L;
                    hbitp = subtract1(hbitp);
                    w += 4L;
                }

            } 

            // Write final partial bitmap byte if any.
            // We know w > nw, or else we'd still be in the loop above.
            // It can be bigger only due to the 4 entries in hb that it counts.
            // If w == nw+4 then there's nothing left to do: we wrote all nw entries
            // and can discard the 4 sitting in hb.
            // But if w == nw+2, we need to write first two in hb.
            // The byte is shared with the next object, so be careful with
            // existing bits.
            if (w == nw + 2L)
            {
                hbitp.Value = hbitp & ~(bitPointer | bitScan | (bitPointer | bitScan) << (int)(heapBitsShift)) | uint8(hb).Value;
            }
Phase4:
            if (doubleCheck)
            {
                var end = heapBitsForAddr(x + size);
                if (typ.kind & kindGCProg == 0L && (hbitp != end.bitp || (w == nw + 2L) != (end.shift == 2L)))
                {
                    println("ended at wrong bitmap byte for", typ.@string(), "x", dataSize / typ.size);
                    print("typ.size=", typ.size, " typ.ptrdata=", typ.ptrdata, " dataSize=", dataSize, " size=", size, "\n");
                    print("w=", w, " nw=", nw, " b=", hex(b), " nb=", nb, " hb=", hex(hb), "\n");
                    var h0 = heapBitsForAddr(x);
                    print("initial bits h0.bitp=", h0.bitp, " h0.shift=", h0.shift, "\n");
                    print("ended at hbitp=", hbitp, " but next starts at bitp=", end.bitp, " shift=", end.shift, "\n");
                    throw("bad heapBitsSetType");
                } 

                // Double-check that bits to be written were written correctly.
                // Does not check that other bits were not written, unfortunately.
                h = heapBitsForAddr(x);
                var nptr = typ.ptrdata / sys.PtrSize;
                var ndata = typ.size / sys.PtrSize;
                var count = dataSize / typ.size;
                var totalptr = ((count - 1L) * typ.size + typ.ptrdata) / sys.PtrSize;
                {
                    var i__prev1 = i;

                    for (i = uintptr(0L); i < size / sys.PtrSize; i++)
                    {
                        var j = i % ndata;
                        byte have = default;                        byte want = default;

                        have = (h.bitp >> (int)(h.shift).Value) & (bitPointer | bitScan);
                        if (i >= totalptr)
                        {
                            want = 0L; // deadmarker
                            if (typ.kind & kindGCProg != 0L && i < (totalptr + 3L) / 4L * 4L)
                            {
                                want = bitScan;
                            }
                        }
                        else
                        {
                            if (j < nptr && (addb(ptrmask, j / 8L) >> (int)((j % 8L)).Value) & 1L != 0L)
                            {
                                want |= bitPointer;
                            }
                            if (i != 1L)
                            {
                                want |= bitScan;
                            }
                            else
                            {
                                have &= bitScan;
                            }
                        }
                        if (have != want)
                        {
                            println("mismatch writing bits for", typ.@string(), "x", dataSize / typ.size);
                            print("typ.size=", typ.size, " typ.ptrdata=", typ.ptrdata, " dataSize=", dataSize, " size=", size, "\n");
                            print("kindGCProg=", typ.kind & kindGCProg != 0L, "\n");
                            print("w=", w, " nw=", nw, " b=", hex(b), " nb=", nb, " hb=", hex(hb), "\n");
                            h0 = heapBitsForAddr(x);
                            print("initial bits h0.bitp=", h0.bitp, " h0.shift=", h0.shift, "\n");
                            print("current bits h.bitp=", h.bitp, " h.shift=", h.shift, " *h.bitp=", hex(h.bitp.Value), "\n");
                            print("ptrmask=", ptrmask, " p=", p, " endp=", endp, " endnb=", endnb, " pbits=", hex(pbits), " b=", hex(b), " nb=", nb, "\n");
                            println("at word", i, "offset", i * sys.PtrSize, "have", have, "want", want);
                            if (typ.kind & kindGCProg != 0L)
                            {
                                println("GC program:");
                                dumpGCProg(addb(typ.gcdata, 4L));
                            }
                            throw("bad heapBitsSetType");
                        }
                        h = h.next();
                    }


                    i = i__prev1;
                }
                if (ptrmask == debugPtrmask.data)
                {
                    unlock(ref debugPtrmask.@lock);
                }
            }
        }

        private static var debugPtrmask = default;

        // heapBitsSetTypeGCProg implements heapBitsSetType using a GC program.
        // progSize is the size of the memory described by the program.
        // elemSize is the size of the element that the GC program describes (a prefix of).
        // dataSize is the total size of the intended data, a multiple of elemSize.
        // allocSize is the total size of the allocated memory.
        //
        // GC programs are only used for large allocations.
        // heapBitsSetType requires that allocSize is a multiple of 4 words,
        // so that the relevant bitmap bytes are not shared with surrounding
        // objects.
        private static void heapBitsSetTypeGCProg(heapBits h, System.UIntPtr progSize, System.UIntPtr elemSize, System.UIntPtr dataSize, System.UIntPtr allocSize, ref byte prog)
        {
            if (sys.PtrSize == 8L && allocSize % (4L * sys.PtrSize) != 0L)
            { 
                // Alignment will be wrong.
                throw("heapBitsSetTypeGCProg: small allocation");
            }
            System.UIntPtr totalBits = default;
            if (elemSize == dataSize)
            {
                totalBits = runGCProg(prog, null, h.bitp, 2L);
                if (totalBits * sys.PtrSize != progSize)
                {
                    println("runtime: heapBitsSetTypeGCProg: total bits", totalBits, "but progSize", progSize);
                    throw("heapBitsSetTypeGCProg: unexpected bit count");
                }
            }
            else
            {
                var count = dataSize / elemSize; 

                // Piece together program trailer to run after prog that does:
                //    literal(0)
                //    repeat(1, elemSize-progSize-1) // zeros to fill element size
                //    repeat(elemSize, count-1) // repeat that element for count
                // This zero-pads the data remaining in the first element and then
                // repeats that first element to fill the array.
                array<byte> trailer = new array<byte>(40L); // 3 varints (max 10 each) + some bytes
                long i = 0L;
                {
                    var n__prev2 = n;

                    var n = elemSize / sys.PtrSize - progSize / sys.PtrSize;

                    if (n > 0L)
                    { 
                        // literal(0)
                        trailer[i] = 0x01UL;
                        i++;
                        trailer[i] = 0L;
                        i++;
                        if (n > 1L)
                        { 
                            // repeat(1, n-1)
                            trailer[i] = 0x81UL;
                            i++;
                            n--;
                            while (n >= 0x80UL)
                            {
                                trailer[i] = byte(n | 0x80UL);
                                i++;
                                n >>= 7L;
                            }

                            trailer[i] = byte(n);
                            i++;
                        }
                    } 
                    // repeat(elemSize/ptrSize, count-1)

                    n = n__prev2;

                } 
                // repeat(elemSize/ptrSize, count-1)
                trailer[i] = 0x80UL;
                i++;
                n = elemSize / sys.PtrSize;
                while (n >= 0x80UL)
                {
                    trailer[i] = byte(n | 0x80UL);
                    i++;
                    n >>= 7L;
                }

                trailer[i] = byte(n);
                i++;
                n = count - 1L;
                while (n >= 0x80UL)
                {
                    trailer[i] = byte(n | 0x80UL);
                    i++;
                    n >>= 7L;
                }

                trailer[i] = byte(n);
                i++;
                trailer[i] = 0L;
                i++;

                runGCProg(prog, ref trailer[0L], h.bitp, 2L); 

                // Even though we filled in the full array just now,
                // record that we only filled in up to the ptrdata of the
                // last element. This will cause the code below to
                // memclr the dead section of the final array element,
                // so that scanobject can stop early in the final element.
                totalBits = (elemSize * (count - 1L) + progSize) / sys.PtrSize;
            }
            var endProg = @unsafe.Pointer(subtractb(h.bitp, (totalBits + 3L) / 4L));
            var endAlloc = @unsafe.Pointer(subtractb(h.bitp, allocSize / heapBitmapScale));
            memclrNoHeapPointers(add(endAlloc, 1L), uintptr(endProg) - uintptr(endAlloc));
        }

        // progToPointerMask returns the 1-bit pointer mask output by the GC program prog.
        // size the size of the region described by prog, in bytes.
        // The resulting bitvector will have no more than size/sys.PtrSize bits.
        private static bitvector progToPointerMask(ref byte prog, System.UIntPtr size)
        {
            var n = (size / sys.PtrSize + 7L) / 8L;
            ref array<byte> x = new ptr<ref array<byte>>(persistentalloc(n + 1L, 1L, ref memstats.buckhash_sys))[..n + 1L];
            x[len(x) - 1L] = 0xa1UL; // overflow check sentinel
            n = runGCProg(prog, null, ref x[0L], 1L);
            if (x[len(x) - 1L] != 0xa1UL)
            {
                throw("progToPointerMask: overflow");
            }
            return new bitvector(int32(n),&x[0]);
        }

        // Packed GC pointer bitmaps, aka GC programs.
        //
        // For large types containing arrays, the type information has a
        // natural repetition that can be encoded to save space in the
        // binary and in the memory representation of the type information.
        //
        // The encoding is a simple Lempel-Ziv style bytecode machine
        // with the following instructions:
        //
        //    00000000: stop
        //    0nnnnnnn: emit n bits copied from the next (n+7)/8 bytes
        //    10000000 n c: repeat the previous n bits c times; n, c are varints
        //    1nnnnnnn c: repeat the previous n bits c times; c is a varint

        // runGCProg executes the GC program prog, and then trailer if non-nil,
        // writing to dst with entries of the given size.
        // If size == 1, dst is a 1-bit pointer mask laid out moving forward from dst.
        // If size == 2, dst is the 2-bit heap bitmap, and writes move backward
        // starting at dst (because the heap bitmap does). In this case, the caller guarantees
        // that only whole bytes in dst need to be written.
        //
        // runGCProg returns the number of 1- or 2-bit entries written to memory.
        private static System.UIntPtr runGCProg(ref byte prog, ref byte trailer, ref byte dst, long size)
        {
            var dstStart = dst; 

            // Bits waiting to be written to memory.
            System.UIntPtr bits = default;
            System.UIntPtr nbits = default;

            var p = prog;
Run: 

            // Write any final bits out, using full-byte writes, even for the final byte.
            while (true)
            { 
                // Flush accumulated full bytes.
                // The rest of the loop assumes that nbits <= 7.
                while (nbits >= 8L)
                {
                    if (size == 1L)
                    {
                        dst.Value = uint8(bits);
                        dst = add1(dst);
                        bits >>= 8L;
                    nbits -= 8L;
                    }
                    else
                    {
                        var v = bits & bitPointerAll | bitScanAll;
                        dst.Value = uint8(v);
                        dst = subtract1(dst);
                        bits >>= 4L;
                        v = bits & bitPointerAll | bitScanAll;
                        dst.Value = uint8(v);
                        dst = subtract1(dst);
                        bits >>= 4L;
                    }
                } 

                // Process one instruction.
 

                // Process one instruction.
                var inst = uintptr(p.Value);
                p = add1(p);
                var n = inst & 0x7FUL;
                if (inst & 0x80UL == 0L)
                { 
                    // Literal bits; n == 0 means end of program.
                    if (n == 0L)
                    { 
                        // Program is over; continue in trailer if present.
                        if (trailer != null)
                        { 
                            //println("trailer")
                            p = trailer;
                            trailer = null;
                            continue;
                        } 
                        //println("done")
                        _breakRun = true;
                        break;
                    } 
                    //println("lit", n, dst)
                    var nbyte = n / 8L;
                    {
                        var i__prev2 = i;

                        for (var i = uintptr(0L); i < nbyte; i++)
                        {
                            bits |= uintptr(p.Value) << (int)(nbits);
                            p = add1(p);
                            if (size == 1L)
                            {
                                dst.Value = uint8(bits);
                                dst = add1(dst);
                                bits >>= 8L;
                            }
                            else
                            {
                                v = bits & 0xfUL | bitScanAll;
                                dst.Value = uint8(v);
                                dst = subtract1(dst);
                                bits >>= 4L;
                                v = bits & 0xfUL | bitScanAll;
                                dst.Value = uint8(v);
                                dst = subtract1(dst);
                                bits >>= 4L;
                            }
                        }


                        i = i__prev2;
                    }
                    n %= 8L;

                    if (n > 0L)
                    {
                        bits |= uintptr(p.Value) << (int)(nbits);
                        p = add1(p);
                        nbits += n;
                    }
                    _continueRun = true;
                    break;
                } 

                // Repeat. If n == 0, it is encoded in a varint in the next bytes.
                if (n == 0L)
                {
                    {
                        var off__prev2 = off;

                        var off = uint(0L);

                        while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                        {
                            var x = uintptr(p.Value);
                            p = add1(p);
                            n |= (x & 0x7FUL) << (int)(off);
                            if (x & 0x80UL == 0L)
                            {
                                break;
                            off += 7L;
                            }
                        }


                        off = off__prev2;
                    }
                } 

                // Count is encoded in a varint in the next bytes.
                var c = uintptr(0L);
                {
                    var off__prev2 = off;

                    off = uint(0L);

                    while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                    {
                        x = uintptr(p.Value);
                        p = add1(p);
                        c |= (x & 0x7FUL) << (int)(off);
                        if (x & 0x80UL == 0L)
                        {
                            break;
                        off += 7L;
                        }
                    }


                    off = off__prev2;
                }
                c *= n; // now total number of bits to copy

                // If the number of bits being repeated is small, load them
                // into a register and use that register for the entire loop
                // instead of repeatedly reading from memory.
                // Handling fewer than 8 bits here makes the general loop simpler.
                // The cutoff is sys.PtrSize*8 - 7 to guarantee that when we add
                // the pattern to a bit buffer holding at most 7 bits (a partial byte)
                // it will not overflow.
                var src = dst;
                const var maxBits = sys.PtrSize * 8L - 7L;

                if (n <= maxBits)
                { 
                    // Start with bits in output buffer.
                    var pattern = bits;
                    var npattern = nbits; 

                    // If we need more bits, fetch them from memory.
                    if (size == 1L)
                    {
                        src = subtract1(src);
                        while (npattern < n)
                        {
                            pattern <<= 8L;
                            pattern |= uintptr(src.Value);
                            src = subtract1(src);
                            npattern += 8L;
                        }
                    else

                    }                    {
                        src = add1(src);
                        while (npattern < n)
                        {
                            pattern <<= 4L;
                            pattern |= uintptr(src.Value) & 0xfUL;
                            src = add1(src);
                            npattern += 4L;
                        }

                    } 

                    // We started with the whole bit output buffer,
                    // and then we loaded bits from whole bytes.
                    // Either way, we might now have too many instead of too few.
                    // Discard the extra.
                    if (npattern > n)
                    {
                        pattern >>= npattern - n;
                        npattern = n;
                    } 

                    // Replicate pattern to at most maxBits.
                    if (npattern == 1L)
                    { 
                        // One bit being repeated.
                        // If the bit is 1, make the pattern all 1s.
                        // If the bit is 0, the pattern is already all 0s,
                        // but we can claim that the number of bits
                        // in the word is equal to the number we need (c),
                        // because right shift of bits will zero fill.
                        if (pattern == 1L)
                        {
                            pattern = 1L << (int)(maxBits) - 1L;
                            npattern = maxBits;
                        }
                        else
                        {
                            npattern = c;
                        }
                    }
                    else
                    {
                        var b = pattern;
                        var nb = npattern;
                        if (nb + nb <= maxBits)
                        { 
                            // Double pattern until the whole uintptr is filled.
                            while (nb <= sys.PtrSize * 8L)
                            {
                                b |= b << (int)(nb);
                                nb += nb;
                            } 
                            // Trim away incomplete copy of original pattern in high bits.
                            // TODO(rsc): Replace with table lookup or loop on systems without divide?
 
                            // Trim away incomplete copy of original pattern in high bits.
                            // TODO(rsc): Replace with table lookup or loop on systems without divide?
                            nb = maxBits / npattern * npattern;
                            b &= 1L << (int)(nb) - 1L;
                            pattern = b;
                            npattern = nb;
                        }
                    } 

                    // Add pattern to bit buffer and flush bit buffer, c/npattern times.
                    // Since pattern contains >8 bits, there will be full bytes to flush
                    // on each iteration.
                    while (c >= npattern)
                    {
                        bits |= pattern << (int)(nbits);
                        nbits += npattern;
                        if (size == 1L)
                        {
                            while (nbits >= 8L)
                            {
                                dst.Value = uint8(bits);
                                dst = add1(dst);
                                bits >>= 8L;
                                nbits -= 8L;
                        c -= npattern;
                            }
                        else

                        }                        {
                            while (nbits >= 4L)
                            {
                                dst.Value = uint8(bits & 0xfUL | bitScanAll);
                                dst = subtract1(dst);
                                bits >>= 4L;
                                nbits -= 4L;
                            }

                        }
                    } 

                    // Add final fragment to bit buffer.
 

                    // Add final fragment to bit buffer.
                    if (c > 0L)
                    {
                        pattern &= 1L << (int)(c) - 1L;
                        bits |= pattern << (int)(nbits);
                        nbits += c;
                    }
                    _continueRun = true;
                    break;
                } 

                // Repeat; n too large to fit in a register.
                // Since nbits <= 7, we know the first few bytes of repeated data
                // are already written to memory.
                off = n - nbits; // n > nbits because n > maxBits and nbits <= 7
                if (size == 1L)
                { 
                    // Leading src fragment.
                    src = subtractb(src, (off + 7L) / 8L);
                    {
                        var frag__prev2 = frag;

                        var frag = off & 7L;

                        if (frag != 0L)
                        {
                            bits |= uintptr(src.Value) >> (int)((8L - frag)) << (int)(nbits);
                            src = add1(src);
                            nbits += frag;
                            c -= frag;
                        } 
                        // Main loop: load one byte, write another.
                        // The bits are rotating through the bit buffer.

                        frag = frag__prev2;

                    } 
                    // Main loop: load one byte, write another.
                    // The bits are rotating through the bit buffer.
                    {
                        var i__prev2 = i;

                        for (i = c / 8L; i > 0L; i--)
                        {
                            bits |= uintptr(src.Value) << (int)(nbits);
                            src = add1(src);
                            dst.Value = uint8(bits);
                            dst = add1(dst);
                            bits >>= 8L;
                        }
                else
 
                        // Final src fragment.


                        i = i__prev2;
                    } 
                    // Final src fragment.
                    c %= 8L;

                    if (c > 0L)
                    {
                        bits |= (uintptr(src.Value) & (1L << (int)(c) - 1L)) << (int)(nbits);
                        nbits += c;
                    }
                }                { 
                    // Leading src fragment.
                    src = addb(src, (off + 3L) / 4L);
                    {
                        var frag__prev2 = frag;

                        frag = off & 3L;

                        if (frag != 0L)
                        {
                            bits |= (uintptr(src.Value) & 0xfUL) >> (int)((4L - frag)) << (int)(nbits);
                            src = subtract1(src);
                            nbits += frag;
                            c -= frag;
                        } 
                        // Main loop: load one byte, write another.
                        // The bits are rotating through the bit buffer.

                        frag = frag__prev2;

                    } 
                    // Main loop: load one byte, write another.
                    // The bits are rotating through the bit buffer.
                    {
                        var i__prev2 = i;

                        for (i = c / 4L; i > 0L; i--)
                        {
                            bits |= (uintptr(src.Value) & 0xfUL) << (int)(nbits);
                            src = subtract1(src);
                            dst.Value = uint8(bits & 0xfUL | bitScanAll);
                            dst = subtract1(dst);
                            bits >>= 4L;
                        } 
                        // Final src fragment.


                        i = i__prev2;
                    } 
                    // Final src fragment.
                    c %= 4L;

                    if (c > 0L)
                    {
                        bits |= (uintptr(src.Value) & (1L << (int)(c) - 1L)) << (int)(nbits);
                        nbits += c;
                    }
                }
            } 

            // Write any final bits out, using full-byte writes, even for the final byte.
 

            // Write any final bits out, using full-byte writes, even for the final byte.
            System.UIntPtr totalBits = default;
            if (size == 1L)
            {
                totalBits = (uintptr(@unsafe.Pointer(dst)) - uintptr(@unsafe.Pointer(dstStart))) * 8L + nbits;
                nbits += -nbits & 7L;
                while (nbits > 0L)
                {
                    dst.Value = uint8(bits);
                    dst = add1(dst);
                    bits >>= 8L;
                    nbits -= 8L;
                }
            else

            }            {
                totalBits = (uintptr(@unsafe.Pointer(dstStart)) - uintptr(@unsafe.Pointer(dst))) * 4L + nbits;
                nbits += -nbits & 3L;
                while (nbits > 0L)
                {
                    v = bits & 0xfUL | bitScanAll;
                    dst.Value = uint8(v);
                    dst = subtract1(dst);
                    bits >>= 4L;
                    nbits -= 4L;
                }

            }
            return totalBits;
        }

        private static void dumpGCProg(ref byte p)
        {
            long nptr = 0L;
            while (true)
            {
                var x = p.Value;
                p = add1(p);
                if (x == 0L)
                {
                    print("\t", nptr, " end\n");
                    break;
                }
                if (x & 0x80UL == 0L)
                {
                    print("\t", nptr, " lit ", x, ":");
                    var n = int(x + 7L) / 8L;
                    for (long i = 0L; i < n; i++)
                    {
                        print(" ", hex(p.Value));
                        p = add1(p);
                    }
                else

                    print("\n");
                    nptr += int(x);
                }                {
                    var nbit = int(x & ~0x80UL);
                    if (nbit == 0L)
                    {
                        {
                            var nb__prev2 = nb;

                            var nb = uint(0L);

                            while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                            {
                                x = p.Value;
                                p = add1(p);
                                nbit |= int(x & 0x7fUL) << (int)(nb);
                                if (x & 0x80UL == 0L)
                                {
                                    break;
                                nb += 7L;
                                }
                            }


                            nb = nb__prev2;
                        }
                    }
                    long count = 0L;
                    {
                        var nb__prev2 = nb;

                        nb = uint(0L);

                        while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                        {
                            x = p.Value;
                            p = add1(p);
                            count |= int(x & 0x7fUL) << (int)(nb);
                            if (x & 0x80UL == 0L)
                            {
                                break;
                            nb += 7L;
                            }
                        }


                        nb = nb__prev2;
                    }
                    print("\t", nptr, " repeat ", nbit, " × ", count, "\n");
                    nptr += nbit * count;
                }
            }

        }

        // Testing.

        private static bool getgcmaskcb(ref stkframe frame, unsafe.Pointer ctxt)
        {
            var target = (stkframe.Value)(ctxt);
            if (frame.sp <= target.sp && target.sp < frame.varp)
            {
                target.Value = frame.Value;
                return false;
            }
            return true;
        }

        // gcbits returns the GC type info for x, for testing.
        // The result is the bitmap entries (0 or 1), one entry per byte.
        //go:linkname reflect_gcbits reflect.gcbits
        private static slice<byte> reflect_gcbits(object x)
        {
            var ret = getgcmask(x);
            var typ = (ptrtype.Value)(@unsafe.Pointer(efaceOf(ref x)._type)).elem;
            var nptr = typ.ptrdata / sys.PtrSize;
            while (uintptr(len(ret)) > nptr && ret[len(ret) - 1L] == 0L)
            {
                ret = ret[..len(ret) - 1L];
            }

            return ret;
        }

        // Returns GC type info for object p for testing.
        private static slice<byte> getgcmask(object ep)
        {
            ref efaceOf e = new ptr<ref efaceOf>(ref ep);
            var p = e.data;
            var t = e._type; 
            // data or bss
            foreach (var (_, datap) in activeModules())
            { 
                // data
                if (datap.data <= uintptr(p) && uintptr(p) < datap.edata)
                {
                    var bitmap = datap.gcdatamask.bytedata;
                    var n = (ptrtype.Value)(@unsafe.Pointer(t)).elem.size;
                    mask = make_slice<byte>(n / sys.PtrSize);
                    {
                        var i__prev2 = i;

                        var i = uintptr(0L);

                        while (i < n)
                        {
                            var off = (uintptr(p) + i - datap.data) / sys.PtrSize;
                            mask[i / sys.PtrSize] = (addb(bitmap, off / 8L) >> (int)((off % 8L)).Value) & 1L;
                            i += sys.PtrSize;
                        }


                        i = i__prev2;
                    }
                    return;
                } 

                // bss
                if (datap.bss <= uintptr(p) && uintptr(p) < datap.ebss)
                {
                    bitmap = datap.gcbssmask.bytedata;
                    n = (ptrtype.Value)(@unsafe.Pointer(t)).elem.size;
                    mask = make_slice<byte>(n / sys.PtrSize);
                    {
                        var i__prev2 = i;

                        i = uintptr(0L);

                        while (i < n)
                        {
                            off = (uintptr(p) + i - datap.bss) / sys.PtrSize;
                            mask[i / sys.PtrSize] = (addb(bitmap, off / 8L) >> (int)((off % 8L)).Value) & 1L;
                            i += sys.PtrSize;
                        }


                        i = i__prev2;
                    }
                    return;
                }
            } 

            // heap
            n = default;
            System.UIntPtr @base = default;
            if (mlookup(uintptr(p), ref base, ref n, null) != 0L)
            {
                mask = make_slice<byte>(n / sys.PtrSize);
                {
                    var i__prev1 = i;

                    i = uintptr(0L);

                    while (i < n)
                    {
                        var hbits = heapBitsForAddr(base + i);
                        if (hbits.isPointer())
                        {
                            mask[i / sys.PtrSize] = 1L;
                        i += sys.PtrSize;
                        }
                        if (i != 1L * sys.PtrSize && !hbits.morePointers())
                        {
                            mask = mask[..i / sys.PtrSize];
                            break;
                        }
                    }


                    i = i__prev1;
                }
                return;
            } 

            // stack
            {
                var _g___prev1 = _g_;

                var _g_ = getg();

                if (_g_.m.curg.stack.lo <= uintptr(p) && uintptr(p) < _g_.m.curg.stack.hi)
                {
                    stkframe frame = default;
                    frame.sp = uintptr(p);
                    _g_ = getg();
                    gentraceback(_g_.m.curg.sched.pc, _g_.m.curg.sched.sp, 0L, _g_.m.curg, 0L, null, 1000L, getgcmaskcb, noescape(@unsafe.Pointer(ref frame)), 0L);
                    if (frame.fn.valid())
                    {
                        var f = frame.fn;
                        var targetpc = frame.continpc;
                        if (targetpc == 0L)
                        {
                            return;
                        }
                        if (targetpc != f.entry)
                        {
                            targetpc--;
                        }
                        var pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, targetpc, null);
                        if (pcdata == -1L)
                        {
                            return;
                        }
                        var stkmap = (stackmap.Value)(funcdata(f, _FUNCDATA_LocalsPointerMaps));
                        if (stkmap == null || stkmap.n <= 0L)
                        {
                            return;
                        }
                        var bv = stackmapdata(stkmap, pcdata);
                        var size = uintptr(bv.n) * sys.PtrSize;
                        n = (ptrtype.Value)(@unsafe.Pointer(t)).elem.size;
                        mask = make_slice<byte>(n / sys.PtrSize);
                        {
                            var i__prev1 = i;

                            i = uintptr(0L);

                            while (i < n)
                            {
                                bitmap = bv.bytedata;
                                off = (uintptr(p) + i - frame.varp + size) / sys.PtrSize;
                                mask[i / sys.PtrSize] = (addb(bitmap, off / 8L) >> (int)((off % 8L)).Value) & 1L;
                                i += sys.PtrSize;
                            }


                            i = i__prev1;
                        }
                    }
                    return;
                } 

                // otherwise, not something the GC knows about.
                // possibly read-only data, like malloc(0).
                // must not have pointers

                _g_ = _g___prev1;

            } 

            // otherwise, not something the GC knows about.
            // possibly read-only data, like malloc(0).
            // must not have pointers
            return;
        }
    }
}
