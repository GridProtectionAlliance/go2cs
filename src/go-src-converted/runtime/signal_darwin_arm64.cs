// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_darwin_arm64.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct sigctxt {
    public ptr<siginfo> info;
    public unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
private static ptr<regs64> regs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return _addr__addr_(ucontext.val)(c.ctxt).uc_mcontext.ss!;
}

private static ulong r0(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[0];
}
private static ulong r1(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[1];
}
private static ulong r2(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[2];
}
private static ulong r3(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[3];
}
private static ulong r4(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[4];
}
private static ulong r5(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[5];
}
private static ulong r6(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[6];
}
private static ulong r7(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[7];
}
private static ulong r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[8];
}
private static ulong r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[9];
}
private static ulong r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[10];
}
private static ulong r11(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[11];
}
private static ulong r12(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[12];
}
private static ulong r13(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[13];
}
private static ulong r14(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[14];
}
private static ulong r15(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[15];
}
private static ulong r16(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[16];
}
private static ulong r17(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[17];
}
private static ulong r18(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[18];
}
private static ulong r19(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[19];
}
private static ulong r20(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[20];
}
private static ulong r21(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[21];
}
private static ulong r22(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[22];
}
private static ulong r23(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[23];
}
private static ulong r24(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[24];
}
private static ulong r25(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[25];
}
private static ulong r26(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[26];
}
private static ulong r27(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[27];
}
private static ulong r28(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().x[28];
}
private static ulong r29(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().fp;
}
private static ulong lr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().lr;
}
private static ulong sp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().sp;
}

//go:nosplit
//go:nowritebarrierrec
private static ulong pc(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().pc;
}

private static System.UIntPtr fault(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uintptr(@unsafe.Pointer(c.info.si_addr));
}

private static ulong sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint64(c.info.si_code);
}
private static ulong sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint64(uintptr(@unsafe.Pointer(c.info.si_addr)));
}

private static void set_pc(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().pc = x;
}
private static void set_sp(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().sp = x;
}
private static void set_lr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().lr = x;
}
private static void set_r28(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().x[28] = x;
}

private static void set_sigcode(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_addr = (byte.val)(@unsafe.Pointer(uintptr(x)));
}

//go:nosplit
private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig) {
    ref sigctxt c = ref _addr_c.val;


    if (sig == _SIGTRAP) 
        // OS X sets c.sigcode() == TRAP_BRKPT unconditionally for all SIGTRAPs,
        // leaving no way to distinguish a breakpoint-induced SIGTRAP
        // from an asynchronous signal SIGTRAP.
        // They all look breakpoint-induced by default.
        // Try looking at the code to see if it's a breakpoint.
        // The assumption is that we're very unlikely to get an
        // asynchronous SIGTRAP at just the moment that the
        // PC started to point at unmapped memory.
        var pc = uintptr(c.pc()); 
        // OS X will leave the pc just after the instruction.
        var code = (uint32.val)(@unsafe.Pointer(pc - 4));
        if (code != 0xd4200000.val) { 
            // SIGTRAP on something other than breakpoint.
            c.set_sigcode(_SI_USER);

        }
    }

} // end runtime_package
