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

// package runtime -- go2cs converted at 2020 October 09 04:46:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcstack.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var stackTraceDebug = false;

        // Buffer for pointers found during stack tracing.
        // Must be smaller than or equal to workbuf.
        //
        //go:notinheap


        // Buffer for pointers found during stack tracing.
        // Must be smaller than or equal to workbuf.
        //
        //go:notinheap
        private partial struct stackWorkBuf
        {
            public ref stackWorkBufHdr stackWorkBufHdr => ref stackWorkBufHdr_val;
            public array<System.UIntPtr> obj;
        }

        // Header declaration must come after the buf declaration above, because of issue #14620.
        //
        //go:notinheap
        private partial struct stackWorkBufHdr
        {
            public ref workbufhdr workbufhdr => ref workbufhdr_val;
            public ptr<stackWorkBuf> next; // linked list of workbufs
// Note: we could theoretically repurpose lfnode.next as this next pointer.
// It would save 1 word, but that probably isn't worth busting open
// the lfnode API.
        }

        // Buffer for stack objects found on a goroutine stack.
        // Must be smaller than or equal to workbuf.
        //
        //go:notinheap
        private partial struct stackObjectBuf
        {
            public ref stackObjectBufHdr stackObjectBufHdr => ref stackObjectBufHdr_val;
            public array<stackObject> obj;
        }

        //go:notinheap
        private partial struct stackObjectBufHdr
        {
            public ref workbufhdr workbufhdr => ref workbufhdr_val;
            public ptr<stackObjectBuf> next;
        }

        private static void init() => func((_, panic, __) =>
        {
            if (@unsafe.Sizeof(new stackWorkBuf()) > @unsafe.Sizeof(new workbuf()))
            {
                panic("stackWorkBuf too big");
            }

            if (@unsafe.Sizeof(new stackObjectBuf()) > @unsafe.Sizeof(new workbuf()))
            {
                panic("stackObjectBuf too big");
            }

        });

        // A stackObject represents a variable on the stack that has had
        // its address taken.
        //
        //go:notinheap
        private partial struct stackObject
        {
            public uint off; // offset above stack.lo
            public uint size; // size of object
            public ptr<_type> typ; // type info (for ptr/nonptr bits). nil if object has been scanned.
            public ptr<stackObject> left; // objects with lower addresses
            public ptr<stackObject> right; // objects with higher addresses
        }

        // obj.typ = typ, but with no write barrier.
        //go:nowritebarrier
        private static void setType(this ptr<stackObject> _addr_obj, ptr<_type> _addr_typ)
        {
            ref stackObject obj = ref _addr_obj.val;
            ref _type typ = ref _addr_typ.val;
 
            // Types of stack objects are always in read-only memory, not the heap.
            // So not using a write barrier is ok.
            (uintptr.val)(@unsafe.Pointer(_addr_obj.typ)).val;

            uintptr(@unsafe.Pointer(typ));

        }

        // A stackScanState keeps track of the state used during the GC walk
        // of a goroutine.
        //
        //go:notinheap
        private partial struct stackScanState
        {
            public pcvalueCache cache; // stack limits
            public stack stack; // conservative indicates that the next frame must be scanned conservatively.
// This applies only to the innermost frame at an async safe-point.
            public bool conservative; // buf contains the set of possible pointers to stack objects.
// Organized as a LIFO linked list of buffers.
// All buffers except possibly the head buffer are full.
            public ptr<stackWorkBuf> buf;
            public ptr<stackWorkBuf> freeBuf; // keep around one free buffer for allocation hysteresis

// cbuf contains conservative pointers to stack objects. If
// all pointers to a stack object are obtained via
// conservative scanning, then the stack object may be dead
// and may contain dead pointers, so it must be scanned
// defensively.
            public ptr<stackWorkBuf> cbuf; // list of stack objects
// Objects are in increasing address order.
            public ptr<stackObjectBuf> head;
            public ptr<stackObjectBuf> tail;
            public long nobjs; // root of binary tree for fast object lookup by address
// Initialized by buildIndex.
            public ptr<stackObject> root;
        }

        // Add p as a potential pointer to a stack object.
        // p must be a stack address.
        private static void putPtr(this ptr<stackScanState> _addr_s, System.UIntPtr p, bool conservative)
        {
            ref stackScanState s = ref _addr_s.val;

            if (p < s.stack.lo || p >= s.stack.hi)
            {
                throw("address not a stack address");
            }

            var head = _addr_s.buf;
            if (conservative)
            {
                head = _addr_s.cbuf;
            }

            var buf = head.val;
            if (buf == null)
            { 
                // Initial setup.
                buf = (stackWorkBuf.val)(@unsafe.Pointer(getempty()));
                buf.nobj = 0L;
                buf.next = null;
                head.val = buf;

            }
            else if (buf.nobj == len(buf.obj))
            {
                if (s.freeBuf != null)
                {
                    buf = s.freeBuf;
                    s.freeBuf = null;
                }
                else
                {
                    buf = (stackWorkBuf.val)(@unsafe.Pointer(getempty()));
                }

                buf.nobj = 0L;
                buf.next = head.val;
                head.val = buf;

            }

            buf.obj[buf.nobj] = p;
            buf.nobj++;

        }

        // Remove and return a potential pointer to a stack object.
        // Returns 0 if there are no more pointers available.
        //
        // This prefers non-conservative pointers so we scan stack objects
        // precisely if there are any non-conservative pointers to them.
        private static (System.UIntPtr, bool) getPtr(this ptr<stackScanState> _addr_s)
        {
            System.UIntPtr p = default;
            bool conservative = default;
            ref stackScanState s = ref _addr_s.val;

            foreach (var (_, head) in new slice<ptr<ptr<stackWorkBuf>>>(new ptr<ptr<stackWorkBuf>>[] { &s.buf, &s.cbuf }))
            {
                var buf = head.val;
                if (buf == null)
                { 
                    // Never had any data.
                    continue;

                }

                if (buf.nobj == 0L)
                {
                    if (s.freeBuf != null)
                    { 
                        // Free old freeBuf.
                        putempty((workbuf.val)(@unsafe.Pointer(s.freeBuf)));

                    } 
                    // Move buf to the freeBuf.
                    s.freeBuf = buf;
                    buf = buf.next;
                    head.val = buf;
                    if (buf == null)
                    { 
                        // No more data in this list.
                        continue;

                    }

                }

                buf.nobj--;
                return (buf.obj[buf.nobj], head == _addr_s.cbuf);

            } 
            // No more data in either list.
            if (s.freeBuf != null)
            {
                putempty((workbuf.val)(@unsafe.Pointer(s.freeBuf)));
                s.freeBuf = null;
            }

            return (0L, false);

        }

        // addObject adds a stack object at addr of type typ to the set of stack objects.
        private static void addObject(this ptr<stackScanState> _addr_s, System.UIntPtr addr, ptr<_type> _addr_typ)
        {
            ref stackScanState s = ref _addr_s.val;
            ref _type typ = ref _addr_typ.val;

            var x = s.tail;
            if (x == null)
            { 
                // initial setup
                x = (stackObjectBuf.val)(@unsafe.Pointer(getempty()));
                x.next = null;
                s.head = x;
                s.tail = x;

            }

            if (x.nobj > 0L && uint32(addr - s.stack.lo) < x.obj[x.nobj - 1L].off + x.obj[x.nobj - 1L].size)
            {
                throw("objects added out of order or overlapping");
            }

            if (x.nobj == len(x.obj))
            { 
                // full buffer - allocate a new buffer, add to end of linked list
                var y = (stackObjectBuf.val)(@unsafe.Pointer(getempty()));
                y.next = null;
                x.next = y;
                s.tail = y;
                x = y;

            }

            var obj = _addr_x.obj[x.nobj];
            x.nobj++;
            obj.off = uint32(addr - s.stack.lo);
            obj.size = uint32(typ.size);
            obj.setType(typ); 
            // obj.left and obj.right will be initialized by buildIndex before use.
            s.nobjs++;

        }

        // buildIndex initializes s.root to a binary search tree.
        // It should be called after all addObject calls but before
        // any call of findObject.
        private static void buildIndex(this ptr<stackScanState> _addr_s)
        {
            ref stackScanState s = ref _addr_s.val;

            s.root, _, _ = binarySearchTree(_addr_s.head, 0L, s.nobjs);
        }

        // Build a binary search tree with the n objects in the list
        // x.obj[idx], x.obj[idx+1], ..., x.next.obj[0], ...
        // Returns the root of that tree, and the buf+idx of the nth object after x.obj[idx].
        // (The first object that was not included in the binary search tree.)
        // If n == 0, returns nil, x.
        private static (ptr<stackObject>, ptr<stackObjectBuf>, long) binarySearchTree(ptr<stackObjectBuf> _addr_x, long idx, long n)
        {
            ptr<stackObject> root = default!;
            ptr<stackObjectBuf> restBuf = default!;
            long restIdx = default;
            ref stackObjectBuf x = ref _addr_x.val;

            if (n == 0L)
            {
                return (_addr_null!, _addr_x!, idx);
            }

            ptr<stackObject> left;            ptr<stackObject> right;

            left, x, idx = binarySearchTree(_addr_x, idx, n / 2L);
            root = _addr_x.obj[idx];
            idx++;
            if (idx == len(x.obj))
            {
                x = x.next;
                idx = 0L;
            }

            right, x, idx = binarySearchTree(_addr_x, idx, n - n / 2L - 1L);
            root.left = left;
            root.right = right;
            return (_addr_root!, _addr_x!, idx);

        }

        // findObject returns the stack object containing address a, if any.
        // Must have called buildIndex previously.
        private static ptr<stackObject> findObject(this ptr<stackScanState> _addr_s, System.UIntPtr a)
        {
            ref stackScanState s = ref _addr_s.val;

            var off = uint32(a - s.stack.lo);
            var obj = s.root;
            while (true)
            {
                if (obj == null)
                {
                    return _addr_null!;
                }

                if (off < obj.off)
                {
                    obj = obj.left;
                    continue;
                }

                if (off >= obj.off + obj.size)
                {
                    obj = obj.right;
                    continue;
                }

                return _addr_obj!;

            }


        }
    }
}
