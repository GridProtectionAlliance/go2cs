// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux darwin

// package runtime -- go2cs converted at 2020 August 29 08:20:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_arm64.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void dumpregs(ref sigctxt c)
        {
            print("r0      ", hex(c.r0()), "\n");
            print("r1      ", hex(c.r1()), "\n");
            print("r2      ", hex(c.r2()), "\n");
            print("r3      ", hex(c.r3()), "\n");
            print("r4      ", hex(c.r4()), "\n");
            print("r5      ", hex(c.r5()), "\n");
            print("r6      ", hex(c.r6()), "\n");
            print("r7      ", hex(c.r7()), "\n");
            print("r8      ", hex(c.r8()), "\n");
            print("r9      ", hex(c.r9()), "\n");
            print("r10     ", hex(c.r10()), "\n");
            print("r11     ", hex(c.r11()), "\n");
            print("r12     ", hex(c.r12()), "\n");
            print("r13     ", hex(c.r13()), "\n");
            print("r14     ", hex(c.r14()), "\n");
            print("r15     ", hex(c.r15()), "\n");
            print("r16     ", hex(c.r16()), "\n");
            print("r17     ", hex(c.r17()), "\n");
            print("r18     ", hex(c.r18()), "\n");
            print("r19     ", hex(c.r19()), "\n");
            print("r20     ", hex(c.r20()), "\n");
            print("r21     ", hex(c.r21()), "\n");
            print("r22     ", hex(c.r22()), "\n");
            print("r23     ", hex(c.r23()), "\n");
            print("r24     ", hex(c.r24()), "\n");
            print("r25     ", hex(c.r25()), "\n");
            print("r26     ", hex(c.r26()), "\n");
            print("r27     ", hex(c.r27()), "\n");
            print("r28     ", hex(c.r28()), "\n");
            print("r29     ", hex(c.r29()), "\n");
            print("lr      ", hex(c.lr()), "\n");
            print("sp      ", hex(c.sp()), "\n");
            print("pc      ", hex(c.pc()), "\n");
            print("fault   ", hex(c.fault()), "\n");
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
            return uintptr(c.lr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ref sigctxt c, uint sig, ref g gp)
        { 
            // We arrange lr, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save LR to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.SpAlign; // needs only sizeof uint64, but must align the stack
            c.set_sp(sp) * (uint64.Value)(@unsafe.Pointer(uintptr(sp)));

            c.lr();

            var pc = gp.sigpc; 

            // If we don't recognize the PC as code
            // but we do recognize the link register as code,
            // then assume this was a call to non-code and treat like
            // pc == 0, to make unwinding show the context.
            if (pc != 0L && !findfunc(pc).valid() && findfunc(uintptr(c.lr())).valid())
            {
                pc = 0L;
            } 

            // Don't bother saving PC if it's zero, which is
            // probably a call to a nil func: the old link register
            // is more useful in the stack trace.
            if (pc != 0L)
            {
                c.set_lr(uint64(pc));
            } 

            // In case we are panicking from external C code
            c.set_r28(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint64(funcPC(sigpanic)));
        }
    }
}
