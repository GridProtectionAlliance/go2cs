// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_netbsd_arm.go
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
        private static ref mcontextt regs(this ref sigctxt c)
        {
            return ref (ucontextt.Value)(c.ctxt).uc_mcontext;
        }

        private static uint r0(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R0];
        }
        private static uint r1(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R1];
        }
        private static uint r2(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R2];
        }
        private static uint r3(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R3];
        }
        private static uint r4(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R4];
        }
        private static uint r5(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R5];
        }
        private static uint r6(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R6];
        }
        private static uint r7(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R7];
        }
        private static uint r8(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R8];
        }
        private static uint r9(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R9];
        }
        private static uint r10(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R10];
        }
        private static uint fp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R11];
        }
        private static uint ip(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R12];
        }
        private static uint sp(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R13];
        }
        private static uint lr(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R14];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint pc(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_R15];
        }

        private static uint cpsr(this ref sigctxt c)
        {
            return c.regs().__gregs[_REG_CPSR];
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.info._reason);
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
            return uint32(c.info._code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return uint32(c.info._reason);
        }

        private static void set_pc(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_R15] = x;

        }
        private static void set_sp(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_R13] = x;

        }
        private static void set_lr(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_R14] = x;

        }
        private static void set_r10(this ref sigctxt c, uint x)
        {
            c.regs().__gregs[_REG_R10] = x;

        }

        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info._code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            c.info._reason = uintptr(x);
        }
    }
}
