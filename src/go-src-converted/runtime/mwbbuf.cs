// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This implements the write barrier buffer. The write barrier itself
// is gcWriteBarrier and is implemented in assembly.
//
// The write barrier has a fast path and a slow path. The fast path
// simply enqueues to a per-P write barrier buffer. It's written in
// assembly and doesn't clobber any general purpose registers, so it
// doesn't have the usual overheads of a Go call.
//
// When the buffer fills up, the write barrier invokes the slow path
// (wbBufFlush) to flush the buffer to the GC work queues. In this
// path, since the compiler didn't spill registers, we spill *all*
// registers and disallow any GC safe points that could observe the
// stack frame (since we don't know the types of the spilled
// registers).

// package runtime -- go2cs converted at 2020 August 29 08:18:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mwbbuf.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // testSmallBuf forces a small write barrier buffer to stress write
        // barrier flushing.
        private static readonly var testSmallBuf = false;

        // wbBuf is a per-P buffer of pointers queued by the write barrier.
        // This buffer is flushed to the GC workbufs when it fills up and on
        // various GC transitions.
        //
        // This is closely related to a "sequential store buffer" (SSB),
        // except that SSBs are usually used for maintaining remembered sets,
        // while this is used for marking.


        // wbBuf is a per-P buffer of pointers queued by the write barrier.
        // This buffer is flushed to the GC workbufs when it fills up and on
        // various GC transitions.
        //
        // This is closely related to a "sequential store buffer" (SSB),
        // except that SSBs are usually used for maintaining remembered sets,
        // while this is used for marking.
        private partial struct wbBuf
        {
            public System.UIntPtr next; // end points to just past the end of buf. It must not be a
// pointer type because it points past the end of buf and must
// be updated without write barriers.
            public System.UIntPtr end; // buf stores a series of pointers to execute write barriers
// on. This must be a multiple of wbBufEntryPointers because
// the write barrier only checks for overflow once per entry.
            public array<System.UIntPtr> buf;
        }

 
        // wbBufEntries is the number of write barriers between
        // flushes of the write barrier buffer.
        //
        // This trades latency for throughput amortization. Higher
        // values amortize flushing overhead more, but increase the
        // latency of flushing. Higher values also increase the cache
        // footprint of the buffer.
        //
        // TODO: What is the latency cost of this? Tune this value.
        private static readonly long wbBufEntries = 256L; 

        // wbBufEntryPointers is the number of pointers added to the
        // buffer by each write barrier.
        private static readonly long wbBufEntryPointers = 2L;

        // reset empties b by resetting its next and end pointers.
        private static void reset(this ref wbBuf b)
        {
            var start = uintptr(@unsafe.Pointer(ref b.buf[0L]));
            b.next = start;
            if (gcBlackenPromptly || writeBarrier.cgo)
            { 
                // Effectively disable the buffer by forcing a flush
                // on every barrier.
                b.end = uintptr(@unsafe.Pointer(ref b.buf[wbBufEntryPointers]));
            }
            else if (testSmallBuf)
            { 
                // For testing, allow two barriers in the buffer. If
                // we only did one, then barriers of non-heap pointers
                // would be no-ops. This lets us combine a buffered
                // barrier with a flush at a later time.
                b.end = uintptr(@unsafe.Pointer(ref b.buf[2L * wbBufEntryPointers]));
            }
            else
            {
                b.end = start + uintptr(len(b.buf)) * @unsafe.Sizeof(b.buf[0L]);
            }
            if ((b.end - b.next) % (wbBufEntryPointers * @unsafe.Sizeof(b.buf[0L])) != 0L)
            {
                throw("bad write barrier buffer bounds");
            }
        }

        // discard resets b's next pointer, but not its end pointer.
        //
        // This must be nosplit because it's called by wbBufFlush.
        //
        //go:nosplit
        private static void discard(this ref wbBuf b)
        {
            b.next = uintptr(@unsafe.Pointer(ref b.buf[0L]));
        }

        // putFast adds old and new to the write barrier buffer and returns
        // false if a flush is necessary. Callers should use this as:
        //
        //     buf := &getg().m.p.ptr().wbBuf
        //     if !buf.putFast(old, new) {
        //         wbBufFlush(...)
        //     }
        //
        // The arguments to wbBufFlush depend on whether the caller is doing
        // its own cgo pointer checks. If it is, then this can be
        // wbBufFlush(nil, 0). Otherwise, it must pass the slot address and
        // new.
        //
        // Since buf is a per-P resource, the caller must ensure there are no
        // preemption points while buf is in use.
        //
        // It must be nowritebarrierrec to because write barriers here would
        // corrupt the write barrier buffer. It (and everything it calls, if
        // it called anything) has to be nosplit to avoid scheduling on to a
        // different P and a different buffer.
        //
        //go:nowritebarrierrec
        //go:nosplit
        private static bool putFast(this ref wbBuf b, System.UIntPtr old, System.UIntPtr @new)
        {
            ref array<System.UIntPtr> p = new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(b.next));
            p[0L] = old;
            p[1L] = new;
            b.next += 2L * sys.PtrSize;
            return b.next != b.end;
        }

        // wbBufFlush flushes the current P's write barrier buffer to the GC
        // workbufs. It is passed the slot and value of the write barrier that
        // caused the flush so that it can implement cgocheck.
        //
        // This must not have write barriers because it is part of the write
        // barrier implementation.
        //
        // This and everything it calls must be nosplit because 1) the stack
        // contains untyped slots from gcWriteBarrier and 2) there must not be
        // a GC safe point between the write barrier test in the caller and
        // flushing the buffer.
        //
        // TODO: A "go:nosplitrec" annotation would be perfect for this.
        //
        //go:nowritebarrierrec
        //go:nosplit
        private static void wbBufFlush(ref System.UIntPtr dst, System.UIntPtr src)
        { 
            // Note: Every possible return from this function must reset
            // the buffer's next pointer to prevent buffer overflow.

            if (getg().m.dying > 0L)
            { 
                // We're going down. Not much point in write barriers
                // and this way we can allow write barriers in the
                // panic path.
                getg().m.p.ptr().wbBuf.discard();
                return;
            }
            if (writeBarrier.cgo && dst != null)
            { 
                // This must be called from the stack that did the
                // write. It's nosplit all the way down.
                cgoCheckWriteBarrier(dst, src);
                if (!writeBarrier.needed)
                { 
                    // We were only called for cgocheck.
                    getg().m.p.ptr().wbBuf.discard();
                    return;
                }
            } 

            // Switch to the system stack so we don't have to worry about
            // the untyped stack slots or safe points.
            systemstack(() =>
            {
                wbBufFlush1(getg().m.p.ptr());
            });
        }

        // wbBufFlush1 flushes p's write barrier buffer to the GC work queue.
        //
        // This must not have write barriers because it is part of the write
        // barrier implementation, so this may lead to infinite loops or
        // buffer corruption.
        //
        // This must be non-preemptible because it uses the P's workbuf.
        //
        //go:nowritebarrierrec
        //go:systemstack
        private static void wbBufFlush1(ref p _p_)
        { 
            // Get the buffered pointers.
            var start = uintptr(@unsafe.Pointer(ref _p_.wbBuf.buf[0L]));
            var n = (_p_.wbBuf.next - start) / @unsafe.Sizeof(_p_.wbBuf.buf[0L]);
            var ptrs = _p_.wbBuf.buf[..n]; 

            // Reset the buffer.
            _p_.wbBuf.reset();

            if (useCheckmark)
            { 
                // Slow path for checkmark mode.
                {
                    var ptr__prev1 = ptr;

                    foreach (var (_, __ptr) in ptrs)
                    {
                        ptr = __ptr;
                        shade(ptr);
                    }

                    ptr = ptr__prev1;
                }

                return;
            } 

            // Mark all of the pointers in the buffer and record only the
            // pointers we greyed. We use the buffer itself to temporarily
            // record greyed pointers.
            //
            // TODO: Should scanobject/scanblock just stuff pointers into
            // the wbBuf? Then this would become the sole greying path.
            var gcw = ref _p_.gcw;
            long pos = 0L;
            var arenaStart = mheap_.arena_start;
            {
                var ptr__prev1 = ptr;

                foreach (var (_, __ptr) in ptrs)
                {
                    ptr = __ptr;
                    if (ptr < arenaStart)
                    { 
                        // nil pointers are very common, especially
                        // for the "old" values. Filter out these and
                        // other "obvious" non-heap pointers ASAP.
                        //
                        // TODO: Should we filter out nils in the fast
                        // path to reduce the rate of flushes?
                        continue;
                    } 
                    // TODO: This doesn't use hbits, so calling
                    // heapBitsForObject seems a little silly. We could
                    // easily separate this out since heapBitsForObject
                    // just calls heapBitsForAddr(obj) to get hbits.
                    var (obj, _, span, objIndex) = heapBitsForObject(ptr, 0L, 0L);
                    if (obj == 0L)
                    {
                        continue;
                    } 
                    // TODO: Consider making two passes where the first
                    // just prefetches the mark bits.
                    var mbits = span.markBitsForIndex(objIndex);
                    if (mbits.isMarked())
                    {
                        continue;
                    }
                    mbits.setMarked();
                    if (span.spanclass.noscan())
                    {
                        gcw.bytesMarked += uint64(span.elemsize);
                        continue;
                    }
                    ptrs[pos] = obj;
                    pos++;
                } 

                // Enqueue the greyed objects.

                ptr = ptr__prev1;
            }

            gcw.putBatch(ptrs[..pos]);
            if (gcphase == _GCmarktermination || gcBlackenPromptly)
            { 
                // Ps aren't allowed to cache work during mark
                // termination.
                gcw.dispose();
            }
        }
    }
}
