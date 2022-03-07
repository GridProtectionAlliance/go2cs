// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mem_darwin.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // Don't split the stack as this function may be invoked without a valid G,
    // which prevents us from allocating more stack.
    //go:nosplit
private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    var (v, err) = mmap(null, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1, 0);
    if (err != 0) {
        return null;
    }
    sysStat.add(int64(n));
    return v;

}

private static void sysUnused(unsafe.Pointer v, System.UIntPtr n) { 
    // MADV_FREE_REUSABLE is like MADV_FREE except it also propagates
    // accounting information about the process to task_info.
    madvise(v, n, _MADV_FREE_REUSABLE);

}

private static void sysUsed(unsafe.Pointer v, System.UIntPtr n) { 
    // MADV_FREE_REUSE is necessary to keep the kernel's accounting
    // accurate. If called on any memory region that hasn't been
    // MADV_FREE_REUSABLE'd, it's a no-op.
    madvise(v, n, _MADV_FREE_REUSE);

}

private static void sysHugePage(unsafe.Pointer v, System.UIntPtr n) {
}

// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//go:nosplit
private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(-int64(n));
    munmap(v, n);
}

private static void sysFault(unsafe.Pointer v, System.UIntPtr n) {
    mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE | _MAP_FIXED, -1, 0);
}

private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n) {
    var (p, err) = mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1, 0);
    if (err != 0) {
        return null;
    }
    return p;

}

private static readonly nint _ENOMEM = 12;



private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(int64(n));

    var (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_FIXED | _MAP_PRIVATE, -1, 0);
    if (err == _ENOMEM) {
        throw("runtime: out of memory");
    }
    if (p != v || err != 0) {
        throw("runtime: cannot map pages in arena address space");
    }
}

} // end runtime_package
