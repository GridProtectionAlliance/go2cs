// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:40 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_openbsd_arm64.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct sigctxt {
    public ptr<siginfo> info;
    public unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
private static ptr<sigcontext> regs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return _addr_(sigcontext.val)(c.ctxt)!;
}

private static ulong r0(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[0]);
}
private static ulong r1(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[1]);
}
private static ulong r2(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[2]);
}
private static ulong r3(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[3]);
}
private static ulong r4(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[4]);
}
private static ulong r5(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[5]);
}
private static ulong r6(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[6]);
}
private static ulong r7(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[7]);
}
private static ulong r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[8]);
}
private static ulong r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[9]);
}
private static ulong r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[10]);
}
private static ulong r11(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[11]);
}
private static ulong r12(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[12]);
}
private static ulong r13(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[13]);
}
private static ulong r14(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[14]);
}
private static ulong r15(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[15]);
}
private static ulong r16(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[16]);
}
private static ulong r17(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[17]);
}
private static ulong r18(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[18]);
}
private static ulong r19(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[19]);
}
private static ulong r20(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[20]);
}
private static ulong r21(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[21]);
}
private static ulong r22(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[22]);
}
private static ulong r23(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[23]);
}
private static ulong r24(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[24]);
}
private static ulong r25(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[25]);
}
private static ulong r26(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[26]);
}
private static ulong r27(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[27]);
}
private static ulong r28(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[28]);
}
private static ulong r29(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_x[29]);
}
private static ulong lr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_lr);
}
private static ulong sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_sp);
}

//go:nosplit
//go:nowritebarrierrec
private static ulong rip(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return (uint64)(c.regs().sc_lr);
}/* XXX */

private static ulong fault(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.sigaddr();
}
private static ulong sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint64(c.info.si_code);
}
private static ulong sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return new ptr<ptr<ptr<ulong>>>(add(@unsafe.Pointer(c.info), 16));
}

//go:nosplit
//go:nowritebarrierrec
private static ulong pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint64(c.regs().sc_elr);
}

private static void set_pc(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_elr = uintptr(x);
}
private static void set_sp(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_sp = uintptr(x);
}
private static void set_lr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_lr = uintptr(x);
}
private static void set_r28(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_x[28] = uintptr(x);
}

private static void set_sigcode(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    (uint64.val)(add(@unsafe.Pointer(c.info), 16)).val;

    x;

}

} // end runtime_package
