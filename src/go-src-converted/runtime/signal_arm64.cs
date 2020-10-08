// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin freebsd linux netbsd openbsd

// package runtime -- go2cs converted at 2020 October 08 03:23:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_arm64.go
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

            return uintptr(c.lr());
        }

        // preparePanic sets up the stack to look like a call to sigpanic.
        private static void preparePanic(this ptr<sigctxt> _addr_c, uint sig, ptr<g> _addr_gp)
        {
            ref sigctxt c = ref _addr_c.val;
            ref g gp = ref _addr_gp.val;
 
            // We arrange lr, and pc to pretend the panicking
            // function calls sigpanic directly.
            // Always save LR to stack so that panics in leaf
            // functions are correctly handled. This smashes
            // the stack frame but we're not going back there
            // anyway.
            var sp = c.sp() - sys.SpAlign; // needs only sizeof uint64, but must align the stack
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.lr();

            var pc = gp.sigpc;

            if (shouldPushSigpanic(gp, pc, uintptr(c.lr())))
            { 
                // Make it look the like faulting PC called sigpanic.
                c.set_lr(uint64(pc));

            } 

            // In case we are panicking from external C code
            c.set_r28(uint64(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint64(funcPC(sigpanic)));

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Push the LR to stack, as we'll clobber it in order to
            // push the call. The function being pushed is responsible
            // for restoring the LR and setting the SP back.
            // This extra space is known to gentraceback.
            var sp = c.sp() - 16L; // SP needs 16-byte alignment
            c.set_sp(sp) * (uint64.val)(@unsafe.Pointer(uintptr(sp)));

            c.lr(); 
            // Set up PC and LR to pretend the function being signaled
            // calls targetPC at resumePC.
            c.set_lr(uint64(resumePC));
            c.set_pc(uint64(targetPC));

        }
    }
}
