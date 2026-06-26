// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class sync_package {

// poolDequeue is a lock-free fixed-size single-producer,
// multi-consumer queue. The single producer can both push and pop
// from the head, and consumers can pop from the tail.
//
// It has the added feature that it nils out unused slots to avoid
// unnecessary retention of objects. This is important for sync.Pool,
// but not typically a property considered in the literature.
[GoType] partial struct poolDequeue {
    // headTail packs together a 32-bit head index and a 32-bit
    // tail index. Both are indexes into vals modulo len(vals)-1.
    //
    // tail = index of oldest data in queue
    // head = index of next slot to fill
    //
    // Slots in the range [tail, head) are owned by consumers.
    // A consumer continues to own a slot outside this range until
    // it nils the slot, at which point ownership passes to the
    // producer.
    //
    // The head index is stored in the most-significant bits so
    // that we can atomically add to it and the overflow is
    // harmless.
    internal sync.atomic_package.Uint64 headTail;
    // vals is a ring buffer of interface{} values stored in this
    // dequeue. The size of this must be a power of 2.
    //
    // vals[i].typ is nil if the slot is empty and non-nil
    // otherwise. A slot is still in use until *both* the tail
    // index has moved beyond it and typ has been set to nil. This
    // is set to nil atomically by the consumer and read
    // atomically by the producer.
    internal slice<eface> vals;
}

[GoType] partial struct eface {
    internal @unsafe.Pointer typ;
    internal @unsafe.Pointer val;
}

internal static readonly UntypedInt dequeueBits = 32;

// dequeueLimit is the maximum size of a poolDequeue.
//
// This must be at most (1<<dequeueBits)/2 because detecting fullness
// depends on wrapping around the ring buffer without wrapping around
// the index. We divide by 4 so this fits in an int on 32-bit.
internal static readonly UntypedInt dequeueLimit = /* (1 << dequeueBits) / 4 */ 1073741824;

[GoType("dyn")] partial struct Δtype {
}
Δtype.val
[GoRecv] internal static (uint32 head, uint32 tail) unpack(this ref poolDequeue d, uint64 ptrs) {
    uint32 head = default!;
    uint32 tail = default!;

    static readonly UntypedInt mask = /* 1<<dequeueBits - 1 */ 4294967295;
    head = ((uint32)((uint64)((ptrs >> (int)(dequeueBits)) & mask)));
    tail = ((uint32)((uint64)(ptrs & mask)));
    return (head, tail);
}

[GoRecv] internal static uint64 pack(this ref poolDequeue d, uint32 head, uint32 tail) {
    static readonly UntypedInt mask = /* 1<<dequeueBits - 1 */ 4294967295;
    return (uint64)((((uint64)head) << (int)(dequeueBits)) | ((uint64)((uint32)(tail & mask))));
}

// pushHead adds val at the head of the queue. It returns false if the
// queue is full. It must only be called by a single producer.
[GoRecv] internal static bool pushHead(this ref poolDequeue d, any val) {
    var ptrs = d.headTail.Load();
    var (head, tail) = d.unpack(ptrs);
    if ((uint32)((tail + ((uint32)len(d.vals))) & (1 << (int)(dequeueBits) - 1)) == head) {
        // Queue is full.
        return false;
    }
    var slot = Ꮡ(d.vals[(uint32)(head & ((uint32)(len(d.vals) - 1)))]);
    // Check if the head slot has been released by popTail.
    @unsafe.Pointer typ = (uintptr)atomic.LoadPointer(Ꮡ((~slot).typ));
    if (typ != nil) {
        // Another goroutine is still cleaning up the tail, so
        // the queue is actually still full.
        return false;
    }
    // The head slot is free, so we own it.
    if (val == default!) {
        val = ((dequeueNil)default!);
    }
    ((ж<any>)(uintptr)(new @unsafe.Pointer(slot))).val = val;
    // Increment head. This passes ownership of slot to popTail
    // and acts as a store barrier for writing the slot.
    d.headTail.Add(1 << (int)(dequeueBits));
    return true;
}

// popHead removes and returns the element at the head of the queue.
// It returns false if the queue is empty. It must only be called by a
// single producer.
[GoRecv] internal static (any, bool) popHead(this ref poolDequeue d) {
    ж<eface> slot = default!;
    while (ᐧ) {
        var ptrs = d.headTail.Load();
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) {
            // Queue is empty.
            return (default!, false);
        }
        // Confirm tail and decrement head. We do this before
        // reading the value to take back ownership of this
        // slot.
        head--;
        var ptrs2 = d.pack(head, tail);
        if (d.headTail.CompareAndSwap(ptrs, ptrs2)) {
            // We successfully took back slot.
            slot = Ꮡ(d.vals[(uint32)(head & ((uint32)(len(d.vals) - 1)))]);
            break;
        }
    }
    var val = ~(ж<any>)(uintptr)(new @unsafe.Pointer(slot));
    if (Ꮡval == ((dequeueNil)default!)) {
        val = default!;
    }
    // Zero the slot. Unlike popTail, this isn't racing with
    // pushHead, so we don't need to be careful here.
    slot.val = new eface(nil);
    return (val, true);
}

// popTail removes and returns the element at the tail of the queue.
// It returns false if the queue is empty. It may be called by any
// number of consumers.
[GoRecv] internal static (any, bool) popTail(this ref poolDequeue d) {
    ж<eface> slot = default!;
    while (ᐧ) {
        var ptrs = d.headTail.Load();
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) {
            // Queue is empty.
            return (default!, false);
        }
        // Confirm head and tail (for our speculative check
        // above) and increment tail. If this succeeds, then
        // we own the slot at tail.
        var ptrs2 = d.pack(head, tail + 1);
        if (d.headTail.CompareAndSwap(ptrs, ptrs2)) {
            // Success.
            slot = Ꮡ(d.vals[(uint32)(tail & ((uint32)(len(d.vals) - 1)))]);
            break;
        }
    }
    // We now own slot.
    var val = ~(ж<any>)(uintptr)(new @unsafe.Pointer(slot));
    if (Ꮡval == ((dequeueNil)default!)) {
        val = default!;
    }
    // Tell pushHead that we're done with this slot. Zeroing the
    // slot is also important so we don't leave behind references
    // that could keep this object live longer than necessary.
    //
    // We write to val first and then publish that we're done with
    // this slot by atomically writing to typ.
    slot.val.val = default!;
    atomic.StorePointer(Ꮡ((~slot).typ), nil);
    // At this point pushHead owns the slot.
    return (val, true);
}

// poolChain is a dynamically-sized version of poolDequeue.
//
// This is implemented as a doubly-linked list queue of poolDequeues
// where each dequeue is double the size of the previous one. Once a
// dequeue fills up, this allocates a new one and only ever pushes to
// the latest dequeue. Pops happen from the other end of the list and
// once a dequeue is exhausted, it gets removed from the list.
[GoType] partial struct poolChain {
    // head is the poolDequeue to push to. This is only accessed
    // by the producer, so doesn't need to be synchronized.
    internal ж<poolChainElt> head;
    // tail is the poolDequeue to popTail from. This is accessed
    // by consumers, so reads and writes must be atomic.
    internal sync.atomic_package.Pointer tail;
}

[GoType] partial struct poolChainElt {
    internal partial ref poolDequeue poolDequeue { get; }
    // next and prev link to the adjacent poolChainElts in this
    // poolChain.
    //
    // next is written atomically by the producer and read
    // atomically by the consumer. It only transitions from nil to
    // non-nil.
    //
    // prev is written atomically by the consumer and read
    // atomically by the producer. It only transitions from
    // non-nil to nil.
    internal sync.atomic_package.Pointer next;
    internal sync.atomic_package.Pointer prev;
}

[GoRecv] internal static void pushHead(this ref poolChain c, any val) {
    var d = c.head;
    if (d == nil) {
        // Initialize the chain.
        static readonly UntypedInt initSize = 8; // Must be a power of 2
        d = @new<poolChainElt>();
        d.vals = new slice<eface>(initSize);
        c.head = d;
        c.tail.Store(d);
    }
    if (d.pushHead(val)) {
        return;
    }
    // The current dequeue is full. Allocate a new one of twice
    // the size.
    nint newSize = len(d.vals) * 2;
    if (newSize >= dequeueLimit) {
        // Can't make it any bigger.
        newSize = dequeueLimit;
    }
    var d2 = Ꮡ(new poolChainElt(nil));
    (~d2).prev.Store(d);
    d2.vals = new slice<eface>(newSize);
    c.head = d2;
    (~d).next.Store(d2);
    d2.pushHead(val);
}

[GoRecv] internal static (any, bool) popHead(this ref poolChain c) {
    var d = c.head;
    while (d != nil) {
        {
            var (val, ok) = d.popHead(); if (ok) {
                return (val, ok);
            }
        }
        // There may still be unconsumed elements in the
        // previous dequeue, so try backing up.
        d = (~d).prev.Load();
    }
    return (default!, false);
}

[GoRecv] internal static (any, bool) popTail(this ref poolChain c) {
    var d = c.tail.Load();
    if (d == nil) {
        return (default!, false);
    }
    while (ᐧ) {
        // It's important that we load the next pointer
        // *before* popping the tail. In general, d may be
        // transiently empty, but if next is non-nil before
        // the pop and the pop fails, then d is permanently
        // empty, which is the only condition under which it's
        // safe to drop d from the chain.
        var d2 = (~d).next.Load();
        {
            var (val, ok) = d.popTail(); if (ok) {
                return (val, ok);
            }
        }
        if (d2 == nil) {
            // This is the only dequeue. It's empty right
            // now, but could be pushed to in the future.
            return (default!, false);
        }
        // The tail of the chain has been drained, so move on
        // to the next dequeue. Try to drop it from the chain
        // so the next pop doesn't have to look at the empty
        // dequeue again.
        if (c.tail.CompareAndSwap(d, d2)) {
            // We won the race. Clear the prev pointer so
            // the garbage collector can collect the empty
            // dequeue and so popHead doesn't back up
            // further than necessary.
            (~d2).prev.Store(nil);
        }
        d = d2;
    }
}

} // end sync_package
