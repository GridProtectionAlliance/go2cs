// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_freebsd_amd64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct sigctxt
        {
            public ptr<siginfo> info;
            public unsafe.Pointer ctxt;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ptr<mcontext> regs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return _addr_(mcontext.val)(@unsafe.Pointer(_addr_(ucontext.val)(c.ctxt).uc_mcontext))!;
        }

        private static ulong rax(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rax;
        }
        private static ulong rbx(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rbx;
        }
        private static ulong rcx(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rcx;
        }
        private static ulong rdx(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rdx;
        }
        private static ulong rdi(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rdi;
        }
        private static ulong rsi(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rsi;
        }
        private static ulong rbp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rbp;
        }
        private static ulong rsp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rsp;
        }
        private static ulong r8(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r8;
        }
        private static ulong r9(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r9;
        }
        private static ulong r10(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r10;
        }
        private static ulong r11(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r11;
        }
        private static ulong r12(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r12;
        }
        private static ulong r13(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r13;
        }
        private static ulong r14(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r14;
        }
        private static ulong r15(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_r15;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong rip(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rip;
        }

        private static ulong rflags(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_rflags;
        }
        private static ulong cs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().mc_cs;
        }
        private static ulong fs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint64(c.regs().mc_fs);
        }
        private static ulong gs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint64(c.regs().mc_gs);
        }
        private static ulong sigcode(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint64(c.info.si_code);
        }
        private static ulong sigaddr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.info.si_addr;
        }

        private static void set_rip(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().mc_rip = x;
        }
        private static void set_rsp(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().mc_rsp = x;
        }
        private static void set_sigcode(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.info.si_code = int32(x);
        }
        private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.info.si_addr = x;
        }
    }
}
