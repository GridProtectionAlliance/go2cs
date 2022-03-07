// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (mips || mipsle)
// +build linux
// +build mips mipsle

// package runtime -- go2cs converted at 2022 March 06 22:11:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_linux_mipsx.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct sigctxt {
    public ptr<siginfo> info;
    public unsafe.Pointer ctxt;
}

private static ptr<sigcontext> regs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return _addr__addr_(ucontext.val)(c.ctxt).uc_mcontext!;
}
private static uint r0(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[0]);
}
private static uint r1(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[1]);
}
private static uint r2(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[2]);
}
private static uint r3(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[3]);
}
private static uint r4(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[4]);
}
private static uint r5(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[5]);
}
private static uint r6(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[6]);
}
private static uint r7(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[7]);
}
private static uint r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[8]);
}
private static uint r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[9]);
}
private static uint r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[10]);
}
private static uint r11(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[11]);
}
private static uint r12(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[12]);
}
private static uint r13(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[13]);
}
private static uint r14(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[14]);
}
private static uint r15(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[15]);
}
private static uint r16(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[16]);
}
private static uint r17(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[17]);
}
private static uint r18(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[18]);
}
private static uint r19(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[19]);
}
private static uint r20(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[20]);
}
private static uint r21(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[21]);
}
private static uint r22(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[22]);
}
private static uint r23(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[23]);
}
private static uint r24(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[24]);
}
private static uint r25(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[25]);
}
private static uint r26(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[26]);
}
private static uint r27(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[27]);
}
private static uint r28(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[28]);
}
private static uint r29(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[29]);
}
private static uint r30(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[30]);
}
private static uint r31(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[31]);
}
private static uint sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[29]);
}
private static uint pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_pc);
}
private static uint link(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_regs[31]);
}
private static uint lo(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_mdlo);
}
private static uint hi(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.regs().sc_mdhi);
}

private static uint sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.info.si_code);
}
private static uint sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.info.si_addr;
}

private static void set_r30(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_regs[30] = uint64(x);
}
private static void set_pc(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_pc = uint64(x);
}
private static void set_sp(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_regs[29] = uint64(x);
}
private static void set_link(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_regs[31] = uint64(x);
}

private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_addr = x;
}

} // end runtime_package
