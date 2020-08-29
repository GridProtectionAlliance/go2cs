// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_openbsd_386.go
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

        private static uint eax(this ref sigctxt c)
        {
            return c.regs().sc_eax;
        }
        private static uint ebx(this ref sigctxt c)
        {
            return c.regs().sc_ebx;
        }
        private static uint ecx(this ref sigctxt c)
        {
            return c.regs().sc_ecx;
        }
        private static uint edx(this ref sigctxt c)
        {
            return c.regs().sc_edx;
        }
        private static uint edi(this ref sigctxt c)
        {
            return c.regs().sc_edi;
        }
        private static uint esi(this ref sigctxt c)
        {
            return c.regs().sc_esi;
        }
        private static uint ebp(this ref sigctxt c)
        {
            return c.regs().sc_ebp;
        }
        private static uint esp(this ref sigctxt c)
        {
            return c.regs().sc_esp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint eip(this ref sigctxt c)
        {
            return c.regs().sc_eip;
        }

        private static uint eflags(this ref sigctxt c)
        {
            return c.regs().sc_eflags;
        }
        private static uint cs(this ref sigctxt c)
        {
            return c.regs().sc_cs;
        }
        private static uint fs(this ref sigctxt c)
        {
            return c.regs().sc_fs;
        }
        private static uint gs(this ref sigctxt c)
        {
            return c.regs().sc_gs;
        }
        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return add(@unsafe.Pointer(c.info), 12L).Value;
        }

        private static void set_eip(this ref sigctxt c, uint x)
        {
            c.regs().sc_eip = x;

        }
        private static void set_esp(this ref sigctxt c, uint x)
        {
            c.regs().sc_esp = x;

        }
        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            (uint32.Value)(add(@unsafe.Pointer(c.info), 12L)).Value;

            x;
        }
    }
}
