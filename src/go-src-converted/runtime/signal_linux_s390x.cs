// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:21 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_linux_s390x.go
using sys = go.runtime.@internal.sys_package;
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
        private static ref sigcontext regs(this ref sigctxt c)
        {
            return (sigcontext.Value)(@unsafe.Pointer(ref (ucontext.Value)(c.ctxt).uc_mcontext));
        }

        private static ulong r0(this ref sigctxt c)
        {
            return c.regs().gregs[0L];
        }
        private static ulong r1(this ref sigctxt c)
        {
            return c.regs().gregs[1L];
        }
        private static ulong r2(this ref sigctxt c)
        {
            return c.regs().gregs[2L];
        }
        private static ulong r3(this ref sigctxt c)
        {
            return c.regs().gregs[3L];
        }
        private static ulong r4(this ref sigctxt c)
        {
            return c.regs().gregs[4L];
        }
        private static ulong r5(this ref sigctxt c)
        {
            return c.regs().gregs[5L];
        }
        private static ulong r6(this ref sigctxt c)
        {
            return c.regs().gregs[6L];
        }
        private static ulong r7(this ref sigctxt c)
        {
            return c.regs().gregs[7L];
        }
        private static ulong r8(this ref sigctxt c)
        {
            return c.regs().gregs[8L];
        }
        private static ulong r9(this ref sigctxt c)
        {
            return c.regs().gregs[9L];
        }
        private static ulong r10(this ref sigctxt c)
        {
            return c.regs().gregs[10L];
        }
        private static ulong r11(this ref sigctxt c)
        {
            return c.regs().gregs[11L];
        }
        private static ulong r12(this ref sigctxt c)
        {
            return c.regs().gregs[12L];
        }
        private static ulong r13(this ref sigctxt c)
        {
            return c.regs().gregs[13L];
        }
        private static ulong r14(this ref sigctxt c)
        {
            return c.regs().gregs[14L];
        }
        private static ulong r15(this ref sigctxt c)
        {
            return c.regs().gregs[15L];
        }
        private static ulong link(this ref sigctxt c)
        {
            return c.regs().gregs[14L];
        }
        private static ulong sp(this ref sigctxt c)
        {
            return c.regs().gregs[15L];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong pc(this ref sigctxt c)
        {
            return c.regs().psw_addr;
        }

        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static ulong sigaddr(this ref sigctxt c)
        {
            return c.info.si_addr;
        }

        private static void set_r0(this ref sigctxt c, ulong x)
        {
            c.regs().gregs[0L] = x;

        }
        private static void set_r13(this ref sigctxt c, ulong x)
        {
            c.regs().gregs[13L] = x;

        }
        private static void set_link(this ref sigctxt c, ulong x)
        {
            c.regs().gregs[14L] = x;

        }
        private static void set_sp(this ref sigctxt c, ulong x)
        {
            c.regs().gregs[15L] = x;

        }
        private static void set_pc(this ref sigctxt c, ulong x)
        {
            c.regs().psw_addr = x;

        }
        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, ulong x)
        {
            (uintptr.Value)(add(@unsafe.Pointer(c.info), 2L * sys.PtrSize)).Value;

            uintptr(x);
        }

        private static void dumpregs(ref sigctxt c)
        {
            print("r0   ", hex(c.r0()), "\t");
            print("r1   ", hex(c.r1()), "\n");
            print("r2   ", hex(c.r2()), "\t");
            print("r3   ", hex(c.r3()), "\n");
            print("r4   ", hex(c.r4()), "\t");
            print("r5   ", hex(c.r5()), "\n");
            print("r6   ", hex(c.r6()), "\t");
            print("r7   ", hex(c.r7()), "\n");
            print("r8   ", hex(c.r8()), "\t");
            print("r9   ", hex(c.r9()), "\n");
            print("r10  ", hex(c.r10()), "\t");
            print("r11  ", hex(c.r11()), "\n");
            print("r12  ", hex(c.r12()), "\t");
            print("r13  ", hex(c.r13()), "\n");
            print("r14  ", hex(c.r14()), "\t");
            print("r15  ", hex(c.r15()), "\n");
            print("pc   ", hex(c.pc()), "\t");
            print("link ", hex(c.link()), "\n");
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr sigpc(this ref sigctxt c)
        {
            return uintptr(c.pc());
        }

        private static System.UIntPtr sigsp(this ref sigctxt c)
        {
            return uintptr(c.sp());
        }
        private static System.UIntPtr siglr(this ref sigctxt c)
        {
            return uintptr(c.link());
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.sigaddr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ref sigctxt c, uint sig, ref g gp)
        { 
            // We arrange link, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save LINK to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.MinFrameSize;
            c.set_sp(sp) * (uint64.Value)(@unsafe.Pointer(uintptr(sp)));

            c.link();

            var pc = uintptr(gp.sigpc); 

            // If we don't recognize the PC as code
            // but we do recognize the link register as code,
            // then assume this was a call to non-code and treat like
            // pc == 0, to make unwinding show the context.
            if (pc != 0L && !findfunc(pc).valid() && findfunc(uintptr(c.link())).valid())
            {
                pc = 0L;
            } 

            // Don't bother saving PC if it's zero, which is
            // probably a call to a nil func: the old link register
            // is more useful in the stack trace.
            if (pc != 0L)
            {
                c.set_link(uint64(pc));
            } 

            // In case we are panicking from external C code
            c.set_r0(0L);
            c.set_r13(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint64(funcPC(sigpanic)));
        }
    }
}
