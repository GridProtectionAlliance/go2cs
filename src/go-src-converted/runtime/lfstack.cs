// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Lock-free stack.

// package runtime -- go2cs converted at 2020 October 09 04:46:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lfstack.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // lfstack is the head of a lock-free stack.
        //
        // The zero value of lfstack is an empty list.
        //
        // This stack is intrusive. Nodes must embed lfnode as the first field.
        //
        // The stack does not keep GC-visible pointers to nodes, so the caller
        // is responsible for ensuring the nodes are not garbage collected
        // (typically by allocating them from manually-managed memory).
        private partial struct lfstack // : ulong
        {
        }

        private static void push(this ptr<lfstack> _addr_head, ptr<lfnode> _addr_node)
        {
            ref lfstack head = ref _addr_head.val;
            ref lfnode node = ref _addr_node.val;

            node.pushcnt++;
            var @new = lfstackPack(node, node.pushcnt);
            {
                var node1 = lfstackUnpack(new);

                if (node1 != node)
                {
                    print("runtime: lfstack.push invalid packing: node=", node, " cnt=", hex(node.pushcnt), " packed=", hex(new), " -> node=", node1, "\n");
                    throw("lfstack.push");
                }

            }

            while (true)
            {
                var old = atomic.Load64((uint64.val)(head));
                node.next = old;
                if (atomic.Cas64((uint64.val)(head), old, new))
                {
                    break;
                }

            }


        }

        private static unsafe.Pointer pop(this ptr<lfstack> _addr_head)
        {
            ref lfstack head = ref _addr_head.val;

            while (true)
            {
                var old = atomic.Load64((uint64.val)(head));
                if (old == 0L)
                {
                    return null;
                }

                var node = lfstackUnpack(old);
                var next = atomic.Load64(_addr_node.next);
                if (atomic.Cas64((uint64.val)(head), old, next))
                {
                    return @unsafe.Pointer(node);
                }

            }


        }

        private static bool empty(this ptr<lfstack> _addr_head)
        {
            ref lfstack head = ref _addr_head.val;

            return atomic.Load64((uint64.val)(head)) == 0L;
        }

        // lfnodeValidate panics if node is not a valid address for use with
        // lfstack.push. This only needs to be called when node is allocated.
        private static void lfnodeValidate(ptr<lfnode> _addr_node)
        {
            ref lfnode node = ref _addr_node.val;

            if (lfstackUnpack(lfstackPack(node, ~uintptr(0L))) != node)
            {
                printlock();
                println("runtime: bad lfnode address", hex(uintptr(@unsafe.Pointer(node))));
                throw("bad lfnode address");
            }

        }
    }
}
