// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: write barriers.
//
// For the concurrent garbage collector, the Go compiler implements
// updates to pointer-valued fields that may be in heap objects by
// emitting calls to write barriers. The main write barrier for
// individual pointer writes is gcWriteBarrier and is implemented in
// assembly. This file contains write barrier entry points for bulk
// operations. See also mwbbuf.go.

// package runtime -- go2cs converted at 2020 October 08 03:20:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mbarrier.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Go uses a hybrid barrier that combines a Yuasa-style deletion
        // barrier—which shades the object whose reference is being
        // overwritten—with Dijkstra insertion barrier—which shades the object
        // whose reference is being written. The insertion part of the barrier
        // is necessary while the calling goroutine's stack is grey. In
        // pseudocode, the barrier is:
        //
        //     writePointer(slot, ptr):
        //         shade(*slot)
        //         if current stack is grey:
        //             shade(ptr)
        //         *slot = ptr
        //
        // slot is the destination in Go code.
        // ptr is the value that goes into the slot in Go code.
        //
        // Shade indicates that it has seen a white pointer by adding the referent
        // to wbuf as well as marking it.
        //
        // The two shades and the condition work together to prevent a mutator
        // from hiding an object from the garbage collector:
        //
        // 1. shade(*slot) prevents a mutator from hiding an object by moving
        // the sole pointer to it from the heap to its stack. If it attempts
        // to unlink an object from the heap, this will shade it.
        //
        // 2. shade(ptr) prevents a mutator from hiding an object by moving
        // the sole pointer to it from its stack into a black object in the
        // heap. If it attempts to install the pointer into a black object,
        // this will shade it.
        //
        // 3. Once a goroutine's stack is black, the shade(ptr) becomes
        // unnecessary. shade(ptr) prevents hiding an object by moving it from
        // the stack to the heap, but this requires first having a pointer
        // hidden on the stack. Immediately after a stack is scanned, it only
        // points to shaded objects, so it's not hiding anything, and the
        // shade(*slot) prevents it from hiding any other pointers on its
        // stack.
        //
        // For a detailed description of this barrier and proof of
        // correctness, see https://github.com/golang/proposal/blob/master/design/17503-eliminate-rescan.md
        //
        //
        //
        // Dealing with memory ordering:
        //
        // Both the Yuasa and Dijkstra barriers can be made conditional on the
        // color of the object containing the slot. We chose not to make these
        // conditional because the cost of ensuring that the object holding
        // the slot doesn't concurrently change color without the mutator
        // noticing seems prohibitive.
        //
        // Consider the following example where the mutator writes into
        // a slot and then loads the slot's mark bit while the GC thread
        // writes to the slot's mark bit and then as part of scanning reads
        // the slot.
        //
        // Initially both [slot] and [slotmark] are 0 (nil)
        // Mutator thread          GC thread
        // st [slot], ptr          st [slotmark], 1
        //
        // ld r1, [slotmark]       ld r2, [slot]
        //
        // Without an expensive memory barrier between the st and the ld, the final
        // result on most HW (including 386/amd64) can be r1==r2==0. This is a classic
        // example of what can happen when loads are allowed to be reordered with older
        // stores (avoiding such reorderings lies at the heart of the classic
        // Peterson/Dekker algorithms for mutual exclusion). Rather than require memory
        // barriers, which will slow down both the mutator and the GC, we always grey
        // the ptr object regardless of the slot's color.
        //
        // Another place where we intentionally omit memory barriers is when
        // accessing mheap_.arena_used to check if a pointer points into the
        // heap. On relaxed memory machines, it's possible for a mutator to
        // extend the size of the heap by updating arena_used, allocate an
        // object from this new region, and publish a pointer to that object,
        // but for tracing running on another processor to observe the pointer
        // but use the old value of arena_used. In this case, tracing will not
        // mark the object, even though it's reachable. However, the mutator
        // is guaranteed to execute a write barrier when it publishes the
        // pointer, so it will take care of marking the object. A general
        // consequence of this is that the garbage collector may cache the
        // value of mheap_.arena_used. (See issue #9984.)
        //
        //
        // Stack writes:
        //
        // The compiler omits write barriers for writes to the current frame,
        // but if a stack pointer has been passed down the call stack, the
        // compiler will generate a write barrier for writes through that
        // pointer (because it doesn't know it's not a heap pointer).
        //
        // One might be tempted to ignore the write barrier if slot points
        // into to the stack. Don't do it! Mark termination only re-scans
        // frames that have potentially been active since the concurrent scan,
        // so it depends on write barriers to track changes to pointers in
        // stack frames that have not been active.
        //
        //
        // Global writes:
        //
        // The Go garbage collector requires write barriers when heap pointers
        // are stored in globals. Many garbage collectors ignore writes to
        // globals and instead pick up global -> heap pointers during
        // termination. This increases pause time, so we instead rely on write
        // barriers for writes to globals so that we don't have to rescan
        // global during mark termination.
        //
        //
        // Publication ordering:
        //
        // The write barrier is *pre-publication*, meaning that the write
        // barrier happens prior to the *slot = ptr write that may make ptr
        // reachable by some goroutine that currently cannot reach it.
        //
        //
        // Signal handler pointer writes:
        //
        // In general, the signal handler cannot safely invoke the write
        // barrier because it may run without a P or even during the write
        // barrier.
        //
        // There is exactly one exception: profbuf.go omits a barrier during
        // signal handler profile logging. That's safe only because of the
        // deletion barrier. See profbuf.go for a detailed argument. If we
        // remove the deletion barrier, we'll have to work out a new way to
        // handle the profile logging.

        // typedmemmove copies a value of type t to dst from src.
        // Must be nosplit, see #16026.
        //
        // TODO: Perfect for go:nosplitrec since we can't have a safe point
        // anywhere in the bulk barrier or memmove.
        //
        //go:nosplit
        private static void typedmemmove(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src)
        {
            ref _type typ = ref _addr_typ.val;

            if (dst == src)
            {
                return ;
            }
            if (writeBarrier.needed && typ.ptrdata != 0L)
            {
                bulkBarrierPreWrite(uintptr(dst), uintptr(src), typ.ptrdata);
            }
            memmove(dst, src, typ.size);
            if (writeBarrier.cgo)
            {
                cgoCheckMemmove(typ, dst, src, 0L, typ.size);
            }
        }

        //go:linkname reflect_typedmemmove reflect.typedmemmove
        private static void reflect_typedmemmove(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src)
        {
            ref _type typ = ref _addr_typ.val;

            if (raceenabled)
            {
                raceWriteObjectPC(typ, dst, getcallerpc(), funcPC(reflect_typedmemmove));
                raceReadObjectPC(typ, src, getcallerpc(), funcPC(reflect_typedmemmove));
            }

            if (msanenabled)
            {
                msanwrite(dst, typ.size);
                msanread(src, typ.size);
            }

            typedmemmove(_addr_typ, dst, src);

        }

        //go:linkname reflectlite_typedmemmove internal/reflectlite.typedmemmove
        private static void reflectlite_typedmemmove(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src)
        {
            ref _type typ = ref _addr_typ.val;

            reflect_typedmemmove(_addr_typ, dst, src);
        }

        // typedmemmovepartial is like typedmemmove but assumes that
        // dst and src point off bytes into the value and only copies size bytes.
        // off must be a multiple of sys.PtrSize.
        //go:linkname reflect_typedmemmovepartial reflect.typedmemmovepartial
        private static void reflect_typedmemmovepartial(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr off, System.UIntPtr size) => func((_, panic, __) =>
        {
            ref _type typ = ref _addr_typ.val;

            if (writeBarrier.needed && typ.ptrdata > off && size >= sys.PtrSize)
            {
                if (off & (sys.PtrSize - 1L) != 0L)
                {
                    panic("reflect: internal error: misaligned offset");
                }

                var pwsize = alignDown(size, sys.PtrSize);
                {
                    var poff = typ.ptrdata - off;

                    if (pwsize > poff)
                    {
                        pwsize = poff;
                    }

                }

                bulkBarrierPreWrite(uintptr(dst), uintptr(src), pwsize);

            }

            memmove(dst, src, size);
            if (writeBarrier.cgo)
            {
                cgoCheckMemmove(typ, dst, src, off, size);
            }

        });

        // reflectcallmove is invoked by reflectcall to copy the return values
        // out of the stack and into the heap, invoking the necessary write
        // barriers. dst, src, and size describe the return value area to
        // copy. typ describes the entire frame (not just the return values).
        // typ may be nil, which indicates write barriers are not needed.
        //
        // It must be nosplit and must only call nosplit functions because the
        // stack map of reflectcall is wrong.
        //
        //go:nosplit
        private static void reflectcallmove(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;

            if (writeBarrier.needed && typ != null && typ.ptrdata != 0L && size >= sys.PtrSize)
            {
                bulkBarrierPreWrite(uintptr(dst), uintptr(src), size);
            }

            memmove(dst, src, size);

        }

        //go:nosplit
        private static long typedslicecopy(ptr<_type> _addr_typ, unsafe.Pointer dstPtr, long dstLen, unsafe.Pointer srcPtr, long srcLen)
        {
            ref _type typ = ref _addr_typ.val;

            var n = dstLen;
            if (n > srcLen)
            {
                n = srcLen;
            }

            if (n == 0L)
            {
                return 0L;
            } 

            // The compiler emits calls to typedslicecopy before
            // instrumentation runs, so unlike the other copying and
            // assignment operations, it's not instrumented in the calling
            // code and needs its own instrumentation.
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                var pc = funcPC(slicecopy);
                racewriterangepc(dstPtr, uintptr(n) * typ.size, callerpc, pc);
                racereadrangepc(srcPtr, uintptr(n) * typ.size, callerpc, pc);
            }

            if (msanenabled)
            {
                msanwrite(dstPtr, uintptr(n) * typ.size);
                msanread(srcPtr, uintptr(n) * typ.size);
            }

            if (writeBarrier.cgo)
            {
                cgoCheckSliceCopy(typ, dstPtr, srcPtr, n);
            }

            if (dstPtr == srcPtr)
            {
                return n;
            } 

            // Note: No point in checking typ.ptrdata here:
            // compiler only emits calls to typedslicecopy for types with pointers,
            // and growslice and reflect_typedslicecopy check for pointers
            // before calling typedslicecopy.
            var size = uintptr(n) * typ.size;
            if (writeBarrier.needed)
            {
                var pwsize = size - typ.size + typ.ptrdata;
                bulkBarrierPreWrite(uintptr(dstPtr), uintptr(srcPtr), pwsize);
            } 
            // See typedmemmove for a discussion of the race between the
            // barrier and memmove.
            memmove(dstPtr, srcPtr, size);
            return n;

        }

        //go:linkname reflect_typedslicecopy reflect.typedslicecopy
        private static long reflect_typedslicecopy(ptr<_type> _addr_elemType, slice dst, slice src)
        {
            ref _type elemType = ref _addr_elemType.val;

            if (elemType.ptrdata == 0L)
            {
                var n = dst.len;
                if (n > src.len)
                {
                    n = src.len;
                }

                if (n == 0L)
                {
                    return 0L;
                }

                var size = uintptr(n) * elemType.size;
                if (raceenabled)
                {
                    var callerpc = getcallerpc();
                    var pc = funcPC(reflect_typedslicecopy);
                    racewriterangepc(dst.array, size, callerpc, pc);
                    racereadrangepc(src.array, size, callerpc, pc);
                }

                if (msanenabled)
                {
                    msanwrite(dst.array, size);
                    msanread(src.array, size);
                }

                memmove(dst.array, src.array, size);
                return n;

            }

            return typedslicecopy(_addr_elemType, dst.array, dst.len, src.array, src.len);

        }

        // typedmemclr clears the typed memory at ptr with type typ. The
        // memory at ptr must already be initialized (and hence in type-safe
        // state). If the memory is being initialized for the first time, see
        // memclrNoHeapPointers.
        //
        // If the caller knows that typ has pointers, it can alternatively
        // call memclrHasPointers.
        //
        //go:nosplit
        private static void typedmemclr(ptr<_type> _addr_typ, unsafe.Pointer ptr)
        {
            ref _type typ = ref _addr_typ.val;

            if (writeBarrier.needed && typ.ptrdata != 0L)
            {
                bulkBarrierPreWrite(uintptr(ptr), 0L, typ.ptrdata);
            }

            memclrNoHeapPointers(ptr, typ.size);

        }

        //go:linkname reflect_typedmemclr reflect.typedmemclr
        private static void reflect_typedmemclr(ptr<_type> _addr_typ, unsafe.Pointer ptr)
        {
            ref _type typ = ref _addr_typ.val;

            typedmemclr(_addr_typ, ptr);
        }

        //go:linkname reflect_typedmemclrpartial reflect.typedmemclrpartial
        private static void reflect_typedmemclrpartial(ptr<_type> _addr_typ, unsafe.Pointer ptr, System.UIntPtr off, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;

            if (writeBarrier.needed && typ.ptrdata != 0L)
            {
                bulkBarrierPreWrite(uintptr(ptr), 0L, size);
            }

            memclrNoHeapPointers(ptr, size);

        }

        // memclrHasPointers clears n bytes of typed memory starting at ptr.
        // The caller must ensure that the type of the object at ptr has
        // pointers, usually by checking typ.ptrdata. However, ptr
        // does not have to point to the start of the allocation.
        //
        //go:nosplit
        private static void memclrHasPointers(unsafe.Pointer ptr, System.UIntPtr n)
        {
            bulkBarrierPreWrite(uintptr(ptr), 0L, n);
            memclrNoHeapPointers(ptr, n);
        }
    }
}
