// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector: stack objects and stack tracing
// See the design doc at https://docs.google.com/document/d/1un-Jn47yByHL7I0aVIP_uVCMxjdM5mpelJhiKlIqxkE/edit?usp=sharing
// Also see issue 22350.
// Stack tracing solves the problem of determining which parts of the
// stack are live and should be scanned. It runs as part of scanning
// a single goroutine stack.
//
// Normally determining which parts of the stack are live is easy to
// do statically, as user code has explicit references (reads and
// writes) to stack variables. The compiler can do a simple dataflow
// analysis to determine liveness of stack variables at every point in
// the code. See cmd/compile/internal/gc/plive.go for that analysis.
//
// However, when we take the address of a stack variable, determining
// whether that variable is still live is less clear. We can still
// look for static accesses, but accesses through a pointer to the
// variable are difficult in general to track statically. That pointer
// can be passed among functions on the stack, conditionally retained,
// etc.
//
// Instead, we will track pointers to stack variables dynamically.
// All pointers to stack-allocated variables will themselves be on the
// stack somewhere (or in associated locations, like defer records), so
// we can find them all efficiently.
//
// Stack tracing is organized as a mini garbage collection tracing
// pass. The objects in this garbage collection are all the variables
// on the stack whose address is taken, and which themselves contain a
// pointer. We call these variables "stack objects".
//
// We begin by determining all the stack objects on the stack and all
// the statically live pointers that may point into the stack. We then
// process each pointer to see if it points to a stack object. If it
// does, we scan that stack object. It may contain pointers into the
// heap, in which case those pointers are passed to the main garbage
// collection. It may also contain pointers into the stack, in which
// case we add them to our set of stack pointers.
//
// Once we're done processing all the pointers (including the ones we
// added during processing), we've found all the stack objects that
// are live. Any dead stack objects are not scanned and their contents
// will not keep heap objects live. Unlike the main garbage
// collection, we can't sweep the dead stack objects; they live on in
// a moribund state until the stack frame that contains them is
// popped.
//
// A stack can look like this:
//
// +----------+
// | foo()    |
// | +------+ |
// | |  A   | | <---\
// | +------+ |     |
// |          |     |
// | +------+ |     |
// | |  B   | |     |
// | +------+ |     |
// |          |     |
// +----------+     |
// | bar()    |     |
// | +------+ |     |
// | |  C   | | <-\ |
// | +----|-+ |   | |
// |      |   |   | |
// | +----v-+ |   | |
// | |  D  ---------/
// | +------+ |   |
// |          |   |
// +----------+   |
// | baz()    |   |
// | +------+ |   |
// | |  E  -------/
// | +------+ |
// |      ^   |
// | F: --/   |
// |          |
// +----------+
//
// foo() calls bar() calls baz(). Each has a frame on the stack.
// foo() has stack objects A and B.
// bar() has stack objects C and D, with C pointing to D and D pointing to A.
// baz() has a stack object E pointing to C, and a local variable F pointing to E.
//
// Starting from the pointer in local variable F, we will eventually
// scan all of E, C, D, and A (in that order). B is never scanned
// because there is no live pointer to it. If B is also statically
// dead (meaning that foo() never accesses B again after it calls
// bar()), then B's pointers into the heap are not considered live.
namespace go;

using goarch = @internal.goarch_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using runtime.@internal;

partial class runtime_package {

internal const bool stackTraceDebug = false;

// Buffer for pointers found during stack tracing.
// Must be smaller than or equal to workbuf.
[GoType] partial struct stackWorkBuf {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref stackWorkBufHdr stackWorkBufHdr { get; }
    internal array<uintptr> obj = new((_WorkbufSize - @unsafe.Sizeof(new stackWorkBufHdr(nil))) / goarch.PtrSize);
}

// Header declaration must come after the buf declaration above, because of issue #14620.
[GoType] partial struct stackWorkBufHdr {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref workbufhdr workbufhdr { get; }
    internal ж<stackWorkBuf> next; // linked list of workbufs
}

// Note: we could theoretically repurpose lfnode.next as this next pointer.
// It would save 1 word, but that probably isn't worth busting open
// the lfnode API.

// Buffer for stack objects found on a goroutine stack.
// Must be smaller than or equal to workbuf.
[GoType] partial struct stackObjectBuf {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref stackObjectBufHdr stackObjectBufHdr { get; }
    internal array<stackObject> obj = new((_WorkbufSize - @unsafe.Sizeof(new stackObjectBufHdr(nil))) / @unsafe.Sizeof(new stackObject(nil)));
}

[GoType] partial struct stackObjectBufHdr {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref workbufhdr workbufhdr { get; }
    internal ж<stackObjectBuf> next;
}

[GoInit] internal static void initΔ2() {
    if (@unsafe.Sizeof(new stackWorkBuf(nil)) > @unsafe.Sizeof(new workbuf(nil))) {
        throw panic("stackWorkBuf too big");
    }
    if (@unsafe.Sizeof(new stackObjectBuf(nil)) > @unsafe.Sizeof(new workbuf(nil))) {
        throw panic("stackObjectBuf too big");
    }
}

// A stackObject represents a variable on the stack that has had
// its address taken.
[GoType] partial struct stackObject {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal uint32 off;             // offset above stack.lo
    internal uint32 size;             // size of object
    internal ж<stackObjectRecord> r; // info of the object (for ptr/nonptr bits). nil if object has been scanned.
    internal ж<stackObject> left;    // objects with lower addresses
    internal ж<stackObject> right;    // objects with higher addresses
}

// obj.r = r, but with no write barrier.
//
//go:nowritebarrier
[GoRecv] internal static void setRecord(this ref stackObject obj, ж<stackObjectRecord> Ꮡr) {
    ref var r = ref Ꮡr.val;

    // Types of stack objects are always in read-only memory, not the heap.
    // So not using a write barrier is ok.
    ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡ(obj.r))))).val = ((uintptr)new @unsafe.Pointer(Ꮡr));
}

// A stackScanState keeps track of the state used during the GC walk
// of a goroutine.
[GoType] partial struct stackScanState {
    // stack limits
    internal Δstack stack;
    // conservative indicates that the next frame must be scanned conservatively.
    // This applies only to the innermost frame at an async safe-point.
    internal bool conservative;
    // buf contains the set of possible pointers to stack objects.
    // Organized as a LIFO linked list of buffers.
    // All buffers except possibly the head buffer are full.
    internal ж<stackWorkBuf> buf;
    internal ж<stackWorkBuf> freeBuf; // keep around one free buffer for allocation hysteresis
    // cbuf contains conservative pointers to stack objects. If
    // all pointers to a stack object are obtained via
    // conservative scanning, then the stack object may be dead
    // and may contain dead pointers, so it must be scanned
    // defensively.
    internal ж<stackWorkBuf> cbuf;
    // list of stack objects
    // Objects are in increasing address order.
    internal ж<stackObjectBuf> head;
    internal ж<stackObjectBuf> tail;
    internal nint nobjs;
    // root of binary tree for fast object lookup by address
    // Initialized by buildIndex.
    internal ж<stackObject> root;
}

// Add p as a potential pointer to a stack object.
// p must be a stack address.
[GoRecv] internal static void putPtr(this ref stackScanState s, uintptr Δp, bool conservative) {
    if (Δp < s.stack.lo || Δp >= s.stack.hi) {
        @throw("address not a stack address"u8);
    }
    var head = Ꮡ(s.buf);
    if (conservative) {
        head = Ꮡ(s.cbuf);
    }
    var buf = head.val;
    if (buf == nil){
        // Initial setup.
        buf = (ж<stackWorkBuf>)(uintptr)(new @unsafe.Pointer(getempty()));
        buf.nobj = 0;
        buf.next = default!;
        head.val = buf;
    } else 
    if (buf.nobj == len((~buf).obj)) {
        if (s.freeBuf != nil){
            buf = s.freeBuf;
            s.freeBuf = default!;
        } else {
            buf = (ж<stackWorkBuf>)(uintptr)(new @unsafe.Pointer(getempty()));
        }
        buf.nobj = 0;
        buf.next = head.val;
        head.val = buf;
    }
    (~buf).obj[buf.nobj] = Δp;
    buf.nobj++;
}

// Remove and return a potential pointer to a stack object.
// Returns 0 if there are no more pointers available.
//
// This prefers non-conservative pointers so we scan stack objects
// precisely if there are any non-conservative pointers to them.
[GoRecv] internal static (uintptr Δp, bool conservative) getPtr(this ref stackScanState s) {
    uintptr Δp = default!;
    bool conservative = default!;

    foreach (var (_, head) in new ж<ж<stackWorkBuf>>[]{Ꮡ(s.buf), Ꮡ(s.cbuf)}.slice()) {
        var buf = head.val;
        if (buf == nil) {
            // Never had any data.
            continue;
        }
        if (buf.nobj == 0) {
            if (s.freeBuf != nil) {
                // Free old freeBuf.
                putempty((ж<workbuf>)(uintptr)(new @unsafe.Pointer(s.freeBuf)));
            }
            // Move buf to the freeBuf.
            s.freeBuf = buf;
            buf = buf.next;
            head.val = buf;
            if (buf == nil) {
                // No more data in this list.
                continue;
            }
        }
        buf.nobj--;
        return ((~buf).obj[buf.nobj], head == Ꮡ(s.cbuf));
    }
    // No more data in either list.
    if (s.freeBuf != nil) {
        putempty((ж<workbuf>)(uintptr)(new @unsafe.Pointer(s.freeBuf)));
        s.freeBuf = default!;
    }
    return (0, false);
}

// addObject adds a stack object at addr of type typ to the set of stack objects.
[GoRecv] internal static void addObject(this ref stackScanState s, uintptr addr, ж<stackObjectRecord> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var x = s.tail;
    if (x == nil) {
        // initial setup
        x = (ж<stackObjectBuf>)(uintptr)(new @unsafe.Pointer(getempty()));
        x.next = default!;
        s.head = x;
        s.tail = x;
    }
    if (x.nobj > 0 && ((uint32)(addr - s.stack.lo)) < (~x).obj[x.nobj - 1].off + (~x).obj[x.nobj - 1].size) {
        @throw("objects added out of order or overlapping"u8);
    }
    if (x.nobj == len((~x).obj)) {
        // full buffer - allocate a new buffer, add to end of linked list
        var y = (ж<stackObjectBuf>)(uintptr)(new @unsafe.Pointer(getempty()));
        y.next = default!;
        x.next = y;
        s.tail = y;
        x = y;
    }
    var obj = Ꮡ(~x).obj.at<stackObject>(x.nobj);
    x.nobj++;
    obj.val.off = ((uint32)(addr - s.stack.lo));
    obj.val.size = ((uint32)r.size);
    obj.setRecord(Ꮡr);
    // obj.left and obj.right will be initialized by buildIndex before use.
    s.nobjs++;
}

// buildIndex initializes s.root to a binary search tree.
// It should be called after all addObject calls but before
// any call of findObject.
[GoRecv] internal static void buildIndex(this ref stackScanState s) {
    (s.root, _, _) = binarySearchTree(s.head, 0, s.nobjs);
}

// Build a binary search tree with the n objects in the list
// x.obj[idx], x.obj[idx+1], ..., x.next.obj[0], ...
// Returns the root of that tree, and the buf+idx of the nth object after x.obj[idx].
// (The first object that was not included in the binary search tree.)
// If n == 0, returns nil, x.
internal static (ж<stackObject> root, ж<stackObjectBuf> restBuf, nint restIdx) binarySearchTree(ж<stackObjectBuf> Ꮡx, nint idx, nint n) {
    ж<stackObject> root = default!;
    ж<stackObjectBuf> restBuf = default!;
    nint restIdx = default!;

    ref var x = ref Ꮡx.val;
    if (n == 0) {
        return (default!, Ꮡx, idx);
    }
    ж<stackObject> left = default!;
    ж<stackObject> right = default!;
    (left, x, idx) = binarySearchTree(Ꮡx, idx, n / 2);
    root = Ꮡx.obj.at<stackObject>(idx);
    idx++;
    if (idx == len(x.obj)) {
        x = x.next;
        idx = 0;
    }
    (right, x, idx) = binarySearchTree(Ꮡx, idx, n - n / 2 - 1);
    root.val.left = left;
    root.val.right = right;
    return (root, Ꮡx, idx);
}

// findObject returns the stack object containing address a, if any.
// Must have called buildIndex previously.
[GoRecv] internal static ж<stackObject> findObject(this ref stackScanState s, uintptr a) {
    var off = ((uint32)(a - s.stack.lo));
    var obj = s.root;
    while (ᐧ) {
        if (obj == nil) {
            return default!;
        }
        if (off < (~obj).off) {
            obj = obj.val.left;
            continue;
        }
        if (off >= (~obj).off + (~obj).size) {
            obj = obj.val.right;
            continue;
        }
        return obj;
    }
}

} // end runtime_package
