// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2020 August 29 08:20:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_linux_ppc64x.go
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
        private static ref ptregs regs(this ref sigctxt c)
        {
            return (ucontext.Value)(c.ctxt).uc_mcontext.regs;
        }

        private static ulong r0(this ref sigctxt c)
        {
            return c.regs().gpr[0L];
        }
        private static ulong r1(this ref sigctxt c)
        {
            return c.regs().gpr[1L];
        }
        private static ulong r2(this ref sigctxt c)
        {
            return c.regs().gpr[2L];
        }
        private static ulong r3(this ref sigctxt c)
        {
            return c.regs().gpr[3L];
        }
        private static ulong r4(this ref sigctxt c)
        {
            return c.regs().gpr[4L];
        }
        private static ulong r5(this ref sigctxt c)
        {
            return c.regs().gpr[5L];
        }
        private static ulong r6(this ref sigctxt c)
        {
            return c.regs().gpr[6L];
        }
        private static ulong r7(this ref sigctxt c)
        {
            return c.regs().gpr[7L];
        }
        private static ulong r8(this ref sigctxt c)
        {
            return c.regs().gpr[8L];
        }
        private static ulong r9(this ref sigctxt c)
        {
            return c.regs().gpr[9L];
        }
        private static ulong r10(this ref sigctxt c)
        {
            return c.regs().gpr[10L];
        }
        private static ulong r11(this ref sigctxt c)
        {
            return c.regs().gpr[11L];
        }
        private static ulong r12(this ref sigctxt c)
        {
            return c.regs().gpr[12L];
        }
        private static ulong r13(this ref sigctxt c)
        {
            return c.regs().gpr[13L];
        }
        private static ulong r14(this ref sigctxt c)
        {
            return c.regs().gpr[14L];
        }
        private static ulong r15(this ref sigctxt c)
        {
            return c.regs().gpr[15L];
        }
        private static ulong r16(this ref sigctxt c)
        {
            return c.regs().gpr[16L];
        }
        private static ulong r17(this ref sigctxt c)
        {
            return c.regs().gpr[17L];
        }
        private static ulong r18(this ref sigctxt c)
        {
            return c.regs().gpr[18L];
        }
        private static ulong r19(this ref sigctxt c)
        {
            return c.regs().gpr[19L];
        }
        private static ulong r20(this ref sigctxt c)
        {
            return c.regs().gpr[20L];
        }
        private static ulong r21(this ref sigctxt c)
        {
            return c.regs().gpr[21L];
        }
        private static ulong r22(this ref sigctxt c)
        {
            return c.regs().gpr[22L];
        }
        private static ulong r23(this ref sigctxt c)
        {
            return c.regs().gpr[23L];
        }
        private static ulong r24(this ref sigctxt c)
        {
            return c.regs().gpr[24L];
        }
        private static ulong r25(this ref sigctxt c)
        {
            return c.regs().gpr[25L];
        }
        private static ulong r26(this ref sigctxt c)
        {
            return c.regs().gpr[26L];
        }
        private static ulong r27(this ref sigctxt c)
        {
            return c.regs().gpr[27L];
        }
        private static ulong r28(this ref sigctxt c)
        {
            return c.regs().gpr[28L];
        }
        private static ulong r29(this ref sigctxt c)
        {
            return c.regs().gpr[29L];
        }
        private static ulong r30(this ref sigctxt c)
        {
            return c.regs().gpr[30L];
        }
        private static ulong r31(this ref sigctxt c)
        {
            return c.regs().gpr[31L];
        }
        private static ulong sp(this ref sigctxt c)
        {
            return c.regs().gpr[1L];
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong pc(this ref sigctxt c)
        {
            return c.regs().nip;
        }

        private static ulong trap(this ref sigctxt c)
        {
            return c.regs().trap;
        }
        private static ulong ctr(this ref sigctxt c)
        {
            return c.regs().ctr;
        }
        private static ulong link(this ref sigctxt c)
        {
            return c.regs().link;
        }
        private static ulong xer(this ref sigctxt c)
        {
            return c.regs().xer;
        }
        private static ulong ccr(this ref sigctxt c)
        {
            return c.regs().ccr;
        }

        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static ulong sigaddr(this ref sigctxt c)
        {
            return c.info.si_addr;
        }
        private static System.UIntPtr fault(this ref sigctxt c)
        {
            return uintptr(c.regs().dar);
        }

        private static void set_r0(this ref sigctxt c, ulong x)
        {
            c.regs().gpr[0L] = x;

        }
        private static void set_r12(this ref sigctxt c, ulong x)
        {
            c.regs().gpr[12L] = x;

        }
        private static void set_r30(this ref sigctxt c, ulong x)
        {
            c.regs().gpr[30L] = x;

        }
        private static void set_pc(this ref sigctxt c, ulong x)
        {
            c.regs().nip = x;

        }
        private static void set_sp(this ref sigctxt c, ulong x)
        {
            c.regs().gpr[1L] = x;

        }
        private static void set_link(this ref sigctxt c, ulong x)
        {
            c.regs().link = x;

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
    }
}
