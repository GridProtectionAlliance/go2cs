// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mem_windows.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint _MEM_COMMIT = 0x1000;
private static readonly nuint _MEM_RESERVE = 0x2000;
private static readonly nuint _MEM_DECOMMIT = 0x4000;
private static readonly nuint _MEM_RELEASE = 0x8000;

private static readonly nuint _PAGE_READWRITE = 0x0004;
private static readonly nuint _PAGE_NOACCESS = 0x0001;

private static readonly nint _ERROR_NOT_ENOUGH_MEMORY = 8;
private static readonly nint _ERROR_COMMITMENT_LIMIT = 1455;


// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//go:nosplit
private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(int64(n));
    return @unsafe.Pointer(stdcall4(_VirtualAlloc, 0, n, _MEM_COMMIT | _MEM_RESERVE, _PAGE_READWRITE));
}

private static void sysUnused(unsafe.Pointer v, System.UIntPtr n) {
    var r = stdcall3(_VirtualFree, uintptr(v), n, _MEM_DECOMMIT);
    if (r != 0) {
        return ;
    }
    while (n > 0) {
        var small = n;
        while (small >= 4096 && stdcall3(_VirtualFree, uintptr(v), small, _MEM_DECOMMIT) == 0) {
            small /= 2;
            small &= 4096 - 1;
        }
        if (small < 4096) {
            print("runtime: VirtualFree of ", small, " bytes failed with errno=", getlasterror(), "\n");
            throw("runtime: failed to decommit pages");
        }
        v = add(v, small);
        n -= small;

    }

}

private static void sysUsed(unsafe.Pointer v, System.UIntPtr n) {
    var p = stdcall4(_VirtualAlloc, uintptr(v), n, _MEM_COMMIT, _PAGE_READWRITE);
    if (p == uintptr(v)) {
        return ;
    }
    var k = n;
    while (k > 0) {
        var small = k;
        while (small >= 4096 && stdcall4(_VirtualAlloc, uintptr(v), small, _MEM_COMMIT, _PAGE_READWRITE) == 0) {
            small /= 2;
            small &= 4096 - 1;
        }
        if (small < 4096) {
            var errno = getlasterror();

            if (errno == _ERROR_NOT_ENOUGH_MEMORY || errno == _ERROR_COMMITMENT_LIMIT) 
                print("runtime: VirtualAlloc of ", n, " bytes failed with errno=", errno, "\n");
                throw("out of memory");
            else 
                print("runtime: VirtualAlloc of ", small, " bytes failed with errno=", errno, "\n");
                throw("runtime: failed to commit pages");
            
        }
        v = add(v, small);
        k -= small;

    }

}

private static void sysHugePage(unsafe.Pointer v, System.UIntPtr n) {
}

// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//go:nosplit
private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(-int64(n));
    var r = stdcall3(_VirtualFree, uintptr(v), 0, _MEM_RELEASE);
    if (r == 0) {
        print("runtime: VirtualFree of ", n, " bytes failed with errno=", getlasterror(), "\n");
        throw("runtime: failed to release pages");
    }
}

private static void sysFault(unsafe.Pointer v, System.UIntPtr n) { 
    // SysUnused makes the memory inaccessible and prevents its reuse
    sysUnused(v, n);

}

private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n) { 
    // v is just a hint.
    // First try at v.
    // This will fail if any of [v, v+n) is already reserved.
    v = @unsafe.Pointer(stdcall4(_VirtualAlloc, uintptr(v), n, _MEM_RESERVE, _PAGE_READWRITE));
    if (v != null) {
        return v;
    }
    return @unsafe.Pointer(stdcall4(_VirtualAlloc, 0, n, _MEM_RESERVE, _PAGE_READWRITE));

}

private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(int64(n));
}

} // end runtime_package
