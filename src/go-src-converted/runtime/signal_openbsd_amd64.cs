// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_openbsd_amd64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private partial struct sigctxt
        {
            public ptr<siginfo> info;
            public unsafe.Pointer ctxt;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ref sigcontext regs(this ref sigctxt c)
        {
            return (sigcontext.Value)(c.ctxt);
        }

        private static ulong rax(this ref sigctxt c)
        {
            return c.regs().sc_rax;
        }
        private static ulong rbx(this ref sigctxt c)
        {
            return c.regs().sc_rbx;
        }
        private static ulong rcx(this ref sigctxt c)
        {
            return c.regs().sc_rcx;
        }
        private static ulong rdx(this ref sigctxt c)
        {
            return c.regs().sc_rdx;
        }
        private static ulong rdi(this ref sigctxt c)
        {
            return c.regs().sc_rdi;
        }
        private static ulong rsi(this ref sigctxt c)
        {
            return c.regs().sc_rsi;
        }
        private static ulong rbp(this ref sigctxt c)
        {
            return c.regs().sc_rbp;
        }
        private static ulong rsp(this ref sigctxt c)
        {
            return c.regs().sc_rsp;
        }
        private static ulong r8(this ref sigctxt c)
        {
            return c.regs().sc_r8;
        }
        private static ulong r9(this ref sigctxt c)
        {
            return c.regs().sc_r9;
        }
        private static ulong r10(this ref sigctxt c)
        {
            return c.regs().sc_r10;
        }
        private static ulong r11(this ref sigctxt c)
        {
            return c.regs().sc_r11;
        }
        private static ulong r12(this ref sigctxt c)
        {
            return c.regs().sc_r12;
        }
        private static ulong r13(this ref sigctxt c)
        {
            return c.regs().sc_r13;
        }
        private static ulong r14(this ref sigctxt c)
        {
            return c.regs().sc_r14;
        }
        private static ulong r15(this ref sigctxt c)
        {
            return c.regs().sc_r15;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong rip(this ref sigctxt c)
        {
            return c.regs().sc_rip;
        }

        private static ulong rflags(this ref sigctxt c)
        {
            return c.regs().sc_rflags;
        }
        private static ulong cs(this ref sigctxt c)
        {
            return c.regs().sc_cs;
        }
        private static ulong fs(this ref sigctxt c)
        {
            return c.regs().sc_fs;
        }
        private static ulong gs(this ref sigctxt c)
        {
            return c.regs().sc_gs;
        }
        private static ulong sigcode(this ref sigctxt c)
        {
            return uint64(c.info.si_code);
        }
        private static ulong sigaddr(this ref sigctxt c)
        {
            return add(@unsafe.Pointer(c.info), 16L).Value;
        }

        private static void set_rip(this ref sigctxt c, ulong x)
        {
            c.regs().sc_rip = x;

        }
        private static void set_rsp(this ref sigctxt c, ulong x)
        {
            c.regs().sc_rsp = x;

        }
        private static void set_sigcode(this ref sigctxt c, ulong x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, ulong x)
        {
            (uint64.Value)(add(@unsafe.Pointer(c.info), 16L)).Value;

            x;
        }
    }
}
