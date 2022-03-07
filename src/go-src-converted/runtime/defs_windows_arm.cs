// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_windows_arm.go


namespace go;

public static partial class runtime_package {

    // NOTE(rsc): _CONTEXT_CONTROL is actually 0x200001 and should include PC, SP, and LR.
    // However, empirically, LR doesn't come along on Windows 10
    // unless you also set _CONTEXT_INTEGER (0x200002).
    // Without LR, we skip over the next-to-bottom function in profiles
    // when the bottom function is frameless.
    // So we set both here, to make a working _CONTEXT_CONTROL.
private static readonly nuint _CONTEXT_CONTROL = 0x200003;



private partial struct neon128 {
    public ulong low;
    public long high;
}

private partial struct context {
    public uint contextflags;
    public uint r0;
    public uint r1;
    public uint r2;
    public uint r3;
    public uint r4;
    public uint r5;
    public uint r6;
    public uint r7;
    public uint r8;
    public uint r9;
    public uint r10;
    public uint r11;
    public uint r12;
    public uint spr;
    public uint lrr;
    public uint pc;
    public uint cpsr;
    public uint fpscr;
    public uint padding;
    public array<neon128> floatNeon;
    public array<uint> bvr;
    public array<uint> bcr;
    public array<uint> wvr;
    public array<uint> wcr;
    public array<uint> padding2;
}

private static System.UIntPtr ip(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.pc);
}
private static System.UIntPtr sp(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.spr);
}
private static System.UIntPtr lr(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.lrr);
}

private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.pc = uint32(x);
}
private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.spr = uint32(x);
}
private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.lrr = uint32(x);
}

private static void dumpregs(ptr<context> _addr_r) {
    ref context r = ref _addr_r.val;

    print("r0   ", hex(r.r0), "\n");
    print("r1   ", hex(r.r1), "\n");
    print("r2   ", hex(r.r2), "\n");
    print("r3   ", hex(r.r3), "\n");
    print("r4   ", hex(r.r4), "\n");
    print("r5   ", hex(r.r5), "\n");
    print("r6   ", hex(r.r6), "\n");
    print("r7   ", hex(r.r7), "\n");
    print("r8   ", hex(r.r8), "\n");
    print("r9   ", hex(r.r9), "\n");
    print("r10  ", hex(r.r10), "\n");
    print("r11  ", hex(r.r11), "\n");
    print("r12  ", hex(r.r12), "\n");
    print("sp   ", hex(r.spr), "\n");
    print("lr   ", hex(r.lrr), "\n");
    print("pc   ", hex(r.pc), "\n");
    print("cpsr ", hex(r.cpsr), "\n");
}

private static void stackcheck() { 
    // TODO: not implemented on ARM
}

} // end runtime_package
