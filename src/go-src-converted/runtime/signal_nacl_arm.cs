// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_nacl_arm.go
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
        private static ref excregsarm regs(this ref sigctxt c)
        {
            return ref (exccontext.Value)(c.ctxt).regs;
        }

        private static uint r0(this ref sigctxt c)
        {
            return c.regs().r0;
        }
        private static uint r1(this ref sigctxt c)
        {
            return c.regs().r1;
        }
        private static uint r2(this ref sigctxt c)
        {
            return c.regs().r2;
        }
        private static uint r3(this ref sigctxt c)
        {
            return c.regs().r3;
        }
        private static uint r4(this ref sigctxt c)
        {
            return c.regs().r4;
        }
        private static uint r5(this ref sigctxt c)
        {
            return c.regs().r5;
        }
        private static uint r6(this ref sigctxt c)
        {
            return c.regs().r6;
        }
        private static uint r7(this ref sigctxt c)
        {
            return c.regs().r7;
        }
        private static uint r8(this ref sigctxt c)
        {
            return c.regs().r8;
        }
        private static uint r9(this ref sigctxt c)
        {
            return c.regs().r9;
        }
        private static uint r10(this ref sigctxt c)
        {
            return c.regs().r10;
        }
        private static uint fp(this ref sigctxt c)
        {
            return c.regs().r11;
        }
        private static uint ip(this ref sigctxt c)
        {
            return c.regs().r12;
        }
        private static uint sp(this ref sigctxt c)
        {
            return c.regs().sp;
        }
        private static uint lr(this ref sigctxt c)
        {
            return c.regs().lr;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static uint pc(this ref sigctxt c)
        {
            return c.regs().pc;
        }

        private static uint cpsr(this ref sigctxt c)
        {
            return c.regs().cpsr;
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return ~uintptr(0L);
        }
        private static uint trap(this ref sigctxt c)
        {
            return ~uint32(0L);
        }
        private static uint error(this ref sigctxt c)
        {
            return ~uint32(0L);
        }
        private static uint oldmask(this ref sigctxt c)
        {
            return ~uint32(0L);
        }

        private static uint sigcode(this ref sigctxt c)
        {
            return 0L;
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return 0L;
        }

        private static void set_pc(this ref sigctxt c, uint x)
        {
            c.regs().pc = x;

        }
        private static void set_sp(this ref sigctxt c, uint x)
        {
            c.regs().sp = x;

        }
        private static void set_lr(this ref sigctxt c, uint x)
        {
            c.regs().lr = x;

        }
        private static void set_r10(this ref sigctxt c, uint x)
        {
            c.regs().r10 = x;

        }

        private static void set_sigcode(this ref sigctxt c, uint x)
        {
        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
        }
    }
}
