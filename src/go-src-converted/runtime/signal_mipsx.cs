// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build mips mipsle

// package runtime -- go2cs converted at 2020 August 29 08:20:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_mipsx.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
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
            print("r16  ", hex(c.r16()), "\t");
            print("r17  ", hex(c.r17()), "\n");
            print("r18  ", hex(c.r18()), "\t");
            print("r19  ", hex(c.r19()), "\n");
            print("r20  ", hex(c.r20()), "\t");
            print("r21  ", hex(c.r21()), "\n");
            print("r22  ", hex(c.r22()), "\t");
            print("r23  ", hex(c.r23()), "\n");
            print("r24  ", hex(c.r24()), "\t");
            print("r25  ", hex(c.r25()), "\n");
            print("r26  ", hex(c.r26()), "\t");
            print("r27  ", hex(c.r27()), "\n");
            print("r28  ", hex(c.r28()), "\t");
            print("r29  ", hex(c.r29()), "\n");
            print("r30  ", hex(c.r30()), "\t");
            print("r31  ", hex(c.r31()), "\n");
            print("pc   ", hex(c.pc()), "\t");
            print("link ", hex(c.link()), "\n");
            print("lo   ", hex(c.lo()), "\t");
            print("hi   ", hex(c.hi()), "\n");
        }

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
            c.set_sp(sp) * (uint32.Value)(@unsafe.Pointer(uintptr(sp)));

            c.link();

            var pc = gp.sigpc; 

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
                c.set_link(uint32(pc));
            } 

            // In case we are panicking from external C code
            c.set_r30(uint32(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint32(funcPC(sigpanic)));
        }
    }
}
