// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

internal static readonly UntypedInt _CONTEXT_CONTROL = /* 0x100001 */ 1048577;

[GoType] partial struct m128a {
    internal uint64 low;
    internal int64 high;
}

[GoType] partial struct context {
    internal uint64 p1home;
    internal uint64 p2home;
    internal uint64 p3home;
    internal uint64 p4home;
    internal uint64 p5home;
    internal uint64 p6home;
    internal uint32 contextflags;
    internal uint32 mxcsr;
    internal uint16 segcs;
    internal uint16 segds;
    internal uint16 seges;
    internal uint16 segfs;
    internal uint16 seggs;
    internal uint16 segss;
    internal uint32 eflags;
    internal uint64 dr0;
    internal uint64 dr1;
    internal uint64 dr2;
    internal uint64 dr3;
    internal uint64 dr6;
    internal uint64 dr7;
    internal uint64 rax;
    internal uint64 rcx;
    internal uint64 rdx;
    internal uint64 rbx;
    internal uint64 rsp;
    internal uint64 rbp;
    internal uint64 rsi;
    internal uint64 rdi;
    internal uint64 r8;
    internal uint64 r9;
    internal uint64 r10;
    internal uint64 r11;
    internal uint64 r12;
    internal uint64 r13;
    internal uint64 r14;
    internal uint64 r15;
    internal uint64 rip;
    internal array<byte> anon0 = new(512);
    internal array<m128a> vectorregister = new(26);
    internal uint64 vectorcontrol;
    internal uint64 debugcontrol;
    internal uint64 lastbranchtorip;
    internal uint64 lastbranchfromrip;
    internal uint64 lastexceptiontorip;
    internal uint64 lastexceptionfromrip;
}

[GoRecv] internal static uintptr ip(this ref context c) {
    return ((uintptr)c.rip);
}

[GoRecv] internal static uintptr sp(this ref context c) {
    return ((uintptr)c.rsp);
}

// AMD64 does not have link register, so this returns 0.
[GoRecv] internal static uintptr lr(this ref context c) {
    return 0;
}

[GoRecv] internal static void set_lr(this ref context c, uintptr x) {
}

[GoRecv] internal static void set_ip(this ref context c, uintptr x) {
    c.rip = ((uint64)x);
}

[GoRecv] internal static void set_sp(this ref context c, uintptr x) {
    c.rsp = ((uint64)x);
}

[GoRecv] internal static void set_fp(this ref context c, uintptr x) {
    c.rbp = ((uint64)x);
}

internal static void prepareContextForSigResume(ж<context> Ꮡc) {
    ref var c = ref Ꮡc.val;

    c.r8 = c.rsp;
    c.r9 = c.rip;
}

internal static void dumpregs(ж<context> Ꮡr) {
    ref var r = ref Ꮡr.val;

    print("rax     ", ((Δhex)r.rax), "\n");
    print("rbx     ", ((Δhex)r.rbx), "\n");
    print("rcx     ", ((Δhex)r.rcx), "\n");
    print("rdx     ", ((Δhex)r.rdx), "\n");
    print("rdi     ", ((Δhex)r.rdi), "\n");
    print("rsi     ", ((Δhex)r.rsi), "\n");
    print("rbp     ", ((Δhex)r.rbp), "\n");
    print("rsp     ", ((Δhex)r.rsp), "\n");
    print("r8      ", ((Δhex)r.r8), "\n");
    print("r9      ", ((Δhex)r.r9), "\n");
    print("r10     ", ((Δhex)r.r10), "\n");
    print("r11     ", ((Δhex)r.r11), "\n");
    print("r12     ", ((Δhex)r.r12), "\n");
    print("r13     ", ((Δhex)r.r13), "\n");
    print("r14     ", ((Δhex)r.r14), "\n");
    print("r15     ", ((Δhex)r.r15), "\n");
    print("rip     ", ((Δhex)r.rip), "\n");
    print("rflags  ", ((Δhex)r.eflags), "\n");
    print("cs      ", ((Δhex)r.segcs), "\n");
    print("fs      ", ((Δhex)r.segfs), "\n");
    print("gs      ", ((Δhex)r.seggs), "\n");
}

[GoType] partial struct _DISPATCHER_CONTEXT {
    internal uint64 controlPc;
    internal uint64 imageBase;
    internal uintptr functionEntry;
    internal uint64 establisherFrame;
    internal uint64 targetIp;
    internal ж<context> context;
    internal uintptr languageHandler;
    internal uintptr handlerData;
}

[GoRecv] internal static ж<context> ctx(this ref _DISPATCHER_CONTEXT c) {
    return c.context;
}

} // end runtime_package
