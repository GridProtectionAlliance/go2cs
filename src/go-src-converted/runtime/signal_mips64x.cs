// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build mips64 mips64le

// package runtime -- go2cs converted at 2020 October 08 03:23:17 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_mips64x.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void dumpregs(ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

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

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr sigpc(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.pc());
        }

        private static System.UIntPtr sigsp(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.sp());
        }
        private static System.UIntPtr siglr(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.link());
        }
        private static System.UIntPtr fault(this ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            return uintptr(c.sigaddr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ptr<sigctxt> _addr_c, uint sig, ptr<g> _addr_gp)
        {
            ref sigctxt c = ref _addr_c.val;
            ref g gp = ref _addr_gp.val;
 
            // We arrange link, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save LINK to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.PtrSize;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.link();

            var pc = gp.sigpc;

            if (shouldPushSigpanic(gp, pc, uintptr(c.link())))
            { 
                // Make it look the like faulting PC called sigpanic.
                c.set_link(uint64(pc));

            } 

            // In case we are panicking from external C code
            var sigpanicPC = uint64(funcPC(sigpanic));
            c.set_r28(sigpanicPC >> (int)(32L) << (int)(32L)); // RSB register
            c.set_r30(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(sigpanicPC);

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Push the LR to stack, as we'll clobber it in order to
            // push the call. The function being pushed is responsible
            // for restoring the LR and setting the SP back.
            // This extra slot is known to gentraceback.
            var sp = c.sp() - 8L;
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.link(); 
            // Set up PC and LR to pretend the function being signaled
            // calls targetPC at resumePC.
            c.set_link(uint64(resumePC));
            c.set_pc(uint64(targetPC));

        }
    }
}
