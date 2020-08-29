// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build mips mipsle

// package runtime -- go2cs converted at 2020 August 29 08:20:17 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_linux_mipsx.go
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

        private static ref sigcontext regs(this ref sigctxt c)
        {
            return ref (ucontext.Value)(c.ctxt).uc_mcontext;
        }
        private static uint r0(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[0L]);
        }
        private static uint r1(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[1L]);
        }
        private static uint r2(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[2L]);
        }
        private static uint r3(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[3L]);
        }
        private static uint r4(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[4L]);
        }
        private static uint r5(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[5L]);
        }
        private static uint r6(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[6L]);
        }
        private static uint r7(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[7L]);
        }
        private static uint r8(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[8L]);
        }
        private static uint r9(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[9L]);
        }
        private static uint r10(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[10L]);
        }
        private static uint r11(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[11L]);
        }
        private static uint r12(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[12L]);
        }
        private static uint r13(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[13L]);
        }
        private static uint r14(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[14L]);
        }
        private static uint r15(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[15L]);
        }
        private static uint r16(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[16L]);
        }
        private static uint r17(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[17L]);
        }
        private static uint r18(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[18L]);
        }
        private static uint r19(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[19L]);
        }
        private static uint r20(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[20L]);
        }
        private static uint r21(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[21L]);
        }
        private static uint r22(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[22L]);
        }
        private static uint r23(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[23L]);
        }
        private static uint r24(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[24L]);
        }
        private static uint r25(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[25L]);
        }
        private static uint r26(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[26L]);
        }
        private static uint r27(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[27L]);
        }
        private static uint r28(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[28L]);
        }
        private static uint r29(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[29L]);
        }
        private static uint r30(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[30L]);
        }
        private static uint r31(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[31L]);
        }
        private static uint sp(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[29L]);
        }
        private static uint pc(this ref sigctxt c)
        {
            return uint32(c.regs().sc_pc);
        }
        private static uint link(this ref sigctxt c)
        {
            return uint32(c.regs().sc_regs[31L]);
        }
        private static uint lo(this ref sigctxt c)
        {
            return uint32(c.regs().sc_mdlo);
        }
        private static uint hi(this ref sigctxt c)
        {
            return uint32(c.regs().sc_mdhi);
        }

        private static uint sigcode(this ref sigctxt c)
        {
            return uint32(c.info.si_code);
        }
        private static uint sigaddr(this ref sigctxt c)
        {
            return c.info.si_addr;
        }

        private static void set_r30(this ref sigctxt c, uint x)
        {
            c.regs().sc_regs[30L] = uint64(x);

        }
        private static void set_pc(this ref sigctxt c, uint x)
        {
            c.regs().sc_pc = uint64(x);

        }
        private static void set_sp(this ref sigctxt c, uint x)
        {
            c.regs().sc_regs[29L] = uint64(x);

        }
        private static void set_link(this ref sigctxt c, uint x)
        {
            c.regs().sc_regs[31L] = uint64(x);

        }

        private static void set_sigcode(this ref sigctxt c, uint x)
        {
            c.info.si_code = int32(x);

        }
        private static void set_sigaddr(this ref sigctxt c, uint x)
        {
            c.info.si_addr = x;

        }
    }
}
