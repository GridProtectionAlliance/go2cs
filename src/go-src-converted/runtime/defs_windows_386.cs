// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_windows_386.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _CONTEXT_CONTROL = 0x10001;



private partial struct floatingsavearea {
    public uint controlword;
    public uint statusword;
    public uint tagword;
    public uint erroroffset;
    public uint errorselector;
    public uint dataoffset;
    public uint dataselector;
    public array<byte> registerarea;
    public uint cr0npxstate;
}

private partial struct context {
    public uint contextflags;
    public uint dr0;
    public uint dr1;
    public uint dr2;
    public uint dr3;
    public uint dr6;
    public uint dr7;
    public floatingsavearea floatsave;
    public uint seggs;
    public uint segfs;
    public uint seges;
    public uint segds;
    public uint edi;
    public uint esi;
    public uint ebx;
    public uint edx;
    public uint ecx;
    public uint eax;
    public uint ebp;
    public uint eip;
    public uint segcs;
    public uint eflags;
    public uint esp;
    public uint segss;
    public array<byte> extendedregisters;
}

private static System.UIntPtr ip(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.eip);
}
private static System.UIntPtr sp(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return uintptr(c.esp);
}

// 386 does not have link register, so this returns 0.
private static System.UIntPtr lr(this ptr<context> _addr_c) {
    ref context c = ref _addr_c.val;

    return 0;
}
private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

}

private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.eip = uint32(x);
}
private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x) {
    ref context c = ref _addr_c.val;

    c.esp = uint32(x);
}

private static void dumpregs(ptr<context> _addr_r) {
    ref context r = ref _addr_r.val;

    print("eax     ", hex(r.eax), "\n");
    print("ebx     ", hex(r.ebx), "\n");
    print("ecx     ", hex(r.ecx), "\n");
    print("edx     ", hex(r.edx), "\n");
    print("edi     ", hex(r.edi), "\n");
    print("esi     ", hex(r.esi), "\n");
    print("ebp     ", hex(r.ebp), "\n");
    print("esp     ", hex(r.esp), "\n");
    print("eip     ", hex(r.eip), "\n");
    print("eflags  ", hex(r.eflags), "\n");
    print("cs      ", hex(r.segcs), "\n");
    print("fs      ", hex(r.segfs), "\n");
    print("gs      ", hex(r.seggs), "\n");
}

} // end runtime_package
