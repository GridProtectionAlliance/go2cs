// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package runtime -- go2cs converted at 2020 October 08 03:23:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_arm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void dumpregs(ptr<sigctxt> _addr_c)
        {
            ref sigctxt c = ref _addr_c.val;

            print("trap    ", hex(c.trap()), "\n");
            print("error   ", hex(c.error()), "\n");
            print("oldmask ", hex(c.oldmask()), "\n");
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
            print("fp      ", hex(c.fp()), "\n");
            print("ip      ", hex(c.ip()), "\n");
            print("sp      ", hex(c.sp()), "\n");
            print("lr      ", hex(c.lr()), "\n");
            print("pc      ", hex(c.pc()), "\n");
            print("cpsr    ", hex(c.cpsr()), "\n");
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
            var sp = c.sp() - 4L;
            c.set_sp(sp) * (uint32.val)(@unsafe.Pointer(uintptr(sp)));

            c.lr();

            var pc = gp.sigpc;

            if (shouldPushSigpanic(gp, pc, uintptr(c.lr())))
            { 
                // Make it look the like faulting PC called sigpanic.
                c.set_lr(uint32(pc));

            } 

            // In case we are panicking from external C code
            c.set_r10(uint32(uintptr(@unsafe.Pointer(gp))));
            c.set_pc(uint32(funcPC(sigpanic)));

        }

        private static void pushCall(this ptr<sigctxt> _addr_c, System.UIntPtr targetPC, System.UIntPtr resumePC)
        {
            ref sigctxt c = ref _addr_c.val;
 
            // Push the LR to stack, as we'll clobber it in order to
            // push the call. The function being pushed is responsible
            // for restoring the LR and setting the SP back.
            // This extra slot is known to gentraceback.
            var sp = c.sp() - 4L;
            c.set_sp(sp) * (uint32.val)(@unsafe.Pointer(uintptr(sp)));

            c.lr(); 
            // Set up PC and LR to pretend the function being signaled
            // calls targetPC at resumePC.
            c.set_lr(uint32(resumePC));
            c.set_pc(uint32(targetPC));

        }
    }
}
