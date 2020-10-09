// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fixed-size object allocator. Returned memory is not zeroed.
//
// See malloc.go for overview.

// package runtime -- go2cs converted at 2020 October 09 04:46:35 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mfixalloc.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // FixAlloc is a simple free-list allocator for fixed size objects.
        // Malloc uses a FixAlloc wrapped around sysAlloc to manage its
        // mcache and mspan objects.
        //
        // Memory returned by fixalloc.alloc is zeroed by default, but the
        // caller may take responsibility for zeroing allocations by setting
        // the zero flag to false. This is only safe if the memory never
        // contains heap pointers.
        //
        // The caller is responsible for locking around FixAlloc calls.
        // Callers can keep state in the object but the first word is
        // smashed by freeing and reallocating.
        //
        // Consider marking fixalloc'd types go:notinheap.
        private partial struct fixalloc
        {
            public System.UIntPtr size;
            public Action<unsafe.Pointer, unsafe.Pointer> first; // called first time p is returned
            public unsafe.Pointer arg;
            public ptr<mlink> list;
            public System.UIntPtr chunk; // use uintptr instead of unsafe.Pointer to avoid write barriers
            public uint nchunk;
            public System.UIntPtr inuse; // in-use bytes now
            public ptr<ulong> stat;
            public bool zero; // zero allocations
        }

        // A generic linked list of blocks.  (Typically the block is bigger than sizeof(MLink).)
        // Since assignments to mlink.next will result in a write barrier being performed
        // this cannot be used by some of the internal GC structures. For example when
        // the sweeper is placing an unmarked object on the free list it does not want the
        // write barrier to be called since that could result in the object being reachable.
        //
        //go:notinheap
        private partial struct mlink
        {
            public ptr<mlink> next;
        }

        // Initialize f to allocate objects of the given size,
        // using the allocator to obtain chunks of memory.
        private static void init(this ptr<fixalloc> _addr_f, System.UIntPtr size, Action<unsafe.Pointer, unsafe.Pointer> first, unsafe.Pointer arg, ptr<ulong> _addr_stat)
        {
            ref fixalloc f = ref _addr_f.val;
            ref ulong stat = ref _addr_stat.val;

            f.size = size;
            f.first = first;
            f.arg = arg;
            f.list = null;
            f.chunk = 0L;
            f.nchunk = 0L;
            f.inuse = 0L;
            f.stat = stat;
            f.zero = true;
        }

        private static unsafe.Pointer alloc(this ptr<fixalloc> _addr_f)
        {
            ref fixalloc f = ref _addr_f.val;

            if (f.size == 0L)
            {
                print("runtime: use of FixAlloc_Alloc before FixAlloc_Init\n");
                throw("runtime: internal error");
            }

            if (f.list != null)
            {
                var v = @unsafe.Pointer(f.list);
                f.list = f.list.next;
                f.inuse += f.size;
                if (f.zero)
                {
                    memclrNoHeapPointers(v, f.size);
                }

                return v;

            }

            if (uintptr(f.nchunk) < f.size)
            {
                f.chunk = uintptr(persistentalloc(_FixAllocChunk, 0L, f.stat));
                f.nchunk = _FixAllocChunk;
            }

            v = @unsafe.Pointer(f.chunk);
            if (f.first != null)
            {
                f.first(f.arg, v);
            }

            f.chunk = f.chunk + f.size;
            f.nchunk -= uint32(f.size);
            f.inuse += f.size;
            return v;

        }

        private static void free(this ptr<fixalloc> _addr_f, unsafe.Pointer p)
        {
            ref fixalloc f = ref _addr_f.val;

            f.inuse -= f.size;
            var v = (mlink.val)(p);
            v.next = f.list;
            f.list = v;
        }
    }
}
