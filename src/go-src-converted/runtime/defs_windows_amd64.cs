// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_windows_amd64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _CONTEXT_CONTROL = 0x100001;



private partial struct m128a {
    public ulong low;
    public long high;
}

private partial struct context {
    public ulong p1home;
    public ulong p2home;
    public ulong p3home;
    public ulong p4home;
    public ulong p5home;
    public ulong p6home;
    public uint contextflags;
    public uint mxcsr;
    public ushort segcs;
    public ushort segds;
    public ushort seges;
    public ushort segfs;
    public ushort seggs;
    public ushort segss;
    public uint eflags;
    public ulong dr0;
    public ulong dr1;
    public ulong dr2;
    public ulong dr3;
    public ulong dr6;
    public ulong dr7;
    public ulong rax;
    public ulong rcx;
    public ulong rdx;
    public ulong rbx;
    public ulong rsp;
    public ulong rbp;
    public ulong rsi;
    public ulong rdi;
    public ulong r8;
    public ulong r9;
    public ulong r10;
    public ulong r11;
    public ulong r12;
    public ulong r13;
    public ulong r14;
    public ulong r15;
    public ulong rip;
    public array<byte> anon0;
    public array<m128a> vectorregister;
    public ulong vectorcontrol;
    public ulong debugcontrol;
    public ulong lastbranchtorip;
    public ulong lastbranchfromrip;
    public ulong lastexceptiontorip;
    public ulong lastexceptionfromrip;
}

private static System.UIntPtr ip(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.rip);
}
private static System.UIntPtr sp(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.rsp);
}

// AMD64 does not have link register, so this returns 0.
private static System.UIntPtr lr(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return 0;
}
private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

}

private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.rip = uint64(x);
}
private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.rsp = uint64(x);
}

private static void dumpregs(ptr<context> _addr_r) {
    ref context r = ref _addr_r.val;

    print("rax     ", hex(r.rax), "\n");
    print("rbx     ", hex(r.rbx), "\n");
    print("rcx     ", hex(r.rcx), "\n");
    print("rdi     ", hex(r.rdi), "\n");
    print("rsi     ", hex(r.rsi), "\n");
    print("rbp     ", hex(r.rbp), "\n");
    print("rsp     ", hex(r.rsp), "\n");
    print("r8      ", hex(r.r8), "\n");
    print("r9      ", hex(r.r9), "\n");
    print("r10     ", hex(r.r10), "\n");
    print("r11     ", hex(r.r11), "\n");
    print("r12     ", hex(r.r12), "\n");
    print("r13     ", hex(r.r13), "\n");
    print("r14     ", hex(r.r14), "\n");
    print("r15     ", hex(r.r15), "\n");
    print("rip     ", hex(r.rip), "\n");
    print("rflags  ", hex(r.eflags), "\n");
    print("cs      ", hex(r.segcs), "\n");
    print("fs      ", hex(r.segfs), "\n");
    print("gs      ", hex(r.seggs), "\n");
}

} // end runtime_package
