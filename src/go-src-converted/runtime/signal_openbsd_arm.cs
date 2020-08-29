// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_openbsd_arm.go
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

        private static uint r0(this ref sigctxt c)
        {
            return c.regs().sc_r0;
        }
        private static uint r1(this ref sigctxt c)
        {
            return c.regs().sc_r1;
        }
        private static uint r2(this ref sigctxt c)
        {
            return c.regs().sc_r2;
        }
        private static uint r3(this ref sigctxt c)
        {
            return c.regs().sc_r3;
        }
        private static uint r4(this ref sigctxt c)
        {
            return c.regs().sc_r4;
        }
        private static uint r5(this ref sigctxt c)
        {
            return c.regs().sc_r5;
        }
        private static uint r6(this ref sigctxt c)
        {
            return c.regs().sc_r6;
        }
        private static uint r7(this ref sigctxt c)
        {
            return c.regs().sc_r7;
        }
        private static uint r8(this ref sigctxt c)
        {
            return c.regs().sc_r8;
        }
        private static uint r9(this ref sigctxt c)
        {
            return c.regs().sc_r9;
        }
        private static uint r10(this ref sigctxt c)
        {
            return c.regs().sc_r10;
        }
        private static uint fp(this ref sigctxt c)
        {
            return c.regs().sc_r11;
        }
        private static uint ip(this ref sigctxt c)
        {
            return c.regs().sc_r12;
        }
        private static uint sp(this ref sigctxt c)
        {
            return c.regs().sc_usr_sp;
        }
        private static uint lr(this ref sigctxt c)
        {
            return c.regs().sc_usr_lr;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint pc(this ref sigctxt c)
        {
            return c.regs().sc_pc;
        }

        private static uint cpsr(this ref sigctxt c)
        {
            return c.regs().sc_spsr;
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.sigaddr());
        }
        private static uint trap(this ref sigctxt c)
        {
            return 0L;
        }
        private static uint error(this ref sigctxt c)
        {
            return 0L;
        }
        private static uint oldmask(this ref sigctxt c)
        {
            return 0L;
        }

        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return add(@unsafe.Pointer(c.info), 12L).Value;
        }

        private static void set_pc(this ref sigctxt c, uint x)
        {
            c.regs().sc_pc = x;

        }
        private static void set_sp(this ref sigctxt c, uint x)
        {
            c.regs().sc_usr_sp = x;

        }
        private static void set_lr(this ref sigctxt c, uint x)
        {
            c.regs().sc_usr_lr = x;

        }
        private static void set_r10(this ref sigctxt c, uint x)
        {
            c.regs().sc_r10 = x;

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
