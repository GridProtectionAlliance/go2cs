// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_plan9_amd64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _PAGESIZE = 0x1000;



private partial struct ureg {
    public ulong ax;
    public ulong bx;
    public ulong cx;
    public ulong dx;
    public ulong si;
    public ulong di;
    public ulong bp;
    public ulong r8;
    public ulong r9;
    public ulong r10;
    public ulong r11;
    public ulong r12;
    public ulong r13;
    public ulong r14;
    public ulong r15;
    public ushort ds;
    public ushort es;
    public ushort fs;
    public ushort gs;
    public ulong _type;
    public ulong error; /* error code (or zero) */
    public ulong ip; /* pc */
    public ulong cs; /* old context */
    public ulong flags; /* old flags */
    public ulong sp; /* sp */
    public ulong ss; /* old stack segment */
}

private partial struct sigctxt {
    public ptr<ureg> u;
}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(c.u.ip);
}

private static System.UIntPtr sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(c.u.sp);
}
private static System.UIntPtr lr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(0);
}

private static void setpc(this ptr<sigctxt> _addr_c, System.UIntPtr x) {
    ref sigctxt c = ref _addr_c.val;

    c.u.ip = uint64(x);
}
private static void setsp(this ptr<sigctxt> _addr_c, System.UIntPtr x) {
    ref sigctxt c = ref _addr_c.val;

    c.u.sp = uint64(x);
}
private static void setlr(this ptr<sigctxt> _addr_c, System.UIntPtr x) {
    ref sigctxt c = ref _addr_c.val;

}

private static void savelr(this ptr<sigctxt> _addr_c, System.UIntPtr x) {
    ref sigctxt c = ref _addr_c.val;

}

private static void dumpregs(ptr<ureg> _addr_u) {
    ref ureg u = ref _addr_u.val;

    print("ax    ", hex(u.ax), "\n");
    print("bx    ", hex(u.bx), "\n");
    print("cx    ", hex(u.cx), "\n");
    print("dx    ", hex(u.dx), "\n");
    print("di    ", hex(u.di), "\n");
    print("si    ", hex(u.si), "\n");
    print("bp    ", hex(u.bp), "\n");
    print("sp    ", hex(u.sp), "\n");
    print("r8    ", hex(u.r8), "\n");
    print("r9    ", hex(u.r9), "\n");
    print("r10   ", hex(u.r10), "\n");
    print("r11   ", hex(u.r11), "\n");
    print("r12   ", hex(u.r12), "\n");
    print("r13   ", hex(u.r13), "\n");
    print("r14   ", hex(u.r14), "\n");
    print("r15   ", hex(u.r15), "\n");
    print("ip    ", hex(u.ip), "\n");
    print("flags ", hex(u.flags), "\n");
    print("cs    ", hex(u.cs), "\n");
    print("fs    ", hex(u.fs), "\n");
    print("gs    ", hex(u.gs), "\n");
}

private static void sigpanictramp() {
}

} // end runtime_package
