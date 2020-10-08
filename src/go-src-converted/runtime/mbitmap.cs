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
// The heap bitmap comprises 2 bits for each pointer-sized word in the heap,
// stored in the heapArena metadata backing each heap arena.
// That is, if ha is the heapArena for the arena starting a start,
// then ha.bitmap[0] holds the 2-bit entries for the four words start
// through start+3*ptrSize, ha.bitmap[1] holds the entries for
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

// package runtime -- go2cs converted at 2020 October 08 03:20:29 UTC
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
        private static readonly long bitPointer = (long)1L << (int)(0L);
        private static readonly long bitScan = (long)1L << (int)(4L);

        private static readonly long heapBitsShift = (long)1L; // shift offset between successive bitPointer or bitScan entries
        private static readonly long wordsPerBitmapByte = (long)8L / 2L; // heap words described by one bitmap byte

        // all scan/pointer bits in a byte
        private static readonly var bitScanAll = (var)bitScan | bitScan << (int)(heapBitsShift) | bitScan << (int)((2L * heapBitsShift)) | bitScan << (int)((3L * heapBitsShift));
        private static readonly var bitPointerAll = (var)bitPointer | bitPointer << (int)(heapBitsShift) | bitPointer << (int)((2L * heapBitsShift)) | bitPointer << (int)((3L * heapBitsShift));


        // addb returns the byte pointer p+n.
        //go:nowritebarrier
        //go:nosplit
        private static ptr<byte> addb(ptr<byte> _addr_p, System.UIntPtr n)
        {
            ref byte p = ref _addr_p.val;
 
            // Note: wrote out full expression instead of calling add(p, n)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return _addr_(byte.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + n))!;

        }

        // subtractb returns the byte pointer p-n.
        //go:nowritebarrier
        //go:nosplit
        private static ptr<byte> subtractb(ptr<byte> _addr_p, System.UIntPtr n)
        {
            ref byte p = ref _addr_p.val;
 
            // Note: wrote out full expression instead of calling add(p, -n)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return _addr_(byte.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) - n))!;

        }

        // add1 returns the byte pointer p+1.
        //go:nowritebarrier
        //go:nosplit
        private static ptr<byte> add1(ptr<byte> _addr_p)
        {
            ref byte p = ref _addr_p.val;
 
            // Note: wrote out full expression instead of calling addb(p, 1)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return _addr_(byte.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + 1L))!;

        }

        // subtract1 returns the byte pointer p-1.
        //go:nowritebarrier
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static ptr<byte> subtract1(ptr<byte> _addr_p)
        {
            ref byte p = ref _addr_p.val;
 
            // Note: wrote out full expression instead of calling subtractb(p, 1)
            // to reduce the number of temporaries generated by the
            // compiler for this trivial expression during inlining.
            return _addr_(byte.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) - 1L))!;

        }

        // heapBits provides access to the bitmap bits for a single heap word.
        // The methods on heapBits take value receivers so that the compiler
        // can more easily inline calls to those methods and registerize the
        // struct fields independently.
        private partial struct heapBits
        {
            public ptr<byte> bitp;
            public uint shift;
            public uint arena; // Index of heap arena containing bitp
            public ptr<byte> last; // Last byte arena's bitmap
        }

        // Make the compiler check that heapBits.arena is large enough to hold
        // the maximum arena frame number.
        private static heapBits _ = new heapBits(arena:(1<<heapAddrBits)/heapArenaBytes-1);

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
        private static markBits allocBitsForIndex(this ptr<mspan> _addr_s, System.UIntPtr allocBitIndex)
        {
            ref mspan s = ref _addr_s.val;

            var (bytep, mask) = s.allocBits.bitp(allocBitIndex);
            return new markBits(bytep,mask,allocBitIndex);
        }

        // refillAllocCache takes 8 bytes s.allocBits starting at whichByte
        // and negates them so that ctz (count trailing zeros) instructions
        // can be used. It then places these 8 bytes into the cached 64 bit
        // s.allocCache.
        private static void refillAllocCache(this ptr<mspan> _addr_s, System.UIntPtr whichByte)
        {
            ref mspan s = ref _addr_s.val;

            ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(s.allocBits.bytep(whichByte)));
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
        private static System.UIntPtr nextFreeIndex(this ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

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

        // isFree reports whether the index'th object in s is unallocated.
        //
        // The caller must ensure s.state is mSpanInUse, and there must have
        // been no preemption points since ensuring this (which could allow a
        // GC transition, which would allow the state to change).
        private static bool isFree(this ptr<mspan> _addr_s, System.UIntPtr index)
        {
            ref mspan s = ref _addr_s.val;

            if (index < s.freeindex)
            {
                return false;
            }

            var (bytep, mask) = s.allocBits.bitp(index);
            return bytep & mask == 0L.val;

        }

        private static System.UIntPtr objIndex(this ptr<mspan> _addr_s, System.UIntPtr p)
        {
            ref mspan s = ref _addr_s.val;

            var byteOffset = p - s.@base();
            if (byteOffset == 0L)
            {
                return 0L;
            }

            if (s.baseMask != 0L)
            { 
                // s.baseMask is non-0, elemsize is a power of two, so shift by s.divShift
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

        private static markBits markBitsForIndex(this ptr<mspan> _addr_s, System.UIntPtr objIndex)
        {
            ref mspan s = ref _addr_s.val;

            var (bytep, mask) = s.gcmarkBits.bitp(objIndex);
            return new markBits(bytep,mask,objIndex);
        }

        private static markBits markBitsForBase(this ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            return new markBits((*uint8)(s.gcmarkBits),uint8(1),0);
        }

        // isMarked reports whether mark bit m is set.
        private static bool isMarked(this markBits m)
        {
            return m.bytep & m.mask != 0L.val;
        }

        // setMarked sets the marked bit in the markbits, atomically.
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
            m.bytep.val |= m.mask;
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
            markBits mbits = default;

            mbits = markBitsForAddr(base);
            if (mbits.mask != 1L)
            {
                throw("markBitsForSpan: unaligned start");
            }

            return mbits;

        }

        // advance advances the markBits to the next object in the span.
        private static void advance(this ptr<markBits> _addr_m)
        {
            ref markBits m = ref _addr_m.val;

            if (m.mask == 1L << (int)(7L))
            {
                m.bytep = (uint8.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(m.bytep)) + 1L));
                m.mask = 1L;
            }
            else
            {
                m.mask = m.mask << (int)(1L);
            }

            m.index++;

        }

        // heapBitsForAddr returns the heapBits for the address addr.
        // The caller must ensure addr is in an allocated span.
        // In particular, be careful not to point past the end of an object.
        //
        // nosplit because it is used during write barriers and must not be preempted.
        //go:nosplit
        private static heapBits heapBitsForAddr(System.UIntPtr addr)
        {
            heapBits h = default;
 
            // 2 bits per word, 4 pairs per byte, and a mask is hard coded.
            var arena = arenaIndex(addr);
            var ha = mheap_.arenas[arena.l1()][arena.l2()]; 
            // The compiler uses a load for nil checking ha, but in this
            // case we'll almost never hit that cache line again, so it
            // makes more sense to do a value check.
            if (ha == null)
            { 
                // addr is not in the heap. Return nil heapBits, which
                // we expect to crash in the caller.
                return ;

            }

            h.bitp = _addr_ha.bitmap[(addr / (sys.PtrSize * 4L)) % heapArenaBitmapBytes];
            h.shift = uint32((addr / sys.PtrSize) & 3L);
            h.arena = uint32(arena);
            h.last = _addr_ha.bitmap[len(ha.bitmap) - 1L];
            return ;

        }

        // badPointer throws bad pointer in heap panic.
        private static void badPointer(ptr<mspan> _addr_s, System.UIntPtr p, System.UIntPtr refBase, System.UIntPtr refOff)
        {
            ref mspan s = ref _addr_s.val;
 
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
            var state = s.state.get();
            if (state != mSpanInUse)
            {
                print(" to unallocated span");
            }
            else
            {
                print(" to unused region of span");
            }

            print(" span.base()=", hex(s.@base()), " span.limit=", hex(s.limit), " span.state=", state, "\n");
            if (refBase != 0L)
            {
                print("runtime: found in object at *(", hex(refBase), "+", hex(refOff), ")\n");
                gcDumpObject("object", refBase, refOff);
            }

            getg().m.traceback = 2L;
            throw("found bad pointer in Go heap (incorrect use of unsafe or cgo?)");

        }

        // findObject returns the base address for the heap object containing
        // the address p, the object's span, and the index of the object in s.
        // If p does not point into a heap object, it returns base == 0.
        //
        // If p points is an invalid heap pointer and debug.invalidptr != 0,
        // findObject panics.
        //
        // refBase and refOff optionally give the base address of the object
        // in which the pointer p was found and the byte offset at which it
        // was found. These are used for error reporting.
        //
        // It is nosplit so it is safe for p to be a pointer to the current goroutine's stack.
        // Since p is a uintptr, it would not be adjusted if the stack were to move.
        //go:nosplit
        private static (System.UIntPtr, ptr<mspan>, System.UIntPtr) findObject(System.UIntPtr p, System.UIntPtr refBase, System.UIntPtr refOff)
        {
            System.UIntPtr @base = default;
            ptr<mspan> s = default!;
            System.UIntPtr objIndex = default;

            s = spanOf(p); 
            // If s is nil, the virtual address has never been part of the heap.
            // This pointer may be to some mmap'd region, so we allow it.
            if (s == null)
            {
                return ;
            } 
            // If p is a bad pointer, it may not be in s's bounds.
            //
            // Check s.state to synchronize with span initialization
            // before checking other fields. See also spanOfHeap.
            {
                var state = s.state.get();

                if (state != mSpanInUse || p < s.@base() || p >= s.limit)
                { 
                    // Pointers into stacks are also ok, the runtime manages these explicitly.
                    if (state == mSpanManual)
                    {
                        return ;
                    } 
                    // The following ensures that we are rigorous about what data
                    // structures hold valid pointers.
                    if (debug.invalidptr != 0L)
                    {
                        badPointer(_addr_s, p, refBase, refOff);
                    }

                    return ;

                } 
                // If this span holds object of a power of 2 size, just mask off the bits to
                // the interior of the object. Otherwise use the size to get the base.

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

            return ;

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
                h.shift += heapBitsShift;
            }
            else if (h.bitp != h.last)
            {
                h.bitp = add1(_addr_h.bitp);
                h.shift = 0L;

            }
            else
            { 
                // Move to the next arena.
                return h.nextArena();

            }

            return h;

        }

        // nextArena advances h to the beginning of the next heap arena.
        //
        // This is a slow-path helper to next. gc's inliner knows that
        // heapBits.next can be inlined even though it calls this. This is
        // marked noinline so it doesn't get inlined into next and cause next
        // to be too big to inline.
        //
        //go:nosplit
        //go:noinline
        private static heapBits nextArena(this heapBits h)
        {
            h.arena++;
            var ai = arenaIdx(h.arena);
            var l2 = mheap_.arenas[ai.l1()];
            if (l2 == null)
            { 
                // We just passed the end of the object, which
                // was also the end of the heap. Poison h. It
                // should never be dereferenced at this point.
                return new heapBits();

            }

            var ha = l2[ai.l2()];
            if (ha == null)
            {
                return new heapBits();
            }

            h.bitp = _addr_ha.bitmap[0L];
            h.shift = 0L;
            h.last = _addr_ha.bitmap[len(ha.bitmap) - 1L];
            return h;

        }

        // forward returns the heapBits describing n pointer-sized words ahead of h in memory.
        // That is, if h describes address p, h.forward(n) describes p+n*ptrSize.
        // h.forward(1) is equivalent to h.next(), just slower.
        // Note that forward does not modify h. The caller must record the result.
        // bits returns the heap bits for the current word.
        //go:nosplit
        private static heapBits forward(this heapBits h, System.UIntPtr n)
        {
            n += uintptr(h.shift) / heapBitsShift;
            var nbitp = uintptr(@unsafe.Pointer(h.bitp)) + n / 4L;
            h.shift = uint32(n % 4L) * heapBitsShift;
            if (nbitp <= uintptr(@unsafe.Pointer(h.last)))
            {
                h.bitp = (uint8.val)(@unsafe.Pointer(nbitp));
                return h;
            } 

            // We're in a new heap arena.
            var past = nbitp - (uintptr(@unsafe.Pointer(h.last)) + 1L);
            h.arena += 1L + uint32(past / heapArenaBitmapBytes);
            var ai = arenaIdx(h.arena);
            {
                var l2 = mheap_.arenas[ai.l1()];

                if (l2 != null && l2[ai.l2()] != null)
                {
                    var a = l2[ai.l2()];
                    h.bitp = _addr_a.bitmap[past % heapArenaBitmapBytes];
                    h.last = _addr_a.bitmap[len(a.bitmap) - 1L];
                }
                else
                {
                    h.bitp = null;
                    h.last = null;

                }

            }

            return h;

        }

        // forwardOrBoundary is like forward, but stops at boundaries between
        // contiguous sections of the bitmap. It returns the number of words
        // advanced over, which will be <= n.
        private static (heapBits, System.UIntPtr) forwardOrBoundary(this heapBits h, System.UIntPtr n)
        {
            heapBits _p0 = default;
            System.UIntPtr _p0 = default;

            long maxn = 4L * ((uintptr(@unsafe.Pointer(h.last)) + 1L) - uintptr(@unsafe.Pointer(h.bitp)));
            if (n > maxn)
            {
                n = maxn;
            }

            return (h.forward(n), n);

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
            return uint32(h.bitp.val) >> (int)((h.shift & 31L));

        }

        // morePointers reports whether this word and all remaining words in this object
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
                return (h.bitp >> (int)(h.shift).val) & bitPointer != 0L;
            } 
            // All multiword objects are 2-word aligned,
            // so we know that the initial word's 2-bit pair
            // and the second word's 2-bit pair are in the
            // same heap bitmap byte, *h.bitp.
            return (h.bitp >> (int)((heapBitsShift + h.shift)).val) & bitScan != 0L;

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
                return ;
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
        // by checking typ.ptrdata.
        //
        // Callers must perform cgo checks if writeBarrier.cgo.
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
                return ;
            }

            {
                var s = spanOf(dst);

                if (s == null)
                { 
                    // If dst is a global, use the data or BSS bitmaps to
                    // execute write barriers.
                    {
                        var datap__prev1 = datap;

                        foreach (var (_, __datap) in activeModules())
                        {
                            datap = __datap;
                            if (datap.data <= dst && dst < datap.edata)
                            {
                                bulkBarrierBitmap(dst, src, size, dst - datap.data, _addr_datap.gcdatamask.bytedata);
                                return ;
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
                                bulkBarrierBitmap(dst, src, size, dst - datap.bss, _addr_datap.gcbssmask.bytedata);
                                return ;
                            }

                        }

                        datap = datap__prev1;
                    }

                    return ;

                }
                else if (s.state.get() != mSpanInUse || dst < s.@base() || s.limit <= dst)
                { 
                    // dst was heap memory at some point, but isn't now.
                    // It can't be a global. It must be either our stack,
                    // or in the case of direct channel sends, it could be
                    // another stack. Either way, no need for barriers.
                    // This will also catch if dst is in a freed span,
                    // though that should never have.
                    return ;

                }


            }


            var buf = _addr_getg().m.p.ptr().wbBuf;
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
                            var dstx = (uintptr.val)(@unsafe.Pointer(dst + i));
                            if (!buf.putFast(dstx.val, 0L))
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
                            dstx = (uintptr.val)(@unsafe.Pointer(dst + i));
                            var srcx = (uintptr.val)(@unsafe.Pointer(src + i));
                            if (!buf.putFast(dstx.val, srcx.val))
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

        // bulkBarrierPreWriteSrcOnly is like bulkBarrierPreWrite but
        // does not execute write barriers for [dst, dst+size).
        //
        // In addition to the requirements of bulkBarrierPreWrite
        // callers need to ensure [dst, dst+size) is zeroed.
        //
        // This is used for special cases where e.g. dst was just
        // created and zeroed with malloc.
        //go:nosplit
        private static void bulkBarrierPreWriteSrcOnly(System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size)
        {
            if ((dst | src | size) & (sys.PtrSize - 1L) != 0L)
            {
                throw("bulkBarrierPreWrite: unaligned arguments");
            }

            if (!writeBarrier.needed)
            {
                return ;
            }

            var buf = _addr_getg().m.p.ptr().wbBuf;
            var h = heapBitsForAddr(dst);
            {
                var i = uintptr(0L);

                while (i < size)
                {
                    if (h.isPointer())
                    {
                        var srcx = (uintptr.val)(@unsafe.Pointer(src + i));
                        if (!buf.putFast(0L, srcx.val))
                        {
                            wbBufFlush(null, 0L);
                    i += sys.PtrSize;
                        }

                    }

                    h = h.next();

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
        private static void bulkBarrierBitmap(System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size, System.UIntPtr maskOffset, ptr<byte> _addr_bits)
        {
            ref byte bits = ref _addr_bits.val;

            var word = maskOffset / sys.PtrSize;
            bits = addb(_addr_bits, word / 8L);
            var mask = uint8(1L) << (int)((word % 8L));

            var buf = _addr_getg().m.p.ptr().wbBuf;
            {
                var i = uintptr(0L);

                while (i < size)
                {
                    if (mask == 0L)
                    {
                        bits = addb(_addr_bits, 1L);
                        if (bits == 0L.val)
                        { 
                            // Skip 8 words.
                            i += 7L * sys.PtrSize;
                            continue;
                    i += sys.PtrSize;
                        }

                        mask = 1L;

                    }

                    if (bits & mask != 0L.val)
                    {
                        var dstx = (uintptr.val)(@unsafe.Pointer(dst + i));
                        if (src == 0L)
                        {
                            if (!buf.putFast(dstx.val, 0L))
                            {
                                wbBufFlush(null, 0L);
                            }

                        }
                        else
                        {
                            var srcx = (uintptr.val)(@unsafe.Pointer(src + i));
                            if (!buf.putFast(dstx.val, srcx.val))
                            {
                                wbBufFlush(null, 0L);
                            }

                        }

                    }

                    mask <<= 1L;

                }

            }

        }

        // typeBitsBulkBarrier executes a write barrier for every
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
        // Callers must perform cgo checks if writeBarrier.cgo.
        //
        //go:nosplit
        private static void typeBitsBulkBarrier(ptr<_type> _addr_typ, System.UIntPtr dst, System.UIntPtr src, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;

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
                return ;
            }

            var ptrmask = typ.gcdata;
            var buf = _addr_getg().m.p.ptr().wbBuf;
            uint bits = default;
            {
                var i = uintptr(0L);

                while (i < typ.ptrdata)
                {
                    if (i & (sys.PtrSize * 8L - 1L) == 0L)
                    {
                        bits = uint32(ptrmask.val);
                        ptrmask = addb(_addr_ptrmask, 1L);
                    i += sys.PtrSize;
                    }
                    else
                    {
                        bits = bits >> (int)(1L);
                    }

                    if (bits & 1L != 0L)
                    {
                        var dstx = (uintptr.val)(@unsafe.Pointer(dst + i));
                        var srcx = (uintptr.val)(@unsafe.Pointer(src + i));
                        if (!buf.putFast(dstx.val, srcx.val))
                        {
                            wbBufFlush(null, 0L);
                        }

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
        private static void initSpan(this heapBits h, ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;
 
            // Clear bits corresponding to objects.
            var nw = (s.npages << (int)(_PageShift)) / sys.PtrSize;
            if (nw % wordsPerBitmapByte != 0L)
            {
                throw("initSpan: unaligned length");
            }

            if (h.shift != 0L)
            {
                throw("initSpan: unaligned base");
            }

            var isPtrs = sys.PtrSize == 8L && s.elemsize == sys.PtrSize;
            while (nw > 0L)
            {
                var (hNext, anw) = h.forwardOrBoundary(nw);
                var nbyte = anw / wordsPerBitmapByte;
                if (isPtrs)
                {
                    var bitp = h.bitp;
                    for (var i = uintptr(0L); i < nbyte; i++)
                    {
                        bitp.val = bitPointerAll | bitScanAll;
                        bitp = add1(_addr_bitp);
                    }
                else


                }                {
                    memclrNoHeapPointers(@unsafe.Pointer(h.bitp), nbyte);
                }

                h = hNext;
                nw -= anw;

            }


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
                {
                    var i__prev1 = i;

                    var i = uintptr(0L);

                    while (i < n)
                    {
                        h.bitp.val &= bitPointerAll;
                        h = h.forward(wordsPerBitmapByte);
                        i += wordsPerBitmapByte;
                    }


                    i = i__prev1;
                }
                return ;

            }

            {
                var i__prev1 = i;

                for (i = uintptr(0L); i < n; i++)
                {
                    h.bitp.val &= bitScan << (int)((heapBitsShift + h.shift));
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
                {
                    var i = uintptr(0L);

                    while (i < n)
                    {
                        h.bitp.val |= bitPointerAll;
                        h = h.forward(wordsPerBitmapByte);
                        i += wordsPerBitmapByte;
                    }

                }

            }

        }

        // countAlloc returns the number of objects allocated in span s by
        // scanning the allocation bitmap.
        private static long countAlloc(this ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            long count = 0L;
            var bytes = divRoundUp(s.nelems, 8L); 
            // Iterate over each 8-byte chunk and count allocations
            // with an intrinsic. Note that newMarkBits guarantees that
            // gcmarkBits will be 8-byte aligned, so we don't have to
            // worry about edge cases, irrelevant bits will simply be zero.
            {
                var i = uintptr(0L);

                while (i < bytes)
                { 
                    // Extract 64 bits from the byte pointer and get a OnesCount.
                    // Note that the unsafe cast here doesn't preserve endianness,
                    // but that's OK. We only care about how many bits are 1, not
                    // about the order we discover them in.
                    ptr<ptr<ulong>> mrkBits = new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(s.gcmarkBits.bytep(i)));
                    count += sys.OnesCount64(mrkBits);
                    i += 8L;
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
        private static void heapBitsSetType(System.UIntPtr x, System.UIntPtr size, System.UIntPtr dataSize, ptr<_type> _addr_typ)
        {
            ref _type typ = ref _addr_typ.val;

            const var doubleCheck = (var)false; // slow but helpful; enable to test modifications to this code

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

                return ;

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
                        h.bitp.val &= (bitPointer | bitScan | ((bitPointer | bitScan) << (int)(heapBitsShift))) << (int)(h.shift);
                        h.bitp.val |= (bitPointer | bitScan) << (int)(h.shift);

                    }
                    else
                    { 
                        // 2-element slice of pointer.
                        h.bitp.val |= (bitPointer | bitScan | bitPointer << (int)(heapBitsShift)) << (int)(h.shift);

                    }

                    return ;

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

                var b = uint32(ptrmask.val);
                var hb = (b & 3L) | bitScan; 
                // bitPointer == 1, bitScan is 1 << 4, heapBitsShift is 1.
                // 110011 is shifted h.shift and complemented.
                // This clears out the bits that are about to be
                // ored into *h.hbitp in the next instructions.
                h.bitp.val &= (bitPointer | bitScan | ((bitPointer | bitScan) << (int)(heapBitsShift))) << (int)(h.shift);
                h.bitp.val |= uint8(hb << (int)(h.shift));
                return ;

            } 

            // Copy from 1-bit ptrmask into 2-bit bitmap.
            // The basic approach is to use a single uintptr as a bit buffer,
            // alternating between reloading the buffer and writing bitmap bytes.
            // In general, one load can supply two bitmap byte writes.
            // This is a lot of lines of code, but it compiles into relatively few
            // machine instructions.
            var outOfPlace = false;
            if (arenaIndex(x + size - 1L) != arenaIdx(h.arena) || (doubleCheck && fastrand() % 2L == 0L))
            { 
                // This object spans heap arenas, so the bitmap may be
                // discontiguous. Unroll it into the object instead
                // and then copy it out.
                //
                // In doubleCheck mode, we randomly do this anyway to
                // stress test the bitmap copying path.
                outOfPlace = true;
                h.bitp = (uint8.val)(@unsafe.Pointer(x));
                h.last = null;

            }

 
            // Ptrmask input.
            ptr<byte> p;            b = default;            System.UIntPtr nb = default;            ptr<byte> endp;            System.UIntPtr endnb = default;            System.UIntPtr pbits = default;            System.UIntPtr w = default;            System.UIntPtr nw = default;            ptr<byte> hbitp;            hb = default;

            hbitp = h.bitp; 

            // Handle GC program. Delayed until this part of the code
            // so that we can use the same double-checking mechanism
            // as the 1-bit case. Nothing above could have encountered
            // GC programs: the cases were all too small.
            if (typ.kind & kindGCProg != 0L)
            {
                heapBitsSetTypeGCProg(h, typ.ptrdata, typ.size, dataSize, size, _addr_addb(_addr_typ.gcdata, 4L));
                if (doubleCheck)
                { 
                    // Double-check the heap bits written by GC program
                    // by running the GC program to create a 1-bit pointer mask
                    // and then jumping to the double-check code below.
                    // This doesn't catch bugs shared between the 1-bit and 4-bit
                    // GC program execution, but it does catch mistakes specific
                    // to just one of those and bugs in heapBitsSetTypeGCProg's
                    // implementation of arrays.
                    lock(_addr_debugPtrmask.@lock);
                    if (debugPtrmask.data == null)
                    {
                        debugPtrmask.data = (byte.val)(persistentalloc(1L << (int)(20L), 1L, _addr_memstats.other_sys));
                    }

                    ptrmask = debugPtrmask.data;
                    runGCProg(_addr_addb(_addr_typ.gcdata, 4L), _addr_null, _addr_ptrmask, 1L);

                }

                goto Phase4;

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
                const var maxBits = (var)sys.PtrSize * 8L - 7L;

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
                            b |= uintptr(p.val) << (int)(i);
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
                    endp = addb(_addr_ptrmask, n);
                    endnb = typ.size / sys.PtrSize - n * 8L;

                }

            }

            if (p != null)
            {
                b = uintptr(p.val);
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
                return ;

            }

            if (nw < 2L)
            { 
                // Must write at least 2 words, because the "no scan"
                // encoding doesn't take effect until the third word.
                nw = 2L;

            } 

            // Phase 1: Special case for leading byte (shift==0) or half-byte (shift==2).
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

                hbitp.val = uint8(hb);
                hbitp = add1(hbitp);
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
                hbitp.val &= uint8((bitPointer | bitScan | (bitPointer << (int)(heapBitsShift))) << (int)((2L * heapBitsShift)));
                hbitp.val |= uint8(hb);
                hbitp = add1(hbitp);
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

                hbitp.val = uint8(hb);
                hbitp = add1(hbitp);
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
                        b |= uintptr(p.val) << (int)(nb);
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
                    b |= uintptr(p.val) << (int)(nb);
                    nb += endnb;
                    if (nb < 8L)
                    {
                        b |= uintptr(ptrmask.val) << (int)(nb);
                        p = add1(_addr_ptrmask);
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

                hbitp.val = uint8(hb);
                hbitp = add1(hbitp);
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
                hbitp.val = uint8(hb);
                hbitp = add1(hbitp);
                hb = 0L; // for possible final half-byte below
                w += 4L;

                while (w <= nw)
                {
                    hbitp.val = 0L;
                    hbitp = add1(hbitp);
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
                hbitp.val = hbitp & ~(bitPointer | bitScan | (bitPointer | bitScan) << (int)(heapBitsShift)) | uint8(hb).val;
            }

Phase4: 

            // Double check the whole bitmap.
            if (outOfPlace)
            { 
                // TODO: We could probably make this faster by
                // handling [x+dataSize, x+size) specially.
                h = heapBitsForAddr(x); 
                // cnw is the number of heap words, or bit pairs
                // remaining (like nw above).
                var cnw = size / sys.PtrSize;
                var src = (uint8.val)(@unsafe.Pointer(x)); 
                // We know the first and last byte of the bitmap are
                // not the same, but it's still possible for small
                // objects span arenas, so it may share bitmap bytes
                // with neighboring objects.
                //
                // Handle the first byte specially if it's shared. See
                // Phase 1 for why this is the only special case we need.
                if (doubleCheck)
                {
                    if (!(h.shift == 0L || (sys.PtrSize == 8L && h.shift == 2L)))
                    {
                        print("x=", x, " size=", size, " cnw=", h.shift, "\n");
                        throw("bad start shift");
                    }

                }

                if (sys.PtrSize == 8L && h.shift == 2L)
                {
                    h.bitp.val = h.bitp & ~((bitPointer | bitScan | (bitPointer | bitScan) << (int)(heapBitsShift)) << (int)((2L * heapBitsShift))) | src.val;
                    h = h.next().next();
                    cnw -= 2L;
                    src = addb(_addr_src, 1L);
                } 
                // We're now byte aligned. Copy out to per-arena
                // bitmaps until the last byte (which may again be
                // partial).
                while (cnw >= 4L)
                { 
                    // This loop processes four words at a time,
                    // so round cnw down accordingly.
                    var (hNext, words) = h.forwardOrBoundary(cnw / 4L * 4L); 

                    // n is the number of bitmap bytes to copy.
                    n = words / 4L;
                    memmove(@unsafe.Pointer(h.bitp), @unsafe.Pointer(src), n);
                    cnw -= words;
                    h = hNext;
                    src = addb(_addr_src, n);

                }

                if (doubleCheck && h.shift != 0L)
                {
                    print("cnw=", cnw, " h.shift=", h.shift, "\n");
                    throw("bad shift after block copy");
                } 
                // Handle the last byte if it's shared.
                if (cnw == 2L)
                {
                    h.bitp.val = h.bitp & ~(bitPointer | bitScan | (bitPointer | bitScan) << (int)(heapBitsShift)) | src.val;
                    src = addb(_addr_src, 1L);
                    h = h.next().next();
                }

                if (doubleCheck)
                {
                    if (uintptr(@unsafe.Pointer(src)) > x + size)
                    {
                        throw("copy exceeded object size");
                    }

                    if (!(cnw == 0L || cnw == 2L))
                    {
                        print("x=", x, " size=", size, " cnw=", cnw, "\n");
                        throw("bad number of remaining words");
                    } 
                    // Set up hbitp so doubleCheck code below can check it.
                    hbitp = h.bitp;

                } 
                // Zero the object where we wrote the bitmap.
                memclrNoHeapPointers(@unsafe.Pointer(x), uintptr(@unsafe.Pointer(src)) - x);

            } 

            // Double check the whole bitmap.
            if (doubleCheck)
            { 
                // x+size may not point to the heap, so back up one
                // word and then call next().
                var end = heapBitsForAddr(x + size - sys.PtrSize).next();
                var endAI = arenaIdx(end.arena);
                if (!outOfPlace && (end.bitp == null || (end.shift == 0L && end.bitp == _addr_mheap_.arenas[endAI.l1()][endAI.l2()].bitmap[0L])))
                { 
                    // The unrolling code above walks hbitp just
                    // past the bitmap without moving to the next
                    // arena. Synthesize this for end.bitp.
                    end.arena--;
                    endAI = arenaIdx(end.arena);
                    end.bitp = addb(_addr_mheap_.arenas[endAI.l1()][endAI.l2()].bitmap[0L], heapArenaBitmapBytes);
                    end.last = null;

                }

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

                        have = (h.bitp >> (int)(h.shift).val) & (bitPointer | bitScan);
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
                            if (j < nptr && (addb(_addr_ptrmask, j / 8L) >> (int)((j % 8L)).val) & 1L != 0L)
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
                            print("kindGCProg=", typ.kind & kindGCProg != 0L, " outOfPlace=", outOfPlace, "\n");
                            print("w=", w, " nw=", nw, " b=", hex(b), " nb=", nb, " hb=", hex(hb), "\n");
                            h0 = heapBitsForAddr(x);
                            print("initial bits h0.bitp=", h0.bitp, " h0.shift=", h0.shift, "\n");
                            print("current bits h.bitp=", h.bitp, " h.shift=", h.shift, " *h.bitp=", hex(h.bitp.val), "\n");
                            print("ptrmask=", ptrmask, " p=", p, " endp=", endp, " endnb=", endnb, " pbits=", hex(pbits), " b=", hex(b), " nb=", nb, "\n");
                            println("at word", i, "offset", i * sys.PtrSize, "have", hex(have), "want", hex(want));
                            if (typ.kind & kindGCProg != 0L)
                            {
                                println("GC program:");
                                dumpGCProg(_addr_addb(_addr_typ.gcdata, 4L));
                            }

                            throw("bad heapBitsSetType");

                        }

                        h = h.next();

                    }


                    i = i__prev1;
                }
                if (ptrmask == debugPtrmask.data)
                {
                    unlock(_addr_debugPtrmask.@lock);
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
        private static void heapBitsSetTypeGCProg(heapBits h, System.UIntPtr progSize, System.UIntPtr elemSize, System.UIntPtr dataSize, System.UIntPtr allocSize, ptr<byte> _addr_prog)
        {
            ref byte prog = ref _addr_prog.val;

            if (sys.PtrSize == 8L && allocSize % (4L * sys.PtrSize) != 0L)
            { 
                // Alignment will be wrong.
                throw("heapBitsSetTypeGCProg: small allocation");

            }

            System.UIntPtr totalBits = default;
            if (elemSize == dataSize)
            {
                totalBits = runGCProg(_addr_prog, _addr_null, _addr_h.bitp, 2L);
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

                runGCProg(_addr_prog, _addr_trailer[0L], _addr_h.bitp, 2L); 

                // Even though we filled in the full array just now,
                // record that we only filled in up to the ptrdata of the
                // last element. This will cause the code below to
                // memclr the dead section of the final array element,
                // so that scanobject can stop early in the final element.
                totalBits = (elemSize * (count - 1L) + progSize) / sys.PtrSize;

            }

            var endProg = @unsafe.Pointer(addb(_addr_h.bitp, (totalBits + 3L) / 4L));
            var endAlloc = @unsafe.Pointer(addb(_addr_h.bitp, allocSize / sys.PtrSize / wordsPerBitmapByte));
            memclrNoHeapPointers(endProg, uintptr(endAlloc) - uintptr(endProg));

        }

        // progToPointerMask returns the 1-bit pointer mask output by the GC program prog.
        // size the size of the region described by prog, in bytes.
        // The resulting bitvector will have no more than size/sys.PtrSize bits.
        private static bitvector progToPointerMask(ptr<byte> _addr_prog, System.UIntPtr size)
        {
            ref byte prog = ref _addr_prog.val;

            var n = (size / sys.PtrSize + 7L) / 8L;
            ptr<array<byte>> x = new ptr<ptr<array<byte>>>(persistentalloc(n + 1L, 1L, _addr_memstats.buckhash_sys))[..n + 1L];
            x[len(x) - 1L] = 0xa1UL; // overflow check sentinel
            n = runGCProg(_addr_prog, _addr_null, _addr_x[0L], 1L);
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
        private static System.UIntPtr runGCProg(ptr<byte> _addr_prog, ptr<byte> _addr_trailer, ptr<byte> _addr_dst, long size)
        {
            ref byte prog = ref _addr_prog.val;
            ref byte trailer = ref _addr_trailer.val;
            ref byte dst = ref _addr_dst.val;

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
                        dst = uint8(bits);
                        dst = add1(_addr_dst);
                        bits >>= 8L;
                    nbits -= 8L;
                    }
                    else
                    {
                        var v = bits & bitPointerAll | bitScanAll;
                        dst = uint8(v);
                        dst = add1(_addr_dst);
                        bits >>= 4L;
                        v = bits & bitPointerAll | bitScanAll;
                        dst = uint8(v);
                        dst = add1(_addr_dst);
                        bits >>= 4L;
                    }

                } 

                // Process one instruction.
 

                // Process one instruction.
                var inst = uintptr(p.val);
                p = add1(_addr_p);
                var n = inst & 0x7FUL;
                if (inst & 0x80UL == 0L)
                { 
                    // Literal bits; n == 0 means end of program.
                    if (n == 0L)
                    { 
                        // Program is over; continue in trailer if present.
                        if (trailer != null)
                        {
                            p = trailer;
                            trailer = null;
                            continue;
                        }

                        _breakRun = true;
                        break;
                    }

                    var nbyte = n / 8L;
                    {
                        var i__prev2 = i;

                        for (var i = uintptr(0L); i < nbyte; i++)
                        {
                            bits |= uintptr(p.val) << (int)(nbits);
                            p = add1(_addr_p);
                            if (size == 1L)
                            {
                                dst = uint8(bits);
                                dst = add1(_addr_dst);
                                bits >>= 8L;
                            }
                            else
                            {
                                v = bits & 0xfUL | bitScanAll;
                                dst = uint8(v);
                                dst = add1(_addr_dst);
                                bits >>= 4L;
                                v = bits & 0xfUL | bitScanAll;
                                dst = uint8(v);
                                dst = add1(_addr_dst);
                                bits >>= 4L;
                            }

                        }


                        i = i__prev2;
                    }
                    n %= 8L;

                    if (n > 0L)
                    {
                        bits |= uintptr(p.val) << (int)(nbits);
                        p = add1(_addr_p);
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
                            var x = uintptr(p.val);
                            p = add1(_addr_p);
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
                        x = uintptr(p.val);
                        p = add1(_addr_p);
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
                const var maxBits = (var)sys.PtrSize * 8L - 7L;

                if (n <= maxBits)
                { 
                    // Start with bits in output buffer.
                    var pattern = bits;
                    var npattern = nbits; 

                    // If we need more bits, fetch them from memory.
                    if (size == 1L)
                    {
                        src = subtract1(_addr_src);
                        while (npattern < n)
                        {
                            pattern <<= 8L;
                            pattern |= uintptr(src.val);
                            src = subtract1(_addr_src);
                            npattern += 8L;
                        }
                    else


                    }                    {
                        src = subtract1(_addr_src);
                        while (npattern < n)
                        {
                            pattern <<= 4L;
                            pattern |= uintptr(src.val) & 0xfUL;
                            src = subtract1(_addr_src);
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
                                dst = uint8(bits);
                                dst = add1(_addr_dst);
                                bits >>= 8L;
                                nbits -= 8L;
                        c -= npattern;
                            }
                        else


                        }                        {
                            while (nbits >= 4L)
                            {
                                dst = uint8(bits & 0xfUL | bitScanAll);
                                dst = add1(_addr_dst);
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
                    src = subtractb(_addr_src, (off + 7L) / 8L);
                    {
                        var frag__prev2 = frag;

                        var frag = off & 7L;

                        if (frag != 0L)
                        {
                            bits |= uintptr(src.val) >> (int)((8L - frag)) << (int)(nbits);
                            src = add1(_addr_src);
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
                            bits |= uintptr(src.val) << (int)(nbits);
                            src = add1(_addr_src);
                            dst = uint8(bits);
                            dst = add1(_addr_dst);
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
                        bits |= (uintptr(src.val) & (1L << (int)(c) - 1L)) << (int)(nbits);
                        nbits += c;
                    }

                }                { 
                    // Leading src fragment.
                    src = subtractb(_addr_src, (off + 3L) / 4L);
                    {
                        var frag__prev2 = frag;

                        frag = off & 3L;

                        if (frag != 0L)
                        {
                            bits |= (uintptr(src.val) & 0xfUL) >> (int)((4L - frag)) << (int)(nbits);
                            src = add1(_addr_src);
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
                            bits |= (uintptr(src.val) & 0xfUL) << (int)(nbits);
                            src = add1(_addr_src);
                            dst = uint8(bits & 0xfUL | bitScanAll);
                            dst = add1(_addr_dst);
                            bits >>= 4L;
                        } 
                        // Final src fragment.


                        i = i__prev2;
                    } 
                    // Final src fragment.
                    c %= 4L;

                    if (c > 0L)
                    {
                        bits |= (uintptr(src.val) & (1L << (int)(c) - 1L)) << (int)(nbits);
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
                    dst = uint8(bits);
                    dst = add1(_addr_dst);
                    bits >>= 8L;
                    nbits -= 8L;
                }
            else


            }            {
                totalBits = (uintptr(@unsafe.Pointer(dst)) - uintptr(@unsafe.Pointer(dstStart))) * 4L + nbits;
                nbits += -nbits & 3L;
                while (nbits > 0L)
                {
                    v = bits & 0xfUL | bitScanAll;
                    dst = uint8(v);
                    dst = add1(_addr_dst);
                    bits >>= 4L;
                    nbits -= 4L;
                }


            }

            return totalBits;

        }

        // materializeGCProg allocates space for the (1-bit) pointer bitmask
        // for an object of size ptrdata.  Then it fills that space with the
        // pointer bitmask specified by the program prog.
        // The bitmask starts at s.startAddr.
        // The result must be deallocated with dematerializeGCProg.
        private static ptr<mspan> materializeGCProg(System.UIntPtr ptrdata, ptr<byte> _addr_prog)
        {
            ref byte prog = ref _addr_prog.val;
 
            // Each word of ptrdata needs one bit in the bitmap.
            var bitmapBytes = divRoundUp(ptrdata, 8L * sys.PtrSize); 
            // Compute the number of pages needed for bitmapBytes.
            var pages = divRoundUp(bitmapBytes, pageSize);
            var s = mheap_.allocManual(pages, _addr_memstats.gc_sys);
            runGCProg(_addr_addb(_addr_prog, 4L), _addr_null, _addr_(byte.val)(@unsafe.Pointer(s.startAddr)), 1L);
            return _addr_s!;

        }
        private static void dematerializeGCProg(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            mheap_.freeManual(s, _addr_memstats.gc_sys);
        }

        private static void dumpGCProg(ptr<byte> _addr_p)
        {
            ref byte p = ref _addr_p.val;

            long nptr = 0L;
            while (true)
            {
                byte x = p;
                p = add1(_addr_p);
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
                        print(" ", hex(p));
                        p = add1(_addr_p);
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
                                x = p;
                                p = add1(_addr_p);
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
                            x = p;
                            p = add1(_addr_p);
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

        private static bool getgcmaskcb(ptr<stkframe> _addr_frame, unsafe.Pointer ctxt)
        {
            ref stkframe frame = ref _addr_frame.val;

            var target = (stkframe.val)(ctxt);
            if (frame.sp <= target.sp && target.sp < frame.varp)
            {
                target.val = frame;
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
            var typ = (ptrtype.val)(@unsafe.Pointer(efaceOf(_addr_x)._type)).elem;
            var nptr = typ.ptrdata / sys.PtrSize;
            while (uintptr(len(ret)) > nptr && ret[len(ret) - 1L] == 0L)
            {
                ret = ret[..len(ret) - 1L];
            }

            return ret;

        }

        // Returns GC type info for the pointer stored in ep for testing.
        // If ep points to the stack, only static live information will be returned
        // (i.e. not for objects which are only dynamically live stack objects).
        private static slice<byte> getgcmask(object ep)
        {
            slice<byte> mask = default;

            ptr<efaceOf> e = new ptr<ptr<efaceOf>>(_addr_ep);
            var p = e.data;
            var t = e._type; 
            // data or bss
            foreach (var (_, datap) in activeModules())
            { 
                // data
                if (datap.data <= uintptr(p) && uintptr(p) < datap.edata)
                {
                    var bitmap = datap.gcdatamask.bytedata;
                    var n = (ptrtype.val)(@unsafe.Pointer(t)).elem.size;
                    mask = make_slice<byte>(n / sys.PtrSize);
                    {
                        var i__prev2 = i;

                        var i = uintptr(0L);

                        while (i < n)
                        {
                            var off = (uintptr(p) + i - datap.data) / sys.PtrSize;
                            mask[i / sys.PtrSize] = (addb(_addr_bitmap, off / 8L) >> (int)((off % 8L)).val) & 1L;
                            i += sys.PtrSize;
                        }


                        i = i__prev2;
                    }
                    return ;

                } 

                // bss
                if (datap.bss <= uintptr(p) && uintptr(p) < datap.ebss)
                {
                    bitmap = datap.gcbssmask.bytedata;
                    n = (ptrtype.val)(@unsafe.Pointer(t)).elem.size;
                    mask = make_slice<byte>(n / sys.PtrSize);
                    {
                        var i__prev2 = i;

                        i = uintptr(0L);

                        while (i < n)
                        {
                            off = (uintptr(p) + i - datap.bss) / sys.PtrSize;
                            mask[i / sys.PtrSize] = (addb(_addr_bitmap, off / 8L) >> (int)((off % 8L)).val) & 1L;
                            i += sys.PtrSize;
                        }


                        i = i__prev2;
                    }
                    return ;

                }

            } 

            // heap
            {
                var (base, s, _) = findObject(uintptr(p), 0L, 0L);

                if (base != 0L)
                {
                    var hbits = heapBitsForAddr(base);
                    n = s.elemsize;
                    mask = make_slice<byte>(n / sys.PtrSize);
                    {
                        var i__prev1 = i;

                        i = uintptr(0L);

                        while (i < n)
                        {
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

                            hbits = hbits.next();

                        }


                        i = i__prev1;
                    }
                    return ;

                } 

                // stack

            } 

            // stack
            {
                var _g___prev1 = _g_;

                var _g_ = getg();

                if (_g_.m.curg.stack.lo <= uintptr(p) && uintptr(p) < _g_.m.curg.stack.hi)
                {
                    ref stkframe frame = ref heap(out ptr<stkframe> _addr_frame);
                    frame.sp = uintptr(p);
                    _g_ = getg();
                    gentraceback(_g_.m.curg.sched.pc, _g_.m.curg.sched.sp, 0L, _g_.m.curg, 0L, null, 1000L, getgcmaskcb, noescape(@unsafe.Pointer(_addr_frame)), 0L);
                    if (frame.fn.valid())
                    {
                        var (locals, _, _) = getStackMap(_addr_frame, null, false);
                        if (locals.n == 0L)
                        {
                            return ;
                        }

                        var size = uintptr(locals.n) * sys.PtrSize;
                        n = (ptrtype.val)(@unsafe.Pointer(t)).elem.size;
                        mask = make_slice<byte>(n / sys.PtrSize);
                        {
                            var i__prev1 = i;

                            i = uintptr(0L);

                            while (i < n)
                            {
                                off = (uintptr(p) + i - frame.varp + size) / sys.PtrSize;
                                mask[i / sys.PtrSize] = locals.ptrbit(off);
                                i += sys.PtrSize;
                            }


                            i = i__prev1;
                        }

                    }

                    return ;

                } 

                // otherwise, not something the GC knows about.
                // possibly read-only data, like malloc(0).
                // must not have pointers

                _g_ = _g___prev1;

            } 

            // otherwise, not something the GC knows about.
            // possibly read-only data, like malloc(0).
            // must not have pointers
            return ;

        }
    }
}
