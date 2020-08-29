// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:09 UTC
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
        private static ref mcontext regs(this ref sigctxt c)
        {
            return (mcontext.Value)(@unsafe.Pointer(ref (ucontext.Value)(c.ctxt).uc_mcontext));
        }

        private static ulong rax(this ref sigctxt c)
        {
            return c.regs().mc_rax;
        }
        private static ulong rbx(this ref sigctxt c)
        {
            return c.regs().mc_rbx;
        }
        private static ulong rcx(this ref sigctxt c)
        {
            return c.regs().mc_rcx;
        }
        private static ulong rdx(this ref sigctxt c)
        {
            return c.regs().mc_rdx;
        }
        private static ulong rdi(this ref sigctxt c)
        {
            return c.regs().mc_rdi;
        }
        private static ulong rsi(this ref sigctxt c)
        {
            return c.regs().mc_rsi;
        }
        private static ulong rbp(this ref sigctxt c)
        {
            return c.regs().mc_rbp;
        }
        private static ulong rsp(this ref sigctxt c)
        {
            return c.regs().mc_rsp;
        }
        private static ulong r8(this ref sigctxt c)
        {
            return c.regs().mc_r8;
        }
        private static ulong r9(this ref sigctxt c)
        {
            return c.regs().mc_r9;
        }
        private static ulong r10(this ref sigctxt c)
        {
            return c.regs().mc_r10;
        }
        private static ulong r11(this ref sigctxt c)
        {
            return c.regs().mc_r11;
        }
        private static ulong r12(this ref sigctxt c)
        {
            return c.regs().mc_r12;
        }
        private static ulong r13(this ref sigctxt c)
        {
            return c.regs().mc_r13;
        }
        private static ulong r14(this ref sigctxt c)
        {
            return c.regs().mc_r14;
        }
        private static ulong r15(this ref sigctxt c)
        {
            return c.regs().mc_r15;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong rip(this ref sigctxt c)
        {
            return c.regs().mc_rip;
        }

        private static ulong rflags(this ref sigctxt c)
        {
            return c.regs().mc_rflags;
        }
        private static ulong cs(this ref sigctxt c)
        {
            return c.regs().mc_cs;
        }
        private static ulong fs(this ref sigctxt c)
        {
            return uint64(c.regs().mc_fs);
        }
        private static ulong gs(this ref sigctxt c)
        {
            return uint64(c.regs().mc_gs);
        }
        private static ulong sigcode(this ref sigctxt c)
        {
            return uint64(c.info.si_code);
        }
        private static ulong sigaddr(this ref sigctxt c)
        {
            return c.info.si_addr;
        }

        private static void set_rip(this ref sigctxt c, ulong x)
        {
            c.regs().mc_rip = x;

        }
        private static void set_rsp(this ref sigctxt c, ulong x)
        {
            c.regs().mc_rsp = x;

        }
        private static void set_sigcode(this ref sigctxt c, ulong x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, ulong x)
        {
            c.info.si_addr = x;

        }
    }
}
