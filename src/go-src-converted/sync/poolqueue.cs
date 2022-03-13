// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2022 March 13 05:24:07 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\poolqueue.go
namespace go;

using atomic = sync.atomic_package;
using @unsafe = @unsafe_package;


// poolDequeue is a lock-free fixed-size single-producer,
// multi-consumer queue. The single producer can both push and pop
// from the head, and consumers can pop from the tail.
//
// It has the added feature that it nils out unused slots to avoid
// unnecessary retention of objects. This is important for sync.Pool,
// but not typically a property considered in the literature.

public static partial class sync_package {

private partial struct poolDequeue {
    public ulong headTail; // vals is a ring buffer of interface{} values stored in this
// dequeue. The size of this must be a power of 2.
//
// vals[i].typ is nil if the slot is empty and non-nil
// otherwise. A slot is still in use until *both* the tail
// index has moved beyond it and typ has been set to nil. This
// is set to nil atomically by the consumer and read
// atomically by the producer.
    public slice<eface> vals;
}

private partial struct eface {
    public unsafe.Pointer typ;
    public unsafe.Pointer val;
}

private static readonly nint dequeueBits = 32;

// dequeueLimit is the maximum size of a poolDequeue.
//
// This must be at most (1<<dequeueBits)/2 because detecting fullness
// depends on wrapping around the ring buffer without wrapping around
// the index. We divide by 4 so this fits in an int on 32-bit.


// dequeueLimit is the maximum size of a poolDequeue.
//
// This must be at most (1<<dequeueBits)/2 because detecting fullness
// depends on wrapping around the ring buffer without wrapping around
// the index. We divide by 4 so this fits in an int on 32-bit.
private static readonly nint dequeueLimit = (1 << (int)(dequeueBits)) / 4;

// dequeueNil is used in poolDequeue to represent interface{}(nil).
// Since we use nil to represent empty slots, we need a sentinel value
// to represent nil.


// dequeueNil is used in poolDequeue to represent interface{}(nil).
// Since we use nil to represent empty slots, we need a sentinel value
// to represent nil.
private static (uint, uint) unpack(this ptr<poolDequeue> _addr_d, ulong ptrs) {
    uint head = default;
    uint tail = default;
    ref poolDequeue d = ref _addr_d.val;

    const nint mask = 1 << (int)(dequeueBits) - 1;

    head = uint32((ptrs >> (int)(dequeueBits)) & mask);
    tail = uint32(ptrs & mask);
    return ;
}

private static ulong pack(this ptr<poolDequeue> _addr_d, uint head, uint tail) {
    ref poolDequeue d = ref _addr_d.val;

    const nint mask = 1 << (int)(dequeueBits) - 1;

    return (uint64(head) << (int)(dequeueBits)) | uint64(tail & mask);
}

// pushHead adds val at the head of the queue. It returns false if the
// queue is full. It must only be called by a single producer.
private static bool pushHead(this ptr<poolDequeue> _addr_d, object val) {
    ref poolDequeue d = ref _addr_d.val;

    var ptrs = atomic.LoadUint64(_addr_d.headTail);
    var (head, tail) = d.unpack(ptrs);
    if ((tail + uint32(len(d.vals))) & (1 << (int)(dequeueBits) - 1) == head) { 
        // Queue is full.
        return false;
    }
    var slot = _addr_d.vals[head & uint32(len(d.vals) - 1)]; 

    // Check if the head slot has been released by popTail.
    var typ = atomic.LoadPointer(_addr_slot.typ);
    if (typ != null) { 
        // Another goroutine is still cleaning up the tail, so
        // the queue is actually still full.
        return false;
    }
    if (val == null) {
        val = dequeueNil(null);
    }
    atomic.AddUint64(_addr_d.headTail, 1 << (int)(dequeueBits));
    return true;
}

// popHead removes and returns the element at the head of the queue.
// It returns false if the queue is empty. It must only be called by a
// single producer.
private static (object, bool) popHead(this ptr<poolDequeue> _addr_d) {
    object _p0 = default;
    bool _p0 = default;
    ref poolDequeue d = ref _addr_d.val;

    ptr<eface> slot;
    while (true) {
        var ptrs = atomic.LoadUint64(_addr_d.headTail);
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) { 
            // Queue is empty.
            return (null, false);
        }
        head--;
        var ptrs2 = d.pack(head, tail);
        if (atomic.CompareAndSwapUint64(_addr_d.headTail, ptrs, ptrs2)) { 
            // We successfully took back slot.
            slot = _addr_d.vals[head & uint32(len(d.vals) - 1)];
            break;
        }
    }

    if (val == dequeueNil(null)) {
        val = null;
    }
    slot.val = new eface();
    return (val, true);
}

// popTail removes and returns the element at the tail of the queue.
// It returns false if the queue is empty. It may be called by any
// number of consumers.
private static (object, bool) popTail(this ptr<poolDequeue> _addr_d) {
    object _p0 = default;
    bool _p0 = default;
    ref poolDequeue d = ref _addr_d.val;

    ptr<eface> slot;
    while (true) {
        var ptrs = atomic.LoadUint64(_addr_d.headTail);
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) { 
            // Queue is empty.
            return (null, false);
        }
        var ptrs2 = d.pack(head, tail + 1);
        if (atomic.CompareAndSwapUint64(_addr_d.headTail, ptrs, ptrs2)) { 
            // Success.
            slot = _addr_d.vals[tail & uint32(len(d.vals) - 1)];
            break;
        }
    } 

    // We now own slot.
    if (val == dequeueNil(null)) {
        val = null;
    }
    slot.val = null;
    atomic.StorePointer(_addr_slot.typ, null); 
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
private partial struct poolChain {
    public ptr<poolChainElt> head; // tail is the poolDequeue to popTail from. This is accessed
// by consumers, so reads and writes must be atomic.
    public ptr<poolChainElt> tail;
}

private partial struct poolChainElt {
    public ref poolDequeue poolDequeue => ref poolDequeue_val; // next and prev link to the adjacent poolChainElts in this
// poolChain.
//
// next is written atomically by the producer and read
// atomically by the consumer. It only transitions from nil to
// non-nil.
//
// prev is written atomically by the consumer and read
// atomically by the producer. It only transitions from
// non-nil to nil.
    public ptr<poolChainElt> next;
    public ptr<poolChainElt> prev;
}

private static void storePoolChainElt(ptr<ptr<poolChainElt>> _addr_pp, ptr<poolChainElt> _addr_v) {
    ref ptr<poolChainElt> pp = ref _addr_pp.val;
    ref poolChainElt v = ref _addr_v.val;

    atomic.StorePointer((@unsafe.Pointer.val)(@unsafe.Pointer(pp)), @unsafe.Pointer(v));
}

private static ptr<poolChainElt> loadPoolChainElt(ptr<ptr<poolChainElt>> _addr_pp) {
    ref ptr<poolChainElt> pp = ref _addr_pp.val;

    return _addr_(poolChainElt.val)(atomic.LoadPointer((@unsafe.Pointer.val)(@unsafe.Pointer(pp))))!;
}

private static void pushHead(this ptr<poolChain> _addr_c, object val) {
    ref poolChain c = ref _addr_c.val;

    var d = c.head;
    if (d == null) { 
        // Initialize the chain.
        const nint initSize = 8; // Must be a power of 2
 // Must be a power of 2
        d = @new<poolChainElt>();
        d.vals = make_slice<eface>(initSize);
        c.head = d;
        storePoolChainElt(_addr_c.tail, _addr_d);
    }
    if (d.pushHead(val)) {
        return ;
    }
    var newSize = len(d.vals) * 2;
    if (newSize >= dequeueLimit) { 
        // Can't make it any bigger.
        newSize = dequeueLimit;
    }
    ptr<poolChainElt> d2 = addr(new poolChainElt(prev:d));
    d2.vals = make_slice<eface>(newSize);
    c.head = d2;
    storePoolChainElt(_addr_d.next, d2);
    d2.pushHead(val);
}

private static (object, bool) popHead(this ptr<poolChain> _addr_c) {
    object _p0 = default;
    bool _p0 = default;
    ref poolChain c = ref _addr_c.val;

    var d = c.head;
    while (d != null) {
        {
            var (val, ok) = d.popHead();

            if (ok) {
                return (val, ok);
            } 
            // There may still be unconsumed elements in the
            // previous dequeue, so try backing up.

        } 
        // There may still be unconsumed elements in the
        // previous dequeue, so try backing up.
        d = loadPoolChainElt(_addr_d.prev);
    }
    return (null, false);
}

private static (object, bool) popTail(this ptr<poolChain> _addr_c) {
    object _p0 = default;
    bool _p0 = default;
    ref poolChain c = ref _addr_c.val;

    var d = loadPoolChainElt(_addr_c.tail);
    if (d == null) {
        return (null, false);
    }
    while (true) { 
        // It's important that we load the next pointer
        // *before* popping the tail. In general, d may be
        // transiently empty, but if next is non-nil before
        // the pop and the pop fails, then d is permanently
        // empty, which is the only condition under which it's
        // safe to drop d from the chain.
        var d2 = loadPoolChainElt(_addr_d.next);

        {
            var (val, ok) = d.popTail();

            if (ok) {
                return (val, ok);
            }

        }

        if (d2 == null) { 
            // This is the only dequeue. It's empty right
            // now, but could be pushed to in the future.
            return (null, false);
        }
        if (atomic.CompareAndSwapPointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_c.tail)), @unsafe.Pointer(d), @unsafe.Pointer(d2))) { 
            // We won the race. Clear the prev pointer so
            // the garbage collector can collect the empty
            // dequeue and so popHead doesn't back up
            // further than necessary.
            storePoolChainElt(_addr_d2.prev, _addr_null);
        }
        d = d2;
    }
}

} // end sync_package
