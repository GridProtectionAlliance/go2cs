// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:10 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_freebsd_arm.go
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
            return ref (ucontext.Value)(c.ctxt).uc_mcontext;
        }

        private static uint r0(this ref sigctxt c)
        {
            return c.regs().__gregs[0L];
        }
        private static uint r1(this ref sigctxt c)
        {
            return c.regs().__gregs[1L];
        }
        private static uint r2(this ref sigctxt c)
        {
            return c.regs().__gregs[2L];
        }
        private static uint r3(this ref sigctxt c)
        {
            return c.regs().__gregs[3L];
        }
        private static uint r4(this ref sigctxt c)
        {
            return c.regs().__gregs[4L];
        }
        private static uint r5(this ref sigctxt c)
        {
            return c.regs().__gregs[5L];
        }
        private static uint r6(this ref sigctxt c)
        {
            return c.regs().__gregs[6L];
        }
        private static uint r7(this ref sigctxt c)
        {
            return c.regs().__gregs[7L];
        }
        private static uint r8(this ref sigctxt c)
        {
            return c.regs().__gregs[8L];
        }
        private static uint r9(this ref sigctxt c)
        {
            return c.regs().__gregs[9L];
        }
        private static uint r10(this ref sigctxt c)
        {
            return c.regs().__gregs[10L];
        }
        private static uint fp(this ref sigctxt c)
        {
            return c.regs().__gregs[11L];
        }
        private static uint ip(this ref sigctxt c)
        {
            return c.regs().__gregs[12L];
        }
        private static uint sp(this ref sigctxt c)
        {
            return c.regs().__gregs[13L];
        }
        private static uint lr(this ref sigctxt c)
        {
            return c.regs().__gregs[14L];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint pc(this ref sigctxt c)
        {
            return c.regs().__gregs[15L];
        }

        private static uint cpsr(this ref sigctxt c)
        {
            return c.regs().__gregs[16L];
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.info.si_addr);
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
            return uint32(c.info.si_addr);
        }

        private static void set_pc(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[15L] = x;

        }
        private static void set_sp(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[13L] = x;

        }
        private static void set_lr(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[14L] = x;

        }
        private static void set_r10(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[10L] = x;

        }

        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            c.info.si_addr = uintptr(x);
        }
    }
}
