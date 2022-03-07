// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// CPU affinity functions

// package unix -- go2cs converted at 2022 March 06 23:26:28 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\affinity_linux.go
using bits = go.math.bits_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static readonly var cpuSetSize = _CPU_SETSIZE / _NCPUBITS;

// CPUSet represents a CPU affinity mask.


// CPUSet represents a CPU affinity mask.
public partial struct CPUSet { // : array<cpuMask>
}

private static error schedAffinity(System.UIntPtr trap, nint pid, ptr<CPUSet> _addr_set) {
    ref CPUSet set = ref _addr_set.val;

    var (_, _, e) = RawSyscall(trap, uintptr(pid), uintptr(@unsafe.Sizeof(set)), uintptr(@unsafe.Pointer(set)));
    if (e != 0) {
        return error.As(errnoErr(e))!;
    }
    return error.As(null!)!;

}

// SchedGetaffinity gets the CPU affinity mask of the thread specified by pid.
// If pid is 0 the calling thread is used.
public static error SchedGetaffinity(nint pid, ptr<CPUSet> _addr_set) {
    ref CPUSet set = ref _addr_set.val;

    return error.As(schedAffinity(SYS_SCHED_GETAFFINITY, pid, _addr_set))!;
}

// SchedSetaffinity sets the CPU affinity mask of the thread specified by pid.
// If pid is 0 the calling thread is used.
public static error SchedSetaffinity(nint pid, ptr<CPUSet> _addr_set) {
    ref CPUSet set = ref _addr_set.val;

    return error.As(schedAffinity(SYS_SCHED_SETAFFINITY, pid, _addr_set))!;
}

// Zero clears the set s, so that it contains no CPUs.
private static void Zero(this ptr<CPUSet> _addr_s) {
    ref CPUSet s = ref _addr_s.val;

    foreach (var (i) in s) {
        s[i] = 0;
    }
}

private static nint cpuBitsIndex(nint cpu) {
    return cpu / _NCPUBITS;
}

private static cpuMask cpuBitsMask(nint cpu) {
    return cpuMask(1 << (int)((uint(cpu) % _NCPUBITS)));
}

// Set adds cpu to the set s.
private static void Set(this ptr<CPUSet> _addr_s, nint cpu) {
    ref CPUSet s = ref _addr_s.val;

    var i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        s[i] |= cpuBitsMask(cpu);
    }
}

// Clear removes cpu from the set s.
private static void Clear(this ptr<CPUSet> _addr_s, nint cpu) {
    ref CPUSet s = ref _addr_s.val;

    var i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        s[i] &= cpuBitsMask(cpu);
    }
}

// IsSet reports whether cpu is in the set s.
private static bool IsSet(this ptr<CPUSet> _addr_s, nint cpu) {
    ref CPUSet s = ref _addr_s.val;

    var i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        return s[i] & cpuBitsMask(cpu) != 0;
    }
    return false;

}

// Count returns the number of CPUs in the set s.
private static nint Count(this ptr<CPUSet> _addr_s) {
    ref CPUSet s = ref _addr_s.val;

    nint c = 0;
    foreach (var (_, b) in s) {
        c += bits.OnesCount64(uint64(b));
    }    return c;
}

} // end unix_package
