// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_openbsd_arm64.go
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

            return _addr_(sigcontext.val)(c.ctxt)!;
        }

        private static ulong r0(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[0L]);
        }
        private static ulong r1(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[1L]);
        }
        private static ulong r2(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[2L]);
        }
        private static ulong r3(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[3L]);
        }
        private static ulong r4(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[4L]);
        }
        private static ulong r5(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[5L]);
        }
        private static ulong r6(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[6L]);
        }
        private static ulong r7(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[7L]);
        }
        private static ulong r8(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[8L]);
        }
        private static ulong r9(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[9L]);
        }
        private static ulong r10(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[10L]);
        }
        private static ulong r11(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[11L]);
        }
        private static ulong r12(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[12L]);
        }
        private static ulong r13(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[13L]);
        }
        private static ulong r14(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[14L]);
        }
        private static ulong r15(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[15L]);
        }
        private static ulong r16(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[16L]);
        }
        private static ulong r17(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[17L]);
        }
        private static ulong r18(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[18L]);
        }
        private static ulong r19(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[19L]);
        }
        private static ulong r20(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[20L]);
        }
        private static ulong r21(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[21L]);
        }
        private static ulong r22(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[22L]);
        }
        private static ulong r23(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[23L]);
        }
        private static ulong r24(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[24L]);
        }
        private static ulong r25(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[25L]);
        }
        private static ulong r26(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[26L]);
        }
        private static ulong r27(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[27L]);
        }
        private static ulong r28(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[28L]);
        }
        private static ulong r29(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_x[29L]);
        }
        private static ulong lr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_lr);
        }
        private static ulong sp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_sp);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong rip(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return (uint64)(c.regs().sc_lr);
        }        /* XXX */

        private static ulong fault(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return c.sigaddr();
        }
        private static ulong sigcode(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint64(c.info.si_code);
        }
        private static ulong sigaddr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return new ptr<ptr<ptr<ulong>>>(add(@unsafe.Pointer(c.info), 16L));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static ulong pc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uint64(c.regs().sc_elr);
        }

        private static void set_pc(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_elr = uintptr(x);
        }
        private static void set_sp(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_sp = uintptr(x);
        }
        private static void set_lr(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_lr = uintptr(x);
        }
        private static void set_r28(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.regs().sc_x[28L] = uintptr(x);
        }

        private static void set_sigcode(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            c.info.si_code = int32(x);
        }
        private static void set_sigaddr(this ptr<sigctxt> _addr_c, ulong x)
        {
            ref sigctxt c = ref _addr_c.val;

            (uint64.val)(add(@unsafe.Pointer(c.info), 16L)).val;

            x;

        }
    }
}
