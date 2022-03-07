// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_windows_arm64.go


namespace go;

public static partial class runtime_package {

    // NOTE(rsc): _CONTEXT_CONTROL is actually 0x400001 and should include PC, SP, and LR.
    // However, empirically, LR doesn't come along on Windows 10
    // unless you also set _CONTEXT_INTEGER (0x400002).
    // Without LR, we skip over the next-to-bottom function in profiles
    // when the bottom function is frameless.
    // So we set both here, to make a working _CONTEXT_CONTROL.
private static readonly nuint _CONTEXT_CONTROL = 0x400003;



private partial struct neon128 {
    public ulong low;
    public long high;
}

// See https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-arm64_nt_context
private partial struct context {
    public uint contextflags;
    public uint cpsr;
    public array<ulong> x; // fp is x[29], lr is x[30]
    public ulong xsp;
    public ulong pc;
    public array<neon128> v;
    public uint fpcr;
    public uint fpsr;
    public array<uint> bcr;
    public array<ulong> bvr;
    public array<uint> wcr;
    public array<ulong> wvr;
}

private static System.UIntPtr ip(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.pc);
}
private static System.UIntPtr sp(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.xsp);
}
private static System.UIntPtr lr(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.x[30]);
}

private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.pc = uint64(x);
}
private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.xsp = uint64(x);
}
private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.x[30] = uint64(x);
}

private static void dumpregs(ptr<context> _addr_r) {
    ref context r = ref _addr_r.val;

    print("r0   ", hex(r.x[0]), "\n");
    print("r1   ", hex(r.x[1]), "\n");
    print("r2   ", hex(r.x[2]), "\n");
    print("r3   ", hex(r.x[3]), "\n");
    print("r4   ", hex(r.x[4]), "\n");
    print("r5   ", hex(r.x[5]), "\n");
    print("r6   ", hex(r.x[6]), "\n");
    print("r7   ", hex(r.x[7]), "\n");
    print("r8   ", hex(r.x[8]), "\n");
    print("r9   ", hex(r.x[9]), "\n");
    print("r10  ", hex(r.x[10]), "\n");
    print("r11  ", hex(r.x[11]), "\n");
    print("r12  ", hex(r.x[12]), "\n");
    print("r13  ", hex(r.x[13]), "\n");
    print("r14  ", hex(r.x[14]), "\n");
    print("r15  ", hex(r.x[15]), "\n");
    print("r16  ", hex(r.x[16]), "\n");
    print("r17  ", hex(r.x[17]), "\n");
    print("r18  ", hex(r.x[18]), "\n");
    print("r19  ", hex(r.x[19]), "\n");
    print("r20  ", hex(r.x[20]), "\n");
    print("r21  ", hex(r.x[21]), "\n");
    print("r22  ", hex(r.x[22]), "\n");
    print("r23  ", hex(r.x[23]), "\n");
    print("r24  ", hex(r.x[24]), "\n");
    print("r25  ", hex(r.x[25]), "\n");
    print("r26  ", hex(r.x[26]), "\n");
    print("r27  ", hex(r.x[27]), "\n");
    print("r28  ", hex(r.x[28]), "\n");
    print("r29  ", hex(r.x[29]), "\n");
    print("lr   ", hex(r.x[30]), "\n");
    print("sp   ", hex(r.xsp), "\n");
    print("pc   ", hex(r.pc), "\n");
    print("cpsr ", hex(r.cpsr), "\n");
}

private static void stackcheck() { 
    // TODO: not implemented on ARM
}

} // end runtime_package
