// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips || mipsle
// +build mips mipsle

// Export some functions via linkname to assembly in sync/atomic.
//go:linkname Xadd64
//go:linkname Xchg64
//go:linkname Cas64
//go:linkname Load64
//go:linkname Store64

// package atomic -- go2cs converted at 2022 March 06 22:08:19 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Program Files\Go\src\runtime\internal\atomic\atomic_mipsx.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;

namespace go.runtime.@internal;

public static partial class atomic_package {

    // TODO implement lock striping
private static var @lock = default;

//go:noescape
private static void spinLock(ptr<uint> state);

//go:noescape
private static void spinUnlock(ptr<uint> state);

//go:nosplit
private static void lockAndCheck(ptr<ulong> _addr_addr) {
    ref ulong addr = ref _addr_addr.val;
 
    // ensure 8-byte alignment
    if (uintptr(@unsafe.Pointer(addr)) & 7 != 0) {>>MARKER:FUNCTION_spinUnlock_BLOCK_PREFIX<<
        panicUnaligned();
    }
    _ = addr;

    spinLock(_addr_@lock.state);

}

//go:nosplit
private static void unlock() {
    spinUnlock(_addr_@lock.state);
}

//go:nosplit
private static void unlockNoFence() {
    @lock.state = 0;
}

//go:nosplit
public static ulong Xadd64(ptr<ulong> _addr_addr, long delta) {
    ulong @new = default;
    ref ulong addr = ref _addr_addr.val;

    lockAndCheck(_addr_addr);

    new = addr + uint64(delta).val;
    addr = new;

    unlock();
    return ;
}

//go:nosplit
public static ulong Xchg64(ptr<ulong> _addr_addr, ulong @new) {
    ulong old = default;
    ref ulong addr = ref _addr_addr.val;

    lockAndCheck(_addr_addr);

    old = addr;
    addr = new;

    unlock();
    return ;
}

//go:nosplit
public static bool Cas64(ptr<ulong> _addr_addr, ulong old, ulong @new) {
    bool swapped = default;
    ref ulong addr = ref _addr_addr.val;

    lockAndCheck(_addr_addr);

    if ((addr) == old) {>>MARKER:FUNCTION_spinLock_BLOCK_PREFIX<<
        addr = new;
        unlock();
        return true;
    }
    unlockNoFence();
    return false;

}

//go:nosplit
public static ulong Load64(ptr<ulong> _addr_addr) {
    ulong val = default;
    ref ulong addr = ref _addr_addr.val;

    lockAndCheck(_addr_addr);

    val = addr;

    unlock();
    return ;
}

//go:nosplit
public static void Store64(ptr<ulong> _addr_addr, ulong val) {
    ref ulong addr = ref _addr_addr.val;

    lockAndCheck(_addr_addr);

    addr = val;

    unlock();
    return ;
}

//go:noescape
public static uint Xadd(ptr<uint> ptr, int delta);

//go:noescape
public static System.UIntPtr Xadduintptr(ptr<System.UIntPtr> ptr, System.UIntPtr delta);

//go:noescape
public static uint Xchg(ptr<uint> ptr, uint @new);

//go:noescape
public static System.UIntPtr Xchguintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new);

//go:noescape
public static uint Load(ptr<uint> ptr);

//go:noescape
public static byte Load8(ptr<byte> ptr);

// NO go:noescape annotation; *ptr escapes if result escapes (#31525)
public static unsafe.Pointer Loadp(unsafe.Pointer ptr);

//go:noescape
public static uint LoadAcq(ptr<uint> ptr);

//go:noescape
public static System.UIntPtr LoadAcquintptr(ptr<System.UIntPtr> ptr);

//go:noescape
public static void And8(ptr<byte> ptr, byte val);

//go:noescape
public static void Or8(ptr<byte> ptr, byte val);

//go:noescape
public static void And(ptr<uint> ptr, uint val);

//go:noescape
public static void Or(ptr<uint> ptr, uint val);

//go:noescape
public static void Store(ptr<uint> ptr, uint val);

//go:noescape
public static void Store8(ptr<byte> ptr, byte val);

// NO go:noescape annotation; see atomic_pointer.go.
public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val);

//go:noescape
public static void StoreRel(ptr<uint> ptr, uint val);

//go:noescape
public static void StoreReluintptr(ptr<System.UIntPtr> ptr, System.UIntPtr val);

//go:noescape
public static bool CasRel(ptr<uint> addr, uint old, uint @new);

} // end atomic_package
