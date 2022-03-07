// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mem_plan9.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly var memDebug = false;



private static System.UIntPtr bloc = default;
private static System.UIntPtr blocMax = default;
private static mutex memlock = default;

private partial struct memHdr {
    public memHdrPtr next;
    public System.UIntPtr size;
}

private static memHdrPtr memFreelist = default; // sorted in ascending order

private partial struct memHdrPtr { // : System.UIntPtr
}

private static ptr<memHdr> ptr(this memHdrPtr p) {
    return _addr_(memHdr.val)(@unsafe.Pointer(p))!;
}
private static void set(this ptr<memHdrPtr> _addr_p, ptr<memHdr> _addr_x) {
    ref memHdrPtr p = ref _addr_p.val;
    ref memHdr x = ref _addr_x.val;

    p.val = memHdrPtr(@unsafe.Pointer(x));
}

private static unsafe.Pointer memAlloc(System.UIntPtr n) {
    n = memRound(n);
    ptr<memHdr> prevp;
    {
        var p = memFreelist.ptr();

        while (p != null) {
            if (p.size >= n) {
                if (p.size == n) {
                    if (prevp != null) {
                        prevp.next = p.next;
            p = p.next.ptr();
                    }
                    else
 {
                        memFreelist = p.next;
                    }

                }
                else
 {
                    p.size -= n;
                    p = (memHdr.val)(add(@unsafe.Pointer(p), p.size));
                }

                p.val = new memHdr();
                return @unsafe.Pointer(p);

            }

            prevp = p;

        }
    }
    return sbrk(n);

}

private static void memFree(unsafe.Pointer ap, System.UIntPtr n) {
    n = memRound(n);
    memclrNoHeapPointers(ap, n);
    var bp = (memHdr.val)(ap);
    bp.size = n;
    var bpn = uintptr(ap);
    if (memFreelist == 0) {
        bp.next = 0;
        memFreelist.set(bp);
        return ;
    }
    var p = memFreelist.ptr();
    if (bpn < uintptr(@unsafe.Pointer(p))) {
        memFreelist.set(bp);
        if (bpn + bp.size == uintptr(@unsafe.Pointer(p))) {
            bp.size += p.size;
            bp.next = p.next;
            p.val = new memHdr();
        }
        else
 {
            bp.next.set(p);
        }
        return ;

    }
    while (p.next != 0) {
        if (bpn > uintptr(@unsafe.Pointer(p)) && bpn < uintptr(@unsafe.Pointer(p.next))) {
            break;
        p = p.next.ptr();
        }
    }
    if (bpn + bp.size == uintptr(@unsafe.Pointer(p.next))) {
        bp.size += p.next.ptr().size;
        bp.next = p.next.ptr().next;
        p.next.ptr().val = new memHdr();
    }
    else
 {
        bp.next = p.next;
    }
    if (uintptr(@unsafe.Pointer(p)) + p.size == bpn) {
        p.size += bp.size;
        p.next = bp.next;
        bp.val = new memHdr();
    }
    else
 {
        p.next.set(bp);
    }
}

private static void memCheck() {
    if (memDebug == false) {
        return ;
    }
    {
        var p = memFreelist.ptr();

        while (p != null && p.next != 0) {
            if (uintptr(@unsafe.Pointer(p)) == uintptr(@unsafe.Pointer(p.next))) {
                print("runtime: ", @unsafe.Pointer(p), " == ", @unsafe.Pointer(p.next), "\n");
                throw("mem: infinite loop");
            p = p.next.ptr();
            }

            if (uintptr(@unsafe.Pointer(p)) > uintptr(@unsafe.Pointer(p.next))) {
                print("runtime: ", @unsafe.Pointer(p), " > ", @unsafe.Pointer(p.next), "\n");
                throw("mem: unordered list");
            }

            if (uintptr(@unsafe.Pointer(p)) + p.size > uintptr(@unsafe.Pointer(p.next))) {
                print("runtime: ", @unsafe.Pointer(p), "+", p.size, " > ", @unsafe.Pointer(p.next), "\n");
                throw("mem: overlapping blocks");
            }

            {
                var b = add(@unsafe.Pointer(p), @unsafe.Sizeof(new memHdr()));

                while (uintptr(b) < uintptr(@unsafe.Pointer(p)) + p.size) {
                    if (new ptr<ptr<ptr<byte>>>(b) != 0) {
                        print("runtime: value at addr ", b, " with offset ", uintptr(b) - uintptr(@unsafe.Pointer(p)), " in block ", p, " of size ", p.size, " is not zero\n");
                        throw("mem: uninitialised memory");
                    b = add(b, 1);
                    }

                }

            }

        }
    }

}

private static System.UIntPtr memRound(System.UIntPtr p) {
    return (p + _PAGESIZE - 1) & ~(_PAGESIZE - 1);
}

private static void initBloc() {
    bloc = memRound(firstmoduledata.end);
    blocMax = bloc;
}

private static unsafe.Pointer sbrk(System.UIntPtr n) { 
    // Plan 9 sbrk from /sys/src/libc/9sys/sbrk.c
    var bl = bloc;
    n = memRound(n);
    if (bl + n > blocMax) {
        if (brk_(@unsafe.Pointer(bl + n)) < 0) {
            return null;
        }
        blocMax = bl + n;

    }
    bloc += n;
    return @unsafe.Pointer(bl);

}

private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    lock(_addr_memlock);
    var p = memAlloc(n);
    memCheck();
    unlock(_addr_memlock);
    if (p != null) {
        sysStat.add(int64(n));
    }
    return p;

}

private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    sysStat.add(-int64(n));
    lock(_addr_memlock);
    if (uintptr(v) + n == bloc) { 
        // Address range being freed is at the end of memory,
        // so record a new lower value for end of memory.
        // Can't actually shrink address space because segment is shared.
        memclrNoHeapPointers(v, n);
        bloc -= n;

    }
    else
 {
        memFree(v, n);
        memCheck();
    }
    unlock(_addr_memlock);

}

private static void sysUnused(unsafe.Pointer v, System.UIntPtr n) {
}

private static void sysUsed(unsafe.Pointer v, System.UIntPtr n) {
}

private static void sysHugePage(unsafe.Pointer v, System.UIntPtr n) {
}

private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<sysMemStat> _addr_sysStat) {
    ref sysMemStat sysStat = ref _addr_sysStat.val;
 
    // sysReserve has already allocated all heap memory,
    // but has not adjusted stats.
    sysStat.add(int64(n));

}

private static void sysFault(unsafe.Pointer v, System.UIntPtr n) {
}

private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n) {
    lock(_addr_memlock);
    unsafe.Pointer p = default;
    if (uintptr(v) == bloc) { 
        // Address hint is the current end of memory,
        // so try to extend the address space.
        p = sbrk(n);

    }
    if (p == null && v == null) {
        p = memAlloc(n);
        memCheck();
    }
    unlock(_addr_memlock);
    return p;

}

} // end runtime_package
