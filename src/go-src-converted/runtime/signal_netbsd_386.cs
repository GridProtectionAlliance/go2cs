// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_netbsd_386.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct sigctxt {
    public ptr<siginfo> info;
    public unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
private static ptr<mcontextt> regs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return _addr__addr_(ucontextt.val)(c.ctxt).uc_mcontext!;
}

private static uint eax(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EAX];
}
private static uint ebx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EBX];
}
private static uint ecx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_ECX];
}
private static uint edx(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EDX];
}
private static uint edi(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EDI];
}
private static uint esi(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_ESI];
}
private static uint ebp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EBP];
}
private static uint esp(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_UESP];
}

//go:nosplit
//go:nowritebarrierrec
private static uint eip(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EIP];
}

private static uint eflags(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_EFL];
}
private static uint cs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_CS];
}
private static uint fs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_FS];
}
private static uint gs(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return c.regs().__gregs[_REG_GS];
}
private static uint sigcode(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return uint32(c.info._code);
}
private static uint sigaddr(this ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    return new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_c.info._reason[0]));
}

private static void set_eip(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().__gregs[_REG_EIP] = x;
}
private static void set_esp(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.regs().__gregs[_REG_UESP] = x;
}
private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    c.info._code = int32(x);
}
private static void set_sigaddr(this ptr<sigctxt> _addr_c, uint x) {
    ref sigctxt c = ref _addr_c.val;

    (uint32.val)(@unsafe.Pointer(_addr_c.info._reason[0])).val;

    x;

}

} // end runtime_package
