// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using atomic = go.sync.atomic_package;
using go.sync;

partial class poll_package {

// fdMutex is a specialized synchronization primitive that manages
// lifetime of an fd and serializes access to Read, Write and Close
// methods on FD.
[GoType] partial struct fdMutex {
    internal uint64 state;
    internal uint32 rsema;
    internal uint32 wsema;
}

// fdMutex.state is organized as follows:
// 1 bit - whether FD is closed, if set all subsequent lock operations will fail.
// 1 bit - lock for read operations.
// 1 bit - lock for write operations.
// 20 bits - total number of references (read+write+misc).
// 20 bits - number of outstanding read waiters.
// 20 bits - number of outstanding write waiters.
internal static readonly UntypedInt mutexClosed = /* 1 << 0 */ 1;

internal static readonly UntypedInt mutexRLock = /* 1 << 1 */ 2;

internal static readonly UntypedInt mutexWLock = /* 1 << 2 */ 4;

internal static readonly UntypedInt mutexRef = /* 1 << 3 */ 8;

internal static readonly UntypedInt mutexRefMask = /* (1<<20 - 1) << 3 */ 8388600;

internal static readonly UntypedInt mutexRWait = /* 1 << 23 */ 8388608;

internal static readonly UntypedInt mutexRMask = /* (1<<20 - 1) << 23 */ 8796084633600;

internal static readonly UntypedInt mutexWWait = /* 1 << 43 */ 8796093022208;

internal static readonly UntypedInt mutexWMask = /* (1<<20 - 1) << 43 */ 9223363240761753600;

internal static readonly @string overflowMsg = "too many concurrent operations on a single file or socket (max 1048575)"u8;

// Read operations must do rwlock(true)/rwunlock(true).
//
// Write operations must do rwlock(false)/rwunlock(false).
//
// Misc operations must do incref/decref.
// Misc operations include functions like setsockopt and setDeadline.
// They need to use incref/decref to ensure that they operate on the
// correct fd in presence of a concurrent close call (otherwise fd can
// be closed under their feet).
//
// Close operations must do increfAndClose/decref.

// incref adds a reference to mu.
// It reports whether mu is available for reading or writing.
internal static bool incref(this ж<fdMutex> Ꮡmu) {
    ref var mu = ref Ꮡmu.Value;

    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }
        var @new = old + (uint64)mutexRef;
        if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
            throw panic(overflowMsg);
        }
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            return true;
        }
    }
}

// increfAndClose sets the state of mu to closed.
// It returns false if the file was already closed.
internal static bool increfAndClose(this ж<fdMutex> Ꮡmu) {
    ref var mu = ref Ꮡmu.Value;

    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }
        // Mark as closed and acquire a reference.
        var @new = ((uint64)(old | (uint64)mutexClosed)) + (uint64)mutexRef;
        if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
            throw panic(overflowMsg);
        }
        // Remove all read and write waiters.
        @new &= unchecked((uint64)~(uint64)((uint64)((uint64)mutexRMask | (uint64)mutexWMask)));
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            // Wake all read and write waiters,
            // they will observe closed flag after wakeup.
            while ((uint64)(old & (uint64)mutexRMask) != 0) {
                old -= mutexRWait;
                runtime_Semrelease(Ꮡmu.of(fdMutex.Ꮡrsema));
            }
            while ((uint64)(old & (uint64)mutexWMask) != 0) {
                old -= mutexWWait;
                runtime_Semrelease(Ꮡmu.of(fdMutex.Ꮡwsema));
            }
            return true;
        }
    }
}

// decref removes a reference from mu.
// It reports whether there is no remaining reference.
internal static bool decref(this ж<fdMutex> Ꮡmu) {
    ref var mu = ref Ꮡmu.Value;

    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexRefMask) == 0) {
            throw panic("inconsistent poll.fdMutex");
        }
        var @new = old - (uint64)mutexRef;
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            return (uint64)(@new & ((uint64)((uint64)mutexClosed | (uint64)mutexRefMask))) == mutexClosed;
        }
    }
}

// lock adds a reference to mu and locks mu.
// It reports whether mu is available for reading or writing.
internal static bool rwlock(this ж<fdMutex> Ꮡmu, bool read) {
    ref var mu = ref Ꮡmu.Value;

    uint64 mutexBit = default!;
    uint64 mutexWait = default!;
    uint64 mutexMask = default!;
    ж<uint32> mutexSema = default!;
    if (read){
        mutexBit = mutexRLock;
        mutexWait = mutexRWait;
        mutexMask = mutexRMask;
        mutexSema = Ꮡmu.of(fdMutex.Ꮡrsema);
    } else {
        mutexBit = mutexWLock;
        mutexWait = mutexWWait;
        mutexMask = mutexWMask;
        mutexSema = Ꮡmu.of(fdMutex.Ꮡwsema);
    }
    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }
        uint64 @new = default!;
        if ((uint64)(old & mutexBit) == 0){
            // Lock is free, acquire it.
            @new = ((uint64)(old | mutexBit)) + (uint64)mutexRef;
            if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
                throw panic(overflowMsg);
            }
        } else {
            // Wait for lock.
            @new = old + mutexWait;
            if ((uint64)(@new & mutexMask) == 0) {
                throw panic(overflowMsg);
            }
        }
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            if ((uint64)(old & mutexBit) == 0) {
                return true;
            }
            runtime_Semacquire(mutexSema);
        }
    }
}

// The signaller has subtracted mutexWait.

// unlock removes a reference from mu and unlocks mu.
// It reports whether there is no remaining reference.
internal static bool rwunlock(this ж<fdMutex> Ꮡmu, bool read) {
    ref var mu = ref Ꮡmu.Value;

    uint64 mutexBit = default!;
    uint64 mutexWait = default!;
    uint64 mutexMask = default!;
    ж<uint32> mutexSema = default!;
    if (read){
        mutexBit = mutexRLock;
        mutexWait = mutexRWait;
        mutexMask = mutexRMask;
        mutexSema = Ꮡmu.of(fdMutex.Ꮡrsema);
    } else {
        mutexBit = mutexWLock;
        mutexWait = mutexWWait;
        mutexMask = mutexWMask;
        mutexSema = Ꮡmu.of(fdMutex.Ꮡwsema);
    }
    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & mutexBit) == 0 || (uint64)(old & (uint64)mutexRefMask) == 0) {
            throw panic("inconsistent poll.fdMutex");
        }
        // Drop lock, drop reference and wake read waiter if present.
        var @new = ((uint64)(old & ~mutexBit)) - (uint64)mutexRef;
        if ((uint64)(old & mutexMask) != 0) {
            @new -= mutexWait;
        }
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            if ((uint64)(old & mutexMask) != 0) {
                runtime_Semrelease(mutexSema);
            }
            return (uint64)(@new & ((uint64)((uint64)mutexClosed | (uint64)mutexRefMask))) == mutexClosed;
        }
    }
}

// Implemented in runtime package.
internal static partial void runtime_Semacquire(ж<uint32> sema);

internal static partial void runtime_Semrelease(ж<uint32> sema);

// incref adds a reference to fd.
// It returns an error when fd cannot be used.
internal static error incref(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (!Ꮡfd.of(FD.Ꮡfdmu).incref()) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// decref removes a reference from fd.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static error decref(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (Ꮡfd.of(FD.Ꮡfdmu).decref()) {
        return Ꮡfd.destroy();
    }
    return default!;
}

// readLock adds a reference to fd and locks fd for reading.
// It returns an error when fd cannot be used for reading.
internal static error readLock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (!Ꮡfd.of(FD.Ꮡfdmu).rwlock(true)) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// readUnlock removes a reference from fd and unlocks fd for reading.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static void readUnlock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (Ꮡfd.of(FD.Ꮡfdmu).rwunlock(true)) {
        Ꮡfd.destroy();
    }
}

// writeLock adds a reference to fd and locks fd for writing.
// It returns an error when fd cannot be used for writing.
internal static error writeLock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (!Ꮡfd.of(FD.Ꮡfdmu).rwlock(false)) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// writeUnlock removes a reference from fd and unlocks fd for writing.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static void writeUnlock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (Ꮡfd.of(FD.Ꮡfdmu).rwunlock(false)) {
        Ꮡfd.destroy();
    }
}

} // end poll_package
