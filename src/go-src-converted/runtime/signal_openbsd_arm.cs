// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_openbsd_arm.go
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

private static uint r0(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r0;
}
private static uint r1(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r1;
}
private static uint r2(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r2;
}
private static uint r3(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r3;
}
private static uint r4(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r4;
}
private static uint r5(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r5;
}
private static uint r6(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r6;
}
private static uint r7(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r7;
}
private static uint r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r8;
}
private static uint r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r9;
}
private static uint r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r10;
}
private static uint fp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r11;
}
private static uint ip(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_r12;
}
private static uint sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_usr_sp;
}
private static uint lr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_usr_lr;
}

//go:nosplit
//go:nowritebarrierrec
private static uint pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_pc;
}

private static uint cpsr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sc_spsr;
}
private static System.UIntPtr fault(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(c.sigaddr());
}
private static uint trap(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return 0;
}
private static uint error(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return 0;
}
private static uint oldmask(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return 0;
}

private static uint sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.info.si_code);
}
private static uint sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return new ptr<ptr<ptr<uint>>>(add(@unsafe.Pointer(c.info), 16));
}

private static void set_pc(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_pc = x;
}
private static void set_sp(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_usr_sp = x;
}
private static void set_lr(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_usr_lr = x;
}
private static void set_r10(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sc_r10 = x;
}

private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    (uint32.val)(add(@unsafe.Pointer(c.info), 16)).val;

    x;

}

} // end runtime_package
