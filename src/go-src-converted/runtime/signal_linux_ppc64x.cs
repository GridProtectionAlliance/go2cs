// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (ppc64 || ppc64le)
// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2022 March 13 05:26:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_linux_ppc64x.go
namespace go;

using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;

public static partial class runtime_package {

private partial struct sigctxt {
    public ptr<siginfo> info;
    public unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
private static ptr<ptregs> regs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return _addr_(ucontext.val)(c.ctxt).uc_mcontext.regs!;
}

private static ulong r0(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[0];
}
private static ulong r1(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[1];
}
private static ulong r2(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[2];
}
private static ulong r3(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[3];
}
private static ulong r4(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[4];
}
private static ulong r5(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[5];
}
private static ulong r6(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[6];
}
private static ulong r7(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[7];
}
private static ulong r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[8];
}
private static ulong r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[9];
}
private static ulong r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[10];
}
private static ulong r11(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[11];
}
private static ulong r12(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[12];
}
private static ulong r13(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[13];
}
private static ulong r14(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[14];
}
private static ulong r15(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[15];
}
private static ulong r16(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[16];
}
private static ulong r17(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[17];
}
private static ulong r18(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[18];
}
private static ulong r19(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[19];
}
private static ulong r20(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[20];
}
private static ulong r21(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[21];
}
private static ulong r22(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[22];
}
private static ulong r23(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[23];
}
private static ulong r24(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[24];
}
private static ulong r25(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[25];
}
private static ulong r26(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[26];
}
private static ulong r27(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[27];
}
private static ulong r28(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[28];
}
private static ulong r29(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[29];
}
private static ulong r30(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[30];
}
private static ulong r31(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[31];
}
private static ulong sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gpr[1];
}

//go:nosplit
//go:nowritebarrierrec
private static ulong pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().nip;
}

private static ulong trap(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().trap;
}
private static ulong ctr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().ctr;
}
private static ulong link(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().link;
}
private static ulong xer(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().xer;
}
private static ulong ccr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().ccr;
}

private static uint sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.info.si_code);
}
private static ulong sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.info.si_addr;
}
private static System.UIntPtr fault(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(c.regs().dar);
}

private static void set_r0(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().gpr[0] = x;
}
private static void set_r12(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().gpr[12] = x;
}
private static void set_r30(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().gpr[30] = x;
}
private static void set_pc(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().nip = x;
}
private static void set_sp(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().gpr[1] = x;
}
private static void set_link(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().link = x;
}

private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    (uintptr.val).val;

    (add(@unsafe.Pointer(c.info), 2 * sys.PtrSize)) = uintptr(x);
}

} // end runtime_package
