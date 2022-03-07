// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || arm || mips || mipsle
// +build 386 arm mips mipsle

// package runtime -- go2cs converted at 2022 March 06 22:08:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lfstack_32bit.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // On 32-bit systems, the stored uint64 has a 32-bit pointer and 32-bit count.
private static ulong lfstackPack(ptr<lfnode> _addr_node, System.UIntPtr cnt) {
    ref lfnode node = ref _addr_node.val;

    return uint64(uintptr(@unsafe.Pointer(node))) << 32 | uint64(cnt);
}

private static ptr<lfnode> lfstackUnpack(ulong val) {
    return _addr_(lfnode.val)(@unsafe.Pointer(uintptr(val >> 32)))!;
}

} // end runtime_package
