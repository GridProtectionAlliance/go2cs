// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mem_aix.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // Don't split the stack as this method may be invoked without a valid G, which
    // prevents us from allocating more stack.
    //go:nosplit
private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    var (p, err) = mmap(null, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1, 0);
    if (err != 0) {
        if (err == _EACCES) {
            print("runtime: mmap: access denied\n");
            exit(2);
        }
        if (err == _EAGAIN) {
            print("runtime: mmap: too much locked memory (check 'ulimit -l').\n");
            exit(2);
        }
        return null;

    }
    sysStat.add(int64(n));
    return p;

}

private static void sysUnused(unsafe.Pointer v, System.UIntPtr n) {
    madvise(v, n, _MADV_DONTNEED);
}

private static void sysUsed(unsafe.Pointer v, System.UIntPtr n) {
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

private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(int64(n)); 

    // AIX does not allow mapping a range that is already mapped.
    // So, call mprotect to change permissions.
    // Note that sysMap is always called with a non-nil pointer
    // since it transitions a Reserved memory region to Prepared,
    // so mprotect is always possible.
    var (_, err) = mprotect(v, n, _PROT_READ | _PROT_WRITE);
    if (err == _ENOMEM) {
        throw("runtime: out of memory");
    }
    if (err != 0) {
        throw("runtime: cannot map pages in arena address space");
    }
}

} // end runtime_package
