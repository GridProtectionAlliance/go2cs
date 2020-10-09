// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_linux_riscv64.go
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
        private static ptr<sigcontext> regs(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return _addr__addr_(ucontext.val)(c.ctxt).uc_mcontext!;
        }

        private static ulong ra(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.ra;
        }
        private static ulong sp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.sp;
        }
        private static ulong gp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.gp;
        }
        private static ulong tp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.tp;
        }
        private static ulong t0(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t0;
        }
        private static ulong t1(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t1;
        }
        private static ulong t2(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t2;
        }
        private static ulong s0(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s0;
        }
        private static ulong s1(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s1;
        }
        private static ulong a0(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a0;
        }
        private static ulong a1(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a1;
        }
        private static ulong a2(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a2;
        }
        private static ulong a3(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a3;
        }
        private static ulong a4(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a4;
        }
        private static ulong a5(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a5;
        }
        private static ulong a6(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a6;
        }
        private static ulong a7(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.a7;
        }
        private static ulong s2(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s2;
        }
        private static ulong s3(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s3;
        }
        private static ulong s4(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s4;
        }
        private static ulong s5(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s5;
        }
        private static ulong s6(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s6;
        }
        private static ulong s7(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s7;
        }
        private static ulong s8(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s8;
        }
        private static ulong s9(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s9;
        }
        private static ulong s10(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s10;
        }
        private static ulong s11(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.s11;
        }
        private static ulong t3(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t3;
        }
        private static ulong t4(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t4;
        }
        private static ulong t5(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t5;
        }
        private static ulong t6(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.t6;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong pc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.regs().sc_regs.pc;
        }

        private static uint sigcode(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint32(c.info.si_code);
        }
        private static ulong sigaddr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.info.si_addr;
        }

        private static void set_pc(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_regs.pc = x;
        }
        private static void set_ra(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_regs.ra = x;
        }
        private static void set_sp(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_regs.sp = x;
        }
        private static void set_gp(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_regs.gp = x;
        }

        private static void set_sigcode(this ptr<sigctxt> _addr_c, uint x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.info.si_code = int32(x);
        }
        private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            (uintptr.val)(add(@unsafe.Pointer(c.info), 2L * sys.PtrSize)).val;

            uintptr(x);

        }
    }
}
