// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_darwin_arm.go
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
        private static ref regs32 regs(this ref sigctxt c)
        {
            return ref (ucontext.Value)(c.ctxt).uc_mcontext.ss;
        }

        private static uint r0(this ref sigctxt c)
        {
            return c.regs().r[0L];
        }
        private static uint r1(this ref sigctxt c)
        {
            return c.regs().r[1L];
        }
        private static uint r2(this ref sigctxt c)
        {
            return c.regs().r[2L];
        }
        private static uint r3(this ref sigctxt c)
        {
            return c.regs().r[3L];
        }
        private static uint r4(this ref sigctxt c)
        {
            return c.regs().r[4L];
        }
        private static uint r5(this ref sigctxt c)
        {
            return c.regs().r[5L];
        }
        private static uint r6(this ref sigctxt c)
        {
            return c.regs().r[6L];
        }
        private static uint r7(this ref sigctxt c)
        {
            return c.regs().r[7L];
        }
        private static uint r8(this ref sigctxt c)
        {
            return c.regs().r[8L];
        }
        private static uint r9(this ref sigctxt c)
        {
            return c.regs().r[9L];
        }
        private static uint r10(this ref sigctxt c)
        {
            return c.regs().r[10L];
        }
        private static uint fp(this ref sigctxt c)
        {
            return c.regs().r[11L];
        }
        private static uint ip(this ref sigctxt c)
        {
            return c.regs().r[12L];
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
            return uintptr(c.info.si_addr);
        }
        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
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
            c.regs().r[10L] = x;

        }

        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            c.info.si_addr = x;

        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {

            if (sig == _SIGTRAP) 
                // OS X sets c.sigcode() == TRAP_BRKPT unconditionally for all SIGTRAPs,
                // leaving no way to distinguish a breakpoint-induced SIGTRAP
                // from an asynchronous signal SIGTRAP.
                // They all look breakpoint-induced by default.
                // Try looking at the code to see if it's a breakpoint.
                // The assumption is that we're very unlikely to get an
                // asynchronous SIGTRAP at just the moment that the
                // PC started to point at unmapped memory.
                var pc = uintptr(c.pc()); 
                // OS X will leave the pc just after the instruction.
                var code = (uint32.Value)(@unsafe.Pointer(pc - 4L));
                if (code != 0xe7f001f0UL.Value)
                { 
                    // SIGTRAP on something other than breakpoint.
                    c.set_sigcode(_SI_USER);
                }
                    }
    }
}
