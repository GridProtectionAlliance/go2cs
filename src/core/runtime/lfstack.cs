// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Lock-free stack.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

[GoType("num:uint64")] partial struct lfstack;

[GoRecv] internal static void push(this ref lfstack head, ж<lfnode> Ꮡnode) {
    ref var node = ref Ꮡnode.val;

    node.pushcnt++;
    var @new = lfstackPack(Ꮡnode, node.pushcnt);
    {
        var node1 = lfstackUnpack(@new); if (node1 != Ꮡnode) {
            print("runtime: lfstack.push invalid packing: node=", node, " cnt=", ((Δhex)node.pushcnt), " packed=", ((Δhex)@new), " -> node=", node1, "\n");
            @throw("lfstack.push"u8);
        }
    }
    while (ᐧ) {
        var old = atomic.Load64(((ж<uint64>)head));
        node.next = old;
        if (atomic.Cas64(((ж<uint64>)head), old, @new)) {
            break;
        }
    }
}

[GoRecv] internal static @unsafe.Pointer pop(this ref lfstack head) {
    while (ᐧ) {
        var old = atomic.Load64(((ж<uint64>)head));
        if (old == 0) {
            return default!;
        }
        var node = lfstackUnpack(old);
        var next = atomic.Load64(Ꮡ((~node).next));
        if (atomic.Cas64(((ж<uint64>)head), old, next)) {
            return new @unsafe.Pointer(node);
        }
    }
}

[GoRecv] internal static bool empty(this ref lfstack head) {
    return atomic.Load64(((ж<uint64>)head)) == 0;
}

// lfnodeValidate panics if node is not a valid address for use with
// lfstack.push. This only needs to be called when node is allocated.
internal static void lfnodeValidate(ж<lfnode> Ꮡnode) {
    ref var node = ref Ꮡnode.val;

    {
        var (@base, _, _) = findObject(((uintptr)new @unsafe.Pointer(Ꮡnode)), 0, 0); if (@base != 0) {
            @throw("lfstack node allocated from the heap"u8);
        }
    }
    if (lfstackUnpack(lfstackPack(Ꮡnode, ^((uintptr)0))) != Ꮡnode) {
        printlock();
        println("runtime: bad lfnode address", ((Δhex)((uintptr)new @unsafe.Pointer(Ꮡnode))));
        @throw("bad lfnode address"u8);
    }
}

internal static uint64 lfstackPack(ж<lfnode> Ꮡnode, uintptr cnt) {
    ref var node = ref Ꮡnode.val;

    return ((uint64)taggedPointerPack(new @unsafe.Pointer(Ꮡnode), cnt));
}

internal static ж<lfnode> lfstackUnpack(uint64 val) {
    return (ж<lfnode>)(uintptr)(((taggedPointer)val).pointer());
}

} // end runtime_package
