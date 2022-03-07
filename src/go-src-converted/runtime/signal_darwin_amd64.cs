// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:25 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_darwin_amd64.go
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

private static ulong rax(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rax;
}
private static ulong rbx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rbx;
}
private static ulong rcx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rcx;
}
private static ulong rdx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rdx;
}
private static ulong rdi(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rdi;
}
private static ulong rsi(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rsi;
}
private static ulong rbp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rbp;
}
private static ulong rsp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rsp;
}
private static ulong r8(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r8;
}
private static ulong r9(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r9;
}
private static ulong r10(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r10;
}
private static ulong r11(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r11;
}
private static ulong r12(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r12;
}
private static ulong r13(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r13;
}
private static ulong r14(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r14;
}
private static ulong r15(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().r15;
}

//go:nosplit
//go:nowritebarrierrec
private static ulong rip(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rip;
}

private static ulong rflags(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().rflags;
}
private static ulong cs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().cs;
}
private static ulong fs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().fs;
}
private static ulong gs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().gs;
}
private static ulong sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint64(c.info.si_code);
}
private static ulong sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.info.si_addr;
}

private static void set_rip(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().rip = x;
}
private static void set_rsp(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().rsp = x;
}
private static void set_sigcode(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x) {
    ref sigctxt c = ref _addr_c.val;

    c.info.si_addr = x;
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
        var pc = uintptr(c.rip()); 
        // OS X will leave the pc just after the INT 3 instruction.
        // INT 3 is usually 1 byte, but there is a 2-byte form.
        ptr<array<byte>> code = new ptr<ptr<array<byte>>>(@unsafe.Pointer(pc - 2));
        if (code[1] != 0xCC && (code[0] != 0xCD || code[1] != 3)) { 
            // SIGTRAP on something other than INT 3.
            c.set_sigcode(_SI_USER);

        }
    else if (sig == _SIGSEGV) 
        // x86-64 has 48-bit virtual addresses. The top 16 bits must echo bit 47.
        // The hardware delivers a different kind of fault for a malformed address
        // than it does for an attempt to access a valid but unmapped address.
        // OS X 10.9.2 mishandles the malformed address case, making it look like
        // a user-generated signal (like someone ran kill -SEGV ourpid).
        // We pass user-generated signals to os/signal, or else ignore them.
        // Doing that here - and returning to the faulting code - results in an
        // infinite loop. It appears the best we can do is rewrite what the kernel
        // delivers into something more like the truth. The address used below
        // has very little chance of being the one that caused the fault, but it is
        // malformed, it is clearly not a real pointer, and if it does get printed
        // in real life, people will probably search for it and find this code.
        // There are no Google hits for b01dfacedebac1e or 0xb01dfacedebac1e
        // as I type this comment.
        if (c.sigcode() == _SI_USER) {
            c.set_sigcode(_SI_USER + 1);
            c.set_sigaddr(0xb01dfacedebac1e);
        }
    }

} // end runtime_package
